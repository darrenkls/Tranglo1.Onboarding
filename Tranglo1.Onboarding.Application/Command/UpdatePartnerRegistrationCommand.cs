using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerRegistration;
using Tranglo1.Onboarding.Application.Managers;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.Onboarding.Application.Services.SignalR;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerDetails, UACAction.Edit)]
    [Permission(Permission.ManagePartnerPartnerDetails.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerPartnerDetails.Action_View_Code })]
    internal class UpdatePartnerRegistrationCommand : BaseCommand<Result<UpdatePartnerRegistrationCommandOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public UpdatePartnerInputDTO UpdatePartnerRegistration;
        public string UserBearerToken { get; set; }
        //public long? CustomerTypeCode { get; set; }
        //public string NationalityISO2 { get; set; }
        //public string RegisteredCompanyName { get; set; } 
        //public string PersonInChargeName { get; set; }
        //public string AliasName { get; set; }
        //public long? RelationshipTieUpCode { get; set; }
        //public string FormerRegisteredCompanyName { get; set; }
        //public string FormerName { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }

        public override Task<string> GetAuditLogAsync(Result<UpdatePartnerRegistrationCommandOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Edited partner details";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdatePartnerRegistrationCommandHandler : IRequestHandler<UpdatePartnerRegistrationCommand, Result<UpdatePartnerRegistrationCommandOutputDTO>>
    {
        private readonly PartnerService _partnerService;
        private readonly IntegrationManager _integrationManager;
        private readonly ILogger<UpdatePartnerRegistrationCommandHandler> _logger;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly ISignUpCodeRepository _signUpCodeRepository;
        private readonly BusinessProfileService _businessProfileService;
        //private HttpClient _HttpClient = new HttpClient();
        private readonly IIdentityContext identityContext;
        private readonly IConfiguration _config;
        private IHttpClientFactory _httpClientFactory;
        private readonly TrangloUserManager _userManager;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly SignalRMessageService _signalRNotificationService;
        private readonly IWebHostEnvironment _environment;
        private readonly INotificationService _notificationService;

        private class ApiResponse
        {
            public string Detail { get; set; }
        }
        public UpdatePartnerRegistrationCommandHandler(IntegrationManager integrationManger,
                                                        PartnerService partnerService,
                                                       ILogger<UpdatePartnerRegistrationCommandHandler> logger,
                                                       IApplicationUserRepository applicationUserRepository,
                                                        ISignUpCodeRepository signUpCodeRepository,
                                                        BusinessProfileService businessProfileService,
                                                        TrangloUserManager trangloUserManager,
                                                        IConfiguration config,
                                                        IIdentityContext identityContext,
                                                        IPartnerRepository partnerRepository,
                                                        IHttpClientFactory httpClientFactory,
                                                        IBusinessProfileRepository businessProfileRepository,
                                                        SignalRMessageService signalRNotificationService,
                                                        IWebHostEnvironment environment,
                                                        INotificationService notificationService
            )
        {
            _integrationManager = integrationManger;
            _applicationUserRepository = applicationUserRepository;
            _signUpCodeRepository = signUpCodeRepository;
            _partnerService = partnerService;
            _logger = logger;
            _userManager = trangloUserManager;
            this.identityContext = identityContext;
            _businessProfileService = businessProfileService;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _partnerRepository = partnerRepository;
            _businessProfileRepository = businessProfileRepository;
            _signalRNotificationService = signalRNotificationService;
            _environment = environment;
            _notificationService = notificationService;
        }

        public async Task<Result<UpdatePartnerRegistrationCommandOutputDTO>> Handle(UpdatePartnerRegistrationCommand request, CancellationToken cancellationToken)
        {

            var partnerBusiness = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
            var businessProfilePartner = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(partnerBusiness.BusinessProfileCode);
            var businessProfile = businessProfilePartner.Value;
            var inputPartnerRegistration = request.UpdatePartnerRegistration;
            var nature = Enumeration.FindById<BusinessNature>(inputPartnerRegistration.BusinessNature.GetValueOrDefault());
            var country = CountryMeta.GetCountryByISO2Async(inputPartnerRegistration.CountryISO2);
            var nationality = CountryMeta.GetCountryByISO2Async(request.UpdatePartnerRegistration.NationalityISO2);
            var MailingCountry = CountryMeta.GetCountryByISO2Async(inputPartnerRegistration.MailingCountryISO2);
            var customerType = await _businessProfileRepository.GetCustomerTypeByCode(request.UpdatePartnerRegistration.CustomerType);
            var partnerSubscription = await _partnerRepository.GetPartnerSubscriptionListAsync(request.PartnerCode);

            var businessNatureDescription = nature.Name;

            if (nature == BusinessNature.Other)
            {
                businessNatureDescription = BusinessNature.Other.Name;
            }


            // CurrentCustomerType value needed for change customer type handling
            long? currentCustomerTypeCode = 0;

            if (request.AdminSolution == Solution.Connect.Id)
            {
                currentCustomerTypeCode = null;
            }
            if (request.AdminSolution == Solution.Business.Id)
            {
                currentCustomerTypeCode = partnerBusiness.CustomerType.Id;
            }

            Result<ContactNumber> createContactNumber = string.IsNullOrWhiteSpace(inputPartnerRegistration.ContactNumber)
                || string.IsNullOrWhiteSpace(inputPartnerRegistration.DialCode)
                ? null
                : ContactNumber.Create(
                    inputPartnerRegistration.DialCode.Replace(" ", string.Empty),
                    inputPartnerRegistration.ContactNumberCountryISO2.Replace(" ", string.Empty),
                    inputPartnerRegistration.ContactNumber.Replace(" ", string.Empty));

            Result<ContactNumber> createTelephoneNumber = string.IsNullOrWhiteSpace(inputPartnerRegistration.TelephoneNumber)
                || string.IsNullOrWhiteSpace(inputPartnerRegistration.TelephoneDialCode)
                ? null
                : ContactNumber.Create(
                    inputPartnerRegistration.TelephoneDialCode.Replace(" ", string.Empty),
                    inputPartnerRegistration.TelephoneNumberCountryISO2.Replace(" ", string.Empty),
                    inputPartnerRegistration.TelephoneNumber.Replace(" ", string.Empty));

            Result<ContactNumber> createFacsimileNumber = string.IsNullOrWhiteSpace(inputPartnerRegistration.FacsimileNumber)
                || string.IsNullOrWhiteSpace(inputPartnerRegistration.FacsimileDialCode)
                ? null
                : ContactNumber.Create(
                    inputPartnerRegistration.FacsimileDialCode.Replace(" ", string.Empty),
                    inputPartnerRegistration.FacsimileNumberCountryISO2.Replace(" ", string.Empty),
                    inputPartnerRegistration.FacsimileNumber.Replace(" ", string.Empty));


            //Email Input
            var email = string.IsNullOrWhiteSpace(inputPartnerRegistration.Email) ? null : Email.Create(inputPartnerRegistration.Email).Value;

            //PartnerType
            //var partnerType = Enumeration.FindById<PartnerType>(inputPartnerRegistration.PartnerType.GetValueOrDefault());

            if (businessProfile.CompanyName != inputPartnerRegistration.PartnerName)
            {
                //duplicate checking on new partner name
                var _isExistingPartnerName = await _businessProfileService.IsCompanyNameDuplicateDuringUpdate(inputPartnerRegistration.PartnerName, businessProfile.Id);
                if (_isExistingPartnerName)
                {
                    return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>(
                           $"Partner Name Already Exist."
                       );
                }
            }

            //Get User input for Tranglo Staff Agent
            string trangloStaffAgentName = null;
            if (inputPartnerRegistration.Agent != null)
            {
                var ApplicationUser = await _applicationUserRepository.GetApplicationUserByLoginId(inputPartnerRegistration.Agent);
                if (ApplicationUser == null)
                {
                    return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>(
                            $"Application User {inputPartnerRegistration.Agent} Not Exist."
                        );
                }

                trangloStaffAgentName = ApplicationUser.LoginId;
            }
            

            businessProfile.CompanyName = inputPartnerRegistration.PartnerName;
            businessProfile.ContactNumber = createContactNumber.Value;
            businessProfile.CompanyRegisteredCountryCode = country?.Id;
            businessProfile.CompanyRegisteredZipCodePostCode = inputPartnerRegistration.ZipCodePostCode;
            businessProfile.ForOthers = inputPartnerRegistration.ForOthers;
            //businessProfile.TaxIdentificationNo = inputPartnerRegistration.TaxIdentificationNo; //CR to remove TaxIdentificationNo in #49842
            businessProfile.FacsimileNumber = createFacsimileNumber.Value;
            businessProfile.FormerRegisteredCompanyName = inputPartnerRegistration.FormerRegisteredCompanyName;

            partnerBusiness.Email = email;
            partnerBusiness.IMID = inputPartnerRegistration.IMID;
            partnerBusiness.AgentLoginId = trangloStaffAgentName;
            partnerBusiness.ProductLoginId = inputPartnerRegistration.ProductLoginId;
            partnerBusiness.SalesOperationLoginId = inputPartnerRegistration.SalesOperationLoginId;

            var _previousCompanyRegistrationName =  businessProfile.CompanyRegistrationName; //$56480 : get previous name for company registration

            if (request.AdminSolution == Solution.Connect.Id)
            {
                await _partnerService.UpdatePartnerRegistrationAsync(partnerBusiness);

                if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.RegisteredCompanyName))
                {
                    return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Full Registered Company Legal Name is required.");
                }

                if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.TradeName))
                {
                    return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Trade Name is required.");
                }

                if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.CompanyRegisteredNo))
                {
                    return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Company Registered No is required.");
                }

                if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.ContactPersonName))
                {
                    return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Contact Person Name is required.");
                }

                if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.ContactNumber))
                {
                    return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Contact Telephone is required.");
                }

                if (MailingCountry is null)
                {
                    return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Mailing Country is required.");
                }
                

                businessProfile.CompanyRegistrationName = inputPartnerRegistration.RegisteredCompanyName;
                businessProfile.TradeName = inputPartnerRegistration.TradeName;
                businessProfile.CompanyRegistrationNo = inputPartnerRegistration.CompanyRegisteredNo;
                businessProfile.CompanyRegisteredAddress = inputPartnerRegistration.CompanyAddress;
                businessProfile.MailingAddress = inputPartnerRegistration.MailingAddress;
                businessProfile.MailingZipCodePostCode = inputPartnerRegistration.MailingZipCodePostCode;
                businessProfile.MailingCountryCode = MailingCountry?.Id;
                businessProfile.BusinessNature = nature;
                businessProfile.SetContactPersonName(inputPartnerRegistration.ContactPersonName, inputPartnerRegistration.CustomerType);
                businessProfile.TelephoneNumber = createTelephoneNumber.Value;
            }
            else if (request.AdminSolution == Solution.Business.Id)
            {
                // Temporary fix to unblock QA. FE not passing in customer type
                // Blocker occured while partner details development not completed
                // Remove once FE fix is added
                #region
                if (customerType is null)
                {
                    customerType = partnerBusiness.CustomerType;
                }
                #endregion

                if (customerType != CustomerType.Individual)
                {
                    if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.RegisteredCompanyName))
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Full Registered Company Legal Name is required.");
                    }

                    if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.CompanyRegisteredNo))
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Company Registered No is required.");
                    }

                    if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.MailingAddress))
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Address of Principal Place of Business is required.");
                    }

                    //if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.MailingZipCodePostCode))
                    //{
                    //    return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Mailing PostCode is required.");
                    //}                    

                    if (MailingCountry is null)
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Mailing Country is required.");
                    }

                    if (request.UpdatePartnerRegistration.BusinessNature is null)
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Business Nature is required.");
                    }

                    if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.ContactPersonName))
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Contact Person Name is required.");
                    }

                    if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.TelephoneNumber))
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Telephone Number is required.");
                    }

                    if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.CompanyAddress))
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Address of Registered Office is required.");
                    }

                  

                    businessProfile.CompanyRegistrationName = inputPartnerRegistration.RegisteredCompanyName;
                    businessProfile.TradeName = inputPartnerRegistration.TradeName;
                    businessProfile.CompanyRegistrationNo = inputPartnerRegistration.CompanyRegisteredNo;
                    businessProfile.MailingAddress = inputPartnerRegistration.MailingAddress;
                    businessProfile.MailingZipCodePostCode = inputPartnerRegistration.MailingZipCodePostCode;
                    businessProfile.MailingCountryCode = MailingCountry?.Id;
                    businessProfile.BusinessNature = nature;
                    businessProfile.SetContactPersonName(inputPartnerRegistration.ContactPersonName, inputPartnerRegistration.CustomerType);
                    businessProfile.TelephoneNumber = createTelephoneNumber.Value;
                    businessProfile.CompanyRegisteredAddress = inputPartnerRegistration.CompanyAddress;
                    businessProfile.RelationshipTieUpCode = inputPartnerRegistration.RelationshipTieUp;
                }

                if (customerType == CustomerType.Individual)
                {
                    if (nationality is null)
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Nationality is required.");
                    }

                    if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.RegisteredCompanyName))
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Fullname is required.");
                    }

                    if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.ContactPersonName))
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Name (P-I-C) is required.");
                    }

                    if (request.UpdatePartnerRegistration.BusinessNature is null)
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Business Nature is required.");
                    }

                    if (string.IsNullOrEmpty(request.UpdatePartnerRegistration.CompanyAddress))
                    {
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Residential Address is required.");
                    }

                    businessProfile.NationalityCode = nationality.Id;
                    businessProfile.AliasName = inputPartnerRegistration.AliasName;
                    businessProfile.FormerRegisteredCompanyName = inputPartnerRegistration.FormerName;
                    businessProfile.CompanyRegistrationName = inputPartnerRegistration.RegisteredCompanyName;
                    businessProfile.BusinessNature = nature;
                    businessProfile.SetContactPersonName(inputPartnerRegistration.ContactPersonName, inputPartnerRegistration.CustomerType);
                    businessProfile.CompanyRegisteredAddress = inputPartnerRegistration.CompanyAddress;

                }
                partnerBusiness.CustomerTypeCode = customerType.Id;
            }
            else
            {
                return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>(
                        $"Unable to update Partner Registration for {request.PartnerCode}."
                    );
            }

            await _partnerService.UpdatePartnerRegistrationAsync(partnerBusiness);

            // Update Partner Name in SignupCode & CustomerUserRegistration table as well if Partner Name changed
            if (businessProfile.CompanyName != inputPartnerRegistration.PartnerName)
            {
                var signUpCodeInfo = await _signUpCodeRepository.GetActiveSignUpCodeByCompanyNameAsync(businessProfile.CompanyName);
                if (signUpCodeInfo != null)
                {
                    signUpCodeInfo.CompanyName = inputPartnerRegistration.PartnerName;
                    await _signUpCodeRepository.UpdateSignUpCodesAsync(signUpCodeInfo);
                }

                var customerUserRegistration = await _applicationUserRepository.GetCustomerUserRegistrationsByCompanyNameAsync(businessProfile.CompanyName);
                if (customerUserRegistration != null)
                {
                    customerUserRegistration.CompanyName = inputPartnerRegistration.PartnerName;
                    await _applicationUserRepository.UpdateCustomerUserRegistrationsAsync(customerUserRegistration);
                }
            }

            await _businessProfileService.UpdateBusinessProfileAsync(businessProfile);

            // Change Customer Type handling
            if (request.UpdatePartnerRegistration.CustomerType.HasValue)
            {
                var adminUser = await _userManager.FindByIdAsync(request.LoginId);
                var newCustomerTypeCode = request.UpdatePartnerRegistration.CustomerType.Value;

                var changeCustomerTypeHandling = await _businessProfileService.ChangeCustomerTypeHandling(businessProfile.Id, newCustomerTypeCode, currentCustomerTypeCode, adminUser.Id);
                if (changeCustomerTypeHandling.IsFailure)
                {
                    return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>(
                                        $"Failed to change Customer Type for PartnerCode: {request.PartnerCode}. {changeCustomerTypeHandling.Error}"
                                        );
                }
                else if (changeCustomerTypeHandling.Value is true) // returns true if redo declaration
                {
                    // Broadcast redo log off alert
                    await _signalRNotificationService.RedoBusinessDeclarationLogOffAlert(businessProfile.Id);
                }
                else if (changeCustomerTypeHandling.Value is false) // returns false if non redo declaration
                {
                    // Broadcast non redo log off alert
                    await _signalRNotificationService.NonRedoBusinessDeclarationLogOffAlert(businessProfile.Id);
                }
            }

            UpdatePartnerRegistrationCommandOutputDTO result = new UpdatePartnerRegistrationCommandOutputDTO();

            if (request.AdminSolution == Solution.Connect.Id)
            {
                result = new UpdatePartnerRegistrationCommandOutputDTO()
                {
                    PartnerCode = partnerBusiness.Id,
                    PartnerName = businessProfile.CompanyName,
                    FullRegisteredCompanyLegalName = businessProfile.CompanyRegistrationName,
                    FormerRegisteredCompanyName = businessProfile.FormerRegisteredCompanyName,
                    CompanyRegisteredNo = businessProfile.CompanyRegistrationNo,
                    ContactTelephone = businessProfile.ContactNumber,
                    ContactPersonName = businessProfile.ContactPersonName,
                    ContactEmailAddress = partnerBusiness.Email,
                    CompanyPostCode = businessProfile.CompanyRegisteredZipCodePostCode,
                    CompanyCountry = businessProfile.CompanyRegisteredCountryMeta,
                    RelationshipForTieUp = businessProfile.RelationshipTieUp

                };
            }
            else if (request.AdminSolution == Solution.Business.Id)
            {
                result = new UpdatePartnerRegistrationCommandOutputDTO()
                {
                    PartnerCode = partnerBusiness.Id,
                    PartnerName = businessProfile.CompanyName,
                    CustomerType = customerType.Name,
                    FullRegisteredCompanyLegalName = businessProfile.CompanyRegistrationName,
                    FormerRegisteredCompanyName = businessProfile.FormerRegisteredCompanyName,
                    CompanyRegisteredNo = businessProfile.CompanyRegistrationNo,
                    ContactTelephone = businessProfile.ContactNumber,
                    ContactPersonName = businessProfile.ContactPersonName,
                    ContactEmailAddress = partnerBusiness.Email,
                    CompanyPostCode = businessProfile.CompanyRegisteredZipCodePostCode,
                    CompanyCountry = businessProfile.CompanyRegisteredCountryMeta,
                    RelationshipForTieUp = businessProfile.RelationshipTieUp
                };

            }

            foreach (var partnerSub in partnerSubscription)
            {
                if (partnerSub.PartnerType != null
                    && partnerSub.TrangloEntity != null
                    && partnerSub.Solution != null
                    && partnerSub.SettlementCurrencyCode != null)
                {
                    var partnerProfileChangedEvent = new PartnerProfileChangedEvent(
                        partnerSub.TrangloEntity,  //TrangloEntity
                        partnerSub.Solution.Id,    //SolutionCode
                        partnerSub.PartnerType.Id,    //PartnerTypeCode
                        partnerBusiness.Id, //PartnerCode
                        partnerBusiness.PartnerId,  //PartnerId
                        partnerSub.Id,     //PartnerSubscriptionCode get from subscription
                        partnerSub.SettlementCurrencyCode, //SettlementCurrencyCode get from subscription
                        businessProfile.CompanyRegisteredCountryMeta?.CountryISO2, //CustomerIncorporationCountry
                        businessProfile.CompanyName,//CompanyName
                        businessProfile.CompanyRegistrationNo, //CompanyRegistrationNo
                        businessProfile.CompanyRegisteredAddress, //CompanyRegisteredAddress
                        businessProfile.IDExpiryDate,  //IDExpiryDate
                        businessProfile.DateOfBirth, //DateOfBirth
                        businessProfile.TelephoneNumber, //TelephoneNumber
                        partnerBusiness.Email,  //Email
                        businessProfile.CompanyRegisteredZipCodePostCode, //CompanyRegisteredZipCodePostCode
                        businessProfile.BusinessNature.Id, //BusinessNatureCode
                        businessNatureDescription, //BusinessNatureDescription
                        partnerSub.Environment?.Id,  //EnvironmentCode
                        businessProfile.DateOfIncorporation,
                        businessProfile.IncorporationCompanyTypeCode,
                        partnerBusiness.CustomerTypeCode,
                        businessProfile.CollectionTier?.Id,
                        businessProfile.Id
                        );

                    var savePartnerProfileEvents = await _partnerRepository.AddPartnerOnboardingCreationEventAsync(partnerProfileChangedEvent);

                    if (savePartnerProfileEvents.IsFailure)
                    {
                        _logger.LogError("Failed to save partner profile events");
                        return Result.Failure<UpdatePartnerRegistrationCommandOutputDTO>($"Saving Partner Profile Event Failed.");
                    }

                } 

            }

            //#56840 : to send email notification if registered company name is change
            var distinctPartnerSubscriptionEntitySolution = partnerSubscription
                .GroupBy(p => new { p.TrangloEntity, p.Solution }) // Group by multiple columns
                .Select(g => g.First()) // Select the first item from each group
                .ToList();

            long recipientType = 1;
            long ccType = 2;
            long bccType = 3;
            long notificationTemplate = 32;
            foreach (var partnerSubEntity in distinctPartnerSubscriptionEntitySolution)
            {
                if (_previousCompanyRegistrationName != inputPartnerRegistration.RegisteredCompanyName)
                {
                    var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientType, notificationTemplate);

                    var recipients = new List<RecipientsInputDTO>()
                            { new RecipientsInputDTO()
                                { email = recipientInfo[0].Email, name = recipientInfo[0].Name }
                            };


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

                    var SubjectTemplate = "[" + partnerSubEntity.TrangloEntity + "]" + " " + _previousCompanyRegistrationName + " has changed its name to " + inputPartnerRegistration.RegisteredCompanyName + " in " + partnerSubEntity.Solution.Name;
                    var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
                    string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                    var generator = new Common.ContentGenerator(xsltTemplateRootPath);

                    // 2. Use xsl to inject the properties from @event
                    StringBuilder _xml = new StringBuilder();
                    using (XmlWriter writer = XmlWriter.Create(_xml))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("BusinessPartner");
                        writer.WriteElementString("existingCompanyRegisteredName", _previousCompanyRegistrationName);
                        writer.WriteElementString("currentRegisteredCompanyName", inputPartnerRegistration.RegisteredCompanyName);
                        writer.WriteElementString("SolutionName", partnerSubEntity.Solution.Name);
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                    string content = generator.GenerateContent(_xml.ToString(), "PartnerNameChangeNotificationTemplate", cultureName);

                    Result<HttpStatusCode> sendPartnerNameChangeNotification = await _notificationService.SendNotification(recipients, bcc, cc, new List<IFormFile>() { },
                    SubjectTemplate, content, NotificationTypes.Email);

                    if (sendPartnerNameChangeNotification.IsFailure)
                    {
                        _logger.LogError($"[NotifyPartnerChangeName] 'Insufficient/Incomplete' Notification failed for {partnerSubEntity.TrangloEntity}. {sendPartnerNameChangeNotification.Error}.");
                    }
                }
            }

            return Result.Success(result);
        }

    }

}


