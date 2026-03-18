using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Common.Constant
{
    public static class PermissionGroupCode
    {
        //Reference to ACL DOC:  https://tranglosdnbhd.sharepoint.com/:x:/r/sites/Tranglo10/_layouts/15/doc2.aspx?sourcedoc=%7B4E92D310-CFD3-4AF9-94AD-E0A0F4A7BBA7%7D&file=ACL%20List.xlsx&action=default&mobileredirect=true&cid=ad595988-ef5b-41de-9d6d-7e0f3fa02ab4

        public const string KYCBusinessProfile = "KYCBusinessProfile"; //Admin: KYC Management_Business Profile //Connect: Business Profile
        public const string KYCLicenseInformation = "KYCLicenseInformation"; //Admin: KYC Management_License Information //Connect: License Information
        public const string KYCOwnershipAndManagementStructure = "KYCOwnershipAndManagementStructure"; //Admin: KYC Management_Ownership //Connect: Ownership
        public const string KYCDocumentation = "KYCDocumentation"; //Admin: KYC Management_Documentation //Connect: Documentations
        public const string KYCAMLCFT = "KYCAMLCFT"; //Admin: KYC Management_AMLCFT //Connect: AMLCFT
        public const string KYCCOInformation = "KYCCOInformation"; //Admin: KYC Management_CO Information //Connect: CO Information
        public const string KYCDeclaration = "KYCDeclaration"; //Admin: KYC Management_Declaration //Connect: Declaration
        public const string KYCSummary = "KYCSummary"; //Admin: KYC Management_Review Summary //Connect: Summary
        public const string KYCManagement = "KYCManagement"; //Admin: KYC Management
        public const string KYCApprovalHistory = "KYCApprovalHistory"; //Admin: KYC Approval History
        public const string KYCOnlineAMLCFTQuestionnaires = "KYCOnlineAMLCFTQuestionnaires"; //Admin: KYC Administration_Online AMLCFT Questionnaires
        public const string KYCTemplatesManagement = "KYCTemplatesManagement"; //Admin: KYC Administration_Templates Management
        public const string KYCSanctionCountryManagement = "KYCSanctionCountryManagement"; //Admin: KYC Administration_Sanction Country Management
        public const string KYCPartnerKYCApprovalList = "KYCPartnerKYCApprovalList"; //Admin: KYC Management_Partner KYC Approval List
        public const string KYCBusinessDeclaration = "KYCBusinessDeclaration"; //Admin: KYC Business_Declaration
        public const string KYCTransactionEvaluation = "KYCTransactionEvaluation"; //Admin: KYC Transaction_Evaluation
        public const string KYCVerification = "KYCVerification"; //Admin: KYC Verification

        public const string PartnerManagement = "PartnerManagement"; //Admin: Manage Partner
        public const string PartnerDetails = "PartnerDetails"; //Admin: Manage Partner_Partner Details
        public const string PartnerSignUpCode = "PartnerSignUpCode"; //Admin: Signup Code
        public const string PartnerAgreement = "PartnerAgreement"; //Admin: Manage Partner_Partner Documents //Connect: Agreement
        public const string PartnerOnboardProgress = "PartnerOnboardProgress"; //Admin: Manage Partner_Onboard Progress //Connect: Home Request Go Live Button
        public const string PartnerAccountStatus = "PartnerAccountStatus"; //Admin: Manage Partner_Account Status
        public const string PartnerRegistration = "PartnerRegistration"; //Admin: Register New Partner
        public const string PartnerManageExternalRole = "PartnerExternalRole"; // Admin: Manage External Roles
        public const string PricingPackage = "PricingPackage"; //Admin: Manage Package
        public const string PricingDefaultPackage = "PricingDefaultPackage"; //Admin: Manage Default Package //Connect: Default Price List
        public const string PricingFee = "PricingFee"; //Admin: Manage Fee //Connect: Assigned Price List
        public const string PartnerUser = "PartnerUser";  //Admin: Manage Partner User //Connect: Manage Staging User
        public const string PartnerAPISetting = "PartnerAPISetting"; //Admin: API Partner List //Connect: API Settings
        public const string TrangloUser = "TrangloUser"; //Admin: Manage Admin User
        public const string TrangloRole = "TrangloRole"; //Admin: Manage Roles
        public const string Compliance = "Compliance"; //Admin:  Compliance
        //public const string CountrySetting = "CountrySetting"; //Admin: Country Setting

        public const string PartnerRequestGoLive = "PartnerRequestGoLive"; //Connect: Request Go Live Button

        //public const string ConnectTeamUser = "ConnectTeamUser";
    }
}
