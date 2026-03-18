using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Verification;
using Tranglo1.Onboarding.Domain.Entities.Meta;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    [Permission(Permission.KYCManagementVerification.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementVerification.Action_View_Code })]
    internal class SaveCustomerVerificationCommand : BaseCommand<Result<CustomerVerificationOutputDTO>>
    {
        public CustomerVerificationInputDTO InputDTO;
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }
        public Guid? CustomerVerificationConcurrencyToken { get; set; }


        public override Task<string> GetAuditLogAsync(Result<CustomerVerificationOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Adding/Updating Verification for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }


        internal class SaveCustomerVerificationCommandHandler : IRequestHandler<SaveCustomerVerificationCommand, Result<CustomerVerificationOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly ILogger<SaveCustomerVerificationCommandHandler> _logger;
            private readonly TrangloUserManager _userManager;
            private readonly StorageManager _storageManager;
            private readonly IWebHostEnvironment _environment;
            private readonly IPartnerRepository _partnerRepository;
            private readonly IConfiguration _config;
            private readonly BusinessProfileService _businessProfileService;
            private readonly INotificationService _notificationService;
            private readonly PartnerService _partnerService;


            public SaveCustomerVerificationCommandHandler(IBusinessProfileRepository repository,
                ILogger<SaveCustomerVerificationCommandHandler> logger,
                TrangloUserManager userManager,
                StorageManager storageManager,
                IWebHostEnvironment environment,
                IPartnerRepository partnerRepository,
                IConfiguration config,
                BusinessProfileService businessProfileService,
                INotificationService notification,
                PartnerService partnerService
                )
            {
                _repository = repository;
                _logger = logger;
                _userManager = userManager;
                _storageManager = storageManager;
                _environment = environment;
                _partnerRepository = partnerRepository;
                _config = config;
                _businessProfileService = businessProfileService;
                _notificationService = notification;
                _partnerService = partnerService;
            }

            public async Task<Result<CustomerVerificationOutputDTO>> Handle(SaveCustomerVerificationCommand request, CancellationToken cancellationToken)
            {
                // Get Verification Per Business Profile 
                var businessProfile = await _repository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);
                var customerVerification = await _repository.GetCustomerVerificationbyBusinessProfileCodeAsync(businessProfile.Id);

                Result<CustomerVerificationOutputDTO> result = null; // Declare the result variable here

                // Handle Concurrency
                if (request.CustomerSolution == ClaimCode.Business || request.AdminSolution == Solution.Business.Id)
                {
                    var concurrencyCheck = ConcurrencyCheck(request.CustomerVerificationConcurrencyToken, customerVerification);
                    if (concurrencyCheck.IsFailure)
                    {
                        return Result.Failure<CustomerVerificationOutputDTO>(concurrencyCheck.Error);
                    }
                }                

                if (request.AdminSolution != null || request.CustomerSolution != null)
                {
                    if (ClaimCode.Connect == request.CustomerSolution)
                    {
                        return Result.Failure<CustomerVerificationOutputDTO>(
                            $"Connect Customer user is unable to update for {request.BusinessProfileCode}."
                        );
                    }
                    else if (ClaimCode.Business == request.CustomerSolution)
                    {
                        result = await UpdateCustomerVerification(request, businessProfile, customerVerification); // Call UpdateCustomerVerification with null customerVerification

                        if (result.IsFailure)
                        {
                            return Result.Failure<CustomerVerificationOutputDTO>(
                                $"Customer user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                            );
                        }
                    }
                    else if (Solution.Connect.Id == request.AdminSolution)
                    {
                        return Result.Failure<CustomerVerificationOutputDTO>(
                            $"Admin user is unable to update for Connect User with Business Profile: {request.BusinessProfileCode}."
                        );
                    }
                    else if (Solution.Business.Id == request.AdminSolution)
                    {
                        result = await UpdateCustomerVerification(request, businessProfile, customerVerification); // Call UpdateCustomerVerification with null customerVerification

                        if (result.IsFailure)
                        {
                            return Result.Failure<CustomerVerificationOutputDTO>(
                                $"Admin user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                            );
                        }
                    }
                    else
                    {
                        return Result.Failure<CustomerVerificationOutputDTO>(
                            $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                        );
                    }

                    return result;
                }

                return Result.Failure<CustomerVerificationOutputDTO>("Invalid request");
            }
            private async Task<Result<CustomerVerificationOutputDTO>> UpdateCustomerVerification(SaveCustomerVerificationCommand request, BusinessProfile businessProfile, CustomerVerification customerVerification)
            {
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
                var ekycVerificationStatus = await _repository.GetVerificationStatusByCodeAsync(request.InputDTO.EKYCVerificationStatusCode);
                var f2fVerificationStatus = await _repository.GetVerificationStatusByCodeAsync(request.InputDTO.F2FVerificationStatusCode);
                var verificationIDType = await _repository.GetVerificationIDByCodeAsync(request.InputDTO.VerificationIDTypeCode);

                if (customerVerification is null)
                {
                    if (verificationIDType is null)
                    {
                        return Result.Failure<CustomerVerificationOutputDTO>("Verification ID Type is required.");
                    }

                    CustomerVerification customer = new CustomerVerification(
                        businessProfile,
                        VerificationStatus.Pending,
                        VerificationStatus.Pending,
                        verificationIDType,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        Guid.NewGuid() //ConcurrencyToken
                    );

                    var newCustomer = await _repository.AddCustomerVerificationAsync(customer);

                    CustomerVerificationOutputDTO outputDTO = new CustomerVerificationOutputDTO
                    {
                        BusinessProfileCode = businessProfile.Id,
                        CustomerVerificationCode = newCustomer.Id,
                        EKYCVerificationStatusCode = newCustomer.EKYCVerificationStatus?.Id,
                        EKYCVerificationStatusDescription = newCustomer.EKYCVerificationStatus?.Name,
                        F2FVerificationStatusCode = newCustomer.F2FVerificationStatus?.Id,
                        F2FVerificationStatusDescription = newCustomer.F2FVerificationStatus?.Name,
                        VerificationIDTypeCode = newCustomer.VerificationIDType.Id,
                        VerificationIDTypeDescription = newCustomer.VerificationIDType.Name,
                        JustificationRemark = null
                    };

                    return Result.Success<CustomerVerificationOutputDTO>(outputDTO);
                }
                else
                {
                    if (customerVerification.VerificationIDType != verificationIDType)
                    {
                        var customerVerificationUpload = await _repository.GetCustomerVerificationDocumentUploadByVerificationCodeAsync(customerVerification.Id);

                        // Delete associated documents using the StorageManager
                        var documentIds = new List<Guid?>();

                        if (customerVerificationUpload != null)
                        {
                            documentIds.Add(customerVerificationUpload.RawDocumentID);
                            documentIds.Add(customerVerificationUpload.WatermarkDocumentID);
                        }

                        foreach (var documentId in documentIds.Where(id => id.HasValue))
                        {
                            // Perform the removal using the StorageManager
                            await _storageManager.RemoveAsync(documentId.Value);

                            // Verify the deletion by attempting to retrieve the document metadata
                            var deletedDocument = await _storageManager.GetDocumentMetadataAsync(documentId.Value);

                            if (deletedDocument != null)
                            {
                                // The document removal failed
                                return Result.Failure<CustomerVerificationOutputDTO>("Failed to delete document.");
                            }
                        }

                        // Delete current Documents from the repository
                        var deletedDocuments = await _repository.DeleteCustomerVerificationDocumentsByCustomerVerificationCodeAsync(customerVerification.Id);

                        // Update the VerificationIDType only if it needs to be changed
                        customerVerification.VerificationIDType = verificationIDType;
                    }
                    // Check if EKYCVerificationStatus is changed to Passed or Rejected
                    if (ekycVerificationStatus?.Id == VerificationStatus.Passed.Id || ekycVerificationStatus?.Id == VerificationStatus.Rejected.Id)
                    {
                        if (applicationUser is TrangloStaff)
                        {
                            // Update the EKYC verification status
                            customerVerification.EKYCVerificationStatus = ekycVerificationStatus;
                            customerVerification.JustificationRemark = request.InputDTO.JustificationRemarks;

                            // Save the updated customer verification
                            await _repository.UpdateCustomerVerificationAsync(customerVerification);

                            // Send notification email
                            await SendEmailNotification(businessProfile, customerVerification);
                        }
                        else
                        {
                            // Only TrangloStaff users can update the verification status
                            return Result.Failure<CustomerVerificationOutputDTO>("Unauthorized to update verification status.");
                        }
                    }
                    else if(applicationUser is TrangloStaff)
                    {
                        
                        
                            // Update the EKYC verification status without sending notification
                            customerVerification.EKYCVerificationStatus = ekycVerificationStatus;
                            customerVerification.JustificationRemark = request.InputDTO.JustificationRemarks;

                            // Save the updated customer verification
                            await _repository.UpdateCustomerVerificationAsync(customerVerification);
                    }

                    // Check if F2FVerificationStatus is changed to Passed or Rejected
                    if (f2fVerificationStatus?.Id == VerificationStatus.Passed.Id || f2fVerificationStatus?.Id == VerificationStatus.Rejected.Id)
                    {
                        if (applicationUser is TrangloStaff)
                        {
                            // Update the F2F verification status
                            customerVerification.F2FVerificationStatus = f2fVerificationStatus;
                            customerVerification.JustificationRemark = request.InputDTO.JustificationRemarks;

                            // Save the updated customer verification
                            await _repository.UpdateCustomerVerificationAsync(customerVerification);

                            // Send notification email
                            await SendEmailNotification(businessProfile, customerVerification);
                        }
                        else
                        {
                            // Only TrangloStaff users can update the verification status
                            return Result.Failure<CustomerVerificationOutputDTO>("Unauthorized to update verification status.");
                        }
                    }
                    else if (applicationUser is TrangloStaff)
                    {
                       
                        
                            // Update the F2F verification status without sending notification
                            customerVerification.F2FVerificationStatus = f2fVerificationStatus;
                            customerVerification.JustificationRemark = request.InputDTO.JustificationRemarks;

                            // Save the updated customer verification
                            await _repository.UpdateCustomerVerificationAsync(customerVerification);

                    }

                    // Save the updated customer verification
                    await _repository.UpdateCustomerVerificationAsync(customerVerification);

                    CustomerVerificationOutputDTO outputDTO = new CustomerVerificationOutputDTO
                    {
                        BusinessProfileCode = businessProfile.Id,
                        CustomerVerificationCode = customerVerification.Id,
                        EKYCVerificationStatusCode = customerVerification.EKYCVerificationStatus?.Id,
                        EKYCVerificationStatusDescription = customerVerification.EKYCVerificationStatus?.Name,
                        F2FVerificationStatusCode = customerVerification.F2FVerificationStatus?.Id,
                        F2FVerificationStatusDescription = customerVerification.F2FVerificationStatus?.Name,
                        VerificationIDTypeCode = customerVerification.VerificationIDType?.Id,
                        VerificationIDTypeDescription = customerVerification.VerificationIDType?.Name,
                        JustificationRemark = customerVerification.JustificationRemark
                    };

                    return Result.Success<CustomerVerificationOutputDTO>(outputDTO);
                }


            }

            private async Task SendEmailNotification(BusinessProfile businessProfile, CustomerVerification customerVerification)
            {
                var partner = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);
                var partnerSubscriptions = await _partnerRepository.GetPartnerSubscriptionByPartnerCodeAsync(partner.Id);
                var solution = await _partnerRepository.GetSolutionAsync(partnerSubscriptions.Solution.Id);
                var ekycVerificationStatus = await _repository.GetVerificationStatusByCodeAsync(customerVerification.EKYCVerificationStatus?.Id);
                var f2fVerificationStatus = await _repository.GetVerificationStatusByCodeAsync(customerVerification.F2FVerificationStatus?.Id);

                // 1. Get the email template
                var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
                string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                var generator = new Common.ContentGenerator(xsltTemplateRootPath);

                // 2. Use XSL to inject the properties from the event
                StringBuilder _xml = new StringBuilder();
                using (XmlWriter writer = XmlWriter.Create(_xml))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("CustomerEKYCVerificationStatus");
                    writer.WriteElementString("LoginUrl", _config.GetValue<string>("BusinessPortalUri"));
                    writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                    writer.WriteElementString("SolutionName", solution.Name.ToString());
                   /* if (ekycVerificationStatus != null)
                    {
                        writer.WriteElementString("VerificationStatus", ekycVerificationStatus.Name.ToString());
                    }*/
                    if (f2fVerificationStatus != null)
                    {
                        writer.WriteElementString("VerificationStatus", f2fVerificationStatus.Name.ToString());
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                string content = generator.GenerateContent(_xml.ToString(), "CustomerEKYCVerificationStatusTemplate", cultureName);

                var recipients = new List<RecipientsInputDTO>();
                var cc = new List<RecipientsInputDTO>();
                var bcc = new List<RecipientsInputDTO>();

                long notificationTemplate = 29;
               // long recipientTypeCode = 1;
                long ccTypeCode = 2;
                long bccTypeCode = 3;
               // var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, notificationTemplate);
                var ccInfo = await _businessProfileService.GetRecipientEmail(ccTypeCode, notificationTemplate);
                var bccInfo = await _businessProfileService.GetRecipientEmail(bccTypeCode, notificationTemplate);

                var recipientlist = new RecipientsInputDTO()
                {
                    email = partner.Email.Value,
                    name = businessProfile.CompanyName
                };
                recipients.Add(recipientlist);


                foreach (var emailist in ccInfo)
                {
                    var cclist = new RecipientsInputDTO()
                    {
                        email = emailist.Email,
                        name = emailist.Name
                    };
                    cc.Add(cclist);
                }

                foreach (var emailist in bccInfo)
                {
                    var bcclist = new RecipientsInputDTO()
                    {
                        email = emailist.Email,
                        name = emailist.Name
                    };
                    bcc.Add(bcclist);
                }

                var currentDate = DateTime.Today;
                var emailSubject = $"Tranglo Business KYC Verification {currentDate:MMMM yyyy} - {businessProfile.CompanyName} x {partnerSubscriptions.TrangloEntity}";
                var file = new List<IFormFile>() { };

                Result<HttpStatusCode> sendEmailResponse = await _notificationService.SendNotification(
                    recipients,
                    bcc,
                    cc,                 
                    file,
                    emailSubject,
                    content,
                    NotificationTypes.Email
                );

                if (sendEmailResponse.IsFailure)
                {
                    _logger.LogError($"[SaveCustomerVerificationCommand] Notification failed for {businessProfile.CompanyName}. {sendEmailResponse.Error}.");
                }
            }

            private Result ConcurrencyCheck(Guid? concurrencyToken, CustomerVerification customerVerification)
            {
                try
                {
                    if ((concurrencyToken.HasValue && customerVerification?.CustomerVerificationConcurrencyToken != concurrencyToken) ||
                        concurrencyToken is null && customerVerification?.CustomerVerificationConcurrencyToken != null)
                    {
                        // Return a 409 Conflict status code when there's a concurrency issue
                        return Result.Failure("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                    }

                    if (customerVerification != null && customerVerification.Id > 0)
                    {
                        customerVerification.CustomerVerificationConcurrencyToken = Guid.NewGuid(); // Update token
                    }

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while processing the request.");

                    // Return a 409 Conflict status code
                    return Result.Failure("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }
            }
        }
    }
}


  

