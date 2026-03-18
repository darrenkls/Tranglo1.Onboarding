using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using TimeZoneNames;
using Tranglo1.Onboarding.Application.DTO.ExternalUserRole;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.DTO.SignUpCode;
using Tranglo1.Onboarding.Application.DTO.TrangloRole;
using Tranglo1.Onboarding.Application.Queries;

namespace Tranglo1.Onboarding.Application.Controllers
{
    [ApiController]
    [Route("/api/v{version:apiVersion}/meta")]
    [ApiVersion("1")]
    public class MetaController : ControllerBase
    {
        public IMediator Mediator { get; }
        public ILogger<MetaController> _logger;

        public MetaController(IMediator mediator)
        {
            Mediator = mediator;
        }

        /// <summary>
        /// Retrieve Full Countries listing
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("countries")]
        [SwaggerOperation(OperationId = nameof(GetCountries), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<CountryListOutputDTO>> GetCountries()
        {
            GetCountriesQuery query = new GetCountriesQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Countries listing (filtered by isDisplay status in CountrySetting)
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("filtered-countries")]
        [SwaggerOperation(OperationId = nameof(GetFilteredCountries), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<CountryListOutputDTO>> GetFilteredCountries()
        {
            GetDisplayedCountriesQuery query = new GetDisplayedCountriesQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Countries listing (filtered by isDisplay, isHighRisk and isSanction status in CountrySetting)
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("nohighrisknosanction-filtered-countries")]
        [SwaggerOperation(OperationId = nameof(GetNoHighRiskNoSanctionCountries), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<CountryListOutputDTO>> GetNoHighRiskNoSanctionCountries()
        {
            GetNoHighRiskNoSanctionQuery query = new GetNoHighRiskNoSanctionQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve all country dial code listing
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("country-dial-codes")]
        [SwaggerOperation(OperationId = nameof(GetCountryDialCodes), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<CountryDialCodeOutputDTO>> GetCountryDialCodes()
        {
            GetCountryDialCodeListQuery query = new GetCountryDialCodeListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Solution listing. ie: Connect, Recharge, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("business-solutions")]
        [SwaggerOperation(OperationId = nameof(GetSolutions), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<SolutionListOutputDTO>> GetSolutions(long? customerTypeCode)
        {
            GetSolutionsQuery query = new GetSolutionsQuery()
            {
                CustomerTypeCode = customerTypeCode
            };
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Solution listing filtered to only TB and TC solutions
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("filtered-solutions")]
        [SwaggerOperation(OperationId = nameof(GetFilteredSolutions), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<SolutionListOutputDTO>> GetFilteredSolutions()
        {
            GetFilteredSolutionsQuery query = new GetFilteredSolutionsQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve User Type listing. ie: Individual, Business, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("user-types")]
        [SwaggerOperation(OperationId = nameof(GetUserTypes), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<UserTypeListOutputDTO>> GetUserTypes()
        {
            GetUserTypeQuery query = new GetUserTypeQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve System Environment listing. ie: Staging, Production, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("system-environments")]
        [SwaggerOperation(OperationId = nameof(GetSystemEnvironments), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<SystemEnvironmentListOutputDTO>> GetSystemEnvironments()
        {
            GetSystemEnvironmentQuery query = new GetSystemEnvironmentQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Account Status listing. ie: Active, Inactive, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("account-statuses")]
        [SwaggerOperation(OperationId = nameof(GetUserAccountStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<UserStatusListOutputDTO>> GetUserAccountStatus()
        {
            GetUserStatusQuery query = new GetUserStatusQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Business Nature listing. ie: Accounting Firm, Agriculture, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("business-natures")]
        [SwaggerOperation(OperationId = nameof(GetBusinessNatureList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<BusinessNatureListOutputDTO>> GetBusinessNatureList()
        {
            GetBusinessNatureListQuery query = new GetBusinessNatureListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Shareholder Type listing. ie: Individual, Company, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("shareholder-types")]
        [SwaggerOperation(OperationId = nameof(GetShareholderTypeList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ShareholderTypeListOutputDTO>> GetShareholderTypeList()
        {
            GetShareholderTypeListQuery query = new GetShareholderTypeListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Document Status listing. ie: Pending Upload, Pending Submission, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("document-statuses")]
        [SwaggerOperation(OperationId = nameof(GetDocumentCategoryBPStatusList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<DocumentCategoryBPStatusListOutputDTO>> GetDocumentCategoryBPStatusList()
        {
            GetDocumentCategoryBPStatusListQuery query = new GetDocumentCategoryBPStatusListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Screening Type listing. ie: PEP, Sanctions, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("screening-types")]
        [SwaggerOperation(OperationId = nameof(GetScreeningTypeList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ScreeningDetailsCategoryOutputDTO>> GetScreeningTypeList()
        {
            GetScreeningTypeListQuery query = new GetScreeningTypeListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve ID Type listing. ie: Driving License, Identification Card, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("id-types")]
        [SwaggerOperation(OperationId = nameof(GetIDTypeList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<IDTypeListOutputDTO>> GetIDTypeList()
        {
            GetIDTypeListQuery query = new GetIDTypeListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve ID Type listing. ie: Driving License, Identification Card, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("business-profile-id-types")]
        [SwaggerOperation(OperationId = nameof(GetIDTypeList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<BusinessProfileIDTypeListOutputDTO>> GetBusinessProfileIDTypeList()
        {
            GetBusinessProfileIDTypeListQuery query = new GetBusinessProfileIDTypeListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve ID Type listing. ie: Female, Male, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("genders")]
        [SwaggerOperation(OperationId = nameof(GetGenderList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<GenderListOutputDTO>> GetGenderList()
        {
            GetGenderListQuery query = new GetGenderListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Watchlist Status listing. ie: KIV, Pending Review, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("watchlist-status")]
        [SwaggerOperation(OperationId = nameof(GetWatchlistStatusList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<WatchlistStatusListOutputDTO>> GetWatchlistStatusList()
        {
            GetWatchlistStatusListQuery query = new GetWatchlistStatusListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Profile Type. ie: Individual, Corporate, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("profile-types")]
        [SwaggerOperation(OperationId = nameof(GetScreeningEntityType), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ScreeningEntityTypeListOutputDTO>> GetScreeningEntityType()
        {
            GetScreeningEntityTypeQuery query = new GetScreeningEntityTypeQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Watchlist Status. ie: Business Profile, Company Shareholder, etc
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("ownership-structure")]
        [SwaggerOperation(OperationId = nameof(GetOwnershipStructureTypeList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<KYCOwnershipStructureTypeOutputDTO>> GetOwnershipStructureTypeList()
        {
            GetOwnershipStructureTypeQuery query = new GetOwnershipStructureTypeQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Tranglo Entities
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("tranglo-entities")]
        [SwaggerOperation(OperationId = nameof(GetTrangloEntityList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<TrangloEntityListOutputDTO>> GetTrangloEntityList()
        {
            GetTrangloEntityListQuery query = new GetTrangloEntityListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Tranglo Departments
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        /// 
        [HttpGet("tranglo-departments")]
        [SwaggerOperation(OperationId = nameof(GetTrangloDepartmentList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<TrangloDepartmentListOutputDTO>> GetTrangloDepartmentList()
        {
            GetTrangloDepartmentListQuery query = new GetTrangloDepartmentListQuery();
            return await Mediator.Send(query);
        }
        
        /// <summary>
        /// Retrieve list of Company User Account Status
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("company-account-status")]
        [SwaggerOperation(OperationId = nameof(CompanyUserAccountStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<CompanyUserAccountStatusOutputDTO>> CompanyUserAccountStatus()
        {
            GetCompanyUserAccountStatusListQuery query = new GetCompanyUserAccountStatusListQuery();
            return await Mediator.Send(query);
        }
        ///

        /// <summary>
        /// Retrieve list of Company User Block Status
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("company-block-status")]
        [SwaggerOperation(OperationId = nameof(CompanyUserBlockStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<CompanyUserBlockStatusOutputDTO>> CompanyUserBlockStatus()
        {
            GetCompanyUserBlockStatusListQuery query = new GetCompanyUserBlockStatusListQuery();
            return await Mediator.Send(query);
        }

     
        /// <summary>
        /// Retrieve a list of all Timezones
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("timezones")]
        [SwaggerOperation(OperationId = nameof(GetTimezones), Tags = new[] { "Metadata" })]
        public Task<IDictionary<string, string>> GetTimezones()
        {
            var languageCode = CultureInfo.CurrentUICulture.Name;
           
            var listOfTimezones = TZNames.GetDisplayNames(languageCode, useIanaZoneIds: true);
            // DateTime dateTime = DateTime.UtcNow;
            // var getSG = Domain.Common.TimezoneConversion.ConvertFromUTC("Asia/Singapore", languageCode, dateTime);

            return Task.FromResult(listOfTimezones);
        }

        /// <summary>
        /// Retrieve list of Environments
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("environments")]
        [SwaggerOperation(OperationId = nameof(GetEnvironments), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<EnvironmentsOutputDTO>> GetEnvironments()
        {
            GetEnvironmentsQuery query = new GetEnvironmentsQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of User Roles
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("user-roles")]
        [SwaggerOperation(OperationId = nameof(GetUserRoles), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<UserRolesOutputDTO>> GetUserRoles()
        {
            GetUserRolesQuery query = new GetUserRolesQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Get partner agreement status types for dropdown
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("agreement-status-types")]
        [SwaggerOperation(OperationId = nameof(GetPartnerAgreementStatusTypes), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<PartnerAgreementStatusTypeOutputDTO>> GetPartnerAgreementStatusTypes()
        {
            GetPartnerAgreementStatusTypesQuery query = new GetPartnerAgreementStatusTypesQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve external user role statuses
        /// </summary>
        /// <returns></returns>
        [HttpGet("external-user-role-status")]
        [SwaggerOperation(OperationId = nameof(GetExternalUserRoleStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ExternalUserRoleStatusOutputDTO>> GetExternalUserRoleStatus()
        {
            GetExternalUserRoleStatusQuery query = new GetExternalUserRoleStatusQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Role Status
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("role-status")]
        [SwaggerOperation(OperationId = nameof(GetRoleStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<RoleStatusOutputDTO>> GetRoleStatus()
        {
            GetRoleStatusQuery query = new GetRoleStatusQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Authority Level
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("authority-level")]
        [SwaggerOperation(OperationId = nameof(GetRoleStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<AuthorityLevelOutputDTO>> GetAuthorityLevel()
        {
            GetAuthorityLevelQuery query = new GetAuthorityLevelQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Action Code
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("action-operation")]
        [SwaggerOperation(OperationId = nameof(GetActionOperation), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ActionOperationOutputDTO>> GetActionOperation()
        {
            GetActionOperationQuery query = new GetActionOperationQuery();
            return await Mediator.Send(query);
        }

        #region KYC Management #38152
        /// <summary>
        /// Retreive KYC Status listing, ie: Insufficient Verified, Rejected, etc..
        /// </summary>
        /// <returns></returns>
        [HttpGet("kyc-statuses")]
        [SwaggerOperation(OperationId = nameof(GetKYCStatusList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<KYCStatusListOutputDTO>> GetKYCStatusList()
        {
            GetKYCStatusListQuery query = new GetKYCStatusListQuery();
            return await Mediator.Send(query);
        }
        /// <summary>
        /// Retreive Workflow Status listing, ie: Pending, In Progress, Rejected, etc..
        /// </summary>
        /// <returns></returns>
        [HttpGet("workflow-statuses")]
        [SwaggerOperation(OperationId = nameof(GetWorkflowStatusList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<WorkflowStatusListOutputDTO>> GetWorkflowStatusList(long? adminSolution)
        {
            GetWorkflowStatusListQuery query = new GetWorkflowStatusListQuery()
            {
                AdminSolution = adminSolution
            };
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retreive Partner Type List
        /// </summary>
        /// <returns></returns>
        [HttpGet("partner-type")]
        [SwaggerOperation(OperationId = nameof(GetPartnerTypes), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<PartnerTypeOutputDTO>> GetPartnerTypes()
        {
            GetPartnerTypeQuery query = new GetPartnerTypeQuery();
            return await Mediator.Send(query);
        }
        /// <summary>
        /// Retreive Partner Account Status Change Type List
        /// </summary>
        /// <returns></returns>
        [HttpGet("partner-account-status-change-type")]
        [SwaggerOperation(OperationId = nameof(GetPartnerAccountStatusChangeType), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<PartnerAccountStatusChangeTypeOutputDTO>> GetPartnerAccountStatusChangeType()
        {
            GetPartnerAccountStatusChangeTypeListQuery query = new GetPartnerAccountStatusChangeTypeListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retreive Partner Account Status Type List
        /// </summary>
        /// <returns></returns>
        [HttpGet("partner-account-status")]
        [SwaggerOperation(OperationId = nameof(GetPartnerAccountStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<PartnerAccountStatusTypeOutputDTO>> GetPartnerAccountStatus()
        {
            GetPartnerAccountStatusTypeListQuery query = new GetPartnerAccountStatusTypeListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retreive Review Resultlisting, ie: None, Insufficient, Complete, etc..
        /// </summary>
        /// <returns></returns>
        [HttpGet("review-results")]
        [SwaggerOperation(OperationId = nameof(GetReviewResultList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ReviewResultListOutputDTO>> GetReviewResultList()
        {
            GetReviewResultListQuery query = new GetReviewResultListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retreive Partner Onboard Workflow Status
        /// </summary>
        /// <returns></returns>
        [HttpGet("onboard-workflow-statuses")]
        [SwaggerOperation(OperationId = nameof(GetOnboardWorkflowStatusList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<OnboardWorkflowStatusListOutputDTO>> GetOnboardWorkflowStatusList()
        {
            GetOnboardWorkflowStatusListQuery query = new GetOnboardWorkflowStatusListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve SignUp Code Leads Origin List
        /// </summary>
        /// <returns></returns>
        [HttpGet("signup-codes-leads-origin")]
        [SwaggerOperation(OperationId = nameof(GetSignUpCodesLeadsOrigin), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<SignUpCodesLeadsOriginOutputDTO>> GetSignUpCodesLeadsOrigin()
        {
            GetSignUpCodeLeadsOriginListQuery query = new GetSignUpCodeLeadsOriginListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve SignUp Code Account Status List
        /// </summary>
        /// <returns></returns>
        [HttpGet("signup-codes-status")]
        [SwaggerOperation(OperationId = nameof(GetSignUpCodesAccountStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<SignUpCodeAccountStatusOutputDTO>> GetSignUpCodesAccountStatus()
        {
            GetSignUpCodeAccountStatusListQuery query = new GetSignUpCodeAccountStatusListQuery();
            return await Mediator.Send(query);
        }

        #endregion
        /// <summary>
        /// Retrieve list of Incorporation Company Types
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("company-incorporation-types")]
        [SwaggerOperation(OperationId = nameof(GetIncorporationCompanyTypes), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<IncorporationCompanyTypeOutputDTO>> GetIncorporationCompanyTypes()
        {
            GetIncorporationCompanyTypeQuery query = new GetIncorporationCompanyTypeQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Entity Types
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("entity-types")]
        [SwaggerOperation(OperationId = nameof(GetEntityTypes), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<EntityTypesOutputDTO>> GetEntityTypes()
        {
            GetEntityTypesQuery query = new GetEntityTypesQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Relationship Tie Up 
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("relationship-tie-up")]
        [SwaggerOperation(OperationId = nameof(GetRelationshipTieUp), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<RelationshipTieUpOutputDTO>> GetRelationshipTieUp()
        {
            GetRelationshipTieUpQuery query = new GetRelationshipTieUpQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Services Offered 
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("services-offered")]
        [SwaggerOperation(OperationId = nameof(GetServicesOffered), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ServicesOfferedOutputDTO>> GetServicesOffered()
        {
            GetServicesOfferedQuery query = new GetServicesOfferedQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Permission Info for Admin
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("admin-permission-info")]
        [SwaggerOperation(OperationId = nameof(GetAdminPermissionInfo), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<AdminPermissionInfoOutputDTO>> GetAdminPermissionInfo()
        {
            GetAdminPermissionInfoQuery query = new GetAdminPermissionInfoQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Permission Info for Customer
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("customer-permission-info")]
        [SwaggerOperation(OperationId = nameof(GetConnectPermissionInfo), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ConnectPermissionInfoOutputDTO>> GetConnectPermissionInfo(long solutionCode)
        {
            GetConnectPermissionInfoQuery query = new GetConnectPermissionInfoQuery
            {
                SolutionCode = solutionCode
            };
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Question Input Types
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("question-input-types")]
        [SwaggerOperation(OperationId = nameof(GetQuestionInputTypes), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<QuestionInputTypeOutputDTO>> GetQuestionInputTypes()
        {
            GetQuestionInputTypesQuery query = new GetQuestionInputTypesQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Approval Statuses
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("approval-status")]
        [SwaggerOperation(OperationId = nameof(GetApprovalStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ApprovalStatusOutputDTO>> GetApprovalStatus()
        {
            GetApprovalStatusQuery query = new GetApprovalStatusQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Customer Types
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("customer-type")]
        [SwaggerOperation(OperationId = nameof(GetCustomerType), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<CustomerTypeOutputDTO>> GetCustomerType()
        {
            GetCustomerTypeQuery query = new GetCustomerTypeQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Service Types
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("service-type")]
        [SwaggerOperation(OperationId = nameof(GetServiceType), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ServiceTypeOutputDTO>> GetServiceType()
        {
            GetServiceTypeQuery query = new GetServiceTypeQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Collection Tiers
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("collection-tier-type")]
        [SwaggerOperation(OperationId = nameof(GetCollectionTier), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<CollectionTierOutputDTO>> GetCollectionTier()
        {
            GetCollectionTierQuery query = new GetCollectionTierQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Customer Types
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("partner-emails")]
        [SwaggerOperation(OperationId = nameof(GetPartnerUserEmail), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<PartnerUserEmailOutputDTO>> GetPartnerUserEmail()
        {
            GetPartnerUserEmailQuery query = new GetPartnerUserEmailQuery();
            return await Mediator.Send(query);
        }

        

        /// <summary>
        /// Retrieve list of Authorisation Level
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("authorisation-level")]
        [SwaggerOperation(OperationId = nameof(GetAuthorisationLevel), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<AuthorisationLevelOutputDTO>> GetAuthorisationLevel()
        {
            GetAuthorisationLevelQuery query = new GetAuthorisationLevelQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Customer ID Types
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("customerID-types")]
        [SwaggerOperation(OperationId = nameof(GetCustomerIDType), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<VerificationIDTypeOutputDTO>> GetCustomerIDType()
        {
            GetVerificationIDTypeQuery query = new GetVerificationIDTypeQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Customer ID Type Sections
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("customerID-type-sections")]
        [SwaggerOperation(OperationId = nameof(GetCustomerIDTypeSection), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<VerificationIDTypeSectionOutputDTO>> GetCustomerIDTypeSection()
        {
            GetVerificationIDTypeSectionQuery query = new GetVerificationIDTypeSectionQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Verification Status
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("verification-statuses")]
        [SwaggerOperation(OperationId = nameof(GetVerificationStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<VerificationStatusOutputDTO>> GetVerificationStatus(bool? isAdmin)
        {
            GetVerificationStatusQuery query = new GetVerificationStatusQuery()
            {
                IsAdmin = isAdmin
            };
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Submission Result 
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("submission-results")]
        [SwaggerOperation(OperationId = nameof(GetSubmissionResult), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<SubmissionResultOutputDTO>> GetSubmissionResult()
        {
            GetSubmissionResultQuery query = new GetSubmissionResultQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Risk Scores
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("risk-scores")]
        [SwaggerOperation(OperationId = nameof(GetRiskScores), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<RiskScoreOutputDTO>> GetRiskScores()
        {
            GetRiskScoreQuery query = new GetRiskScoreQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Risk Types
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("risk-types")]
        [SwaggerOperation(OperationId = nameof(GetRiskTypes), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<RiskTypeOutputDTO>> GetRiskTypes()
        {
            GetRiskTypeQuery query = new GetRiskTypeQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retreive KYC Workflow Status listing, ie: Pending, In Progress, Rejected, etc..
        /// </summary>
        /// <returns></returns>
        [HttpGet("kyc-workflow-statuses")]
        [SwaggerOperation(OperationId = nameof(GetKYCWorkflowStatusList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<WorkflowStatusListOutputDTO>> GetKYCWorkflowStatusList()
        {
            GetKYCWorkflowStatusListQuery query = new GetKYCWorkflowStatusListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retreive Combined Workflow (KYC + Compliance) Status listing, ie: Pending, In Progress, Rejected, etc..
        /// </summary>
        /// <returns></returns>
        [HttpGet("combined-workflow-statuses")]
        [SwaggerOperation(OperationId = nameof(GetCombinedWorkflowStatusList), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<WorkflowStatusListOutputDTO>> GetCombinedWorkflowStatusList()
        {
            GetCombinedWorkflowStatusListQuery query = new GetCombinedWorkflowStatusListQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Account Status & Block Status listing. ie: Active, Inactive, Pending, Block, Unblock
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("company-user-statuses")]
        [SwaggerOperation(OperationId = nameof(GetCompanyUserStatus), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<CompanyUserStatusOutputDTO>> GetCompanyUserStatus()
        {
            GetCompanyUserStatusQuery query = new GetCompanyUserStatusQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve List of Compliance Requisition Types
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("compliance-requisition-types")]
        [SwaggerOperation(OperationId = nameof(GetRequisitionTypes), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<RequisitionTypeOutputDTO>> GetRequisitionTypes()
        {
            GetRequisitionTypeQuery query = new GetRequisitionTypeQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve List of Risk Ranking
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("risk-ranking")]
        [SwaggerOperation(OperationId = nameof(GetRiskRatings), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<RiskRatingOutputDTO>> GetRiskRatings()
        {
            GetRiskRatingQuery query = new GetRiskRatingQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve List of Compliance Setting
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("compliance-setting")]
        [SwaggerOperation(OperationId = nameof(GetComplianceSettings), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<ComplianceSettingOutputDTO>> GetComplianceSettings()
        {
            GetComplianceSettingQuery query = new GetComplianceSettingQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve List of KYC Reminder Status
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("kyc-reminder-status")]
        [SwaggerOperation(OperationId = nameof(GetKYCReminderStatuses), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<KYCReminderStatusOutputDTO>> GetKYCReminderStatuses()
        {
            GetKYCReminderStatusQuery query = new GetKYCReminderStatusQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of Titles
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("titles")]
        [SwaggerOperation(OperationId = nameof(GetTitles), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<TitleOutputDTO>> GetTitles()
        {
            GetTitlesQuery query = new GetTitlesQuery();
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve List of Partner Registration Leads Origins.
        /// </summary>
        /// <returns></returns>
        [HttpGet("partner-registration-leads-origins")]
        [SwaggerOperation(OperationId = nameof(GetPartnerRegistrationLeadsOrigins), Tags = new[] { "Metadata" })]
        public async Task<IEnumerable<PartnerRegistrationLeadsOriginOutputDTO>> GetPartnerRegistrationLeadsOrigins()
        {
            GetPartnerRegistrationLeadsOriginsQuery query = new GetPartnerRegistrationLeadsOriginsQuery();
            return await Mediator.Send(query);
        }
    }
}
