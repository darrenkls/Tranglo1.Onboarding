using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Entities.Specifications.BusinessProfiles;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.BusinessProfile;
using Tranglo1.Onboarding.Application.Services.SignalR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCBusinessProfile, UACAction.Edit)]
    [Permission(Permission.KYCManagementBusinessProfile.Action_Edit_Code,
        new int[] { (int)PortalCode.Connect, (int)PortalCode.Admin, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementBusinessProfile.Action_View_Code })]
    internal class UpdateBusinessProfileCommand : BaseCommand<Result<BusinessProfileOutputDTO>>
    {
        public string LoginId { get; set; }
        public int BusinessProfileCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegistrationName { get; set; }
        public string TradeName { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public long? WorkFlowStatusCode { get; set; }
        public long? SolutionCode { get; set; }
        public string CompanyRegisteredAddress { get; set; }
        public string CompanyRegisteredZipCodePostCode { get; set; }
        public string CompanyRegisteredCountryISO2 { get; set; }
        public string MailingAddress { get; set; }
        public string MailingZipCodePostCode { get; set; }
        public string MailingCountryISO2 { get; set; }
        public long? BusinessNatureCode { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public string CompanyRegistrationNo { get; set; }
        public int? NumberOfBranches { get; set; }
        public string ContactNumber { get; set; }
        public string Website { get; set; }
        public bool? IsCompanyListed { get; set; }
        public string StockExchangeName { get; set; }
        public string StockCode { get; set; }

        public bool? IsMoneyTransferRemittance { get; set; }
        public bool? IsForeignCurrencyExchange { get; set; }
        public bool? IsRetailCommercialBankingServices { get; set; }
        public bool? IsForexTrading { get; set; }
        public bool? IsEMoneyEwallet { get; set; }
        public bool? IsIntermediataryRemittance { get; set; }
        public bool? IsCryptocurrency { get; set; }
        public bool? IsOther { get; set; }
        public string OtherReason { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public long? KYCInformationStatusId { get; set; }
        public string DialCode { get; set; }
        public string ContactNumberCountryISO2 { get; set; }


        public string FormerRegisteredCompanyName { get; set; }
        public string ForOthers { get; set; }
        public long? EntityTypeCode { get; set; }
        public long? RelationshipTieUpCode { get; set; }
        public long? IncorporationCompanyTypeCode { get; set; }
        public string TaxIdentificationNo { get; set; }
        public string FacsimileDialCode { get; set; }
        public string FacsimileNumber { get; set; }
        public string FacsimileNumberCountryISO2 { get; set; }

        public string TelephoneDialCode { get; set; }
        public string TelephoneNumber { get; set; }
        public string TelephoneNumberCountryISO2 { get; set; }

        public string ContactPersonName { get; set; }
        public string ContactEmailAddress { get; set; }

        //phase 3 changes
        public long? CustomerTypeCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? BusinessProfileIDTypeCode { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public long? ServiceTypeCode { get; set; }
        public long? CollectionTierCode { get; set; }
        public string AliasName { get; set; }
        public string NationalityISO2 { get; set; }
        public bool? IsMicroEnterprise { get; set; }

        //Solutions
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        //Concurrency Token
        public Guid? BusinessProfileConcurrencyToken { get; set; }

        //Ticket 55839
        public string SSTRegistrationNumber { get; set; }
        public string SenderCity { get; set; }

        //TBT- 1322 
        public bool FromComment { get; set; }

        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }

        public override Task<string> GetAuditLogAsync(Result<BusinessProfileOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update Business Profile for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdateBusinessProfileCommandHandler : IRequestHandler<UpdateBusinessProfileCommand, Result<BusinessProfileOutputDTO>>
    {
        private readonly TrangloUserManager _userManager;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IPartnerRepository _partnerRepository;
        private readonly PartnerService _partnerService;
        private readonly ILogger<UpdateBusinessProfileCommandHandler> _logger;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly SignalRMessageService _signalRNotificationService;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IConfiguration _config;

        public UpdateBusinessProfileCommandHandler(
                TrangloUserManager userManager,
                BusinessProfileService businessProfileService,
                IPartnerRepository partnerRepository,
                PartnerService partnerService,
                ILogger<UpdateBusinessProfileCommandHandler> logger,
                IBusinessProfileRepository businessProfileRepository,
                SignalRMessageService signalRNotificationService,
                IApplicationUserRepository applicationUserRepository,
                IConfiguration config
            )
        {
            _userManager = userManager;
            _businessProfileService = businessProfileService;
            _partnerRepository = partnerRepository;
            _partnerService = partnerService;
            _logger = logger;
            _businessProfileRepository = businessProfileRepository;
            _signalRNotificationService = signalRNotificationService;
            _applicationUserRepository = applicationUserRepository;
            _config = config;
        }

        public async Task<Result<BusinessProfileOutputDTO>> Handle(UpdateBusinessProfileCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            if (applicationUser is CustomerUser customerUser)
            {
                Result<IReadOnlyList<CustomerUserBusinessProfile>> customerUserBusinessProfiles = await _businessProfileService
                                                                                                      .GetCustomerUserBusinessProfilesAsync(
                                                                                                          customerUser,
                                                                                                          request.BusinessProfileCode
                                                                                                      );
                if (customerUserBusinessProfiles.IsFailure)
                {
                    _logger.LogError("GetCustomerUserBusinessProfilesAsync", customerUserBusinessProfiles.Error);

                    return Result.Failure<BusinessProfileOutputDTO>(
                                $"Get Business Profile failed for {request.BusinessProfileCode}. {customerUserBusinessProfiles.Error}"
                            );
                }
                IReadOnlyList<CustomerUserBusinessProfile> customerUserBusinessProfileList = customerUserBusinessProfiles.Value;
                CustomerUserBusinessProfile customerUserBusinessProfile = customerUserBusinessProfileList.TryFirst().Value;
                request.BusinessProfileCode = customerUserBusinessProfile.BusinessProfileCode;
            }

            if (!string.IsNullOrWhiteSpace(request.TelephoneNumber))
            {
                request.TelephoneNumber = request.TelephoneNumber.Replace(" ", string.Empty);
            }

            //if (!string.IsNullOrWhiteSpace(request.FacsimileNumber))
            //{
            //    request.FacsimileNumber = request.FacsimileNumber.Replace(" ", string.Empty);
            //}

            if (!string.IsNullOrWhiteSpace(request.ContactNumber))
            {
                request.ContactNumber = request.ContactNumber.Replace(" ", string.Empty);
            }

            var businessProfilesResult = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
            if (businessProfilesResult.IsFailure)
            {
                return Result.Failure<BusinessProfileOutputDTO>(
                            $"No record found for {request.BusinessProfileCode}."
                        );
            }
            else if (!String.IsNullOrWhiteSpace(request.CompanyName) && businessProfilesResult.Value.CompanyName != request.CompanyName)
            {
                //duplicate checking on new company name
                var _isExistingCompanyName = await _businessProfileService.CheckIsExistingCompanyNameAsync(request.CompanyName);
                if (_isExistingCompanyName.isInUsed)
                {
                    return Result.Failure<BusinessProfileOutputDTO>(
                           $"Company Name Already Exists."
                       );
                }
            }

            BusinessProfile businessProfile = businessProfilesResult.Value;
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            Result<BusinessProfileOutputDTO> result = new Result<BusinessProfileOutputDTO>();

            // Handle concurrency
            var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

            if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
            {
                var concurrencyCheck = ConcurrencyCheck(request.BusinessProfileConcurrencyToken, businessProfile);
                if (concurrencyCheck.IsFailure)
                {
                    return Result.Failure<BusinessProfileOutputDTO>(concurrencyCheck.Error);
                }
            }

            // CurrentCustomerType value needed for change customer type handling
            long? currentCustomerTypeCode = 0;

            if (request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id)
            {
                currentCustomerTypeCode = null;
            }
            if (request.CustomerSolution == ClaimCode.Business || request.AdminSolution == Solution.Business.Id)
            {
                currentCustomerTypeCode = partnerRegistrationInfo.CustomerType.Id;
            }

            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var kycReviewResultConnect = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_BusinessProfile.Id);
            var kycReviewResultBusiness = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_BusinessProfile.Id);
            var partnerType = Enumeration.FindById<PartnerType>(partnerRegistrationInfo.PartnerTypeCode.GetValueOrDefault());
            var partnerUser = await _partnerRepository.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var solutionByPartnerSubscription = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerUser.Id);

            List<PartnerSubscription> partnerSubs = new List<PartnerSubscription>();
            foreach (var solution in solutionByPartnerSubscription)
            {
                partnerSubs.Add(solution);
            }

            var businessSolutionListItem = partnerSubs.Where(x => x.Solution.Id == Solution.Business.Id).FirstOrDefault();

            var businessNatureDescription = await _businessProfileRepository.GetBusinessNatureByCodeAsync(request.BusinessNatureCode);

            if (request.AdminSolution != null || request.CustomerSolution != null)
            {
                if (Solution.Connect.Id == request.AdminSolution)
                {
                    if (applicationUser is TrangloStaff &&
                            ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                            businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || kycReviewResultConnect == ReviewResult.Complete
                            || kycReviewResultBusiness == ReviewResult.Complete || businessSolutionListItem.Solution == Solution.Business))
                    {
                        result = await UpdateBusinessProfile(request, businessProfile);

                        if (result.IsFailure)
                        {
                            return Result.Failure<BusinessProfileOutputDTO>(
                                                $"{result.Error}"
                                                );
                        }

                        //check mandatory fields
                        await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_BusinessProfile);

                        var deleteAMLCFT = await DeleteAMLCFTWithChecking(request, businessProfile, cancellationToken);

                        if (deleteAMLCFT.IsFailure)
                        {
                            return Result.Failure<BusinessProfileOutputDTO>(
                                                $"Failed to delete AMLCFT Questionnaire and Questionnaire Answers for {request.BusinessProfileCode}. {deleteAMLCFT.Error}"
                                                );
                        }
                    }
                }
                else if (Solution.Business.Id == request.AdminSolution)
                {
                    //Delete Transaction Evaluation Answers if Customer Type is different
                    CustomerType newCustomerType = Enumeration.FindById<CustomerType>(request.CustomerTypeCode.GetValueOrDefault());
                    CustomerType oldCustomerType = Enumeration.FindById<CustomerType>(partnerRegistrationInfo.CustomerTypeCode.GetValueOrDefault());
                    await this.DeleteTransactionEvaluation(businessProfile, oldCustomerType, newCustomerType);

                    result = await UpdateBusinessProfile(request, businessProfile);

                    if (result.IsFailure)
                    {
                        return Result.Failure<BusinessProfileOutputDTO>(
                                            $"{result.Error}"
                                            );
                    }

                    //check mandatory fields
                    await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_BusinessProfile);

                    var deleteAMLCFT = await DeleteAMLCFTWithChecking(request, businessProfile, cancellationToken);

                    if (deleteAMLCFT.IsFailure)
                    {
                        return Result.Failure<BusinessProfileOutputDTO>(
                                            $"Failed to delete AMLCFT Questionnaire and Questionnaire Answers for {request.BusinessProfileCode}. {deleteAMLCFT.Error}"
                                            );
                    }
                }
                else if (ClaimCode.Connect == request.CustomerSolution)
                {
                    if (applicationUser is CustomerUser && (businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft || businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted) && (kycReviewResultConnect == ReviewResult.Insufficient_Incomplete || kycReviewResultBusiness == ReviewResult.Insufficient_Incomplete))
                    {
                        var deleteAMLCFT = await DeleteAMLCFTWithChecking(request, businessProfile, cancellationToken);

                        if (deleteAMLCFT.IsFailure)
                        {
                            return Result.Failure<BusinessProfileOutputDTO>(
                                                $"Failed to delete AMLCFT Questionnaire and Questionnaire Answers for {request.BusinessProfileCode}. {deleteAMLCFT.Error}"
                                                );
                        }

                        result = await UpdateBusinessProfile(request, businessProfile);

                        if (result.IsFailure)
                        {
                            return Result.Failure<BusinessProfileOutputDTO>(
                                                $"{result.Error}"
                                                );
                        }
                    }
                }
                else if (ClaimCode.Business == request.CustomerSolution)
                {
                    var deleteAMLCFT = await DeleteAMLCFTWithChecking(request, businessProfile, cancellationToken);

                    if (deleteAMLCFT.IsFailure)
                    {
                        return Result.Failure<BusinessProfileOutputDTO>(
                                            $"Failed to delete AMLCFT Questionnaire and Questionnaire Answers for {request.BusinessProfileCode}. {deleteAMLCFT.Error}"
                                            );
                    }

                    result = await UpdateBusinessProfile(request, businessProfile);

                    if (result.IsFailure)
                    {
                        return Result.Failure<BusinessProfileOutputDTO>(
                                            $"{result.Error}"
                                            );
                    }

                    await MarkKYCSummaryNotificationsAsReadAsync(request.BusinessProfileCode,
                        KYCCategory.Business_BusinessProfile.Id,
                        cancellationToken);

                    if (request.FromComment)
                    {
                        var kycSummaryFeedbackInfo = await _businessProfileRepository.GetListKYCSummaryFeedbackByBusinessProfileCodeAsync(request.BusinessProfileCode);

                        foreach (var i in kycSummaryFeedbackInfo)
                        {
                            if (i.IsResolved == false && i.KYCCategory.Id == KYCCategory.Business_BusinessProfile.Id)
                            {
                                i.IsResolved = true; //set isResolved to true
                                await _businessProfileRepository.SaveKYCSummaryFeedback(i);
                            }
                        }
                    }
                }
                else
                {
                    return Result.Failure<BusinessProfileOutputDTO>(
                                                $"User is unable to update for {request.BusinessProfileCode}."
                                                );
                }

                // Change Customer Type handling
                if (request.CustomerTypeCode.HasValue)
                {
                    var newCustomerTypeCode = request.CustomerTypeCode.Value;

                    var changeCustomerTypeHandling = await _businessProfileService.ChangeCustomerTypeHandling(request.BusinessProfileCode, newCustomerTypeCode, currentCustomerTypeCode, applicationUser.Id);
                    if (changeCustomerTypeHandling.IsFailure)
                    {
                        return Result.Failure<BusinessProfileOutputDTO>(
                                            $"Failed to change Customer Type for BusinessProfileCode: {request.BusinessProfileCode}. {changeCustomerTypeHandling.Error}"
                                            );
                    }
                    else if (changeCustomerTypeHandling.Value is true) // returns true if redo declaration
                    {
                        // Broadcast redo log off alert
                        await _signalRNotificationService.RedoBusinessDeclarationLogOffAlert(request.BusinessProfileCode);
                    }
                    else if (changeCustomerTypeHandling.Value is false) // returns false if non redo declaration
                    {
                        // Broadcast non redo log off alert
                        await _signalRNotificationService.NonRedoBusinessDeclarationLogOffAlert(request.BusinessProfileCode);
                    }
                }


            }
            else
            {
                return Result.Failure<BusinessProfileOutputDTO>($"Solution Code passed is NULL for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
            }

            if (businessSolutionListItem != null)
            {
                if (businessSolutionListItem.PartnerType != null)
                {
                    var partnerProfileChangedEvent = new PartnerProfileChangedEvent(
                        businessSolutionListItem.TrangloEntity,  //TrangloEntity
                        businessSolutionListItem.Solution.Id,    //SolutionCode
                        businessSolutionListItem.PartnerType.Id,    //PartnerTypeCode
                        partnerUser.Id, //PartnerCode
                        partnerUser.PartnerId,  //PartnerId
                        businessSolutionListItem.Id,     //PartnerSubscriptionCode get from subscription
                        businessSolutionListItem.SettlementCurrencyCode, //SettlementCurrencyCode get from subscription
                        businessProfilesResult.Value.CompanyRegisteredCountryMeta.CountryISO2, //CustomerIncorporationCountry
                        businessProfilesResult.Value.CompanyName,//CompanyName
                        businessProfilesResult.Value.CompanyRegistrationNo, //CompanyRegistrationNo
                        businessProfilesResult.Value.CompanyRegisteredAddress, //CompanyRegisteredAddress
                        businessProfilesResult.Value.IDExpiryDate,  //IDExpiryDate
                        businessProfilesResult.Value.DateOfBirth, //DateOfBirth
                        businessProfilesResult.Value.TelephoneNumber, //TelephoneNumber
                        partnerUser.Email,  //Email
                        businessProfilesResult.Value.CompanyRegisteredZipCodePostCode, //CompanyRegisteredZipCodePostCode
                        businessProfilesResult.Value.BusinessNature.Id, //BusinessNatureCode
                        businessNatureDescription.Name, //BusinessNatureDescription
                        businessSolutionListItem.Environment.Id,  //EnvironmentCode
                        businessProfilesResult.Value.DateOfIncorporation,
                        businessProfilesResult.Value.IncorporationCompanyTypeCode,
                        partnerUser.CustomerTypeCode,
                        businessProfilesResult.Value.CollectionTier.Id,
                        businessProfilesResult.Value.Id
                        );

                    var partnerDetailsEvents = await _partnerRepository.AddPartnerOnboardingCreationEventAsync(partnerProfileChangedEvent);
                    if (partnerDetailsEvents.IsFailure)
                    {
                        _logger.LogError("Failed to save partner profile events");
                        return Result.Failure<BusinessProfileOutputDTO>($"Saving Business Profile Event Failed.");
                    }
                }
            }

            return result;
        }

        async Task<Result<BusinessProfileOutputDTO>> UpdateBusinessProfile(UpdateBusinessProfileCommand request, BusinessProfile businessProfile)
        {
            var partnerProfile = await _partnerRepository.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var customerType = partnerProfile.CustomerTypeCode;
            var partnerSolutionBySubscription = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerProfile.Id);
            var collectionTier = await _businessProfileRepository.GetCollectionTierByCode(businessProfile.CollectionTier.Id);
            var titleCode = await _businessProfileRepository.GetTitleTypeByCode(request.TitleCode);

            //var customerUser = await _applicationUserRepository.GetCustomerUserByUserId(customerUserBusinessProfile.UserId);


            //53583 Collection Tier issues in Tranglo Business
            if (request.ServiceTypeCode == ServiceType.Collection_Payout.Id)
            {
                collectionTier = businessProfile.CollectionTier;
            }

            //Add checking on DateOfIncorporation date
            if (request.DateOfIncorporation > DateTime.Now)
            {
                return Result.Failure<BusinessProfileOutputDTO>(
                       $"Future dates {request.DateOfIncorporation} are not allowed for Date of Incorporation."
                   );
            }

            Result<ContactNumber> createContactNumber = string.IsNullOrWhiteSpace(request.ContactNumber)/* || string.IsNullOrWhiteSpace(request.DialCode)*/ ? null : ContactNumber.Create(request.DialCode, request.ContactNumberCountryISO2, request.ContactNumber);
            if (createContactNumber.IsFailure)
            {
                return Result.Failure<BusinessProfileOutputDTO>(
                            $"Create contact number failed for {request.ContactNumber}. {createContactNumber.Error}"
                        );
            }

            Result<ContactNumber> createTelephoneNumber = string.IsNullOrWhiteSpace(request.TelephoneNumber) /*|| string.IsNullOrWhiteSpace(request.TelephoneDialCode)*/ ? null : ContactNumber.Create(request.TelephoneDialCode, request.TelephoneNumberCountryISO2, request.TelephoneNumber);
            if (createTelephoneNumber.IsFailure)
            {
                return Result.Failure<BusinessProfileOutputDTO>(
                            $"Create contact number failed for {request.TelephoneNumber}. {createTelephoneNumber.Error}"
                        );
            }

            //Result<ContactNumber> createFacsimileNumber = string.IsNullOrWhiteSpace(request.FacsimileNumber) /*|| string.IsNullOrWhiteSpace(request.FacsimileDialCode)*/ ? null : ContactNumber.Create(request.FacsimileDialCode, request.FacsimileNumberCountryISO2, request.FacsimileNumber);
            //if (createFacsimileNumber.IsFailure)
            //{
            //    return Result.Failure<BusinessProfileOutputDTO>(
            //                $"Create contact number failed for {request.FacsimileNumber}. {createFacsimileNumber.Error}"
            //            );
            //
            //
            //}

            var companyRegisteredCountry = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(request.CompanyRegisteredCountryISO2);
            var mailingCountryCode = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(request.MailingCountryISO2);
            var nationalityCode = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(request.NationalityISO2);

            {
                businessProfile.RegistrationDate = request.RegistrationDate.HasValue ? request.RegistrationDate.Value : businessProfile.RegistrationDate;
                businessProfile.SolutionCode = request.SolutionCode.HasValue ? request.SolutionCode.Value : businessProfile.SolutionCode;
                businessProfile.CompanyName = (request.CompanyName is null) ? businessProfile.CompanyName : request.CompanyName;
                businessProfile.CompanyRegistrationName = (request.CompanyRegistrationName is null) ? businessProfile.CompanyRegistrationName : request.CompanyRegistrationName;

                if (ClaimCode.Connect == request.CustomerSolution || Solution.Connect.Id == request.AdminSolution)
                    businessProfile.TradeName = (request.TradeName is null) ? businessProfile.TradeName : request.TradeName;

                businessProfile.CompanyRegisteredAddress = (request.CompanyRegisteredAddress is null) ? businessProfile.CompanyRegisteredAddress : request.CompanyRegisteredAddress;
                businessProfile.CompanyRegisteredZipCodePostCode = (request.CompanyRegisteredZipCodePostCode is null) ? businessProfile.CompanyRegisteredZipCodePostCode : request.CompanyRegisteredZipCodePostCode;

                var entityType = await _businessProfileRepository.GetEntityTypeByCodeAsync(request.EntityTypeCode);

                var relationshipTieUp = await _businessProfileRepository.GetRelationshipTieUpByCodeAsync(request.RelationshipTieUpCode);

                var incorporationCompanyType = await _businessProfileRepository.GetIncorporationCompanyTypeByCodeAsync(request.IncorporationCompanyTypeCode);

                var countryCompanyRegistered = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(request.CompanyRegisteredCountryISO2);
                if (!String.IsNullOrEmpty(request.CompanyRegisteredCountryISO2) && countryCompanyRegistered == null)
                {
                    return Result.Failure<BusinessProfileOutputDTO>($"Company Registered Country Code is not valid: {request.CompanyRegisteredCountryISO2}");
                }
                businessProfile.CompanyRegisteredCountryCode = countryCompanyRegistered?.Id;


                businessProfile.MailingAddress = (request.MailingAddress is null) ? businessProfile.MailingAddress : request.MailingAddress;
                businessProfile.MailingZipCodePostCode = (request.MailingZipCodePostCode is null) ? businessProfile.MailingZipCodePostCode : request.MailingZipCodePostCode;


                var countryMailingCountryCode = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(request.MailingCountryISO2);
                if (!String.IsNullOrEmpty(request.MailingCountryISO2) && countryMailingCountryCode == null)
                {
                    return Result.Failure<BusinessProfileOutputDTO>($"Mailing Country Code is not valid: {request.MailingCountryISO2}");
                }
                businessProfile.MailingCountryCode = countryMailingCountryCode?.Id;

                var businessNature = await _businessProfileRepository.GetBusinessNatureByCodeAsync(request.BusinessNatureCode);
                businessProfile.DateOfIncorporation = request.DateOfIncorporation;

                businessProfile.CompanyRegistrationNo = (request.CompanyRegistrationNo is null) ? businessProfile.CompanyRegistrationNo : request.CompanyRegistrationNo;
                businessProfile.NumberOfBranches = request.NumberOfBranches.HasValue ? request.NumberOfBranches : businessProfile.NumberOfBranches;
                businessProfile.ContactNumber = createContactNumber.Value;
                businessProfile.Website = (request.Website is null) ? businessProfile.Website : request.Website;
                businessProfile.IsCompanyListed = request.IsCompanyListed;
                businessProfile.StockExchangeName = request.StockExchangeName;
                businessProfile.StockCode = request.StockCode;

                // New Changes to services offered
                businessProfile.IsOther = request.IsOther ?? false;
                if (businessProfile.IsOther is true)
                {
                    businessProfile.OtherReason = request.OtherReason;
                }
                else
                {
                    businessProfile.OtherReason = null;
                }
                businessProfile.IsMoneyTransferRemittance = request.IsMoneyTransferRemittance ?? false;
                businessProfile.IsForeignCurrencyExchange = request.IsForeignCurrencyExchange ?? false;
                businessProfile.IsRetailCommercialBankingServices = request.IsRetailCommercialBankingServices ?? false;
                businessProfile.IsForexTrading = request.IsForexTrading ?? false;
                businessProfile.IsEMoneyEwallet = request.IsEMoneyEwallet ?? false;
                businessProfile.IsIntermediataryRemittance = request.IsIntermediataryRemittance ?? false;
                businessProfile.IsCryptocurrency = request.IsCryptocurrency ?? false;

                if (ClaimCode.Connect == request.CustomerSolution || Solution.Connect.Id == request.AdminSolution)
                    businessProfile.FormerRegisteredCompanyName = request.FormerRegisteredCompanyName;

                businessProfile.SetContactPersonName(request.ContactPersonName, customerType);// ContactPersonName = request.ContactPersonName;
                businessProfile.IncorporationCompanyType = incorporationCompanyType;

                if (entityType != null)
                {
                    businessProfile.EntityTypeCode = entityType.Id;
                }
                else
                {
                    businessProfile.EntityTypeCode = null;
                }

                var serviceType = await _businessProfileRepository.GetServiceTypeByCode(request.ServiceTypeCode);
                var businessProfileIdType = await _businessProfileRepository.GetBusinessProfileIDTypeByCode(request.BusinessProfileIDTypeCode);
                var servicesOffered = await _businessProfileRepository.GetServicesOfferedByCodeAsync(request.ServiceTypeCode);

                var nationality = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(request.NationalityISO2);

                // Validate TaxIdentificationNo only when the registered country is Malaysia (MY)
                if (companyRegisteredCountry.CountryISO2 == CountryMeta.Malaysia.CountryISO2)
                {
                    if (!string.IsNullOrEmpty(request.TaxIdentificationNo))
                    {
                        // Validate that TaxIdentificationNo is alphanumeric
                        if (!System.Text.RegularExpressions.Regex.IsMatch(request.TaxIdentificationNo, "^[a-zA-Z0-9]+$"))
                        {
                            return Result.Failure<BusinessProfileOutputDTO>("Tax Identification Number must be alphanumeric.");
                        }

                        // Assign only when valid and filled
                        businessProfile.TaxIdentificationNo = request.TaxIdentificationNo;
                    }
                    else
                    {

                        businessProfile.TaxIdentificationNo = null;
                    }
                }
                else
                {
                    // For non-Malaysia countries, assign TIN directly without validation
                    businessProfile.TaxIdentificationNo = request.TaxIdentificationNo;
                }


                //businessProfile.FacsimileNumber = createFacsimileNumber.Value;
                businessProfile.TelephoneNumber = createTelephoneNumber.Value;
                businessProfile.ForOthers = request.ForOthers;

                if ((customerType == CustomerType.Individual.Id || request.CustomerTypeCode == CustomerType.Individual.Id) && collectionTier?.Id == null)
                {

                    var collectedTier = await _businessProfileRepository.GetCollectionTierByCode(CollectionTier.Tier_1.Id);
                    businessProfile.CollectionTier = collectedTier;
                }
                else
                {
                    businessProfile.CollectionTier = collectionTier;
                }



                //Phase 3 Changes
                businessProfile.BusinessProfileIDType = businessProfileIdType;
                businessProfile.DateOfBirth = request.DateOfBirth;
                businessProfile.IDNumber = request.IDNumber;
                businessProfile.IDExpiryDate = request.IDExpiryDate;
                businessProfile.ServiceType = serviceType;
                businessProfile.AliasName = request.AliasName;
                businessProfile.NationalityCode = nationality?.Id;
                businessProfile.IsMicroEnterprise = request.IsMicroEnterprise;
                businessProfile.BusinessNature = businessNature;

                if (ClaimCode.Connect == request.CustomerSolution || Solution.Connect.Id == request.AdminSolution)
                    businessProfile.RelationshipTieUp = relationshipTieUp;

                businessProfile.IncorporationCompanyType = incorporationCompanyType;

                businessProfile.CompanyRegisteredCountryMeta = companyRegisteredCountry;
                businessProfile.MailingCountryMeta = mailingCountryCode;
                businessProfile.NationalityMeta = nationalityCode;

                //Ticket 55839
                businessProfile.SSTRegistrationNumber = request.SSTRegistrationNumber;
                businessProfile.SenderCity = request.SenderCity;

                businessProfile.Title = titleCode;
                businessProfile.TitleOthers = request.TitleOthers;
            }
            ;

            var UpdateBusinessProfileResp = await _businessProfileService.UpdateBusinessProfileAsync(businessProfile);

            //Email Input
            var email = string.IsNullOrWhiteSpace(request.ContactEmailAddress) ? null : Email.Create(request.ContactEmailAddress).Value;
            partnerProfile.Email = email;

            if (request.CustomerTypeCode.HasValue)
            {
                customerType = request.CustomerTypeCode;
            }

            partnerProfile.CustomerTypeCode = customerType;

            var UpdatePartnerRegistration = await _partnerService.UpdatePartnerRegistrationAsync(partnerProfile);

            if (UpdateBusinessProfileResp == null)
            {
                return Result.Failure<BusinessProfileOutputDTO>(
                            $"Update Business Profile failed for {request.BusinessProfileCode}."
                        );
            }

            if (request.BusinessNatureCode != BusinessNature.Other.Id && businessProfile.ForOthers != null)
            {
                businessProfile.ForOthers = null;
            }

            var result = new BusinessProfileOutputDTO()
            {
                BusinessProfileCode = businessProfile.Id,
                CompanyName = businessProfile.CompanyName,
                CompanyRegistrationName = businessProfile.CompanyRegistrationName,
                TradeName = businessProfile.TradeName,
                CompanyRegisteredAddress = businessProfile.CompanyRegisteredAddress,
                CompanyRegisteredZipCodePostCode = businessProfile.CompanyRegisteredZipCodePostCode,
                CompanyRegisteredCountryISO2 = businessProfile.CompanyRegisteredCountryMeta?.CountryISO2 ?? null,
                MailingAddress = businessProfile.MailingAddress,
                MailingZipCodePostCode = businessProfile.MailingZipCodePostCode,
                MailingCountryISO2 = businessProfile.MailingCountryMeta?.CountryISO2 ?? null,
                BusinessNatureCode = businessProfile.BusinessNature.Id,
                DateOfIncorporation = businessProfile.DateOfIncorporation,
                CompanyRegistrationNo = businessProfile.CompanyRegistrationNo,
                NumberOfBranches = businessProfile.NumberOfBranches,
                ContactNumber = businessProfile.ContactNumber != null ? businessProfile.ContactNumber.Value : null,
                DialCode = businessProfile.ContactNumber != null ? businessProfile.ContactNumber.DialCode : null,
                ContactNumberCountryISO2 = businessProfile.ContactNumber != null ? businessProfile.ContactNumber.CountryISO2Code : null,
                Website = businessProfile.Website,
                IsCompanyListed = businessProfile.IsCompanyListed,
                StockExchangeName = businessProfile.StockExchangeName,
                StockCode = businessProfile.StockCode,
                IsMoneyTransferRemittance = businessProfile.IsMoneyTransferRemittance,
                IsForeignCurrencyExchange = businessProfile.IsForeignCurrencyExchange,
                IsRetailCommercialBankingServices = businessProfile.IsRetailCommercialBankingServices,
                IsForexTrading = businessProfile.IsForexTrading,
                IsEMoneyEwallet = businessProfile.IsEMoneyEwallet,
                IsIntermediataryRemittance = businessProfile.IsIntermediataryRemittance,
                IsCryptocurrency = businessProfile.IsCryptocurrency,
                IsOther = businessProfile.IsOther,
                OtherReason = businessProfile.OtherReason,
                FormerRegisteredCompanyName = businessProfile.FormerRegisteredCompanyName,
                ContactPersonName = businessProfile.ContactPersonName,
                IncorporationCompanyTypeCode = businessProfile.IncorporationCompanyTypeCode ?? null,
                RelationshipTieUpCode = businessProfile.RelationshipTieUpCode ?? null,
                EntityTypeCode = businessProfile.EntityTypeCode,
                ForOthers = businessProfile.ForOthers,
                TaxIdentificationNo = businessProfile.TaxIdentificationNo,
                TelephoneNumber = businessProfile.TelephoneNumber != null ? businessProfile.TelephoneNumber.Value : null,
                TelephoneDialCode = businessProfile.TelephoneNumber != null ? businessProfile.TelephoneNumber.DialCode : null,
                TelephoneNumberCountryISO2 = businessProfile.TelephoneNumber != null ? businessProfile.TelephoneNumber.CountryISO2Code : null,
                FacsimileNumber = businessProfile.FacsimileNumber != null ? businessProfile.FacsimileNumber.Value : null,
                FacsimileDialCode = businessProfile.FacsimileNumber != null ? businessProfile.FacsimileNumber.DialCode : null,
                FacsimileNumberCountryISO2 = businessProfile.FacsimileNumber != null ? businessProfile.FacsimileNumber.CountryISO2Code : null,
                ContactEmailAddress = partnerProfile.Email?.Value,
                //phase 3 changes
                CustomerTypeCode = partnerProfile.CustomerTypeCode,
                DateOfBirth = businessProfile.DateOfBirth ?? null,
                BusinessProfileIDTypeCode = businessProfile.BusinessProfileIDType?.Id ?? null,
                IDNumber = businessProfile.IDNumber ?? null,
                IDExpiryDate = businessProfile.IDExpiryDate ?? null,
                ServiceTypeCode = businessProfile.ServiceType?.Id ?? null,
                CollectionTierCode = businessProfile.CollectionTier?.Id,
                AliasName = businessProfile.AliasName ?? null,
                NationalityISO2 = businessProfile.NationalityMeta?.CountryISO2 ?? null,
                IsMicroEnterprise = businessProfile.IsMicroEnterprise,
                BusinessProfileConcurrencyToken = businessProfile.BusinessProfileConcurrencyToken,
                //Ticket 55839
                SSTRegistrationNumber = businessProfile.SSTRegistrationNumber ?? null,
                SenderCity = businessProfile.SenderCity ?? null,
                TitleCode = titleCode?.Id,
                TitleOthers = businessProfile.TitleOthers
            };

            //add update business profile event here

            return Result.Success(result);
        }

        private async Task<Result> DeleteAMLCFTQuestionnaireAndQuestionnaireAnswers(BusinessProfile businessProfile,
                                                                                        CancellationToken cancellationToken,
                                                                                        List<AMLCFTQuestionnaire> aMLCFTQuestionnaires,
                                                                                        List<AMLCFTQuestionnaireAnswer> aMLCFTQuestionnaireAnswers,
                                                                                        List<long> ignoreQuestionnaire)
        {
            IEnumerable<AMLCFTQuestionnaire> existingAMLCFTQuestionnaires;
            IEnumerable<AMLCFTQuestionnaireAnswer> existingAMLCFTQuestionnaireAnswers;

            existingAMLCFTQuestionnaires = await _businessProfileRepository.GetAMLCFTQuestionnairesByBusinessProfileWithIgnoreQuestionnaireAsync(businessProfile, ignoreQuestionnaire);
            existingAMLCFTQuestionnaireAnswers = await _businessProfileRepository.GetAMLCFTQuestionnaireAnswersByBusinessProfileWithIgnoreQuestionnaireAsync(businessProfile, ignoreQuestionnaire);


            var deletedAMLCFTQuestionnaires = from existing in existingAMLCFTQuestionnaires
                                              let fromInput = aMLCFTQuestionnaires
                                              .FirstOrDefault(input =>
                                                  input.Id == existing.Id
                                                  )
                                              where fromInput == null
                                              select existing;

            var deletedAMLCFTQuestionnaireAnswers = from existing in existingAMLCFTQuestionnaireAnswers
                                                    let fromInput = aMLCFTQuestionnaireAnswers
                                                    .FirstOrDefault(input =>
                                                        input.Id == existing.Id
                                                        )
                                                    where fromInput == null
                                                    select existing;

            if (deletedAMLCFTQuestionnaireAnswers.Any())
                await _businessProfileService.DeleteAMLCFTQuestionnaireAnswersAsync(deletedAMLCFTQuestionnaireAnswers, businessProfile, cancellationToken);

            if (deletedAMLCFTQuestionnaires.Any())
                await _businessProfileRepository.DeleteAMLCFTQuestionnairesAsync(deletedAMLCFTQuestionnaires, cancellationToken);

            return Result.Success();
        }

        private async Task<Result> DeleteAMLCFTWithChecking(UpdateBusinessProfileCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
            // Variables for AMLCFT deletion
            var newEntityType = request.EntityTypeCode;
            var newRelationshipTieUp = request.RelationshipTieUpCode;
            List<ServicesOffered> servicesOfferedNew = new List<ServicesOffered>();
            List<ServicesOffered> servicesOfferedOld = new List<ServicesOffered>();
            var entityTypeNew = new EntityType();
            var entityTypeOld = new EntityType();
            var tieUpNew = new RelationshipTieUp();
            var tieUpOld = new RelationshipTieUp();
            if (request.EntityTypeCode != null)
            {
                entityTypeNew = Enumeration.FindById<EntityType>((long)request.EntityTypeCode);
            }
            if (businessProfile.EntityTypeCode != null)
            {
                entityTypeOld = Enumeration.FindById<EntityType>((long)businessProfile.EntityTypeCode);
            }
            if (request.RelationshipTieUpCode != null)
            {
                tieUpNew = Enumeration.FindById<RelationshipTieUp>((long)request.RelationshipTieUpCode);
            }
            if (businessProfile.RelationshipTieUpCode != null)
            {
                tieUpOld = Enumeration.FindById<RelationshipTieUp>((long)businessProfile.RelationshipTieUpCode);
            }

            List<AMLCFTQuestionnaire> aMLCFTQuestionnaires = new List<AMLCFTQuestionnaire>();
            List<AMLCFTQuestionnaireAnswer> aMLCFTQuestionnaireAnswers = new List<AMLCFTQuestionnaireAnswer>();

            // If all input services are null or unchecked (false)
            if ((request.IsMoneyTransferRemittance == null || false) &&
                (request.IsForeignCurrencyExchange == null || false) &&
                (request.IsRetailCommercialBankingServices == null || false) &&
                (request.IsForexTrading == null || false) &&
                (request.IsEMoneyEwallet == null || false) &&
                (request.IsIntermediataryRemittance == null || false) &&
                (request.IsCryptocurrency == null || false))
            {
                servicesOfferedNew.Clear();
                servicesOfferedNew.Add(null);
            }

            if (request.IsMoneyTransferRemittance is true) { servicesOfferedNew.Add(Enumeration.FindById<ServicesOffered>(1)); }
            if (request.IsForeignCurrencyExchange is true) { servicesOfferedNew.Add(Enumeration.FindById<ServicesOffered>(2)); }
            if (request.IsRetailCommercialBankingServices is true) { servicesOfferedNew.Add(Enumeration.FindById<ServicesOffered>(3)); }
            if (request.IsForexTrading is true) { servicesOfferedNew.Add(Enumeration.FindById<ServicesOffered>(4)); }
            if (request.IsEMoneyEwallet is true) { servicesOfferedNew.Add(Enumeration.FindById<ServicesOffered>(5)); }
            if (request.IsIntermediataryRemittance is true) { servicesOfferedNew.Add(Enumeration.FindById<ServicesOffered>(6)); }
            if (request.IsCryptocurrency is true) { servicesOfferedNew.Add(Enumeration.FindById<ServicesOffered>(7)); }

            if (businessProfile.IsMoneyTransferRemittance is true) { servicesOfferedOld.Add(Enumeration.FindById<ServicesOffered>(1)); }
            if (businessProfile.IsForeignCurrencyExchange is true) { servicesOfferedOld.Add(Enumeration.FindById<ServicesOffered>(2)); }
            if (businessProfile.IsRetailCommercialBankingServices is true) { servicesOfferedOld.Add(Enumeration.FindById<ServicesOffered>(3)); }
            if (businessProfile.IsForexTrading is true) { servicesOfferedOld.Add(Enumeration.FindById<ServicesOffered>(4)); }
            if (businessProfile.IsEMoneyEwallet is true) { servicesOfferedOld.Add(Enumeration.FindById<ServicesOffered>(5)); }
            if (businessProfile.IsIntermediataryRemittance is true) { servicesOfferedOld.Add(Enumeration.FindById<ServicesOffered>(6)); }
            if (businessProfile.IsCryptocurrency is true) { servicesOfferedOld.Add(Enumeration.FindById<ServicesOffered>(7)); }

            if (newEntityType != null || newRelationshipTieUp != null)
            {
                var newCombination = await _businessProfileRepository.GetAMLCFTDisplayRuleAsync(entityTypeNew, tieUpNew, servicesOfferedNew);
                var questionnaires = await _businessProfileRepository.GetQuestionnairesByAMLCFTQuestionnairesAsync(businessProfile.Id);

                //var oldCombination = await _businessProfileRepository.GetAMLCFTDisplayRuleAsync(entityTypeOld, tieUpOld, servicesOfferedOld); 

                List<long> ignoreQuestionnaire = new List<long>();
                foreach (var n in newCombination)
                {
                    foreach (var q in questionnaires)
                    {
                        if (q.Id == n.Questionnaire?.Id)
                        {
                            ignoreQuestionnaire.Add(q.Id);
                        }
                        else
                            ignoreQuestionnaire.Add(0);
                    }
                }

                var deleteQuestionnaire = await DeleteAMLCFTQuestionnaireAndQuestionnaireAnswers
                    (businessProfile, cancellationToken, aMLCFTQuestionnaires, aMLCFTQuestionnaireAnswers, ignoreQuestionnaire);
            }

            return Result.Success();
        }


        private async Task DeleteTransactionEvaluation(BusinessProfile businessProfile, CustomerType customerTypeCurrent, CustomerType customerTypeNewChange)
        {
            if (customerTypeCurrent.CustomerTypeGroupCode != customerTypeNewChange.CustomerTypeGroupCode)
            {
                //Proceed to delete transaction evaluation answers
                var customerTransactionEvaluations = await _businessProfileRepository.GetCustomerBusinessTransactionEvaluationAnswersAsync(businessProfile.Id);
                _businessProfileRepository.DeleteCustomerBusinessTransactionEvaluationAnswersByBusinessProfileCodeAsync(customerTransactionEvaluations);
            }
        }

        private Result ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
        {
            try
            {
                if ((concurrencyToken.HasValue && businessProfile.BusinessProfileConcurrencyToken != concurrencyToken) ||
                    concurrencyToken is null && businessProfile.BusinessProfileConcurrencyToken != null)
                {
                    // Return a 409 Conflict status code when there's a concurrency issue
                    return Result.Failure<BusinessProfileOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }

                // Stamp new token
                businessProfile.BusinessProfileConcurrencyToken = Guid.NewGuid();
                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the request.");

                // Return a 409 Conflict status code
                return Result.Failure<BusinessProfileOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
            }
        }

        private async Task MarkKYCSummaryNotificationsAsReadAsync(int businessProfileCode,
            long kycCategoryCode,
            CancellationToken cancellationToken)
        {
            try
            {
                Specification<KYCSummaryFeedbackNotification> specification = new UnreadKYCSummaryFeedbackNotificationByBusinessProfileAndKYCCategory(
                    businessProfileCode,
                    kycCategoryCode
                );

                await _businessProfileRepository.UpdateKYCSummaryFeedbackNotificationsAsReadByCategoryAsync(specification, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{0}]", nameof(UpdateBusinessProfileCommandHandler));
            }
        }
    }
}