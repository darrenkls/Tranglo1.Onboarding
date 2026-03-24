using AutoMapper;
using System;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;
using Tranglo1.Onboarding.Domain.Entities.ExternalUserRoleAggregate;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Domain.Entities.SignUpCodes;
using Tranglo1.Onboarding.Application.DTO.AffiliateAndSubsidiary;
using Tranglo1.Onboarding.Application.DTO.BoardofDirector;
using Tranglo1.Onboarding.Application.DTO.BusinessProfile;
using Tranglo1.Onboarding.Application.DTO.ComplianceOfficers;
using Tranglo1.Onboarding.Application.DTO.Declarations;
using Tranglo1.Onboarding.Application.DTO.ExternalUserRole;
using Tranglo1.Onboarding.Application.DTO.LegalEntitiy;
using Tranglo1.Onboarding.Application.DTO.LicenseInformation;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.LegalEntitiy;
using Tranglo1.Onboarding.Application.DTO.ParentHoldingCompany;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.DTO.PoliticallyExposedPerson;
using Tranglo1.Onboarding.Application.DTO.PrimaryOfficer;
using Tranglo1.Onboarding.Application.DTO.Shareholder;
using Tranglo1.Onboarding.Application.DTO.SignUpCode;
using Tranglo1.Onboarding.Application.Queries;
using Tranglo1.Onboarding.Application.DTO.TrangloRole;

namespace Tranglo1.Onboarding.Application.Mappers
{
    public class DtoToQueryProfile : Profile
    {
        public DtoToQueryProfile()
        {
            #region KYC Management meta #38152
            CreateMap<KYCStatus, KYCStatusListOutputDTO>()
                .ForMember(dto => dto.KYCStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<WorkflowStatus, WorkflowStatusListOutputDTO>()
                .ForMember(dto => dto.WorkflowStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<ReviewResult, ReviewResultListOutputDTO>()
                .ForMember(dto => dto.ReviewResultCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));
            #endregion

            CreateMap<CustomerType, CustomerTypeOutputDTO>()
              .ForMember(dto => dto.CustomerTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
              .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name))
              .ForMember(dto => dto.DescriptionExternal, opt => opt.MapFrom(domain => domain.DescriptionExternal));

            CreateMap<CustomerType, CustomerTypeListOutputDTO>()
              .ForMember(dto => dto.CustomerTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
              .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name))
              .ForMember(dto => dto.DescriptionExternal, opt => opt.MapFrom(domain => domain.DescriptionExternal));

