using System;
using System.Collections.Generic;
using System.Linq;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Events;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class BusinessProfile : AggregateRoot
    {
        public WorkflowStatus WorkflowStatus { get; set; }
        //public long? WorkFlowStatusCode { get; set; }
        public KYCStatus KYCStatus { get; set; }
        //public long? KYCStatusCode { get; set; }
        public DateTime RegistrationDate { get; set; }

        //Phase 3 Solution isnt being used in Business Profile
        public Solution Solution { get; set; }
        public long? SolutionCode { get; set; }
        public Title Title { get; set; }
        public string TitleOthers { get; set; }


        //Only allow BusinessProfile to be added from here
        public void AddBusinessProfile()
        {

        }

        public void UpdateBusinessProfilePartner(string tradeName, BusinessNature businessNature, string companyRegisteredName,
                                                ContactNumber contactNumber, CountryMeta companyRegisteredCountry, Email email,
                                                string iMID, string companyAddress, string zipCodePostCode, string trangloEntity,
                                                string agentLoginId, string contactPersonName, string forOthers, long? RspStagingId,
                                                long? supplierPartnerId, long? customerTypeCode,
                                                string formerRegisteredCompanyName, string aliasName, CountryMeta nationality, long? relationshipTieUpCode,
                                                DateTime? dateOfBirth, BusinessProfileIDType businessProfileIdType,
                                                string idNumber, DateTime? idExpiryDate, ServiceType serviceType, CollectionTier collectionTier, bool isTncTick, Title titleCode, string titleOthers)
        {
            this.CompanyRegistrationName = companyRegisteredName;
            this.TradeName = tradeName;
            this.BusinessNature = businessNature;
            this.ContactNumber = contactNumber;
            this.CompanyRegisteredCountryCode = companyRegisteredCountry?.Id;
            this.CompanyRegisteredAddress = companyAddress;
            this.CompanyRegisteredZipCodePostCode = zipCodePostCode;
            this.ForOthers = forOthers;
            this.AliasName = aliasName;
            this.FormerRegisteredCompanyName = formerRegisteredCompanyName;
            this.NationalityCode = nationality?.Id;
            /*            this.ResidentialAddress = residentialAddress;
                        this.ResidentialZipCodePostCode = residentialAddressZipCodePostCode;
                        this.ResidentialCountryMetaCode = residentialCountry?.Id;*/
            this.RelationshipTieUpCode = relationshipTieUpCode;
            this.DateOfBirth = dateOfBirth;
            this.BusinessProfileIDType = businessProfileIdType;
            this.IDNumber = idNumber;
            this.IDExpiryDate = idExpiryDate;
            this.ServiceType = serviceType;
            this.CollectionTier = collectionTier;

            //this.ContactPersonName = contactPersonName;
            this.SetContactPersonName(contactPersonName, customerTypeCode);

            this.EnsurePartnerIsInitialized(email, isTncTick, trangloEntity, companyRegisteredCountry, customerTypeCode, iMID, agentLoginId, RspStagingId, supplierPartnerId);
            this.Title = titleCode;
            this.TitleOthers = titleOthers;
        }

        public void EnsurePartnerIsInitialized(Email email, bool isTncTick, string trangloEntity, CountryMeta countryISO2, long? customerTypeCode = null, string iMID = null,
            string agentLoginId = null, long? RspStagingId = null, long? supplierPartnerId = null, Solution solution = null,
            PartnerRegistrationLeadsOrigin? leadsOrigin = null, string? otherLeadsOrigin = null)
        {
            BusinessProfileRegisteredEvent businessProfileRegistered =
               new BusinessProfileRegisteredEvent(this, email, customerTypeCode, iMID, trangloEntity, agentLoginId, RspStagingId, supplierPartnerId, solution, isTncTick, leadsOrigin, otherLeadsOrigin, countryISO2);

            this.AddDomainEvent(businessProfileRegistered);
        }


        public void AddBusinessProfileInitial(string companyName, CountryMeta countryCode, string contactPersonName, string trangloEntity, Title titleCode, string titleOthers, int? solutionCode = null, PartnerType partnerType = null)
        {
            this.CompanyName = companyName;

            // To remove stamping of solution in Business Profile as stamping of solution is now catered in Partner Subscription
            // Note: Removal will affect existing data in production (Ensure data is not used anywhere and can be removed first)
            this.SolutionCode = solutionCode;

            var countryCompanyRegistered = CountryMeta.GetCountryByISO2Async(countryCode?.CountryISO2);
            this.CompanyRegisteredCountryCode = countryCompanyRegistered?.Id;
            this.ContactPersonName = contactPersonName;
            this.RegistrationDate = DateTime.UtcNow;
            this.CompanyRegistrationName = companyName;

            this.SetSubmissionStatus(KYCSubmissionStatus.Draft);
            // this.KYCSubmissionStatusCode = KYCSubmissionStatus.Id;
            //Stamp Business Customer User KYCSubmission Status Column to Draft
            this.SetBusinessSubmissionStatus(KYCSubmissionStatus.Draft);
            //#40826
            this.SetSubmissionBasedOnPartnerType(partnerType);

            //Set default service type
            this.SetDefaultServiceType(trangloEntity);

            this.Title = titleCode;
            this.TitleOthers = titleOthers;
        }

        public void SetSubmissionBasedOnPartnerType(PartnerType partnerType)
        {
            //#41634 - Fix bug on submission status not updated
            if (partnerType == PartnerType.Supply_Partner)
            {
                //#40826
                this.SetSubmissionStatus(KYCSubmissionStatus.Submitted);
                this.KYCSubmissionStatusCode = KYCSubmissionStatus.Id;
                this.KYCStatus = KYCStatus.Insufficient_Incomplete;
            }
        }

        public void SetKYCSubmissionStatusBasedOnPartnerType(bool isAllSupplyPartner)
        {
            //#41634 - Fix bug on submission status not updated
            if (isAllSupplyPartner is true)
            {
                //#40826
                this.SetSubmissionStatus(KYCSubmissionStatus.Submitted);
                //this.KYCSubmissionStatusCode = KYCSubmissionStatus.Id;
                if (this.KYCStatus == null)
                {
                    //this.KYCStatusCode = Enumeration.FindById<KYCStatus>(1).Id;
                    this.KYCStatus = KYCStatus.Insufficient_Incomplete;
                }

            }
        }
        public void SetContactPersonName(string contactPersonName, long? customerType)
        {
            this.ContactPersonName = contactPersonName;

            // Check if the CustomerType is individual
            if (customerType == CustomerType.Individual.Id)
            {
                // If the customer type is individual, return early without executing the rest of the logic
                return;
            }

            // Add domain event to also add/update Ownership Authorised Person
            this.AddDomainEvent(new BusinessProfileContactPersonNameUpdatedEvent(this, contactPersonName));
        }
        public void SubmitKYCForReview(long? solution, CollectionTier collectionTier, string customerSolution, int businessProfileCode, ApplicationUser applicationUser)
        {
            //if (customerSolution != null && this.KYCSubmissionStatusCode != KYCSubmissionStatus.Submitted.Id)
            //{
            //    this.AddDomainEvent(new PartnerSubmissionEmailEvent(applicationUser.FullName.Value, customerSolution, businessProfileCode, applicationUser.Id));
            //}

            this.SetSubmissionStatus(KYCSubmissionStatus.Submitted);
            this.SetWorkflowStatus(WorkflowStatus.Compliance_Pending_Review);

            //Register event on submission
            this.AddDomainEvent(new SubmissionResubmissionEmailEvent
                (this.CompanyName, this.KYCSubmissionStatus, this.KYCSubmissionDate, solution, collectionTier, this.KYCSubmissionStatus.Id,
                applicationUser.FullName.Value, customerSolution, businessProfileCode, applicationUser.Id));

            this.KYCSubmissionDate = DateTime.UtcNow;
        }

        public void SubmitBusinessKYCForReview(long? solution, CollectionTier collectionTier, string customerSolution, int businessProfileCode, ApplicationUser applicationUser)
        {

            //set datetime for businesskycsubmissiondate
            if (this.BusinessKYCSubmissionStatus == KYCSubmissionStatus.Submitted)
            {
                this.BusinessKYCSubmissionDate = DateTime.UtcNow;
            }
            if (collectionTier == CollectionTier.Tier_1)
            {
                this.SetBusinessWorkflowStatus(WorkflowStatus.KYC_Operations_Pending_Review);
            }
            if (collectionTier == CollectionTier.Tier_3 || collectionTier == CollectionTier.Tier_2)
            {
                this.SetBusinessWorkflowStatus(WorkflowStatus.Compliance_Pending_Review);
            }

            //if (customerSolution != null && this.BusinessKYCSubmissionStatusCode != KYCSubmissionStatus.Submitted.Id)
            //{
            //    this.AddDomainEvent(new PartnerSubmissionEmailEvent(applicationUser.FullName.Value, customerSolution, businessProfileCode, applicationUser.Id));
            //}


            //Register event on submission
            this.AddDomainEvent(new SubmissionResubmissionEmailEvent
                (this.CompanyName, this.BusinessKYCSubmissionStatus, this.BusinessKYCSubmissionDate, solution, collectionTier, this.BusinessKYCSubmissionStatus.Id,
                applicationUser.FullName.Value, customerSolution, businessProfileCode, applicationUser.Id));

            this.SetBusinessSubmissionStatus(KYCSubmissionStatus.Submitted);

            this.BusinessKYCSubmissionDate = DateTime.UtcNow;
        }

        public void SetKYCStatusToIncomplete(PartnerType partnerType, KYCSubmissionStatus kYCSubmissionStatus)
        {
            this.SetKYCStatus(KYCStatus.Insufficient_Incomplete);
            //if user is supply partner dont set to draft 
            if (partnerType == PartnerType.Supply_Partner)
            {
                if (kYCSubmissionStatus != KYCSubmissionStatus.Submitted)
                {
                    this.SetSubmissionStatus(KYCSubmissionStatus.Submitted);
                }

            }
            else
            {
                if (kYCSubmissionStatus != KYCSubmissionStatus.Draft)
                {
                    this.SetSubmissionStatus(KYCSubmissionStatus.Draft);
                }
            }
            this.SetWorkflowStatus(WorkflowStatus.Compliance_Pending_Review);
            this.ApproveDate = null;
            this.RejectDate = null;
        }

        public void SetKYCStatusToVerified()
        {
            this.SetKYCStatus(KYCStatus.Verified);
            this.SetSubmissionStatus(KYCSubmissionStatus.Submitted);
            this.SetWorkflowStatus(WorkflowStatus.Compliance_Approved);

            this.ApproveDate = DateTime.UtcNow;
            this.RejectDate = null;
        }

        public void SetKYCStatusToRejected(int adminSolution)
        {
            this.AddDomainEvent(new BusinessProfileRejectedEvent(this.Id, this.CompanyName, adminSolution));
            this.SetKYCStatus(KYCStatus.Rejected);
            this.SetSubmissionStatus(KYCSubmissionStatus.Submitted);
            this.SetWorkflowStatus(WorkflowStatus.Compliance_Reject);

            this.RejectDate = DateTime.UtcNow;
            this.ApproveDate = null;
        }
        public void SetKYCStatusToKeepInView()
        {
            this.SetKYCStatus(KYCStatus.Keep_In_View);
        }
        public void SetKYCStatusToTerminated()
        {
            this.SetKYCStatus(KYCStatus.Terminated);
        }
        public void SetKYCStatusToPendingHigherApproval(int adminSolution)
        {
            this.AddDomainEvent(new BusinessProfilePendingHigherApprovalEvent(this.Id, this.CompanyName, adminSolution));
            this.SetKYCStatus(KYCStatus.Pending_Higher_Approval);
        }
        public void SetKYCStatusToDeactivated()
        {
            this.SetKYCStatus(KYCStatus.Deactivated);
        }

        private void SetKYCStatus(KYCStatus kYCStatus)
        {
            if (this.KYCStatus != kYCStatus)
            {
                this.KYCStatus = kYCStatus;
            }
        }

        private void SetSubmissionStatus(KYCSubmissionStatus kYCSubmissionStatus)
        {
            if (this.KYCSubmissionStatus != kYCSubmissionStatus)
            {
                //this.KYCSubmissionStatus = kYCSubmissionStatus;
                this.KYCSubmissionStatusCode = kYCSubmissionStatus.Id;
            }
        }

        private void SetBusinessSubmissionStatus(KYCSubmissionStatus businessKYCSubmissionStatus)
        {
            if (this.BusinessKYCSubmissionStatus != businessKYCSubmissionStatus)
            {
                this.BusinessKYCSubmissionStatus = businessKYCSubmissionStatus;
                this.BusinessKYCSubmissionStatusCode = businessKYCSubmissionStatus.Id;

            }
        }

        public void SetEntityType(EntityType entityType)
        {
            if (this.EntityType != entityType)
            {
                this.EntityType = entityType;
            }
        }
        public void SetRelationshiTieUp(RelationshipTieUp relationshipTieUp)
        {
            if (this.RelationshipTieUp != relationshipTieUp)
            {
                this.RelationshipTieUp = relationshipTieUp;
            }
        }
        private void SetWorkflowStatus(WorkflowStatus workflowStatus)
        {
            if (this.WorkflowStatus != workflowStatus)
            {
                this.WorkflowStatus = workflowStatus;
            }
        }

        public void AssignKYCStatus(KYCStatus kYCStatus, PartnerType partnerType, int adminSolution, KYCSubmissionStatus kycSubmissionStatus)
        {
            if (kYCStatus == KYCStatus.Insufficient_Incomplete)
            {
                this.SetKYCStatusToIncomplete(partnerType, kycSubmissionStatus);
            }
            else if (kYCStatus == KYCStatus.Verified)
            {
                this.SetKYCStatusToVerified();
            }
            else if (kYCStatus == KYCStatus.Rejected)
            {
                this.SetKYCStatusToRejected(adminSolution);
            }
            else if (kYCStatus == KYCStatus.Keep_In_View)
            {
                this.SetKYCStatusToKeepInView();
            }
            else if (kYCStatus == KYCStatus.Terminated)
            {
                this.SetKYCStatusToTerminated();
            }
            else if (kYCStatus == KYCStatus.Pending_Higher_Approval)
            {
                this.SetKYCStatusToPendingHigherApproval(adminSolution);
            }
            else if (kYCStatus == KYCStatus.Deactivated)
            {
                this.SetKYCStatusToDeactivated();
            }
        }

        public void SetComplianceOfficer(string _ComplianceOfficerLoginId, WorkflowStatus workFlowStatus)
        {
            ComplianceOfficerLoginId = _ComplianceOfficerLoginId;
            if (workFlowStatus == null)
            {
                WorkflowStatus = WorkflowStatus.Compliance_Review_In_Progress;
            }
        }

        public void SetBusinessComplianceOfficer(string _ComplianceOfficerLoginId, CollectionTier collectionTier, WorkflowStatus businessWorkflowStatus)
        {
            BusinessComplianceOfficerLoginId = _ComplianceOfficerLoginId;
            if (businessWorkflowStatus == null)
            {
                if (collectionTier == CollectionTier.Tier_1)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.KYC_Operations_In_Progress;
                }
                if (collectionTier == CollectionTier.Tier_2 || collectionTier == CollectionTier.Tier_3)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.Compliance_Review_In_Progress;
                }
            }
            else if
                (businessWorkflowStatus != WorkflowStatus.KYC_Operations_In_Progress && businessWorkflowStatus != WorkflowStatus.Compliance_Review_In_Progress)

            {
                if (collectionTier == CollectionTier.Tier_1)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.KYC_Operations_In_Progress;
                }
                if (collectionTier == CollectionTier.Tier_2 || collectionTier == CollectionTier.Tier_3)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.Compliance_Review_In_Progress;
                }
            }



        }


        // DDD Patterns comment
        // This BusinessProfile AggregateRoot's method "AddCustomerUserBusinessProfile" should be the only way to add Items to the Order,
        // so any behavior (discounts, etc.) and validations are controlled by the AggregateRoot 
        // in order to maintain consistency between the whole Aggregate. 
        public CustomerUserBusinessProfile AddCustomerUserBusinessProfile(CustomerUser customerUser)
        {
            return new CustomerUserBusinessProfile(customerUser, this);
        }

        // DDD Patterns comment
        // Using a private collection field, better for DDD Aggregate's encapsulation
        // so KYC_BusinessProfiles cannot be added from "outside the AggregateRoot" directly to the collection,
        // but only through the method KYCAggrergateRoot.AddKYC_BusinessProfile() which includes behaviour.
        //private readonly KYC_BusinessProfile _kyc_BusinessProfile;

        public string CompanyName { get; set; }
        public string CompanyRegistrationName { get; set; }
        public string TradeName { get; set; }

        public string CompanyRegisteredAddress { get; set; }
        public string CompanyRegisteredZipCodePostCode { get; set; }
        //public string CompanyRegisteredCountryISO2 { get; set; }
        //public Country CompanyRegisteredCountry { get; set; }

        public CountryMeta CompanyRegisteredCountryMeta { get; set; }
        public long? CompanyRegisteredCountryCode { get; set; }

        public string MailingAddress { get; set; }
        public string MailingZipCodePostCode { get; set; }
        //public string MailingCountryISO2 { get; set; }
        //public Country MailingCountry { get; set; }

        public CountryMeta MailingCountryMeta { get; set; }
        public long? MailingCountryCode { get; set; }

        //public string CompanyAddress { get; set; }
        //public int? CompanyAddressPostCode { get; set; }

        public BusinessNature BusinessNature { get; set; }
        //public long? BusinessNatureCode { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public string CorporationType { get; set; }
        public string CompanyRegistrationNo { get; set; }
        public int? NumberOfBranches { get; set; }
        //public int? CallingCode { get; set; }
        public ContactNumber ContactNumber { get; set; }
        public string Website { get; set; }
        public bool? IsCompanyListed { get; set; }
        public string StockExchangeName { get; set; }
        public string StockCode { get; set; }

        // Service offered by your company 
        public bool? IsMoneyTransferRemittance { get; set; }
        public bool? IsForeignCurrencyExchange { get; set; }
        public bool? IsRetailCommercialBankingServices { get; set; }
        public bool? IsForexTrading { get; set; }
        public bool? IsEMoneyEwallet { get; set; }
        public bool? IsIntermediataryRemittance { get; set; }
        public bool? IsCryptocurrency { get; set; }
        public bool? IsOther { get; set; }
        public string OtherReason { get; set; }
        //public DateTime? DateCreated { get; set; }
        //public DateTime? LastModifiedDate { get; set; }
        //public string LastModifiedBy { get; set; }
        public KYCSubmissionStatus KYCSubmissionStatus { get; private set; }
        public long? KYCSubmissionStatusCode { get; private set; }


        public virtual string ComplianceOfficerLoginId { get; private set; }


        public DateTime? ApproveDate { get; set; }
        public DateTime? RejectDate { get; set; }
        public DateTime? KYCSubmissionDate { get; private set; }
        public long? ScreeningCode { get; set; }

        //New Changes
        public string FormerRegisteredCompanyName { get; set; }
        public string ForOthers { get; set; }
        public EntityType EntityType { get; set; }
        public long? EntityTypeCode { get; set; }
        public RelationshipTieUp RelationshipTieUp { get; set; }
        public long? RelationshipTieUpCode { get; set; }
        public IncorporationCompanyType IncorporationCompanyType { get; set; }
        public long? IncorporationCompanyTypeCode { get; set; }
        public string TaxIdentificationNo { get; set; }
        public ContactNumber FacsimileNumber { get; set; }
        public ContactNumber TelephoneNumber { get; set; }
        public string ContactPersonName { get; private set; } //set to private as to ensure encapsulating logic

        //Phase 3 Changes
        public string FullName { get; set; } //To remove: Fullname should be passed in as RegisteredCompanyName for Customer Type: Individual
        public string PersonInChargeName { get; set; } //To remove: Name PIC should be passed in as ContactPersonName for Customer Type: Individual
        public string AliasName { get; set; }
        //public string FormerName { get; set; }
        public CountryMeta NationalityMeta { get; set; }
        public long? NationalityCode { get; set; }


        //Phase 3 Sprint 2 Changes

        public DateTime? DateOfBirth { get; set; }
        public BusinessProfileIDType BusinessProfileIDType { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public ServiceType ServiceType { get; set; }
        public CollectionTier CollectionTier { get; set; }

        public bool? IsMicroEnterprise { get; set; }

        public KYCStatus BusinessKYCStatus { get; set; }
        public WorkflowStatus BusinessWorkflowStatus { get; set; }
        public KYCSubmissionStatus BusinessKYCSubmissionStatus { get; set; }
        public long? BusinessKYCSubmissionStatusCode { get; set; }
        public DateTime? BusinessKYCSubmissionDate { get; private set; }
        public virtual string BusinessComplianceOfficerLoginId { get; private set; }

        public void SetCollectionTierOnTransactionEvaluation(bool isQuestionAnswered)
        {
            if (isQuestionAnswered is true)
            {
                this.CollectionTier = CollectionTier.Tier_2;

                if (this.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_Pending_Review)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.Compliance_Pending_Review;
                }

                if (this.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_In_Progress)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.Compliance_Review_In_Progress;
                }

                if (this.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_Approved)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.Compliance_Approved;
                }

                if (this.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_Reject)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.Compliance_Reject;
                }
            }
        }

        public void SetCollectionTierOnDocumentation(bool isDocumentAttached)
        {
            if (isDocumentAttached is true)
            {
                this.CollectionTier = CollectionTier.Tier_3;

                if (this.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_Pending_Review)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.Compliance_Pending_Review;
                }

                if (this.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_In_Progress)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.Compliance_Review_In_Progress;
                }

                if (this.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_Approved)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.Compliance_Approved;
                }

                if (this.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_Reject)
                {
                    this.BusinessWorkflowStatus = WorkflowStatus.Compliance_Reject;
                }
            }
        }

        public void AssignBusinessKYCStatus(KYCStatus kYCStatus, PartnerType partnerType, int adminSolution, KYCSubmissionStatus kYCSubmissionStatus, CollectionTier collectionTier)
        {
            if (kYCStatus == KYCStatus.Insufficient_Incomplete)
            {
                this.SetBusinessKYCStatusToIncomplete(partnerType, kYCSubmissionStatus, collectionTier);
            }
            else if (kYCStatus == KYCStatus.Verified)
            {
                this.SetBusinessKYCStatusToVerified(collectionTier);
            }
            else if (kYCStatus == KYCStatus.Rejected)
            {
                this.SetBusinessKYCStatusToRejected(adminSolution, collectionTier);
            }
            else if (kYCStatus == KYCStatus.Keep_In_View)
            {
                this.SetBusinessKYCStatusToKeepInView();
            }
            else if (kYCStatus == KYCStatus.Terminated)
            {
                this.SetBusinessKYCStatusToTerminated();
            }
            else if (kYCStatus == KYCStatus.Pending_Higher_Approval)
            {
                this.SetBusinessKYCStatusToPendingHigherApproval(adminSolution);
            }
            else if (kYCStatus == KYCStatus.Deactivated)
            {
                this.SetBusinessKYCStatusToDeactivated();
            }
        }

        private void SetBusinessKYCStatus(KYCStatus kYCStatus)
        {
            if (this.BusinessKYCStatus != kYCStatus)
            {
                this.BusinessKYCStatus = kYCStatus;
            }
        }

        private void SetBusinessWorkflowStatus(WorkflowStatus workflowStatus)
        {
            if (this.BusinessWorkflowStatus != workflowStatus)
            {
                this.BusinessWorkflowStatus = workflowStatus;
            }
        }


        public void SetBusinessKYCStatusToIncomplete(PartnerType partnerType, KYCSubmissionStatus kYCSubmissionStatus, CollectionTier collectionTier)
        {
            this.SetBusinessKYCStatus(KYCStatus.Insufficient_Incomplete);

            this.SetBusinessSubmissionStatus(KYCSubmissionStatus.Draft);


            if (collectionTier == CollectionTier.Tier_1)
            {
                this.SetBusinessWorkflowStatus(WorkflowStatus.KYC_Operations_Pending_Review);
            }
            if (collectionTier == CollectionTier.Tier_2 || collectionTier == CollectionTier.Tier_3)
            {
                this.SetBusinessWorkflowStatus(WorkflowStatus.Compliance_Pending_Review);
            }
            this.ApproveDate = null;
            this.RejectDate = null;
        }

        public void SetBusinessKYCStatusToVerified(CollectionTier collectionTier)
        {
            this.SetBusinessKYCStatus(KYCStatus.Verified);
            this.SetBusinessSubmissionStatus(KYCSubmissionStatus.Submitted);
            if (collectionTier == CollectionTier.Tier_1)
            {
                this.SetBusinessWorkflowStatus(WorkflowStatus.KYC_Operations_Approved);
            }
            if (collectionTier == CollectionTier.Tier_2 || collectionTier == CollectionTier.Tier_3)
            {
                this.SetBusinessWorkflowStatus(WorkflowStatus.Compliance_Approved);
            }
            this.ApproveDate = DateTime.UtcNow;
            this.RejectDate = null;
        }

        public void SetBusinessKYCStatusToRejected(int adminSolution, CollectionTier collectionTier)
        {
            this.AddDomainEvent(new BusinessProfileRejectedEvent(this.Id, this.CompanyName, adminSolution));
            this.SetBusinessKYCStatus(KYCStatus.Rejected);
            this.SetBusinessSubmissionStatus(KYCSubmissionStatus.Submitted);
            if (collectionTier == CollectionTier.Tier_1)
            {
                this.SetBusinessWorkflowStatus(WorkflowStatus.KYC_Operations_Reject);
            }
            if (collectionTier == CollectionTier.Tier_2 || collectionTier == CollectionTier.Tier_3)
            {
                this.SetBusinessWorkflowStatus(WorkflowStatus.Compliance_Reject);
            }
            this.RejectDate = DateTime.UtcNow;
            this.ApproveDate = null;
        }

        public void SetBusinessKYCStatusToKeepInView()
        {
            this.SetBusinessKYCStatus(KYCStatus.Keep_In_View);
        }
        public void SetBusinessKYCStatusToTerminated()
        {
            this.SetBusinessKYCStatus(KYCStatus.Terminated);
        }

        public void SetBusinessKYCStatusToPendingHigherApproval(int adminSolution)
        {
            this.AddDomainEvent(new BusinessProfilePendingHigherApprovalEvent(this.Id, this.CompanyName, adminSolution));
            this.SetBusinessKYCStatus(KYCStatus.Pending_Higher_Approval);
        }
        public void SetBusinessKYCStatusToDeactivated()
        {
            this.SetBusinessKYCStatus(KYCStatus.Deactivated);
        }
        /*
        //Approval can be adjusted later on in a separate table
        public DateTime? LastReviewedDate { get; set; }
        public string Feedback { get; set; }
        public ReviewResult ReviewResult { get; set; }
        public long? ReviewResultCode { get; set; }
        */

        //[Timestamp]
        //public byte[] RowVersion { get; set; }

        public void SetDefaultServiceType(string trangloEntity)
        {
            // Set default Service Type here to cater for both Register New Partner(Admin Portal) & register from Sign Up screen(SSO) flow
            // Default Service Type is required for RBA flow
            if (string.IsNullOrEmpty(trangloEntity) && (this.ServiceType is null))
            {
                if (trangloEntity == TrangloEntity.TSB.TrangloEntityCode)
                {
                    this.ServiceType = ServiceType.Collection_Ownself;
                }
                else
                {
                    this.ServiceType = ServiceType.Collection_Anyone;
                }
            }
        }


        // For review process
        public Guid? ReviewConcurrencyToken { get; set; }
        public DateTime? ReviewConcurrentLastModified { get; set; }

        // For the process of reviewing and sending feedback
        public Guid? ReviewAndFeedbackConcurrencyToken { get; set; }
        public DateTime? ReviewAndFeedbackConcurrentLastModified { get; set; }

        // For saving Business Profile
        public Guid? BusinessProfileConcurrencyToken { get; set; }

        // For saving Transaction Evaluation
        // Why add here?
        //   - Because CustomerBusinessTransactionEvaluationAnswer tbl stores multiple records per business profile.
        //   - The same concurrency token will be stamped in each record. To avoid this, token is stored in aggregate.
        public Guid? TransactionEvalConcurrencyToken { get; set; }

        // Concurrency tokens for each ownership section
        public Guid? ShareholderConcurrencyToken { get; set; }
        public Guid? CompanyShareholderConcurrencyToken { get; set; }
        public Guid? IndividualShareholderConcurrencyToken { get; set; }
        public Guid? LegalEntityConcurrencyToken { get; set; }
        public Guid? CompanyLegalEntityConcurrencyToken { get; set; }
        public Guid? IndividualLegalEntityConcurrencyToken { get; set; }
        public Guid? ParentHoldingsConcurrencyToken { get; set; }
        public Guid? BoardOfDirectorConcurrencyToken { get; set; }
        public Guid? PrimaryOfficerConcurrencyToken { get; set; }
        public Guid? PoliticalExposedPersonsConcurrencyToken { get; set; }
        public Guid? AffiliatesAndSubsidiariesConcurrencyToken { get; set; }
        public Guid? AuthorisedPersonConcurrencyToken { get; set; }

        //Concurrency Token for AML
        public Guid? AMLCFTQuestionnaireConcurrencyToken { get; set; }

        //ticket 55839
        public string SSTRegistrationNumber { get; set; }
        //ticket TBT951
        public string SenderCity { get; set; }

        #region Navigation Properties
        public ICollection<PartnerRegistration> PartnerRegistrations { get; set; } = new List<PartnerRegistration>();
        #endregion Navigation Properties


        public List<Solution> GetSolutions()
        {
            return PartnerRegistrations.SelectMany(pr => pr.PartnerSubscriptions)
                .Select(ps => ps.Solution)
                .ToList();
        }
    }
}
