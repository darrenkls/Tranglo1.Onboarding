using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.Watchlist;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.Onboarding.Domain.Common.Extensions;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.Onboarding.Application.Common;
using Tranglo1.Onboarding.Domain.Common.SingleScreening;

namespace Tranglo1.Onboarding.Application.Command
{
    public class NotifyWatchlistUserCommand : BaseCommand<Result<bool>>
    {
        public WatchlistNotificationInputDTO InputDTO { get; set; }
    }

    public class NotifyWatchlistUserCommandHandler : IRequestHandler<NotifyWatchlistUserCommand, Result<bool>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly CsvExporter _csvExporter;
        private readonly string _DATE_FORMAT = "dd/MM/yyyy";
        private readonly int _MAX_RECORDS_IN_EMAIL = 10; // Max records to show in email before attaching CSV

        public NotifyWatchlistUserCommandHandler(BusinessProfileService businessProfileService,
          INotificationService notificationService, IWebHostEnvironment environment,
          IConfiguration configuration, CsvExporter csvExporter)
        {
            _businessProfileService = businessProfileService;
            _notificationService = notificationService;
            _environment = environment;
            _configuration = configuration;
            _csvExporter = csvExporter;
        }

        public async Task<Result<bool>> Handle(NotifyWatchlistUserCommand request, CancellationToken cancellationToken)
        {
            var groupedChangeDTOs = GetGroupedChangeDTOs(request.InputDTO.ChangeDTOs);

            // Generate XML from DTO
            var xml = GenerateXML(groupedChangeDTOs, request.InputDTO.SinglePartnerName, request.InputDTO.IsSingleProfileScreening, _MAX_RECORDS_IN_EMAIL);

            // Generate email content from XML and XSLT template
            var generator = CreateContentGenerator();
            var templateName = GetNotificationTemplateName(request.InputDTO.IsSingleProfileScreening);
            var content = generator.GenerateContent(xml, templateName, Thread.CurrentThread.CurrentUICulture.Name);

            // Get recipient list
            var recipients = await _businessProfileService.GetRecipientEmail(RecipientType.TO.Id, NotificationTemplate.WatchlistReview.Id);
            var recipientInputDtos = recipients.Select(r => new RecipientsInputDTO
            {
                email = r.Email,
                name = r.Name
            }).ToList();

            // Generate attachments if records exceed threshold of 10, attachement is only required for Daily Profile Screening
            var attachements = await GenerateAttachmentsAsync(groupedChangeDTOs, request.InputDTO.IsSingleProfileScreening, _MAX_RECORDS_IN_EMAIL);

            var result = await _notificationService.SendNotification(
                recipients: recipientInputDtos,
                bcc: new List<RecipientsInputDTO>(),
                cc: new List<RecipientsInputDTO>(),
                attachments: attachements,
                subject: GetSubjectTitle(changeDTOs: request.InputDTO.ChangeDTOs, isSingleProfileScreening: request.InputDTO.IsSingleProfileScreening, partnerName: request.InputDTO.SinglePartnerName),
                body: content,
                notificationTypes: NotificationTypes.Email);

            return Result.Success(result.IsSuccess);
        }

        #region Private Helper Methods
        /// <summary>
        /// Get grouped ChangeDTOs by SolutionNames, CompanyName, FullName, NationalityFullName, DateOfBirth,
        /// aggregating other string properties with distinct values separated by comma.
        /// </summary>
        /// <returns></returns>
        private List<ChangeDTO> GetGroupedChangeDTOs(List<ChangeDTO> changeDTOs)
        {
            // Order by SolutionNames
            // 1. TB
            // 2. TC
            // 3. TB, TC
            var order = new List<string>()
            {
                Solution.Business.GetShortName(),
                Solution.Connect.GetShortName(),
                $"{Solution.Business.GetShortName()}, {Solution.Connect.GetShortName()}"
            };

            return changeDTOs.GroupBy(x =>
                    (
                        SolutionNames: (x.SolutionNames ?? "").Trim(),
                        CompanyName: (x.CompanyName ?? "").Trim(),
                        FullName: (x.FullName ?? "").Trim(),
                        NationalityFullName: (x.NationalityFullName ?? "").Trim(),
                        DateOfBirth: (x.DateOfBirth ?? "").Trim()
                    ))
                    .Select(g => new ChangeDTO
                    {
                        SolutionNames = g.Key.SolutionNames,
                        CompanyName = g.Key.CompanyName,
                        FullName = g.Key.FullName,
                        NationalityFullName = g.Key.NationalityFullName,
                        DateOfBirth = g.Key.DateOfBirth,

                        OwnershipStructureTypeNames = string.Join(", ",
                            g.Select(x => x.OwnershipStructureTypeNames)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(x => x.Trim())
                                .Distinct()
                                .OrderBy(x => x)),
                        EntityTypeName = string.Join(", ",
                            g.Select(x => x.EntityTypeName)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(x => x.Trim())
                                .Distinct()
                                .OrderBy(x => x)),
                        ScreeningDetailCategoryNames = string.Join(", ",
                            g.Select(x => x.ScreeningDetailCategoryNames)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(x => x.Trim())
                                .Distinct()
                                .OrderBy(x => x)),
                        EntityIds = string.Join(", ",
                            g.Select(x => x.EntityIds)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(x => x.Trim())
                                .Distinct()
                                .OrderBy(x => x))
                    })
                    .OrderBy(x =>
                    {
                        var index = order.IndexOf(x.SolutionNames);
                        return index >= 0 ? index : int.MaxValue;
                    })
                    .ThenBy(x => x.CompanyName)
                    .ThenBy(x => x.FullName)
                    .ThenBy(x => x.NationalityFullName)
                    .ThenBy(x => x.DateOfBirth)
                    .ToList();
        }

