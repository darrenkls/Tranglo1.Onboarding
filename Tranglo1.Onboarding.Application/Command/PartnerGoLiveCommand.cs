using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerOnboardProgress, UACAction.Approve)]
    [Permission(Permission.ManagePartnerOnboardProgress.Action_Approve_Code,
       new int[] { (int)PortalCode.Admin },
       new string[] { Permission.ManagePartnerOnboardProgress.Action_View_Code })]
    internal class PartnerGoLiveCommand : BaseCommand<Result<PartnerRequestGoLiveOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public string UserBearerToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<PartnerRequestGoLiveOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update Partner Onboarding Status for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        internal class PartnerGoLiveCommandHandler : IRequestHandler<PartnerGoLiveCommand, Result<PartnerRequestGoLiveOutputDTO>>
        {
            private readonly ILogger<PartnerGoLiveCommandHandler> _logger;
            private readonly PartnerService _partnerService;
            private readonly INotificationService _notificationService;
            private readonly IConfiguration _config;
            private readonly IPartnerRepository _partnerRepository;
            private readonly BusinessProfileService _businessProfileService;
            private HttpClient _httpClient = new HttpClient();

            //CMS DTOs
            private class PartnerRequisitionResponse
			{
				public string RequisitionCode { get; set; }
				public int Approvalstatus { get; set; }
				public int PartnerId { get; set; }
				public string TrnxID { get; set; }
				public string Status { get; set; }
				public bool OperationStatus { get; set; }
				public string ResponseMessage { get; set; }
			}
            private class WalletRequisitionResponse
			{
                public string RequisitionCode { get; set; }
                public string Approvalstatus { get; set; }
                public int PartnerId { get; set; }
                public string TrnxID { get; set; }
                public bool OperationStatus { get; set; }
                public string ResponseMessage { get; set; }
            }
           
            public PartnerGoLiveCommandHandler(ILogger<PartnerGoLiveCommandHandler> logger,
                                                        PartnerService partnerService,
                                                        INotificationService notificationService,
                                                        IConfiguration config,
                                                        BusinessProfileService businessProfileService,
                                                        IPartnerRepository partnerRepository)
            {
                _partnerService = partnerService;
                _logger = logger;
                _businessProfileService = businessProfileService;
                _logger = logger;
                _notificationService = notificationService;
                _config = config;
                _partnerService = partnerService;
                _partnerRepository = partnerRepository;
            }

            public async Task<Result<PartnerRequestGoLiveOutputDTO>> Handle(PartnerGoLiveCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    //todo: refactor CMS api into own class
                    var isReadyGoLive = await _partnerService.IsPartnerReadyGoLive(request.PartnerCode, request.PartnerSubscriptionCode, true); //check
					if (!isReadyGoLive)
					{
                        return Result.Failure<PartnerRequestGoLiveOutputDTO>(
                               $"Partner is Not Ready to Go Live Yet."
                           );
                    }
                    var CMSApiUri = _config.GetValue<string>("CMSApiUri");
                    var CMSApiKey = _config.GetValue<string>("CMSApiKey");

                    var apiName = "/PartnerIntegration/CreateAutoApproval";

                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"api {CMSApiKey}");
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    _httpClient.BaseAddress = new Uri(CMSApiUri);

                    var partnerDetails = await _partnerRepository.GetPartnerDetailsByCodeAsync(request.PartnerCode);
                    var subscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
                    var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(partnerDetails.BusinessProfileCode);
                    BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();

                    var companyRegisteredCountryISO2 = CountryMeta.GetCountryISO2ByIdAsync(businessProfile.CompanyRegisteredCountryCode.Value);
                    var partnerCreateTrnxID = Guid.NewGuid().ToString();
                    //get currency code from pricing api
                    //var pricingAPIObj = await _partnerService.GetPartnerDetailsFromPricingApi(request.PartnerSubscriptionCode, request.UserBearerToken); //check
                    //if (pricingAPIObj.IsFailure)
                    //{
                    //    return Result.Failure<PartnerRequestGoLiveOutputDTO>(
                    //        $"Request GoLive Failed for {request.PartnerCode}. Pricing API Error: {pricingAPIObj.Error} "
                    //    );
                    //}

                    //check if have exisitng one
                    var partnerCMSIntegrationDetails = await _partnerRepository.GetPartnerCMSIntegrationByPartnerSubscriptionCodeAsync(request.PartnerSubscriptionCode); //check

                    //if dont have exising one create a new one
                    if(partnerCMSIntegrationDetails == null)
					{
                        string CMSCompanyCode = "";
                        switch (subscription.TrangloEntity?.ToUpper())
                        {
                            case "TSB":
                                CMSCompanyCode = "CC0001";
                                break;
                            case "TPL":
                                CMSCompanyCode = "CC0008";
                                break;
                            case "TEL":
                                CMSCompanyCode = "CC0006";
                                break;
                            case "PTT":
                                CMSCompanyCode = "CC0005";
                                break;
                            default:
                                throw new NotImplementedException("Entity NOT DEFINED - PartnerGoLiveCommand");
                        }

                        //#41508 check 
                        string contactNo = "", faxNo ="";
                        bool containsError = false;
                        List<string> errorDetails = new List<string>();
                        if(businessProfile.ContactNumber != null)
                        {
                            contactNo = $"{businessProfile.ContactNumber.DialCode}{businessProfile.ContactNumber.Value}";
                        }
                        else
                        {
                            containsError = true;
                            errorDetails.Add("Contact Number Empty");
                        }

                        if(businessProfile.FacsimileNumber != null)
                        {
                            //fax number is not madatory in CMS
                            faxNo = $"{businessProfile.FacsimileNumber.DialCode}{businessProfile.FacsimileNumber.Value}";
                        }

                        if (!partnerDetails.AgreementStartDate.HasValue)
                        {
                            containsError = true;
                            errorDetails.Add("Agreement Start Date Empty");
                        }
                        if (!partnerDetails.AgreementEndDate.HasValue)
                        {
                            containsError = true;
                            errorDetails.Add("Agreement End Date Empty");
                        }

                        if (containsError)
                        {
                            return Result.Failure<PartnerRequestGoLiveOutputDTO>(
                                $"Request GoLive Failed for {request.PartnerCode}. Mandatory Fields Are Empty! : {String.Join(", ",errorDetails)} "
                            );
                        }

                        /* CMS Mapping (T1 - CMS)
                         * PartnerType 1 Sales Partner = 1 Dealer
                         * PartnerType 2 Suppy Partner = 2 Supplier
                         */
                        var CMSPartnerCreateReq = new
                        {
                            PartnerType = subscription.PartnerType.Id, // check
                            ContractNo = "N/A", //must not be empty 
                            RegistrationDate = partnerDetails.AgreementStartDate.Value,
                            RegistrationExpiryDate = partnerDetails.AgreementEndDate.Value,
                            PersonInCharge = businessProfile.ContactPersonName,
                            Email = partnerDetails.Email.Value,
                            CompanyCode = CMSCompanyCode, // need use CC0001 
                            Name = businessProfile.CompanyName,
                            RegistrationNo = businessProfile.CompanyRegistrationNo,
                            AddressLine1 = businessProfile.CompanyRegisteredAddress,
                            AddressPostalCode = businessProfile.CompanyRegisteredZipCodePostCode,
                            AddressCountryCode = companyRegisteredCountryISO2,
                            ContactNo = contactNo,
                            TenancyAgreementStatus = partnerDetails.AgreementStatus.GetValueOrDefault(0),
                            AutoRenew = true,
                            AgreementFileUpload = "",
                            FaxNo = faxNo,
                            T1_Partner_Key = subscription.Id, // NOTE: Changed from PartnerCode to PartnerSubscriptionCode
                            TrnxID = partnerCreateTrnxID
                        };
                        var httpContent = new StringContent(JsonConvert.SerializeObject(CMSPartnerCreateReq), Encoding.UTF8, "application/json");
                        var httpResponse = await _httpClient.PostAsync(apiName, httpContent);


                        if (httpResponse.IsSuccessStatusCode)
                        {
                            var cmsApiResponse = await httpResponse.Content.ReadAsStringAsync();
                            var cmsResp = JsonConvert.DeserializeObject<PartnerRequisitionResponse>(cmsApiResponse);
                            
                            partnerCMSIntegrationDetails = new PartnerCMSIntegrationDetail()
                            {
                                PartnerCode = request.PartnerCode,
                                PartnerSubscriptionCode = request.PartnerSubscriptionCode,
                                TrnxID = partnerCreateTrnxID,
                                RcCode = cmsResp.RequisitionCode,
                                CMSPartnerId = cmsResp.PartnerId, // One subscription in T1 == One partner in CMS
                                CMSStatus = cmsResp.Status
                            };

                            partnerCMSIntegrationDetails = await _partnerRepository.AddPartnerCMSIntegration(partnerCMSIntegrationDetails);

						}
						else
						{
                            var cmsApiResponse = await httpResponse.Content.ReadAsStringAsync();
                            
                            //cms api error
                            _logger.LogError($"[PartnerGoLiveCommand] POST /PartnerIntegration/CreateAutoApproval");
                            _logger.LogError($"[PartnerGoLiveCommand] CMS Trnx ID : {partnerCreateTrnxID}");
                            _logger.LogError($"[PartnerGoLiveCommand] CMS Response: {cmsApiResponse}");
                            _logger.LogError($"[PartnerGoLiveCommand] CMS Status Code: {httpResponse.StatusCode}");
                            if (cmsApiResponse.Contains("ResponseMessage"))
                            {
                                var cmsResp = JsonConvert.DeserializeObject<PartnerRequisitionResponse>(cmsApiResponse);
                                cmsApiResponse = cmsResp.ResponseMessage;
                            }
                            return Result.Failure<PartnerRequestGoLiveOutputDTO>(
                               $"GoLive Failed for PartnerSubscriptionCode: {request.PartnerSubscriptionCode}. Partner Creation CMS API Failure. {cmsApiResponse}"
                           );
                        }
                    }

                    //check if have wallet requisition request or not
                    var cmsWalletIntegrationDetails = await _partnerRepository.GetPartnerWalletIntegrationByPartnerSubscriptionCodeAsync(request.PartnerSubscriptionCode); // check

                    //hardcode the modelType first
                    //TODO: confirm with BA again on wht to do with model type
                    int cmsModelType = 8; //wholesales prepaid (for Dealer only)
                    if(subscription.PartnerType.Id == 2) // check
                    {
                        cmsModelType = 104; //exact amount
                    }

                    if(cmsWalletIntegrationDetails == null)
					{
                        var walletCreateTrnxID = Guid.NewGuid().ToString();

                        var CMSCreateWalletReq = new
                        {
                            //PartnerType = partnerDetails.PartnerTypeCode,
                            //PartnerId = partnerCMSIntegrationDetails.CMSPartnerId,
                            //WalletName = $"{businessProfile.CompanyName} {pricingAPIObj.Value.CurrencyCode}", //wallet name
							WalletName = $"{businessProfile.CompanyName} {subscription.SettlementCurrencyCode}", //wallet name
							WalletDescription = businessProfile.CompanyName + " Wallet",
                            //CurrencyCode = pricingAPIObj.Value.CurrencyCode, // check
							CurrencyCode = subscription.SettlementCurrencyCode,
							InventoryType = 1, //partner wallet inventory type , 1: wallet, 2: stock ,
                            CountryCode = companyRegisteredCountryISO2,
                            timezoneid = 79, //hardcode to KL first
                            LanguageCode = "EN",
                            T1_Partner_Key = subscription.Id, // NOTE: Changed from PartnerCode to PartnerSubscriptionCode
                            //Shortname = $"{businessProfile.CompanyName} {pricingAPIObj.Value.CurrencyCode}", // check
							Shortname = $"{businessProfile.CompanyName} {subscription.SettlementCurrencyCode}", // check
							ModelType = cmsModelType,
                            TrnxID = walletCreateTrnxID // check
                        };
                        apiName = "/WalletIntegration/RemittanceCreate";
                        var httpContent = new StringContent(JsonConvert.SerializeObject(CMSCreateWalletReq), Encoding.UTF8, "application/json");
                        var httpResponse = await _httpClient.PostAsync(apiName, httpContent);

                        if (httpResponse.IsSuccessStatusCode)
                        {
                            var cmsApiResponse = await httpResponse.Content.ReadAsStringAsync();
                            var cmsWalletResp = JsonConvert.DeserializeObject<WalletRequisitionResponse>(cmsApiResponse);

                            cmsWalletIntegrationDetails = new PartnerWalletCMSIntegrationDetail // check
                            {
                                PartnerCode = request.PartnerCode,
                                PartnerSubscriptionCode = request.PartnerSubscriptionCode,
                                RcCode = cmsWalletResp.RequisitionCode,
                                CMSStatus = cmsWalletResp.Approvalstatus,
                                TrnxID = walletCreateTrnxID
                            };

                            cmsWalletIntegrationDetails = await _partnerRepository.AddPartnerWalletIntegration(cmsWalletIntegrationDetails);
						}
						else
						{
                            var cmsApiResponse = await httpResponse.Content.ReadAsStringAsync();                            

                            _logger.LogError($"[PartnerGoLiveCommand] Error POST /WalletIntegration/Create");
                            _logger.LogError($"[PartnerGoLiveCommand] CMS Response: {cmsApiResponse} ");

                            if (cmsApiResponse.Contains("ResponseMessage"))
                            {
                                var cmsResp = JsonConvert.DeserializeObject<PartnerRequisitionResponse>(cmsApiResponse);
                                cmsApiResponse = cmsResp.ResponseMessage;
                            }

                            return Result.Failure<PartnerRequestGoLiveOutputDTO>(
                               $"GoLive Failed for PartnerSubscriptionCode: {request.PartnerSubscriptionCode}. Wallet Creation CMS API Failure. {cmsApiResponse}"
                           );
                        }
					}
					else
					{
                        _logger.LogError($"[PartnerGoLiveCommand] Wallet Creation Data already exist {JsonConvert.SerializeObject(cmsWalletIntegrationDetails)}");

                        return Result.Failure<PartnerRequestGoLiveOutputDTO>(
                               $"Wallet Creation for PartnerSubscriptionCode: {request.PartnerSubscriptionCode} is {cmsWalletIntegrationDetails.CMSStatus}. "
                           );
                    }

                    var result = new PartnerRequestGoLiveOutputDTO
                    {
                        PartnerCode = request.PartnerCode,
                        PartnerSubscriptionCode = request.PartnerSubscriptionCode,
                        Status = true
                    };
                    return Result.Success(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[PartnerGoLiveCommand] PartnerRequestGoLiveCommand {ex}", ex.ToString());
                    _logger.LogError($"[PartnerGoLiveCommand] Stack Trace {ex}", ex.StackTrace);
                }
                return Result.Failure<PartnerRequestGoLiveOutputDTO>(
                            $"Request GoLive Failed for PartnerSubscriptionCode: {request.PartnerSubscriptionCode}."
                        );
            }
        }
    }    
}
