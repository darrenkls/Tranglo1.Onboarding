using AutoMapper;
using CSharpFunctionalExtensions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MediatR;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetPartnerListingExportQuery : BaseQuery<Result<FileOutputDTO>>
    {
        public string Entity { get; set; }
        public string UserBearerToken { get; set; }
        public GetPartnerListExportInputDTO InputDTO { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public class GetPartnerListingExportQueryHandler : IRequestHandler<GetPartnerListingExportQuery, Result<FileOutputDTO>>
        {
            private readonly IMediator _mediator;
            private readonly IMapper _mapper;
            private readonly IIdentityContext _identityContext;
            private readonly IApplicationUserRepository _applicationUserRepository;


            public GetPartnerListingExportQueryHandler(IMediator mediator,
                IMapper mapper,
                IIdentityContext identityContext,
                IApplicationUserRepository applicationUserRepository)
            {
                _mediator = mediator;
                _mapper = mapper;
                _identityContext = identityContext;
                _applicationUserRepository = applicationUserRepository;
            }

            public async Task<Result<FileOutputDTO>> Handle(GetPartnerListingExportQuery request, CancellationToken cancellationToken)
            {
                var languageCode = CultureInfo.CurrentUICulture.Name;
                var subjectId = _identityContext.CurrentUser.GetSubjectId();
                var applicationUser = await _applicationUserRepository.GetApplicationUserByLoginId(subjectId);

                request.InputDTO.Column ??= new GetPartnerListExportColumnInputDTO();

                var stream = new MemoryStream();

                using (var spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    // Add a WorkbookPart to the document.
                    WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();
                    WorkbookStylesPart workStylePart = workbookpart.AddNewPart<WorkbookStylesPart>();

                    WorkbookStylesPart wbsp = CreateStylesheet(workbookpart.WorkbookStylesPart);

                    // Add a WorksheetPart to the WorkbookPart.
                    WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Add Sheets to the Workbook.
                    Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                    // Append a new worksheet and associate it with the workbook.
                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Partner List",
                    };
                    sheets.Append(sheet);

                    var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    Row headerRow = new Row();

                    #region Header columns
                    if (request.InputDTO.Column.PartnerName == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Partner Name") });
                    }

                    if (request.InputDTO.Column.TradeName == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Trade Name") });
                    }

                    if (request.InputDTO.Column.Country == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Country/Nationality") });
                    }

                    if (request.InputDTO.Column.RegistrationDate == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Registration Date") });
                    }

                    if (request.InputDTO.Column.Agent == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Agent") });
                    }

                    if (request.InputDTO.Column.AgreementStatus == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Agreement Status") });
                    }

                    if (request.InputDTO.Column.AgreementStartDate == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Agreement Start Date") });
                    }

                    if (request.InputDTO.Column.AgreementEndDate == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Agreement End Date") });
                    }

                    if (request.InputDTO.Column.Solution == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Solution") });
                    }

                    if (request.InputDTO.Column.Entity == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Entity") });
                    }

                    if (request.InputDTO.Column.PartnerType == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Partner Type") });
                    }

                    if (request.InputDTO.Column.Environment == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Environment") });
                    }

                    if (request.InputDTO.Column.WorkflowStatus == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Workflow Status") });
                    }

                    if (request.InputDTO.Column.KycStatus == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("KYC Status") });
                    }

                    if (request.InputDTO.Column.ReminderStatus == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Reminder Status") });
                    }

                    if (request.InputDTO.Column.PartnerKycApprovalStatus == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Partner KYC Approval Status") });
                    }

                    if (request.InputDTO.Column.LeadsOrigin == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Origin") });
                    }

                    if (request.InputDTO.Column.Status == true)
                    {
                        headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue("Status") });
                    }

                    sheetData.Append(headerRow);
                    #endregion

                    var query = _mapper.Map<GetPartnerListingQuery>(request.InputDTO.Search);
                    query.Entity = request.Entity;
                    query.UserBearerToken = request.UserBearerToken;
                    query.AdminSolution = request.AdminSolution;
                    query.CustomerSolution = request.CustomerSolution;

                    var pagedResult = await _mediator.Send(query);

                    foreach (var dataRow in pagedResult.Results)
                    {
                        var row = new Row();

                        #region Cell columns
                        if (request.InputDTO.Column.PartnerName == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetStringOrDefault(dataRow.PartnerName)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.TradeName == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetStringOrDefault(dataRow.TradeName)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.Country == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dataRow.Country),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.RegistrationDate == true)
                        {
                            DateTime? registrationDate = dataRow.RegistrationDate;
                            if (registrationDate.HasValue && !String.IsNullOrEmpty(applicationUser.Timezone))
                            {
                                Result<DateTime> registrationDateResult = TimezoneConversion.ConvertFromUTCWithTimezoneName(applicationUser.Timezone, languageCode, registrationDate.Value, true);
                                if (registrationDateResult.IsSuccess)
                                    registrationDate = registrationDateResult.Value;
                            }

                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetStringOrDefault(registrationDate?.ToString("dd/MM/yyyy"))),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.Agent == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetStringOrDefault(dataRow.Agent)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.AgreementStatus == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetAgreementStatus(dataRow.AgreementStatus, request.AdminSolution)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.AgreementStartDate == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetAgreementStartDate(dataRow.AgreementStartDate, request.AdminSolution)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.AgreementEndDate == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetAgreementEndDate(dataRow.AgreementEndDate, request.AdminSolution)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.Solution == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetStringOrDefault(dataRow.Subscriptions.FirstOrDefault()?.Solution)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.Entity == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetStringOrDefault(dataRow.Subscriptions.FirstOrDefault()?.Entity)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.PartnerType == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetStringOrDefault(dataRow.Subscriptions.FirstOrDefault()?.PartnerType)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.Environment == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetStringOrDefault(dataRow.Subscriptions.FirstOrDefault()?.EnvironmentDescription)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.WorkflowStatus == true)
                        {
                            string workflowStatus = dataRow.Subscriptions
                                .FirstOrDefault()?
                                .WorkFlowStatus;
                            workflowStatus = GetWorkflowStatusHTML(workflowStatus, request.AdminSolution);

                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(workflowStatus?
                                    .Replace("<br>", Environment.NewLine)),
                                StyleIndex = Convert.ToUInt32(1)
                            });
                        }

                        if (request.InputDTO.Column.KycStatus == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dataRow.KYCStatusCodeDescription),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.ReminderStatus == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetStringOrDefault(dataRow.KYCReminderStatus)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.PartnerKycApprovalStatus == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dataRow.ApprovalStatus),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.LeadsOrigin == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(GetStringOrDefault(dataRow.FullLeadsOriginDescription)),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }

                        if (request.InputDTO.Column.Status == true)
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(dataRow.Subscriptions.FirstOrDefault()?.Status),
                                StyleIndex = Convert.ToUInt32(2)
                            });
                        }
                        #endregion

                        sheetData.Append(row);
                    }

                    workbookpart.Workbook.Save();
                }

                stream.Position = 0;

                string excelName = $"PartnerList_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";

                return Result.Success(new FileOutputDTO
                {
                    File = stream,
                    ContentType = "application/octet-stream",
                    FileName = excelName
                });
            }

            private WorkbookStylesPart CreateStylesheet(WorkbookStylesPart spreadsheet)
            {
                WorkbookStylesPart stylesheet = spreadsheet;
                Stylesheet workbookstylesheet = new Stylesheet();

                // Fonts
                Font defaultFont = new Font();            // Default font
                Fonts fonts = new Fonts();
                fonts.Append(defaultFont);

                // Fills
                Fill defaultFill = new Fill();            // Default fill
                Fill textwrapFill = new Fill();
                PatternFill patternFill = new PatternFill();
                patternFill.PatternType = PatternValues.None;
                textwrapFill.PatternFill = patternFill;
                Fills fills = new Fills();
                fills.Append(textwrapFill);

                // Borders
                Border defaultBorder = new Border();        // Default border
                Borders borders = new Borders();
                borders.Append(defaultBorder);

                // CellStyleFormats
                CellStyleFormats cellStyleFormats = new CellStyleFormats();
                cellStyleFormats.Append(new CellFormat());

                // CellFormats
                CellFormat defaultCellFormat = new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 };       // Default style
                CellFormat wrapTextCellFormat = new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top });
                CellFormat topAlignCellFormat = new CellFormat(new Alignment() { Vertical = VerticalAlignmentValues.Top });
                CellFormats cellformats = new CellFormats();
                cellformats.Append(defaultCellFormat);
                cellformats.Append(wrapTextCellFormat);
                cellformats.Append(topAlignCellFormat);

                // Append fonts, fills, borders & CellFormats to stylesheet
                workbookstylesheet.Append(fonts);
                workbookstylesheet.Append(fills);
                workbookstylesheet.Append(borders);
                workbookstylesheet.Append(cellStyleFormats);
                workbookstylesheet.Append(cellformats);

                // Finalize
                stylesheet.Stylesheet = workbookstylesheet;
                stylesheet.Stylesheet.Save();

                return stylesheet;
            }

            private string GetStringOrDefault(string value, string @default = "-")
            {
                if (String.IsNullOrEmpty(value))
                    return @default;

                return value;
            }

            private string GetAgreementStatus(string agreementStatus, long? adminSolution)
            {
                if (adminSolution.HasValue && adminSolution.Value != Domain.Entities.Solution.Business.Id)
                {
                    return GetStringOrDefault(agreementStatus);
                }

                if (adminSolution.HasValue && adminSolution.Value == Domain.Entities.Solution.Business.Id)
                {
                    return "N/A";
                }

                return GetStringOrDefault(agreementStatus);
            }

            private string GetAgreementStartDate(DateTime? agreementStartDate, long? adminSolution, string @default = "-")
            {
                if (adminSolution.HasValue && adminSolution.Value != Domain.Entities.Solution.Business.Id)
                {
                    return GetStringOrDefault(agreementStartDate?.ToString("dd/MM/yyyy"), @default);
                }

                if (adminSolution.HasValue && adminSolution.Value == Domain.Entities.Solution.Business.Id)
                {
                    return "N/A";
                }

                return GetStringOrDefault(agreementStartDate?.ToString("dd/MM/yyyy"), @default);
            }

            private string GetAgreementEndDate(DateTime? agreementEndDate, long? adminSolution, string @default = "-")
            {
                if (adminSolution.HasValue && adminSolution.Value != Domain.Entities.Solution.Business.Id)
                {
                    return GetStringOrDefault(agreementEndDate?.ToString("dd/MM/yyyy"), @default);
                }

                if (adminSolution.HasValue && adminSolution.Value == Domain.Entities.Solution.Business.Id)
                {
                    return "N/A";
                }

                return GetStringOrDefault(agreementEndDate?.ToString("dd/MM/yyyy"), @default);
            }

            private string GetWorkflowStatusHTML(string workflowStatus, long? adminSolution)
            {
                if (String.IsNullOrEmpty(workflowStatus))
                    return workflowStatus;

                if (adminSolution.HasValue && adminSolution.Value == Domain.Entities.Solution.Business.Id)
                {
                    return String.Join(",",
                        workflowStatus.Split(',')
                            .Where(x => !x.Contains("Agreement") && !x.Contains("API"))
                    );
                }

                return workflowStatus;
            }
        }
    }
}
