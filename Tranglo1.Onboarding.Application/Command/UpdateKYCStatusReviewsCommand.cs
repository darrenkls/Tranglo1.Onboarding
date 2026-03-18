using AutoMapper;
using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tranglo1.ApprovalWorkflowEngine;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Entities.Requisition;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCManagement, UACAction.Approve)]
    [Permission(Permission.KYCManagement.Action_UpdateOrSubmitForApproval_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagement.Action_View_Code })]
    internal class UpdateKYCStatusReviewsCommand : BaseCommand<Result<KYCStatusReviewsOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public KYCStatusReviewsInputDTO InputDTO { set; get; }
        public int AdminSolution { get; set; }
        public Guid? ReviewAndFeedbackConcurrencyToken { get; set; }
        public string TrangloEntity { get; set; }



        public override Task<string> GetAuditLogAsync(Result<KYCStatusReviewsOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Submitted KYC case for approval";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class UpdateKYCStatusReviewsCommandHandler : IRequestHandler<UpdateKYCStatusReviewsCommand, Result<KYCStatusReviewsOutputDTO>>
        {
            private readonly IMapper _mapper;
            private readonly IApplicationUserRepository _applicationUserRepository;
            private readonly BusinessProfileService _businessProfileService;
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly IPartnerRepository _partnerRepository;
            private readonly ILogger<KYCStatusReviewsOutputDTO> _logger;
            private readonly ApprovalManager<PartnerKYCStatusRequisition> _approvalManager;
            private readonly IWebHostEnvironment _environment;
            private readonly INotificationService _notificationService;
            private readonly IConfiguration _config;
            private readonly TrangloUserManager _userManager;
            private readonly PartnerService _partnerService;

            public UpdateKYCStatusReviewsCommandHandler(
                IMapper mapper, 
                IApplicationUserRepository applicationUserRepository, 
                BusinessProfileService businessProfileService, 
                ILogger<KYCStatusReviewsOutputDTO> logger, 
                IBusinessProfileRepository businessProfileRepository,
                IPartnerRepository partnerRepository,
                ApprovalManager<PartnerKYCStatusRequisition> approvalManager,
                IWebHostEnvironment environment,
                INotificationService notification,
                IConfiguration configuration,
                TrangloUserManager trangloUserManager,
                PartnerService partnerService
                )
            {
                _mapper = mapper;
                _applicationUserRepository = applicationUserRepository;
                _businessProfileService = businessProfileService;
                _logger = logger;
                _businessProfileRepository = businessProfileRepository;
                _partnerRepository = partnerRepository;
                _approvalManager = approvalManager;
                _environment = environment;
                _notificationService = notification;
                _config = configuration;
                _userManager = trangloUserManager;
                _partnerService = partnerService;
            }

            //merge CreateKYCRequisitionApprovalCommandHandler into this cuz new backlog wan do some checking before we update the status
            public async Task<Result<KYCStatusReviewsOutputDTO>> Handle(UpdateKYCStatusReviewsCommand request, CancellationToken cancellationToken)
            {
                var prefix = "CO";
                var businessProfile = _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode).Result.Value;
                var partnerInfo = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(request.BusinessProfileCode);
                var partnerSubInfo = await _partnerRepository.GetPartnerSubscriptionByPartnerCodeAsync(partnerInfo.Id);
                var result = new KYCStatusReviewsOutputDTO
                {
                    IsKYCStatusUpdated = false,
                    IsRequisitionCreated = false,
                    RequisitionCode = "",
                    KYCSubModuleReview = new List<KYCStatusReviewsOutputDTO.KYCSubModuleOutputDTO>()
                };


                #region KYCSubModuleReview
                var kycSubModuleReview = request.InputDTO.kycSubModuleReview;
                Solution solution = request.AdminSolution == Solution.Connect.Id ? Solution.Connect : Solution.Business;
                var isAMLDocumentationUploaded = await _businessProfileService.CheckHasUploadedAMLDocumentation(request.BusinessProfileCode, solution);
                //if (partnerInfo.CustomerType != CustomerType.Individual)
                if (solution == Solution.Connect) //Bypass to only check for Connect
                {
                    if (kycSubModuleReview.Count() < ((isAMLDocumentationUploaded) ? 6 : 7))
                    {
                        return Result.Failure<KYCStatusReviewsOutputDTO>(
                                    $"Parameters not complete."
                                );
                    }
                    for (int i = 0; i < kycSubModuleReview.Count(); i++)
                    {
                        if (isAMLDocumentationUploaded && i == KYCCategory.Connect_AMLOrCFT.Id) continue;
                        var item = kycSubModuleReview.ElementAt(i);
                        if (item == null)
                        {
                            return Result.Failure<KYCStatusReviewsOutputDTO>(
                                    $"Parameters not complete."
                                );
                        }
                    }
                }

                if (request.InputDTO.KYCStatusCode == KYCStatus.Verified.Id)
                {
                    if (request.AdminSolution == Solution.Connect.Id)
                    {
                        var _isMandatoryFieldCompleted = await _businessProfileService.IsMandatoryFieldCompletedAsync(request.BusinessProfileCode, Solution.Connect);
                        if (_isMandatoryFieldCompleted != null)
                        {
                            var errorBMessage = "";
                            var errorLMessage = "";
                            var errorCMessage = "";
                            var errorDMessage = "";
                            var errorAMessage = "";
                            var errorEMessage = "";

                            if (_isMandatoryFieldCompleted.isBusinessProfileCompleted == false)
                            {
                                errorBMessage = "Business Profile, ";
                            }
                            if (_isMandatoryFieldCompleted.isLicenseInfoCompleted == false)
                            {
                                errorLMessage = "License Info, ";
                            }
                            if (_isMandatoryFieldCompleted.isCoInfoCompleted == false)
                            {
                                errorCMessage = "CO Info, ";
                            }
                            if (_isMandatoryFieldCompleted.isDocumentationCompleted == false)
                            {
                                errorDMessage = "Documentation, ";
                            }
                            if (_isMandatoryFieldCompleted.isAMLCompleted == false)
                            {
                                errorAMessage = "AML, ";
                            }
                            if (_isMandatoryFieldCompleted.isDeclarationInfoCompleted == false)
                            {
                                errorEMessage = "Declaration ";
                            }
                            if (_isMandatoryFieldCompleted.isBusinessProfileCompleted == false ||
                                _isMandatoryFieldCompleted.isLicenseInfoCompleted == false ||
                                _isMandatoryFieldCompleted.isCoInfoCompleted == false ||
                                _isMandatoryFieldCompleted.isDocumentationCompleted == false ||
                                _isMandatoryFieldCompleted.isAMLCompleted == false ||
                                _isMandatoryFieldCompleted.isDeclarationInfoCompleted == false)
                            {
                                return Result.Failure<KYCStatusReviewsOutputDTO>(
                                        $"Mandatory {errorBMessage + errorLMessage + errorCMessage + errorDMessage + errorAMessage + errorEMessage} field is not completed."
                                    );
                            }


                        }
                        else
                        {
                            return Result.Failure<KYCStatusReviewsOutputDTO>(
                                        $"Mandatory field for {request.BusinessProfileCode} is not completed."
                                    );
                        }
                        var isInvalidReviewResult = false;
                        foreach (var item in kycSubModuleReview)
                        {
                            if (isAMLDocumentationUploaded && item.KYCCategoryCode == KYCCategory.Connect_AMLOrCFT.Id) continue;

                            var _kycCategory = KYCCategory.FindById<KYCCategory>(item.KYCCategoryCode);
                            var _reviewResult = ReviewResult.FindById<ReviewResult>(item.ReviewResultCode);
                            if (_reviewResult == ReviewResult.Complete)
                            {
                                if ((_kycCategory == KYCCategory.Connect_BusinessProfile &&
                                    !_isMandatoryFieldCompleted.isBusinessProfileCompleted) ||
                                    (_kycCategory == KYCCategory.Connect_LicenseInfo &&
                                    !_isMandatoryFieldCompleted.isLicenseInfoCompleted) ||
                                    (_kycCategory == KYCCategory.Connect_Ownership &&
                                    !_isMandatoryFieldCompleted.isOwnershipCompleted) ||
                                    (_kycCategory == KYCCategory.Connect_Documentation &&
                                    !_isMandatoryFieldCompleted.isDocumentationCompleted) ||
                                    (_kycCategory == KYCCategory.Connect_AMLOrCFT &&
                                    !_isMandatoryFieldCompleted.isAMLCompleted) ||
                                    (_kycCategory == KYCCategory.Connect_ComplianceInfo &&
                                    !_isMandatoryFieldCompleted.isCoInfoCompleted) ||
                                    (_kycCategory == KYCCategory.Connect_Declaration &&
                                    !_isMandatoryFieldCompleted.isDeclarationInfoCompleted) && item.ReviewResultCode == ReviewResult.Complete.Id)
                                {
                                    isInvalidReviewResult = true;
                                    break;
                                }
                            }
                        }
                        if (isInvalidReviewResult)
                        {
                            return Result.Failure<KYCStatusReviewsOutputDTO>(
                                        $"Mandatory field is not completed."
                                    );
                        }
                    }
                    else if (request.AdminSolution == Solution.Business.Id)
                    {
                        var _isBusinessMandatoryFieldCompleted = await _businessProfileService.IsBusinessCustomerMandatoryFieldCompletedAsync(request.BusinessProfileCode);
                        if (_isBusinessMandatoryFieldCompleted != null)
                        {
                            var errorBDMessage = "";
                            var errorBMessage = "";
                            var errorOMessage = "";
                            var errorDMessage = "";

                            if (_isBusinessMandatoryFieldCompleted.IsBusinessDeclarationCompleted == false)
                            {
                                errorBDMessage = "Business Declaration, ";
                            }
                            if (_isBusinessMandatoryFieldCompleted.IsBusinessProfileCompleted == false)
                            {
                                errorBMessage = "Business Profile, ";
                            }

                            if (_isBusinessMandatoryFieldCompleted.IsDeclarationInfoCompleted == false)
                            {
                                errorDMessage = "Declaration";
                            }

                            if (partnerInfo.CustomerType != CustomerType.Individual)
                            {
                                if (_isBusinessMandatoryFieldCompleted.IsOwnershipCompleted == false)
                                {
                                    errorOMessage = "Ownership";
                                }

                                if (_isBusinessMandatoryFieldCompleted.IsBusinessDeclarationCompleted == false ||
                                _isBusinessMandatoryFieldCompleted.IsBusinessProfileCompleted == false ||
                                _isBusinessMandatoryFieldCompleted.IsOwnershipCompleted == false ||
                                _isBusinessMandatoryFieldCompleted.IsDeclarationInfoCompleted == false)
                                {
                                    return Result.Failure<KYCStatusReviewsOutputDTO>(
                                           $"Mandatory {errorBDMessage + errorBMessage + errorOMessage + errorDMessage} field  is not completed."
                                       );
                                }
                            }
                            else
                            {
                                if (_isBusinessMandatoryFieldCompleted.IsBusinessDeclarationCompleted == false ||
                                _isBusinessMandatoryFieldCompleted.IsBusinessProfileCompleted == false ||
                                _isBusinessMandatoryFieldCompleted.IsDeclarationInfoCompleted == false)
                                {
                                    return Result.Failure<KYCStatusReviewsOutputDTO>(
                                           $"Mandatory {errorBDMessage + errorBMessage + errorDMessage} field  is not completed."
                                       );
                                }
                            }



                        }
                        else
                        {
                            return Result.Failure<KYCStatusReviewsOutputDTO>(
                                       $"Mandatory field is not completed."
                                   );
                        }

                        var isBusinessInvalidReviewResult = false;
                        foreach (var item in kycSubModuleReview)
                        {
                            if (isAMLDocumentationUploaded && item.KYCCategoryCode == KYCCategory.Connect_AMLOrCFT.Id) continue;

                            var _kycCategory = KYCCategory.FindById<KYCCategory>(item.KYCCategoryCode);
                            var _reviewResult = ReviewResult.FindById<ReviewResult>(item.ReviewResultCode);
                            if (_reviewResult == ReviewResult.Complete)
                            {
                                if ((_kycCategory == KYCCategory.Business_BusinessDeclaration &&
                                    !_isBusinessMandatoryFieldCompleted.IsBusinessDeclarationCompleted) ||
                                    (_kycCategory == KYCCategory.Business_BusinessProfile &&
                                    !_isBusinessMandatoryFieldCompleted.IsBusinessProfileCompleted) ||
                                    (_kycCategory == KYCCategory.Business_Ownership &&
                                    !_isBusinessMandatoryFieldCompleted.IsOwnershipCompleted) ||
                                    (_kycCategory == KYCCategory.Business_Declaration &&
                                    !_isBusinessMandatoryFieldCompleted.IsDeclarationInfoCompleted) && item.ReviewResultCode == ReviewResult.Complete.Id)
                                {
                                    isBusinessInvalidReviewResult = true;
                                    break;
                                }
                            }
                        }
                        if (isBusinessInvalidReviewResult)
                        {
                            return Result.Failure<KYCStatusReviewsOutputDTO>(
                                        $"Mandatory field  is not completed."
                                    );
                        }
                    }

                }
                var countOfInsufficientReview = request.InputDTO.kycSubModuleReview
                                        .Count(r => r.ReviewResultCode == ReviewResult.Insufficient_Incomplete.Id);

                // If review result has insufficient, not allow to set Kyc status to Verify/Pending Higher Approval
                if (countOfInsufficientReview > 0 && (request.InputDTO.KYCStatusCode == KYCStatus.Verified.Id ||
                  request.InputDTO.KYCStatusCode == KYCStatus.Pending_Higher_Approval.Id))
                {
                    return Result.Failure<KYCStatusReviewsOutputDTO>(
                                $"This KYC status is only allowed when all Review Result is Complete."
                            );
                }
                // If review result has no insufficient, not allow to set Kyc status to Insufficient
                else if (countOfInsufficientReview == 0 &&
                  (request.InputDTO.KYCStatusCode == KYCStatus.Insufficient_Incomplete.Id))
                {
                    return Result.Failure<KYCStatusReviewsOutputDTO>(
                                $"This KYC status is only allowed when has at least 1 Insufficient / Incomplete."
                            );
                }


                try
                {
                    Guid? concurrencyToken = request.ReviewAndFeedbackConcurrencyToken;

                    if ((concurrencyToken.HasValue && businessProfile.ReviewAndFeedbackConcurrencyToken != concurrencyToken) ||
                  concurrencyToken is null && businessProfile.ReviewAndFeedbackConcurrencyToken != null)
                    {
                        // Return a 409 Conflict status code when there's a concurrency issue
                        return Result.Failure<KYCStatusReviewsOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                    }
                    if (businessProfile.ReviewAndFeedbackConcurrencyToken == null && businessProfile.ReviewAndFeedbackConcurrencyToken == null)
                    {
                        // Handle the scenario of fresh data here
                        businessProfile.ReviewAndFeedbackConcurrentLastModified = DateTime.UtcNow;
                        businessProfile.ReviewAndFeedbackConcurrencyToken = Guid.NewGuid();

                        // Update the data asynchronously
                        await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);
                    }
                    else
                    {
                        // Handle the scenario where ConcurrencyToken is provided
                        businessProfile.ReviewAndFeedbackConcurrentLastModified = DateTime.UtcNow;
                        businessProfile.ReviewAndFeedbackConcurrencyToken = Guid.NewGuid();

                        // Update the data asynchronously
                        await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);
                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while processing the request.");

                    // Return a 409 Conflict status code with an appropriate error message
                    return Result.Failure<KYCStatusReviewsOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }


                List<KYCSubModuleReview> _KYCSubModuleReviewList = new List<KYCSubModuleReview>();
                foreach (KYCSubModuleReviewInputDTO category in kycSubModuleReview)
                {
                    KYCCategory _kycCategory = KYCCategory.FindById<KYCCategory>(category.KYCCategoryCode);
                    ReviewResult _reviewResult = ReviewResult.FindById<ReviewResult>(category.ReviewResultCode);

                    var _KYCSubModuleReview = await _businessProfileRepository.GetKYCSubModuleReviewByBusinessProfileCategoryNoTracking(businessProfile, _kycCategory);
                    _KYCSubModuleReview.ReviewResult = _reviewResult;
                    _KYCSubModuleReview.UpdateLastReviewDate();
                    _KYCSubModuleReview.KYCCategoryCode = category.KYCCategoryCode;

                    _KYCSubModuleReviewList.Add(_KYCSubModuleReview);
                }




                var isSuccessUpdateList = await _businessProfileService.SaveKYCSubModuleReviewList(businessProfile, _KYCSubModuleReviewList);

                if (!isSuccessUpdateList)
                {
                    return Result.Failure<KYCStatusReviewsOutputDTO>(
                               "Update KYC SubModule Review failed."
                           );
                }
                var output = await _businessProfileRepository.GetKYCSubModuleReviewByBusinessProfile(businessProfile);
                foreach (var item in output)
                {
                    if (isAMLDocumentationUploaded && item.KYCCategoryCode == KYCCategory.Connect_AMLOrCFT.Id) continue;

                    var kycSubModuleOutput = new KYCStatusReviewsOutputDTO.KYCSubModuleOutputDTO
                    {
                        KYCCategoryCode = item.KYCCategoryCode,
                        KYCCategoryDescription = KYCCategory.FindById<KYCCategory>(item.KYCCategoryCode).Name,
                        UserUpdatedDate = item.UserUpdateDate,
                        LastReviewedDate = item.LastReviewedDate,
                        ReviewResultCode = item.ReviewResult.Id,
                        ReviewResultDescription = item.ReviewResult.Name
                    };

                    // Populate concurrency properties in the result
                    businessProfile.ReviewAndFeedbackConcurrencyToken = businessProfile.ReviewAndFeedbackConcurrencyToken;
                    businessProfile.ReviewAndFeedbackConcurrentLastModified = businessProfile.ReviewAndFeedbackConcurrentLastModified;

                }

                #endregion

                if (request.InputDTO.KYCStatusCode == KYCStatus.Verified.Id)
                {
                    var profileStatus = await _partnerService.GetPartnerProfileStatus(partnerInfo.Id, partnerSubInfo.Id);

                    if (profileStatus.Value.Name != OnboardWorkflowStatus.Approve_Complete.Name)
                    {
                        return Result.Failure<KYCStatusReviewsOutputDTO>(
                                    $"Update KYC Status failed. Incomplete Partner Profile"
                                );
                    }
                    else
                    { // send email after user chose Verified 
                        // 1. get the email template
                        //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
                        var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
                        string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                        var generator = new Common.ContentGenerator(xsltTemplateRootPath);
                        var kycVerifiedStatusTemplate = "AdminKYCVerifiedStatusTemplate";

                        // 2. Use xsl to inject the properties from @event
                        StringBuilder _xml = new StringBuilder();
                        using (XmlWriter writer = XmlWriter.Create(_xml))
                        {
                            writer.WriteStartDocument();
                            writer.WriteStartElement("Profile");
                            writer.WriteElementString("PartnerName", businessProfile.CompanyName);
                            writer.WriteElementString("LoginUrl", _config.GetValue<string>("IntranetUri"));
                            writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                            writer.WriteEndElement();
                            writer.WriteEndDocument();
                        }
                        string content = generator.GenerateContent(_xml.ToString(), kycVerifiedStatusTemplate, cultureName);

                        long notificationTemplate = 30;
                        long recipientType = 1;
                        var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientType, notificationTemplate);

                        var recipients = new List<RecipientsInputDTO>();

                        foreach (var emailist in recipientInfo)
                        {
                            var recipientlist = new RecipientsInputDTO()
                            {
                                email = emailist.Email,
                                name = emailist.Name
                            };
                            recipients.Add(recipientlist);
                        }
                        long ccType = 2;
                        var ccInfo = await _businessProfileService.GetRecipientEmail(ccType, notificationTemplate);

                        var cc = new List<RecipientsInputDTO>();

                        foreach (var emailist in ccInfo)
                        {
                            var cclist = new RecipientsInputDTO()
                            {
                                email = emailist.Email,
                                name = emailist.Name
                            };
                            cc.Add(cclist);
                        }

                        long bccType = 3;
                        var bccInfo = await _businessProfileService.GetRecipientEmail(bccType, notificationTemplate);

                        var bcc = new List<RecipientsInputDTO>();

                        foreach (var emailist in bccInfo)
                        {
                            var bcclist = new RecipientsInputDTO()
                            {
                                email = emailist.Email,
                                name = emailist.Name
                            };
                            bcc.Add(bcclist);
                        }

                        Result<HttpStatusCode> sendApprovedNotificationEmailResponse =
                            await _notificationService
                            .SendNotification(recipients, bcc, cc, new List<IFormFile>() { },
                            $"Pending Approval {businessProfile.CompanyName} x {request.TrangloEntity}", content, NotificationTypes.Email);

                        if (sendApprovedNotificationEmailResponse.IsFailure)
                        {
                            _logger.LogError($"[UpdateKYCStatusCommand] 'Verified' Notification failed for {businessProfile}. {sendApprovedNotificationEmailResponse.Error}.");
                        }
                    }
                }

                

                //kyc requisition
                #region KYCRequisition
                var partnerProfile = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);
                var partnerSubscriptions = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerProfile.Id);
                var psList = partnerSubscriptions.ToList();
                var isPartnerConnect = psList.FindAll(x => x.Solution == Solution.Connect);
                var isPartnerBusiness = psList.FindAll(x => x.Solution == Solution.Business);
                var entities = partnerSubscriptions.Where(x => x.TrangloEntity != null).Select(x => x.TrangloEntity).ToList();
                string entitiesConcat = String.Join(" / ", entities);
                var kycStatus = Enumeration.FindById<KYCStatus>(request.InputDTO.KYCStatusCode);
                KYCSubmissionStatus kYCSubmissionStatus = new KYCSubmissionStatus();
                CollectionTier businessCollectionTier = null;
                if (request.AdminSolution == Solution.Connect.Id)
                {
                     businessCollectionTier = null;
                }
                else if (request.AdminSolution == Solution.Business.Id)
                {
                    businessCollectionTier = Enumeration.FindById<CollectionTier>(businessProfile.CollectionTier.Id);
                }
                 
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = (await connection.QueryAsync<PartnerKYCPendingRequsitionOutputDTO>("GetPendingPartnerKYCRequisition",
                        new
                        {
                            request.BusinessProfileCode
                        }, commandType: System.Data.CommandType.StoredProcedure)).FirstOrDefault();

                    if (reader != null)
                    {

                        return Result.Failure<KYCStatusReviewsOutputDTO>(
                                    $"Update KYC Status failed for {request.BusinessProfileCode}. Contains Pending Requisition"
                                );
                    }

                }

                //insufficient/icomplete does not need approval
                if (kycStatus == KYCStatus.Insufficient_Incomplete)
                {
                    var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerProfile.Id);

                    if (bilateralPartnerFlow is null)
                    {
                        return Result.Failure<KYCStatusReviewsOutputDTO>(
                                    $"PartnerType is null for all subscriptions of business profile: {request.BusinessProfileCode}."
                                );
                    }

                    if (request.AdminSolution == Solution.Connect.Id)
                    {
                        //kYCSubmissionStatus = Enumeration.FindById<KYCSubmissionStatus>(businessProfile.KYCSubmissionStatus.Id);
                        businessProfile.AssignKYCStatus(kycStatus, bilateralPartnerFlow, request.AdminSolution, businessProfile.KYCSubmissionStatus);
                    }
                    else if (request.AdminSolution == Solution.Business.Id)
                    {
                        kYCSubmissionStatus = Enumeration.FindById<KYCSubmissionStatus>(businessProfile.BusinessKYCSubmissionStatus.Id);
                        businessProfile.AssignBusinessKYCStatus(kycStatus, bilateralPartnerFlow, request.AdminSolution, kYCSubmissionStatus, businessCollectionTier);
                    }
                    

                        var update = await _businessProfileService.UpdateBusinessProfileAsync(businessProfile);

                        if (update == null)
                        {
                            return Result.Failure<KYCStatusReviewsOutputDTO>(
                                        $"Update KYC Status failed for {request.BusinessProfileCode}."
                                    );
                        }
                        else
                        {
                            result.IsKYCStatusUpdated = true;
                            return Result.Success(result);
                        }
                    
                    
                }

                //copy from CreateKYCRequisitionApprovalCommandHandler, author: Wing Han
                var runningNumberExists = await _businessProfileRepository.GetRequisitionRunningNumberLatest();
                RequisitionRunningNumber requisitionRunningNumber = new RequisitionRunningNumber();
                if (runningNumberExists == null)
                {

                    requisitionRunningNumber.Prefix = prefix;
                    requisitionRunningNumber.RunningNumber = "000001";

                    runningNumberExists = await _businessProfileRepository.AddRequisitionRunningNumber(requisitionRunningNumber);

                }
                else
                {
                    #region Running Number Increment
                    var rNum = Int64.Parse(runningNumberExists.RunningNumber);
                    rNum++;
                    var rNumString = rNum.ToString();
                    var rNumPadding = rNumString.PadLeft(6, '0');
                    runningNumberExists.RunningNumber = rNumPadding.ToString();
                    #endregion

                    requisitionRunningNumber = await _businessProfileRepository.UpdateRequisitionRunningNumber(runningNumberExists);
                }

                //start to create new requisition 
                var runningNumberChecking = await _businessProfileRepository.GetRequisitionRunningNumberLatest();
                var pendingHigherSubjectTemplate = "";
                var pendingHigherTemplate = "";
                if (request.AdminSolution == Solution.Connect.Id)
                {
                    pendingHigherSubjectTemplate = $"Pending Higher Approval {businessProfile.CompanyName} x {entitiesConcat}";
                    pendingHigherTemplate = "PartnerKYCRequisitionTemplate";
                }
                else if (request.AdminSolution == Solution.Business.Id)
                {
                    pendingHigherSubjectTemplate = $"(Tranglo Business) Pending Higher Approval {businessProfile.CompanyName} x {entitiesConcat}";
                    pendingHigherTemplate = "BusinessPartnerKYCRequisitionTemplate";
                }
                var requsition = new PartnerKYCStatusRequisition()
                {
                    RequisitionCode = prefix + runningNumberChecking.RunningNumber,
                    BusinessProfileCode = request.BusinessProfileCode,
                    KYCStatusCode = request.InputDTO.KYCStatusCode,
                    Remarks = request.InputDTO.Remarks,
                    SolutionCode = request.AdminSolution
                };

                await _approvalManager.AddRequisition(requsition);
                result.IsRequisitionCreated = true;
                result.RequisitionCode = requsition.RequisitionCode;

                if (kycStatus == KYCStatus.Pending_Higher_Approval && result.IsRequisitionCreated)
                {
                    // Send L1 request approval email
                    // 1. get the email template
                    //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
                    var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
                    string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                    var generator = new Common.ContentGenerator(xsltTemplateRootPath);

                    // 2. Use xsl to inject the properties from @event
                    StringBuilder _xml = new StringBuilder();
                    using (XmlWriter writer = XmlWriter.Create(_xml))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("Profile");
                        writer.WriteElementString("FullName", "Head of Compliance");
                        writer.WriteElementString("PartnerName", businessProfile.CompanyName);
                        writer.WriteElementString("LoginUrl", _config.GetValue<string>("IntranetUri"));
                        writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                    string content = generator.GenerateContent(_xml.ToString(), pendingHigherTemplate, cultureName);

                    bool isProduction = _environment.EnvironmentName.ToLower() == Environments.Production.ToLower();;
                    long notificationTemplate = 26;
                    var recipientInfolevel1 = await _businessProfileRepository.GetRecipientEmailByAuthorityLevel(1, 1, notificationTemplate);
                    var recipientInfolevel2 = await _businessProfileRepository.GetRecipientEmailByAuthorityLevel(2, 1, notificationTemplate);
                    var ccInfo = await _businessProfileRepository.GetRecipientEmail(2, notificationTemplate);
                    var recipients = new List<RecipientsInputDTO>();
                    var cc = new List<RecipientsInputDTO>();
                    var bcc = new List<RecipientsInputDTO>();

                    foreach (var emailist in recipientInfolevel1)
                    {
                        var recipientlist = new RecipientsInputDTO()
                        {
                            email = emailist.Email,
                            name = emailist.Name
                        };
                        recipients.Add(recipientlist);
                    }
                    foreach (var emailist in recipientInfolevel2)
                    {
                        var recipientlist = new RecipientsInputDTO()
                        {
                            email = emailist.Email,
                            name = emailist.Name
                        };
                        recipients.Add(recipientlist);
                    }
                    foreach (var cclist in ccInfo)
                    {
                        var recipientlist = new RecipientsInputDTO()
                        {
                            email = cclist.Email,
                            name = cclist.Name
                        };
                        cc.Add(recipientlist);
                    }
                    
                    var sendEmailResponse = await _notificationService.SendNotification(
                                                    recipients, bcc, cc,
                                                    new List<IFormFile>() { },
                                                    pendingHigherSubjectTemplate, content,
                                                    NotificationTypes.Email);
                }

                #endregion

                return Result.Success(result);
            }
        }
    }
}