        private ContentGenerator CreateContentGenerator()
        {
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            var generator = new ContentGenerator(xsltTemplateRootPath);
            return generator;
        }

        private string GenerateXML(List<ChangeDTO> groupedChangeDTOs, string singlePartnerName, bool isSingleProfileScreening, int threshold)
        {
            var stringBuilder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(stringBuilder))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("ProfileScreening");

                writer.WriteElementString("CurrentYear", $"&copy; {DateTime.UtcNow.UTCToMalaysiaTime().Year}");
                writer.WriteElementString("KYCProfileApprovalUrl", _configuration.GetValue<string>("WatchlistNotification:KYCProfileApprovalUrl"));
                writer.WriteElementString("PersonnelWatchlistUrl", _configuration.GetValue<string>("WatchlistNotification:PersonnelWatchlistUrl"));
                writer.WriteElementString("IsSanctionDetected", groupedChangeDTOs.Any().ToString());
                writer.WriteElementString("PartnerName", singlePartnerName.ToEmptyPlaceholder(Placeholder.Dash)); // Only applicable for Single Profile Screening email template
                writer.WriteElementString("ScreeningDate", DateTime.UtcNow.UTCToMalaysiaTime().ToString(_DATE_FORMAT)); // Only applicable for Daily Profile Screening email template

                // For Single Profile Screening: show all records
                // For Daily Profile Screening: limit to threshold records in the email body
                var recordsToShow = isSingleProfileScreening ? groupedChangeDTOs : groupedChangeDTOs.Take(threshold);
                foreach (var groupedChangedDTO in recordsToShow)
                {
                    writer.WriteStartElement("BusinessProfile");
                    writer.WriteElementString("SolutionNames", groupedChangedDTO.SolutionNames.ToEmptyPlaceholder(Placeholder.Dash));
                    writer.WriteElementString("CompanyName", groupedChangedDTO.CompanyName.ToEmptyPlaceholder(Placeholder.Dash));
                    writer.WriteElementString("OwnershipStructureTypeNames", groupedChangedDTO.OwnershipStructureTypeNames.ToEmptyPlaceholder(Placeholder.Dash));
                    writer.WriteElementString("EntityTypeName", groupedChangedDTO.EntityTypeName.ToEmptyPlaceholder(Placeholder.Dash));
                    writer.WriteElementString("FullName", groupedChangedDTO.FullName.ToEmptyPlaceholder(Placeholder.Dash));
                    writer.WriteElementString("NationalityFullName", groupedChangedDTO.NationalityFullName.ToEmptyPlaceholder(Placeholder.Dash));
                    writer.WriteElementString("DateOfBirth", groupedChangedDTO.DateOfBirth.ToEmptyPlaceholder(Placeholder.Dash));
                    writer.WriteElementString("Type", groupedChangedDTO.ScreeningDetailCategoryNames.ToEmptyPlaceholder(Placeholder.Dash));
                    writer.WriteElementString("EntityIds", groupedChangedDTO.EntityIds.ToEmptyPlaceholder(Placeholder.Dash));
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            return stringBuilder.ToString();
        }

        private static string GetNotificationTemplateName(bool isSingleProfileScreening)
        {
            if (isSingleProfileScreening)
            {
                return "ProfileScreeningSingleProfile";
            }

            // Daily Profile Screening
            return "ProfileScreeningDailyScreening";
        }

        private string GetSubjectTitle(List<ChangeDTO> changeDTOs, bool isSingleProfileScreening, string partnerName)
        {
            string localDateString = DateTime.UtcNow.UTCToMalaysiaTime().ToString(_DATE_FORMAT);

            if (isSingleProfileScreening)
            {
                if (changeDTOs.Count > 0)
                {
                    return $"[T1] KYC Profile Screening Result of {partnerName} dated {localDateString}";
                }

                return $"[T1] KYC Profile Screening Result of {partnerName} dated {localDateString} - No Hits Detected";
            }

            if (changeDTOs.Count > 0)
            {
                return $"[T1] Daily KYC Profile Screening Result dated {localDateString}";
            }

            return $"[T1] Daily KYC Profile Screening Result dated {localDateString} - No Hits Detected";
        }

        /// <summary>
        /// Generate attachment if the changeDTOs exceed threshold, only applicable for Daily Profile Screening.
        /// </summary>
        private async Task<List<IFormFile>> GenerateAttachmentsAsync(List<ChangeDTO> changeDTOs, bool isSingleProfileScreening, int threshold = 10)
        {
            if (isSingleProfileScreening || changeDTOs.Count <= threshold)
            {
                return new List<IFormFile>();
            }

            var csvExportResult = await _csvExporter.ExportAsync<ChangeDTO>(
                    records: changeDTOs,
                    fileName: $"KYC Profile Daily Screening {DateTime.UtcNow.UTCToMalaysiaTime().ToString(_DATE_FORMAT)}.csv");

            var attachment = new FormFile(
                baseStream: csvExportResult.Stream,
                baseStreamOffset: 0,
                length: csvExportResult.Stream.Length,
                name: "KYCProfileDailyScreening",
                fileName: csvExportResult.FileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = csvExportResult.ContentType
            };

            return new List<IFormFile> { attachment };
        }
        #endregion Private Helper Methods
    }
}
