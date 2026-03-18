using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription;
using Tranglo1.Onboarding.Application.Managers;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerDetails, UACAction.Edit)]
    [Permission(Permission.ManagePartnerPartnerDetails.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerPartnerDetails.Action_View_Code })]
    internal class SavePartnerSubscriptionCommand : BaseCommand<Result<SavePartnerSubscriptionInputDTO>>
    {
        public SavePartnerSubscriptionInputDTO PartnerSubscription;
        public long PartnerCode;
        public string UserBearerToken;

        public override Task<string> GetAuditLogAsync(Result<SavePartnerSubscriptionInputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Save Partner Subscriptions for PartnerCode: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SavePartnerSubscriptionCommandHandler : IRequestHandler<SavePartnerSubscriptionCommand, Result<SavePartnerSubscriptionInputDTO>>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IConfiguration _config;
        private IHttpClientFactory _httpClientFactory;
        private readonly IntegrationManager _integrationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SavePartnerSubscriptionCommandHandler> _logger;
        private readonly BusinessProfileService _businessProfileService;

        private class ApiResponse
        {
            public string Detail { get; set; }
        }

        private class PartnerPricingAssignmentOutputDTO
        {
            //public long? PricingPackageAssignmentCode { get; set; }
            //public long? PricingPackageCode { get; set; }
            public string CurrencyCode { get; set; }
            public bool DisplayDefaultPackage { get; set; }
        }

        private class AddedSubscription
        {
            public string Entity { get; set; }
            public string PartnerType { get; set; }
            public string Solution { get; set; }
            public string NewLine { get; set; }
        }

        public SavePartnerSubscriptionCommandHandler(IPartnerRepository partnerRepository, IConfiguration config,
            IHttpClientFactory httpClientFactory, IBusinessProfileRepository businessProfileRepository,
            IntegrationManager integrationManager, IWebHostEnvironment environment,
            INotificationService notificationService, ILogger<SavePartnerSubscriptionCommandHandler> logger,
            BusinessProfileService businessProfileService)
        {
            _partnerRepository = partnerRepository;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _businessProfileRepository = businessProfileRepository;
            _integrationManager = integrationManager;
            _environment = environment;
            _notificationService = notificationService;
            _logger = logger;
            _businessProfileService = businessProfileService;
        }

        public async Task<Result<SavePartnerSubscriptionInputDTO>> Handle(SavePartnerSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var partnerCode = request.PartnerCode;
            var trangloEntity = request.PartnerSubscription.TrangloEntity;
            var country = await _partnerRepository.GetCountryAsync(request.PartnerSubscription?.CountryISO2);
            var countryISO2 = country?.CountryISO2;
            var userBearerToken = request.UserBearerToken;

            var partner = await _partnerRepository.GetPartnerDetailsByCodeAsync(partnerCode);
            var partnerName = _businessProfileRepository.GetBusinessProfileByCodeAsync(partner.BusinessProfileCode).Result.CompanyName;
            var partnerSubInfo = await _partnerRepository.GetPartnerSubscriptionByPartnerCodeAsync(partnerCode);
            var businessProfilePartner = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(partner.BusinessProfileCode);

            var businessProf = businessProfilePartner.Value;
            string businessNatureDescription = null;

            if (businessProf.BusinessNature != null)
            {
                businessNatureDescription = businessProf.BusinessNature.Name;

                if (businessProf.BusinessNature == BusinessNature.Other)
                {
                    businessNatureDescription = BusinessNature.Other.Name;
                }
            }

            if (request.PartnerSubscription.TrangloEntity != partnerSubInfo.TrangloEntity && partnerSubInfo.Solution == Solution.Business)
            {
                var changeTrangloEntityHandling = await _businessProfileService.ChangeTrangloEntityHandling(partner.BusinessProfileCode, request.PartnerSubscription.TrangloEntity, partner.CustomerType.Id);
            }

            bool newPartnerSubs = false;
            bool newPartnerStaging = false;
            bool productionPartnerSub = false;
            foreach (var p in request.PartnerSubscription.Subscriptions)
            {
                if (p.PartnerSubscriptionCode == null)
                {
                    newPartnerSubs = true;
                }

            }
            var partnerSubListInfo = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerCode);
            if (partnerSubListInfo.Exists(x => x.Environment == Domain.Entities.Environment.Staging) && partnerSubInfo.Solution == Solution.Business)
            {
                newPartnerStaging = true;
            }

            //check if other subscription already at Production environment
            if (partnerSubListInfo.Exists(x => x.Environment == Domain.Entities.Environment.Production))
            {
                productionPartnerSub = true;
            }
            List<AddedSubscription> addedSubscriptions = new List<AddedSubscription>();

            foreach (var ps in request.PartnerSubscription.Subscriptions)
            {
                if (ps.PartnerType is null)
                {
                    return Result.Failure<SavePartnerSubscriptionInputDTO>(
                                $"Failed to add subscription. Partner Type cannot be empty");
                }
                if (ps.Solution is null)
                {
                    return Result.Failure<SavePartnerSubscriptionInputDTO>(
                                $"Failed to add subscription. Solution cannot be empty");
                }
                if (ps.Currency is null)
                {
                    return Result.Failure<SavePartnerSubscriptionInputDTO>(
                                $"Failed to add subscription. Currency cannot be empty");
                }

                var partnerType = await _partnerRepository.GetPartnerTypeAsync(ps.PartnerType.Value);
                var solution = await _partnerRepository.GetSolutionAsync(ps.Solution.Value);
                long? rspId = null;
                long? supplierPartnerId = null;
                bool? isOnboardComplete = false;
                //bool isPricingPackageAssign = ps.PricingPackage.HasValue ? true : false;

                if (ps.PartnerSubscriptionCode is null) // ADD
                {
                    // same combination of partner type + solution cannot be allowed in different subscription packages.
                    if (partnerType != null && solution != null && trangloEntity != null)
                    {
                        var subscriptions = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerCode);
                        var remittancePartner = await _partnerRepository.GetCustomerTypeByCodeAsync(CustomerType.Remittance_Partner.Id);
                        var existingPairing = subscriptions
                            .Find(x => x.PartnerType == partnerType && x.Solution == solution && x.PartnerCode == partnerCode && x.TrangloEntity == trangloEntity);

                        if (existingPairing != null)
                        {
                            return Result.Failure<SavePartnerSubscriptionInputDTO>(
                                $"Failed to add subscription. The same combination of Partner Type and Solution and Tranglo Entity already exists");
                        }

                        var existingSolution = subscriptions.Find(x => x.Solution == solution);
                        if (existingSolution == null)
                        {
                            partner.CustomerTypeCode = remittancePartner.Id;
                            var updateCustomerType = await _partnerRepository.UpdatePartnerRegistrationAsync(partner);
                            var addCustomerBusinessDeclaration = await _businessProfileService.AddCustomerBusinessDeclaration(partner.BusinessProfileCode);
                            if (addCustomerBusinessDeclaration.IsFailure)
                            {
                                return Result.Failure<SavePartnerSubscriptionInputDTO>($"Failed to add Customer Business Declaration.");
                            }
                        }
                    }

                    // check mandatory for Partner type (mandatory), Solution(mandatory), Currency(mandatory)
                    if (partnerType is null || solution is null || ps.Currency is null)
                    {
                        return Result.Failure<SavePartnerSubscriptionInputDTO>(
                            $"Failed to add subscription. PartnerType, Solution and Currency cannot be null");
                    }

                    // Price Package(mandatory for partner type = Sales Partner,
                    // hide if partner type selected = Supply Partner)
                    //if (partnerType == PartnerType.Sales_Partner
                    //    //&& ps.PricingPackage is null 
                    //    && solution != Solution.Business)
                    //{
                    //    return Result.Failure<SavePartnerSubscriptionInputDTO>(
                    //        $"Failed to add subscription. Pricing Package is mandatory for Sales Partner and cannot be null");
                    //}

                    // add RSP, Supplier Partner Details in GloRemit. Return PartnerId                  
                    if (solution != Solution.Business && (partnerType == PartnerType.Sales_Partner))
                    {
                        //if (ps.PricingPackage != null)
                        //{
                        rspId = await _integrationManager.AddRSPAsync(
                            ps.Currency,
                            countryISO2,
                            partnerName);
                        //}
                        //else
                        //{
                        //    return Result.Failure<SavePartnerSubscriptionInputDTO>($"Price Package is required.");
                        //}
                    }
                    else
                    if (partnerType == PartnerType.Supply_Partner)
                    {
                        supplierPartnerId = await _integrationManager.AddSupplierPartnerAsync(
                            partnerName,
                            ps.Currency);
                    }

                    if (productionPartnerSub == true && ps.Solution == Solution.Connect.Id)// Environment change to Production automatically only applicable to TB
                    {
                        productionPartnerSub = false;
                    }
                    var subscription = new PartnerSubscription(partnerCode, partnerType, solution, trangloEntity, ps.Currency, rspId, supplierPartnerId, isOnboardComplete, productionPartnerSub);
                    var addSubcription = await _partnerRepository.AddSubcriptionAsync(subscription);

                    // For email
                    AddedSubscription addedSubscription = new AddedSubscription()
                    {
                        Entity = subscription.TrangloEntity,
                        PartnerType = subscription.PartnerType.Name,
                        Solution = subscription.Solution.Name,
                        NewLine = null
                    };
                    addedSubscriptions.Add(addedSubscription);

                    // add PricingPackageAssignments
                    //object pricingInputDTO = new
                    //{
                    //    partnerCode = partnerCode,
                    //    pricingPackageCode = ps.PricingPackage,
                    //    currencyCode = ps.Currency,
                    //    displayDefaultPackage = ps.DisplayDefaultPackage
                    //};

                    // var addPricingPackageAssignment = await SavePricingPackageAssignment(subscription, pricingInputDTO, ps, userBearerToken);
                    // if (addSubcription.IsFailure || addPricingPackageAssignment.IsFailure)
                    // {
                    //     return Result.Failure<SavePartnerSubscriptionInputDTO>($"Failed to add subcription");
                    // }


                }
                else if (ps.PartnerSubscriptionCode != null /*&& ps.IsDeleted is false*/) // UPDATE
                {
                    var subscriptions = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerCode);
                    var subs = await _partnerRepository.GetSubscriptionAsync(ps.PartnerSubscriptionCode.Value);
                    var partnerR = await _partnerRepository.GetPartnerRegistrationByCodeAsync(partnerCode);
                    var businessProfileP = await _businessProfileRepository.GetBusinessProfileByCodeAsync(partnerR.BusinessProfileCode);
                    //Send Email
                    if (subscriptions.Count <= 1 && subs.PartnerType is null && subs.Solution is null && (subs.TrangloEntity is null || subs.TrangloEntity != null))
                    {
                        subs.PartnerType = partnerType;
                        subs.Solution = solution;
                        subs.TrangloEntity = trangloEntity;

                        AddedSubscription addedSubscription = new AddedSubscription()
                        {

                            Entity = subs.TrangloEntity,
                            PartnerType = subs.PartnerType.Name,
                            Solution = subs.Solution.Name,
                            NewLine = null
                        };
                        addedSubscriptions.Add(addedSubscription);

                    }
                    // same combination of partner type + solution cannot be allowed in different subscription packages.
                    if (partnerType != null && solution != null && trangloEntity != null)
                    {
                        var existingPairing = subscriptions
                            .Find(x => x.PartnerType == partnerType && x.Solution == solution &&
                                    x.PartnerCode == partnerCode && x.Id != ps.PartnerSubscriptionCode && x.TrangloEntity == trangloEntity);

                        if (existingPairing != null)
                        {
                            return Result.Failure<SavePartnerSubscriptionInputDTO>(
                                $"Failed to update subcription. The same combination of Partner Type and Solution and Tranglo Entity already exists");
                        }
                    }

                    // check mandatory for Partner type (mandatory), Solution(mandatory), Currency(mandatory)
                    if (partnerType is null || solution is null || ps.Currency is null)
                    {
                        return Result.Failure<SavePartnerSubscriptionInputDTO>(
                            $"Failed to update subcription. PartnerType, Solution and Currency cannot be null");
                    }

                    // Price Package(mandatory for partner type = Sales Partner,
                    // hide if partner type selected = Supply Partner)
                    //if ((solution != Solution.Business) 
                    //    && (partnerType == PartnerType.Sales_Partner 
                    //    //&& ps.PricingPackage is null
                    //    ))
                    //{
                    //    return Result.Failure<SavePartnerSubscriptionInputDTO>(
                    //        $"Failed to update subcription. Pricing Package is mandatory for Sales Partner and cannot be null");
                    //}

                    var subscription = await _partnerRepository.GetSubscriptionAsync(ps.PartnerSubscriptionCode.Value);
                    bool isCurrencyCodeAssigned = ps.Currency is null ? false : true;
                    //bool isPricingPackageAssigned = ps.PricingPackage.HasValue ? true : false;

                    // For live partners (at least 1 subscription has gone live),
                    // partner details and subscription that has gone live cannot be edited
                    if (subscription.Environment == Domain.Entities.Environment.Production && newPartnerSubs is false && newPartnerStaging is false)
                    {
                        return Result.Failure<SavePartnerSubscriptionInputDTO>($"Failed to update subcription. Subscription has gone live");
                    }
                    else if (subscription.Environment == Domain.Entities.Environment.Staging || subscription.Environment is null)
                    {
                        subscription.PartnerType = partnerType;
                        subscription.Solution = solution;
                        subscription.TrangloEntity = trangloEntity;
                        subscription.SettlementCurrencyCode = ps.Currency;
                        subscription.IsCurrencyCodeAssigned = isCurrencyCodeAssigned;
                        //subscription.IsPricePackageAssigned = isPricingPackageAssigned;

                        if (solution != Solution.Business && (partnerType == PartnerType.Sales_Partner))
                        {
                            //if (ps.PricingPackage != null)
                            //{
                            if (subscription.RspStagingId != null)
                            {
                                await _integrationManager.UpdateRSPDetails(subscription.RspStagingId,
                                    ps.Currency,
                                    countryISO2,
                                    partnerName);
                            }
                            else
                            {
                                subscription.RspStagingId = await _integrationManager.AddRSPAsync(
                                    ps.Currency,
                                    countryISO2,
                                    partnerName);
                            }
                            //}
                            //else
                            //{
                            //    return Result.Failure<SavePartnerSubscriptionInputDTO>($"Price Package is required.");
                            //}
                        }
                        else if (partnerType == PartnerType.Supply_Partner)
                        {
                            if (subscription.SupplierPartnerStagingId != null)
                            {
                                await _integrationManager.UpdateSupplierPartnerDetails(
                                    subscription.SupplierPartnerStagingId,
                                    partnerName,
                                    ps.Currency);
                            }
                            else
                            {
                                subscription.SupplierPartnerStagingId = await _integrationManager.AddSupplierPartnerAsync(
                                    partnerName,
                                    ps.Currency);
                            }
                        }

                        var updateSubscription = await _partnerRepository.UpdateSubcriptionAsync(subscription);

                        // update PricingPackageAssignments                        
                        //object pricingInputDTO = new
                        //{
                        //    partnerCode = partnerCode,
                        //    pricingPackageCode = ps.PricingPackage,
                        //    currencyCode = ps.Currency,
                        //    displayDefaultPackage = ps.DisplayDefaultPackage
                        //};

                        //var updatePricingPackageAssignment = await SavePricingPackageAssignment(subscription, pricingInputDTO, ps, userBearerToken);
                        //if (updateSubscription.IsFailure || updatePricingPackageAssignment.IsFailure)
                        //{
                        //    return Result.Failure<SavePartnerSubscriptionInputDTO>($"Failed to update subcription");
                        //}
                    }
                }
                /*else if (ps.PartnerSubscriptionCode != null && ps.IsDeleted is true) // DELETE
                {
                    var subscription = await _partnerRepository.GetSubscriptionAsync(ps.PartnerSubscriptionCode.Value);
                    var deleteSubscription = await _partnerRepository.DeleteSubcriptionAsync(subscription);
                    var deletePricingPackageAssignment = await PricingPackageAssignment(subscription, null, ps, userBearerToken);

                    if (deleteSubscription.IsFailure || deletePricingPackageAssignment.IsFailure)
                    {
                        return Result.Failure<SavePartnerSubscriptionInputDTO>($"Failed to delete subcription");
                    }
                }*/
            }

            //Count new subscription and send the email
            if (addedSubscriptions.Count > 0)
            {

                var sendNotification = await SendEmail(partnerCode, partnerName, addedSubscriptions);
                if (sendNotification.IsFailure)
                {
                    return Result.Failure<SavePartnerSubscriptionInputDTO>($"Failed to send notification for added subscription(s). PartnerCode: {partnerCode}");
                }
            }

            // Set KYCSubmissionStatusCode to Submitted if every subscription has PartnerType = SupplyPartner
            var businessProfile = _businessProfileRepository.GetBusinessProfileByCodeAsync(partner.BusinessProfileCode).Result;
            var updatedSubscriptions = _partnerRepository.GetSubscriptionAsync(request.PartnerCode, request.PartnerSubscription.TrangloEntity).Result;
            List<bool> isAllSupplyPartner = new List<bool>();
            updatedSubscriptions.ForEach(x => { if (x.PartnerType == PartnerType.Supply_Partner) isAllSupplyPartner.Add(true); else isAllSupplyPartner.Add(false); });

            if (!isAllSupplyPartner.Contains(false))
            {
                var updateBpPartner = await _businessProfileService.EnsurePartnerType(businessProfile, true);
            }

            //check if partner got business partner
            var partnerRegistration = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);
            var partnerSubscriptions = await _partnerRepository.GetPartnerSubscriptionListAsync(partnerRegistration.Id);
            var isPartnerConnect = partnerSubscriptions.FindAll(x => x.Solution == Solution.Connect);
            var isPartnerBusiness = partnerSubscriptions.FindAll(x => x.Solution == Solution.Business);
            //var kycStatusCode = await _businessProfileRepository.GetKYCStatusByCodeAsync(businessProfile.KYCStatusCode);

            if (isPartnerBusiness.Count == 1 && isPartnerConnect.Count == 1)
            {
                businessProfile.BusinessKYCStatus = businessProfile.KYCStatus;
            }
            else if (isPartnerBusiness.Count > 0 && isPartnerConnect.Count == 0)
            {
                //businessProfile.BusinessKYCStatus = kycStatusCode;
                businessProfile.KYCStatus = null;
            }

            var update = await _businessProfileService.UpdateBusinessProfileAsync(businessProfile);

            //Check if Partner is MSB and created by SSO
            if (partner.CustomerType == CustomerType.Remittance_Partner)
            {
                bool isBusinessExist = false;
                bool isConnectExist = false;
                bool isNewBusiness = false;

                var partnerKYCSubmodule = await _businessProfileRepository.GetKYCSubModuleReviewByBusinessProfile(businessProfile);
                var connectKYCSubmodule = partnerKYCSubmodule.Find(x => x.KYCCategoryCode <= 7);
                var businessKYCSubmodule = partnerKYCSubmodule.Find(x => x.KYCCategoryCode >= 8);

                foreach (var p in request.PartnerSubscription.Subscriptions)
                {
                    if (p.PartnerSubscriptionCode != null && p.Solution == Solution.Business.Id)
                    {
                        isBusinessExist = true;
                    }

                    if (p.PartnerSubscriptionCode != null && p.Solution == Solution.Connect.Id)
                    {
                        isConnectExist = true;
                    }
                    if (p.PartnerSubscriptionCode == null && p.Solution == Solution.Business.Id)
                    {
                        isNewBusiness = true;
                    }
                }

                if (isBusinessExist == true && isConnectExist == false) // It will Add KYCSubModule For TC
                {
                    var AddKYCSubModuleForTCMSB = await _businessProfileService.AddKYCSubModuleForTCMSB(partner.BusinessProfileCode, partner.CustomerType.Id);
                }

                if (isNewBusiness == true && isConnectExist == true) // It will Add KYCSubModule For TB
                {
                    var AddKYCSubModuleForTCMSB = await _businessProfileService.AddKYCSubModuleForTBMSB(partner.BusinessProfileCode, partner.CustomerType.Id, request.PartnerSubscription.TrangloEntity);
                }
            }

            SavePartnerSubscriptionInputDTO outputDTO = new SavePartnerSubscriptionInputDTO()
            {
                PartnerCode = request.PartnerCode,
                CountryISO2 = request.PartnerSubscription.CountryISO2,
                TrangloEntity = request.PartnerSubscription.TrangloEntity
            };

            List<Subscriptions> dtoList = new List<Subscriptions>();

            foreach (var u in updatedSubscriptions)
            {
                if (u.Solution != Solution.Business)
                {
                    // var pricingPackageAssignment = await GetPricingPackageAssignment(u.Id, userBearerToken);
                    //var p = pricingPackageAssignment.Value;

                    Subscriptions dto = new Subscriptions()
                    {
                        PartnerSubscriptionCode = u.Id,
                        PartnerType = u.PartnerType.Id,
                        Solution = u.Solution.Id,
                        //Currency = p is null ? u.SettlementCurrencyCode : p.CurrencyCode,
                        //DisplayDefaultPackage = p is null ? false : p.DisplayDefaultPackage,
                        //PricingPackage = p is null ? null : p.PricingPackageCode
                    };
                    dtoList.Add(dto);

                }

                else
                {
                    Subscriptions dto = new Subscriptions()
                    {
                        PartnerSubscriptionCode = u.Id,
                        PartnerType = u.PartnerType.Id,
                        Solution = u.Solution.Id,
                        Currency = u is null ? null : u.SettlementCurrencyCode

                    };
                    dtoList.Add(dto);

                }

                //Save Update Event
                var partnerProfileChangedEvent = new PartnerProfileChangedEvent(
                    trangloEntity,  //TrangloEntity
                    u.Solution.Id,    //SolutionCode
                    u.PartnerType.Id,    //PartnerTypeCode
                    partner.Id, //PartnerCode
                    partner.PartnerId,  //PartnerId
                    u.Id,     //PartnerSubscriptionCode get from subscription
                    u.SettlementCurrencyCode, //SettlementCurrencyCode get from subscription
                    businessProf.CompanyRegisteredCountryMeta?.CountryISO2 ?? null, //CustomerIncorporationCountry
                    businessProf.CompanyName,//CompanyName
                    businessProf.CompanyRegistrationNo, //CompanyRegistrationNo
                    businessProf.CompanyRegisteredAddress, //CompanyRegisteredAddress
                    businessProf.IDExpiryDate,  //IDExpiryDate
                    businessProf.DateOfBirth, //DateOfBirth
                    businessProf.TelephoneNumber, //TelephoneNumber
                    partner.Email,  //Email
                    businessProf.CompanyRegisteredZipCodePostCode, //CompanyRegisteredZipCodePostCode
                    businessProf.BusinessNature?.Id ?? null, //BusinessNatureCode
                    businessNatureDescription, //BusinessNatureDescription
                    partnerSubInfo.Environment.Id,  //EnvironmentCode
                    businessProf.DateOfIncorporation,
                    businessProf.IncorporationCompanyTypeCode,
                    partner.CustomerTypeCode,
                    businessProf.CollectionTier.Id,
                    businessProf.Id
                );

                var savePartnerProfileEvents = await _partnerRepository.AddPartnerOnboardingCreationEventAsync(partnerProfileChangedEvent);
                if (savePartnerProfileEvents.IsFailure)
                {
                    _logger.LogError("Failed to save partner profile events");
                    return Result.Failure<SavePartnerSubscriptionInputDTO>($"Saving Partner Profile Event Failed.");
                }

            }

            outputDTO.Subscriptions = dtoList;

            return Result.Success(outputDTO);
        }

        private async Task<Result> SendEmail(long partnerCode, string partnerName, List<AddedSubscription> addedSubsriptions)
        {


            //foreach (var i in addedSubsriptions)
            //{
            //    list.Add("Entity: " + i.Entity);
            //    list.Add("Partner Type: " + i.PartnerType);
            //    list.Add("Solution: " + i.Solution);
            //    list.Add(string.Empty);
            //}

            var adminPortal = _config.GetValue<string>("IntranetUri");
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            //notificationTemplateCode for AddSubscription
            long addSubscriptionTemplateCode = 27;

            //RecipientTypeCode for recipient, cc, bcc
            long recipientTypeCode = 1;
            long ccTypeCode = 2;
            long bccTypeCode = 3;

            var recipientInfo = await _businessProfileRepository.GetRecipientEmail(recipientTypeCode, addSubscriptionTemplateCode);
            var ccInfo = await _businessProfileRepository.GetRecipientEmail(ccTypeCode, addSubscriptionTemplateCode);
            var bccInfo = await _businessProfileRepository.GetRecipientEmail(bccTypeCode, addSubscriptionTemplateCode);

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

            var file = new List<IFormFile> { };

            StringBuilder _xml = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(_xml))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Subscription");                 // expected to match: <xsl:template match="Submission">                    
                writer.WriteElementString("PartnerName", partnerName);  // TODO: insert data for company name
                //writer.WriteElementString("SubscriptionList", subs);
                writer.WriteElementString("LoginUrl", adminPortal);     // TODO: insert data for the url endpoint
                writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
                foreach (var i in addedSubsriptions)
                {

                    var trangloEntity = await _partnerRepository.GetTrangloEntityMeta(i.Entity);
                    writer.WriteStartElement("Subscriptions");
                    writer.WriteElementString("EntityName", trangloEntity.Name.ToString());     // TODO: insert data for the EntityName
                    writer.WriteElementString("Entity", "Entity ");
                    writer.WriteElementString("PartnerType", i.PartnerType); // TODO: insert data for the PartnerType
                    writer.WriteElementString("Partner", "PartnerType");
                    writer.WriteElementString("SolutionName", i.Solution);  // TODO: insert data for the SolutionName
                    writer.WriteElementString("Solution", "Solution");
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            string content = generator.GenerateContent(_xml.ToString(), "AddSubscriptionTemplate", cultureName);
            string body = content;

            Result<HttpStatusCode> sendSubscriptionEmailResponse = await _notificationService.SendNotification
            (
                recipients,
                bcc,
                cc,
                file,
                $"Subscription Added for {partnerName}",
                body,
                NotificationTypes.Email
            );

            if (sendSubscriptionEmailResponse.IsFailure)
            {
                _logger.LogError("SendNotification", $"Add Subscription email notification failed for {partnerName} . {sendSubscriptionEmailResponse.Error}.");
            }
            _logger.LogInformation($"Subscription Added for [{partnerName}]");

            return Result.Success();
        }
    }
}