            CreateMap<Gender, GenderListOutputDTO>()
               .ForMember(dto => dto.GenderCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<IDType, IDTypeListOutputDTO>()
               .ForMember(dto => dto.IDTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<BusinessProfileIDType, BusinessProfileIDTypeListOutputDTO>()
               .ForMember(dto => dto.BusinessProfileIDTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<ShareholderType, ShareholderTypeListOutputDTO>()
                .ForMember(dto => dto.ShareholderTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<DocumentCategoryBPStatus, DocumentCategoryBPStatusListOutputDTO>()
               .ForMember(dto => dto.DocumentCategoryBPStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<UserType, UserTypeListOutputDTO>()
                .ForMember(dto => dto.UserTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<ExternalUserRoleStatus, ExternalUserRoleStatusOutputDTO>()
                .ForMember(dto => dto.ExternalUserRoleStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<Solution, SolutionListOutputDTO>()
                .ForMember(dto => dto.SolutionCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<CountryMeta, CountryDialCodeOutputDTO>()
                .ForMember(dto => dto.CountryISO2, opt => opt.MapFrom(domain => domain.CountryISO2))
                .ForMember(dto => dto.DialCode, opt => opt.MapFrom(domain => domain.DialCode));


            CreateMap<CountryMeta, CountryListOutputDTO>()
                .ForMember(dto => dto.CountryISO2, opt => opt.MapFrom(domain => domain.CountryISO2))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<WatchlistStatus, WatchlistStatusListOutputDTO>()
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name))
                .ForMember(dto => dto.WatchlistStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)));
            CreateMap<ScreeningEntityType, ScreeningEntityTypeListOutputDTO>()
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name))
                .ForMember(dto => dto.ScreeningEntityTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.ExternalDescription, opt => opt.MapFrom(domain => domain.ExternalDescription));

            CreateMap<UserRole, UserRolesOutputDTO>()
                .ForMember(dto => dto.UserRoleCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<SystemEnvironment, SystemEnvironmentListOutputDTO>()
                .ForMember(dto => dto.SystemEnvironmentCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<AccountStatus, UserStatusListOutputDTO>()
               .ForMember(dto => dto.AccountStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<RoleStatus, RoleStatusOutputDTO>()
             .ForMember(dto => dto.RoleStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
             .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<AuthorityLevel, AuthorityLevelOutputDTO>()
             .ForMember(dto => dto.AuthorityLevelCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
             .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<ActionOperation, ActionOperationOutputDTO>()
             .ForMember(dto => dto.ActionOperationCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
             .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<CompanyUserAccountStatus, CompanyUserAccountStatusOutputDTO>()
             .ForMember(dto => dto.CompanyUserAccountStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
             .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<CompanyUserBlockStatus, CompanyUserBlockStatusOutputDTO>()
             .ForMember(dto => dto.CompanyUserBlockStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
             .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<ApprovalStatus, ApprovalStatusOutputDTO>()
             .ForMember(dto => dto.ApprovalStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
             .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<BusinessNature, BusinessNatureListOutputDTO>()
               .ForMember(dto => dto.BusinessNatureCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<ScreeningTypeList, ScreeningDetailsCategoryOutputDTO>()
                .ForMember(dto => dto.ScreeningTypeListCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<ScreeningDetailsCategory, ScreeningDetailsCategoryOutputDTO>()
                .ForMember(dto => dto.ScreeningTypeListCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<OwnershipStrucureType, KYCOwnershipStructureTypeOutputDTO>()
               .ForMember(dto => dto.OwnershipStrucureTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<TrangloEntity, TrangloEntityListOutputDTO>()
               .ForMember(dto => dto.TrangloEntityCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name))
               .ForMember(dto => dto.TrangloEntityId, opt => opt.MapFrom(domain => domain.TrangloEntityCode));

            CreateMap<TrangloDepartment, TrangloDepartmentListOutputDTO>()
               .ForMember(dto => dto.TrangloDepartmentCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<TrangloRole, GetTrangloRoleByDeptListOutputDTO>()
              .ForMember(dto => dto.RoleCode, opt => opt.MapFrom(domain => domain.Id))
              .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Description));

            CreateMap<BusinessProfile, BusinessProfileOutputDTO>()
             .ForMember(dto => dto.BusinessProfileCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.Id))
             .ForMember(dto => dto.KYCStatusCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.KYCStatus.Id))
             .ForMember(dto => dto.RegistrationDate, opt => opt.MapFrom(BusinessProfile => BusinessProfile.RegistrationDate))
             .ForMember(dto => dto.SolutionCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.SolutionCode))
             .ForMember(dto => dto.CompanyName, opt => opt.MapFrom(BusinessProfile => BusinessProfile.CompanyName))
             .ForMember(dto => dto.CompanyRegistrationName, opt => opt.MapFrom(BusinessProfile => BusinessProfile.CompanyRegistrationName))
             .ForMember(dto => dto.TradeName, opt => opt.MapFrom(BusinessProfile => BusinessProfile.TradeName))
             .ForMember(dto => dto.CompanyRegisteredAddress, opt => opt.MapFrom(BusinessProfile => BusinessProfile.CompanyRegisteredAddress))
             .ForMember(dto => dto.CompanyRegisteredZipCodePostCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.CompanyRegisteredZipCodePostCode))
             .ForMember(dto => dto.CompanyRegisteredCountryISO2, opt => opt.MapFrom(BusinessProfile => BusinessProfile.CompanyRegisteredCountryMeta.CountryISO2))
             .ForMember(dto => dto.MailingAddress, opt => opt.MapFrom(BusinessProfile => BusinessProfile.MailingAddress))
             .ForMember(dto => dto.MailingZipCodePostCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.MailingZipCodePostCode))
             .ForMember(dto => dto.DialCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.ContactNumber.DialCode))
             .ForMember(dto => dto.ContactNumberCountryISO2, opt => opt.MapFrom(BusinessProfile => BusinessProfile.ContactNumber.CountryISO2Code))
             .ForMember(dto => dto.MailingCountryISO2, opt => opt.MapFrom(BusinessProfile => BusinessProfile.MailingCountryMeta.CountryISO2))
             .ForMember(dto => dto.BusinessNatureCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.BusinessNature))
             .ForMember(dto => dto.DateOfIncorporation, opt => opt.MapFrom(BusinessProfile => BusinessProfile.DateOfIncorporation))
             .ForMember(dto => dto.CompanyRegistrationNo, opt => opt.MapFrom(BusinessProfile => BusinessProfile.CompanyRegistrationNo))
             .ForMember(dto => dto.NumberOfBranches, opt => opt.MapFrom(BusinessProfile => BusinessProfile.NumberOfBranches))
             .ForMember(dto => dto.ContactNumber, opt => opt.MapFrom(BusinessProfile => BusinessProfile.ContactNumber.Value))
             .ForMember(dto => dto.Website, opt => opt.MapFrom(BusinessProfile => BusinessProfile.Website))
             .ForMember(dto => dto.IsCompanyListed, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IsCompanyListed))
             .ForMember(dto => dto.StockExchangeName, opt => opt.MapFrom(BusinessProfile => BusinessProfile.StockExchangeName))
             .ForMember(dto => dto.StockCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.StockCode))
             .ForMember(dto => dto.IsMoneyTransferRemittance, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IsMoneyTransferRemittance))
             .ForMember(dto => dto.IsForeignCurrencyExchange, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IsForeignCurrencyExchange))
             .ForMember(dto => dto.IsRetailCommercialBankingServices, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IsRetailCommercialBankingServices))
             .ForMember(dto => dto.IsForexTrading, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IsForexTrading))
             .ForMember(dto => dto.IsEMoneyEwallet, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IsEMoneyEwallet))
             .ForMember(dto => dto.IsIntermediataryRemittance, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IsIntermediataryRemittance))
             .ForMember(dto => dto.IsCryptocurrency, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IsCryptocurrency))
             .ForMember(dto => dto.IsOther, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IsOther))
             .ForMember(dto => dto.OtherReason, opt => opt.MapFrom(BusinessProfile => BusinessProfile.OtherReason))
             .ForMember(dto => dto.FormerRegisteredCompanyName, opt => opt.MapFrom(BusinessProfile => BusinessProfile.FormerRegisteredCompanyName))
             .ForMember(dto => dto.ForOthers, opt => opt.MapFrom(BusinessProfile => BusinessProfile.ForOthers))
             .ForMember(dto => dto.TaxIdentificationNo, opt => opt.MapFrom(BusinessProfile => BusinessProfile.TaxIdentificationNo))
             .ForMember(dto => dto.EntityTypeCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.EntityTypeCode))
             .ForMember(dto => dto.RelationshipTieUpCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.RelationshipTieUpCode))
             .ForMember(dto => dto.IncorporationCompanyTypeCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IncorporationCompanyTypeCode))
             .ForMember(dto => dto.ContactPersonName, opt => opt.MapFrom(BusinessProfile => BusinessProfile.ContactPersonName))
             .ForMember(dto => dto.FacsimileDialCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.FacsimileNumber.DialCode))
             .ForMember(dto => dto.FacsimileNumber, opt => opt.MapFrom(BusinessProfile => BusinessProfile.FacsimileNumber.Value))
             .ForMember(dto => dto.FacsimileNumberCountryISO2, opt => opt.MapFrom(BusinessProfile => BusinessProfile.FacsimileNumber.CountryISO2Code))
             .ForMember(dto => dto.TelephoneDialCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.TelephoneNumber.DialCode))
             .ForMember(dto => dto.TelephoneNumber, opt => opt.MapFrom(BusinessProfile => BusinessProfile.TelephoneNumber.Value))
             .ForMember(dto => dto.TelephoneNumberCountryISO2, opt => opt.MapFrom(BusinessProfile => BusinessProfile.TelephoneNumber.CountryISO2Code))
             .ForMember(dto => dto.ContactEmailAddress, opt => opt.Ignore())
             .ForMember(dto => dto.DateOfBirth, opt => opt.MapFrom(BusinessProfile => BusinessProfile.DateOfBirth))
             .ForMember(dto => dto.IDExpiryDate, opt => opt.MapFrom(BusinessProfile => BusinessProfile.IDExpiryDate))
             .ForMember(dto => dto.AliasName, opt => opt.MapFrom(BusinessProfile => BusinessProfile.AliasName))
             .ForMember(dto => dto.BusinessProfileIDTypeCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.BusinessProfileIDType.Id))
             .ForMember(dto => dto.TitleCode, opt => opt.MapFrom(BusinessProfile => BusinessProfile.Title.Id));

            CreateMap<LicenseInformation, LicenseInformationOutputDTO>()
                   .ForMember(dto => dto.LicenseInformationCode, opt => opt.MapFrom(LicenseInformation => Convert.ToInt32(LicenseInformation.Id)))
                   .ForMember(dto => dto.BusinessProfileCode, opt => opt.MapFrom(LicenseInformation => LicenseInformation.BusinessProfileCode))
                   .ForMember(dto => dto.IsLicenseRequired, opt => opt.MapFrom(LicenseInformation => LicenseInformation.IsLicenseRequired))
                   .ForMember(dto => dto.LicenseType, opt => opt.MapFrom(LicenseInformation => LicenseInformation.LicenseType))
                   .ForMember(dto => dto.LicenseCertNumber, opt => opt.MapFrom(LicenseInformation => LicenseInformation.LicenseCertNumber))
                   .ForMember(dto => dto.IssuedDate, opt => opt.MapFrom(LicenseInformation => LicenseInformation.IssuedDate))
                   .ForMember(dto => dto.ExpiryDate, opt => opt.MapFrom(LicenseInformation => LicenseInformation.ExpiryDate))
                   .ForMember(dto => dto.PrimaryRegulatorLicenseService, opt => opt.MapFrom(LicenseInformation => LicenseInformation.PrimaryRegulatorLicenseService))
                   .ForMember(dto => dto.PrimaryRegulatorAMLCFT, opt => opt.MapFrom(LicenseInformation => LicenseInformation.PrimaryRegulatorAMLCFT))
                   .ForMember(dto => dto.ActLawRemittanceLicense, opt => opt.MapFrom(LicenseInformation => LicenseInformation.ActLawRemittanceLicense))
                   .ForMember(dto => dto.ActLawAMLCFT, opt => opt.MapFrom(LicenseInformation => LicenseInformation.ActLawAMLCFT))
                   .ForMember(dto => dto.Remark, opt => opt.MapFrom(LicenseInformation => LicenseInformation.Remark))
                   .ForMember(dto => dto.RegulatorDocumentId, opt => opt.MapFrom(LicenseInformation => LicenseInformation.RegulatorDocumentId))
                   .ForMember(dto => dto.RegulatorDocumentName, opt => opt.MapFrom(LicenseInformation => LicenseInformation.RegulatorDocumentName))
                   .ForMember(dto => dto.RegulatorWebsite, opt => opt.MapFrom(LicenseInformation => LicenseInformation.RegulatorWebsite));

            CreateMap<COInformation, ComplianceOfficersOutputDTO>()
               .ForMember(dto => dto.COInformationCode, opt => opt.MapFrom(COInformation => Convert.ToInt32(COInformation.Id)))
               .ForMember(dto => dto.BusinessProfileCode, opt => opt.MapFrom(COInformation => COInformation.BusinessProfileCode))
               .ForMember(dto => dto.IsRegisteredRegulator, opt => opt.MapFrom(COInformation => COInformation.IsRegisteredRegulator))
               .ForMember(dto => dto.IsCertifiedByAML, opt => opt.MapFrom(COInformation => COInformation.IsCertifiedByAML))
               .ForMember(dto => dto.ComplianceOfficer, opt => opt.MapFrom(COInformation => COInformation.ComplianceOfficer))
               .ForMember(dto => dto.PositionTitle, opt => opt.MapFrom(COInformation => COInformation.PositionTitle))
               .ForMember(dto => dto.CompanyAddress, opt => opt.MapFrom(COInformation => COInformation.CompanyAddress))
               .ForMember(dto => dto.ZipCodePostCode, opt => opt.MapFrom(COInformation => COInformation.ZipCodePostCode))
               .ForMember(dto => dto.ContactNumber, opt => opt.MapFrom(COInformation => COInformation.ContactNumber.Value))
               .ForMember(dto => dto.DialCode, opt => opt.MapFrom(COInformation => COInformation.ContactNumber.DialCode))
               .ForMember(dto => dto.ContactNumberCountryISO2, opt => opt.MapFrom(COInformation => COInformation.ContactNumber.CountryISO2Code))
               .ForMember(dto => dto.EmailAddress, opt => opt.MapFrom(COInformation => COInformation.EmailAddress.Value))
               .ForMember(dto => dto.ReportingTo, opt => opt.MapFrom(COInformation => COInformation.ReportingTo))
               .ForMember(dto => dto.CertificationProgram, opt => opt.MapFrom(COInformation => COInformation.CertificationProgram))
               .ForMember(dto => dto.CertificationBodyOrganization, opt => opt.MapFrom(COInformation => COInformation.CertificationBodyOrganization));

            CreateMap<AffiliateAndSubsidiary, AffiliateAndSubsidiaryOutputDTO>()
                .ForMember(dto => dto.AffliateSubsidiaryCode, opt => opt.MapFrom(AffiliateAndSubsidiary => AffiliateAndSubsidiary.Id))
                .ForMember(dto => dto.CountryISO2, opt => opt.MapFrom(AffiliateAndSubsidiary => AffiliateAndSubsidiary.Country.CountryISO2));

            CreateMap<BoardOfDirector, BoardofDirectorOutputDTO>()
                .ForMember(dto => dto.BoardOfDirectorCode, opt => opt.MapFrom(BoardOfDirector => BoardOfDirector.Id))
                .ForMember(dto => dto.NationalityISO2, opt => opt.MapFrom(BoardOfDirector => BoardOfDirector.Nationality.CountryISO2))
                .ForMember(dto => dto.GenderCode, opt => opt.MapFrom(BoardOfDirector => BoardOfDirector.Gender.Id))
                .ForMember(dto => dto.IdTypeCode, opt => opt.MapFrom(BoardOfDirector => BoardOfDirector.IDType.Id))
                .ForMember(dto => dto.CountryOfResidenceISO2, opt => opt.MapFrom(BoardOfDirector => BoardOfDirector.CountryOfResidence.CountryISO2))
                .ForMember(dto => dto.ShareholderCode, opt => opt.MapFrom(BoardOfDirector => BoardOfDirector.Shareholder.Id))
                .ForMember(dto => dto.TitleCode, opt => opt.MapFrom(BoardOfDirector => BoardOfDirector.Title.Id));



            CreateMap<ParentHoldingCompany, ParentHoldingCompanyOutputDTO>()
                .ForMember(dto => dto.ParentHoldingCompanyCode, opt => opt.MapFrom(ParentHoldingCompany => ParentHoldingCompany.Id))
                .ForMember(dto => dto.CountryISO2, opt => opt.MapFrom(ParentHoldingCompany => ParentHoldingCompany.Country.CountryISO2));

            CreateMap<PoliticallyExposedPerson, PoliticallyExposedPersonOutputDTO>()
                .ForMember(dto => dto.PoliticallyExposedPersonCode, opt => opt.MapFrom(PoliticallyExposedPerson => PoliticallyExposedPerson.Id))
                .ForMember(dto => dto.GenderCode, opt => opt.MapFrom(PoliticallyExposedPerson => PoliticallyExposedPerson.Gender.Id))
                .ForMember(dto => dto.NationalityISO2, opt => opt.MapFrom(PoliticallyExposedPerson => PoliticallyExposedPerson.Nationality.CountryISO2))
                .ForMember(dto => dto.CountryOfResidenceISO2, opt => opt.MapFrom(PoliticallyExposedPerson => PoliticallyExposedPerson.CountryOfResidence.CountryISO2))
                .ForMember(dto => dto.IdTypeCode, opt => opt.MapFrom(PoliticallyExposedPerson => PoliticallyExposedPerson.IDType.Id));

            CreateMap<PrimaryOfficer, PrimaryOfficerOutputDTO>()
                .ForMember(dto => dto.PrimaryOfficerCode, opt => opt.MapFrom(PrimaryOfficer => PrimaryOfficer.Id))
                .ForMember(dto => dto.NationalityISO2, opt => opt.MapFrom(PrimaryOfficer => PrimaryOfficer.Nationality.CountryISO2))
                .ForMember(dto => dto.GenderCode, opt => opt.MapFrom(PrimaryOfficer => PrimaryOfficer.Gender.Id))
                .ForMember(dto => dto.IdTypeCode, opt => opt.MapFrom(PrimaryOfficer => PrimaryOfficer.IDType.Id))
                .ForMember(dto => dto.CountryOfResidenceISO2, opt => opt.MapFrom(PrimaryOfficer => PrimaryOfficer.CountryOfResidence.CountryISO2))
                .ForMember(dto => dto.ShareholderCode, opt => opt.MapFrom(PrimaryOfficer => PrimaryOfficer.Shareholder.Id))
                .ForMember(dto => dto.TitleCode, opt => opt.MapFrom(PrimaryOfficer => PrimaryOfficer.Title.Id));

            CreateMap<IndividualShareholder, ShareholderOutputDTO>()
                .ForMember(dto => dto.ShareholderCode, opt => opt.MapFrom(IndividualShareholder => IndividualShareholder.Id))
                .ForMember(dto => dto.ShareholderTypeCode, opt => opt.MapFrom(IndividualShareholder => ShareholderType.Individual.Id))
                .ForMember(dto => dto.NationalityISO2, opt => opt.MapFrom(x => x.Nationality.CountryISO2))
                .ForMember(dto => dto.CountryOfResidenceISO2, opt => opt.MapFrom(x => x.CountryOfResidence.CountryISO2))
                .ForMember(dto => dto.GenderCode, opt => opt.MapFrom(x => x.Gender.Id))
                .ForMember(dto => dto.IdTypeCode, opt => opt.MapFrom(x => x.IDType.Id))
                .ForMember(dto => dto.CountryISO2, opt => opt.Ignore())
                .ForMember(dto => dto.AuthorisedPersonCode, opt => opt.MapFrom(x => x.AuthorisedPerson.Id))
                .ForMember(dto => dto.BoardOfDirectorCode, opt => opt.MapFrom(x => x.BoardOfDirector.Id))
                .ForMember(dto => dto.PrimaryOfficersCode, opt => opt.MapFrom(x => x.PrimaryOfficer.Id))
                .ForMember(dto => dto.UltimateBeneficialOwnerCode, opt => opt.MapFrom(x => x.UltimateBeneficialOwner.Id))
                .ForMember(dto => dto.ResidentialAddress, opt => opt.MapFrom(x => x.ResidentialAddress))
                .ForMember(dto => dto.ZipCodePostCode, opt => opt.MapFrom(x => x.ZipCodePostCode))
                .ForMember(dto => dto.EffectiveShareholding, opt => opt.MapFrom(x => x.EffectiveShareholding))
                .ForMember(dto => dto.TitleCode, opt => opt.MapFrom(x => x.Title.Id));


            CreateMap<CompanyShareholder, ShareholderOutputDTO>()
                .ForMember(dto => dto.ShareholderCode, opt => opt.MapFrom(CompanyShareholder => CompanyShareholder.Id))
                .ForMember(dto => dto.ShareholderTypeCode, opt => opt.MapFrom(CompanyShareholder => ShareholderType.Company.Id))
                .ForMember(dto => dto.CompanyRegNo, opt => opt.MapFrom(CompanyShareholder => CompanyShareholder.CompanyRegNo))
                .ForMember(dto => dto.CountryISO2, opt => opt.MapFrom(x => x.Country.CountryISO2))
                .ForMember(dto => dto.GenderCode, opt => opt.Ignore())
                .ForMember(dto => dto.IdTypeCode, opt => opt.Ignore())
                .ForMember(dto => dto.CountryOfResidenceISO2, opt => opt.Ignore())
                .ForMember(dto => dto.NationalityISO2, opt => opt.Ignore())
                .ForMember(dto => dto.AuthorisedPersonCode, opt => opt.Ignore())
                .ForMember(dto => dto.BoardOfDirectorCode, opt => opt.Ignore())
                .ForMember(dto => dto.PrimaryOfficersCode, opt => opt.Ignore())
                .ForMember(dto => dto.ResidentialAddress, opt => opt.Ignore())
                .ForMember(dto => dto.ZipCodePostCode, opt => opt.Ignore())
                .ForMember(dto => dto.EffectiveShareholding, opt => opt.MapFrom(x => x.EffectiveShareholding));

            CreateMap<IndividualLegalEntity, LegalEntitiyOutputDTO>()
                .ForMember(dto => dto.LegalEntityCode, opt => opt.MapFrom(IndividualLegalEntity => IndividualLegalEntity.Id))
                .ForMember(dto => dto.ShareholderTypeCode, opt => opt.MapFrom(IndividualLegalEntity => ShareholderType.Individual.Id))
                .ForMember(dto => dto.IdTypeCode, opt => opt.MapFrom(IndividualLegalEntity => IndividualLegalEntity.IDType.Id))
                .ForMember(dto => dto.GenderCode, opt => opt.MapFrom(IndividualLegalEntity => IndividualLegalEntity.Gender.Id))
                .ForMember(dto => dto.NationalityISO2, opt => opt.MapFrom(IndividualLegalEntity => IndividualLegalEntity.Nationality.CountryISO2))
                .ForMember(dto => dto.CountryOfResidenceISO2, opt => opt.MapFrom(IndividualLegalEntity => IndividualLegalEntity.CountryOfResidence.CountryISO2))
                .ForMember(dto => dto.CountryISO2, opt => opt.Ignore())
                .ForMember(dto => dto.EffectiveShareholding, opt => opt.MapFrom(x => x.EffectiveShareholding))
                .ForMember(dto => dto.ShareholderCode, opt => opt.MapFrom(IndividualLegalEntity => IndividualLegalEntity.Shareholder.Id))
                .ForMember(dto => dto.TitleCode, opt => opt.MapFrom(IndividualLegalEntity => IndividualLegalEntity.Title.Id));



            CreateMap<CompanyLegalEntity, LegalEntitiyOutputDTO>()
                .ForMember(dto => dto.LegalEntityCode, opt => opt.MapFrom(CompanyLegalEntity => CompanyLegalEntity.Id))
                .ForMember(dto => dto.ShareholderTypeCode, opt => opt.MapFrom(CompanyLegalEntity => ShareholderType.Company.Id))
                .ForMember(dto => dto.CompanyRegNo, opt => opt.MapFrom(CompanyLegalEntity => CompanyLegalEntity.CompanyRegNo))
                .ForMember(dto => dto.CountryISO2, opt => opt.MapFrom(CompanyLegalEntity => CompanyLegalEntity.Country.CountryISO2))
                .ForMember(dto => dto.IdTypeCode, opt => opt.Ignore())
                .ForMember(dto => dto.GenderCode, opt => opt.Ignore())
                .ForMember(dto => dto.CountryOfResidenceISO2, opt => opt.Ignore())
                .ForMember(dto => dto.NationalityISO2, opt => opt.Ignore())
                .ForMember(dto => dto.EffectiveShareholding, opt => opt.MapFrom(x => x.EffectiveShareholding));

            CreateMap<ShareholderIndividualLegalEntity, ShareholderIndividualLegalEntityOutputDTO>()
                    .ForMember(dto => dto.CompanyName, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dto => dto.LegalEntityCode, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dto => dto.ShareholderCode, opt => opt.MapFrom(src => src.Shareholder.Id))
                    .ForMember(dto => dto.ShareholderTypeCode, opt => opt.MapFrom(src => ShareholderType.Individual.Id))
                    .ForMember(dto => dto.IdTypeCode, opt => opt.MapFrom(src => src.IDType.Id))
                    .ForMember(dto => dto.GenderCode, opt => opt.MapFrom(src => src.Gender.Id))
                    .ForMember(dto => dto.NationalityISO2, opt => opt.MapFrom(src => src.Nationality.CountryISO2))
                    .ForMember(dto => dto.CountryOfResidenceISO2, opt => opt.MapFrom(src => src.CountryOfResidence.CountryISO2))
                    .ForMember(dto => dto.ShareholderIndividualLegalEntityCode, opt => opt.MapFrom(src => src.ShareholderIndividualLegalEntityCode))
                    .ForMember(dto => dto.EffectiveShareholding, opt => opt.MapFrom(src => src.EffectiveShareholding))
                    .ForMember(dto => dto.TitleCode, opt => opt.MapFrom(src => src.Title.Id));

            CreateMap<ShareholderCompanyLegalEntity, ShareholderCompanyLegalEntityOutputDTO>()
                .ForMember(dto => dto.LegalEntityCode, opt => opt.MapFrom(src => src.Id))
                .ForMember(dto => dto.ShareholderCode, opt => opt.MapFrom(src => src.Shareholder.Id))
                .ForMember(dto => dto.ShareholderTypeCode, opt => opt.MapFrom(src => ShareholderType.Company.Id))
                .ForMember(dto => dto.CompanyRegNo, opt => opt.MapFrom(src => src.CompanyRegNo))
                .ForMember(dto => dto.CountryISO2, opt => opt.MapFrom(src => src.Country.CountryISO2))
                .ForMember(dto => dto.ShareholderCompanyLegalEntityCode, opt => opt.MapFrom(src => src.ShareholderCompanyLegalEntityCode))
                .ForMember(dto => dto.EffectiveShareholding, opt => opt.MapFrom(src => src.EffectiveShareholding));



            CreateMap<Declaration, DeclarationsOutputDTO>();

            /*
            CreateMap<PartnerRegistration, PartnerRegistrationOutputDTO>()
                .ForMember(dto => dto.PartnerCode, opt => opt.MapFrom(x => x.Id))
                .ForMember(dto => dto.RegisteredCompanyName, opt => opt.MapFrom(x => x.BusinessProfile.CompanyRegistrationName))
                .ForMember(dto => dto.TradeName, opt => opt.MapFrom(x => x.BusinessProfile.TradeName))
                .ForMember(dto => dto.BusinessNature, opt => opt.MapFrom(x => x.BusinessProfile.BusinessNature))
                .ForMember(dto => dto.CountryISO2, opt => opt.MapFrom(x => x.BusinessProfile.CompanyRegisteredCountryISO2))
                .ForMember(dto => dto.CompanyRegisteredNo, opt => opt.MapFrom(x => x.BusinessProfile.CompanyRegistrationNo))
                .ForMember(dto => dto.Email, opt => opt.MapFrom(x => x.Email))
                .ForMember(dto => dto.ContactNumber, opt => opt.MapFrom(x => x.BusinessProfile.ContactNumber))
                .ForMember(dto => dto.IMID, opt => opt.MapFrom(x => x.IMID))
                .ForMember(dto => dto.ZipCodePostCode, opt => opt.MapFrom(x => x.ZipCodePostCode))
                .ForMember(dto => dto.Solution, opt => opt.MapFrom(x => x.BusinessProfile.Solution))
                .ForMember(dto => dto.PricePackageCode, opt => opt.MapFrom(x => x.PricePackageCode))
                .ForMember(dto => dto.TimeZone, opt => opt.MapFrom(x => x.TimeZone))
                .ForMember(dto => dto.Agent, opt => opt.MapFrom(x => x.AgentLoginId))
                .ForMember(dto => dto.Entity, opt => opt.MapFrom(x => x.TrangloEntity))
                .ForMember(dto => dto.PartnerType, opt => opt.MapFrom(x => x.PartnerType))
                .ForMember(dto => dto.Currency, opt => opt.MapFrom(x => x.CurrencyCode))
                .ForMember(dto => dto.CompanyAddress, opt => opt.MapFrom(x => x.CompanyAddress));
            */

            CreateMap<Domain.Entities.Environment, EnvironmentsOutputDTO>()
               .ForMember(dto => dto.EnvironmentCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<PartnerType, PartnerTypeOutputDTO>()
                .ForMember(dto => dto.PartnerTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<PartnerAccountStatusType, PartnerAccountStatusTypeOutputDTO>()
                .ForMember(dto => dto.PartnerAccountStatusTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<OnboardWorkflowStatus, OnboardWorkflowStatusListOutputDTO>()
               .ForMember(dto => dto.OnboardWorkflowStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<ChangeType, PartnerAccountStatusChangeTypeOutputDTO>()
                .ForMember(dto => dto.ChangeTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<PartnerAgreementStatus, PartnerAgreementStatusTypeOutputDTO>()
                .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name))
                .ForMember(dto => dto.PartnerAgreementStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)));

            CreateMap<LeadsOrigin, SignUpCodesLeadsOriginOutputDTO>()
              .ForMember(dto => dto.LeadsOriginCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
              .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<SignUpAccountStatus, SignUpCodeAccountStatusOutputDTO>()
              .ForMember(dto => dto.SignUpCodeStatusCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
              .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<IncorporationCompanyType, IncorporationCompanyTypeOutputDTO>()
               .ForMember(dto => dto.IncorporationCompanyTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<EntityType, EntityTypesOutputDTO>()
               .ForMember(dto => dto.EntityTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<ExternalUserRole, ExternalRoleListOutputDTO>()
                .ForMember(dto => dto.UserRoleCode, opt => opt.MapFrom(role => role.RoleCode))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(role => role.ExternalUserRoleName));

            CreateMap<QuestionInputType, QuestionInputTypeOutputDTO>()
               .ForMember(dto => dto.QuestionInputTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));
            CreateMap<ServicesOffered, ServicesOfferedOutputDTO>()
             .ForMember(dto => dto.ServicesOfferedCode, opt => opt.MapFrom(domain => domain.Id))
             .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<ServiceType, ServiceTypeOutputDTO>()
            .ForMember(dto => dto.ServiceTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
            .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<CollectionTier, CollectionTierOutputDTO>()
            .ForMember(dto => dto.CollectionTierTypeCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
            .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<Title, TitleOutputDTO>()
            .ForMember(dto => dto.TitleCode, opt => opt.MapFrom(domain => Convert.ToInt32(domain.Id)))
            .ForMember(dto => dto.Description, opt => opt.MapFrom(domain => domain.Name));

            CreateMap<GetPartnerListExportSearchInputDTO, PagingOptions>()
                .ForMember(dest => dest.PageIndex, opt => opt.MapFrom(src => src.Page));

            CreateMap<GetPartnerListExportSearchInputDTO, GetPartnerListingQuery>()
                .ForPath(dest => dest.PagingOptions, opt => opt.MapFrom(src => src));
        }
    }

}
