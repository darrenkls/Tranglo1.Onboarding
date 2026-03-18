using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Common.Constant
{
    public static class Permission
    {
        #region KYC
        //CodePrefix and ActionCode wont change even if product side decide to change the naming of action or group name.
        public static class KYCManagement 
        {
            public const string CodePrefix = "KYCManagement";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_UpdateOrSubmitForApproval_Code = CodePrefix + "UpdateOrSubmitForApproval";
            public const string Action_NotifyUser_Code = CodePrefix + "NotifyUser";
        }
        public static class KYCManagementBusinessDeclaration 
        {
            public const string CodePrefix = "KYCManagement_BusinessDeclaration";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Unblock_Code = CodePrefix + "Unblock";
        }
        public static class KYCManagementBusinessProfile
        {
            public const string CodePrefix = "KYCManagement_BusinessProfile";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
            public const string Action_ViewServiceTypeField_Code = CodePrefix + "ViewServiceTypeField";
            public const string Action_ViewCollectionTierField_Code = CodePrefix + "ViewCollectionTierField";
            public const string Action_EditServiceTypeField_Code = CodePrefix + "EditServiceTypeField";
            public const string Action_EditCollectionTierField_Code = CodePrefix + "EditCollectionTierField";
        }
        public static class KYCManagementLicenseInformation
        {
            public const string CodePrefix = "KYCManagement_LicenseInformation";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
        }
        public static class KYCManagementTransactionEvaluation
        {
            public const string CodePrefix = "KYCManagement_TransactionEvaluation";
            public const string Action_View_Code = CodePrefix + "View";
        }
        public static class KYCManagementOwnership
        {
            public const string CodePrefix = "KYCManagement_Ownership";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
        }
        public static class KYCManagementDocumentation
        {
            public const string CodePrefix = "KYCManagement_Documentation";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_ReviewVerify_Code = CodePrefix + "ReviewVerify";
            public const string Action_Upload_Code = CodePrefix + "Upload";
            public const string Action_Remove_Code = CodePrefix + "Remove";
            public const string Action_Comment_Code = CodePrefix + "Comment";
            public const string Action_ReviewResult_Code = CodePrefix + "ReviewResult";
            public const string Action_ReviewRemark_Code = CodePrefix + "ReviewRemark";
            public const string Action_ReleaseDocument_Code = CodePrefix + "ReleaseDocument";
            public const string Action_InternalDocument_Code = CodePrefix + "InternalDocument";
        }
        public static class KYCManagementAMLCFT
        {
            public const string CodePrefix = "KYCManagement_AMLCFT";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
        }
        public static class KYCManagementCOInformation
        {
            public const string CodePrefix = "KYCManagement_COInformation";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
        }
        public static class KYCManagementVerification
        {
            public const string CodePrefix = "KYCManagement_Verification";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
        }
        public static class KYCManagementDeclaration
        {
            public const string CodePrefix = "KYCManagement_Declaration";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
            public const string Action_Submit_Review_Code = CodePrefix + "SubmitForReview";
        }
        public static class KYCManagementReviewSummary
        {
            public const string CodePrefix = "KYCManagement_ReviewSummary";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Add_Code = CodePrefix + "Add";
            public const string Action_Edit_Code = CodePrefix + "Edit";
        }
        public static class KYCAdministration 
        {
            public const string CodePrefix = "KYCAdministration";
            public const string Action_View_Code = CodePrefix + "View";
        }
        public static class KYCAdministrationOnlineAMLCFTQuestionnaires 
        {
            public const string CodePrefix = "KYCAdministration_OnlineAMLCFTQuestionnaires";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Add_Code = CodePrefix + "Add";
        }
        public static class KYCAdministrationTemplatesManagement
        {
            public const string CodePrefix = "KYCAdministration_TemplatesManagement";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Upload_Code = CodePrefix + "Upload";
        }
        public static class KYCAdministrationSanctionCountryManagement 
        {
            public const string CodePrefix = "KYCAdministration_SanctionCountryManagement";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Add_Code = CodePrefix + "Add";
            public const string Action_Edit_Code = CodePrefix + "Edit";
            public const string Action_Remove_Code = CodePrefix + "Remove";
        }
        public static class KYCManagementPartnerKYCApprovalList 
        {
            public const string CodePrefix = "KYCManagement_PartnerKYCApprovalList";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Approval_Code = CodePrefix + "Approval";
        }

        public static class KYCApprovalHistory
        {
            public const string CodePrefix = "KYCApprovalHistory";
            public const string Action_View_Code = CodePrefix + "View";
        }
        #endregion

        #region Compliance
        public static class Compliance
        {
            public const string CodePrefix = "Compliance_PersonnelWatchlist";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Upload_Code = CodePrefix + "Action";
            public const string Action_KIV_Code = CodePrefix + "Action";
        }

        #endregion

        #region Partner

        public static class ManagePartner
        {
            public const string CodePrefix = "ManagePartner";
            public const string Action_View_Code = CodePrefix + "View";
            
        }
        public static class ManagePartnerPartnerDetails 
        {
            public const string CodePrefix = "ManagePartner_PartnerDetails";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
        }
        public static class ManagePartnerOnboardProgress
        {
            public const string CodePrefix = "ManagePartner_OnboardProgress";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Update_Code = CodePrefix + "Update";
            public const string Action_Approve_Code = CodePrefix + "ApproveToGoLive";
        }
        public static class ManagePartnerPartnerDocuments
        {
            public const string CodePrefix = "ManagePartner_PartnerDocuments";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_DocumentToBeSignedUpload_Code = CodePrefix + "DocumentToBeSignedUpload";
            public const string Action_DocumentToBeSignedRemove_Code = CodePrefix + "DocumentToBeSignedRemove";
            public const string Action_StatusUpdate_Code = CodePrefix + "StatusUpdate";
            public const string Action_HelloSignAdd_Code = CodePrefix + "HelloSignAdd";
            public const string Action_DocumentRecordsUpload_Code = CodePrefix + "DocumentRecordsUpload";
            public const string Action_DocumentRecordsRemove_Code = CodePrefix + "DocumentRecordsRemove";
            public const string Action_HelloSignRemove_Code = CodePrefix + "HelloSignRemove";
        }
        public static class ManagePartnerAccountStatus 
        {
            public const string CodePrefix = "ManagePartner_AccountStatus";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
        }
        public static class RegisterNewPartner
        {
            public const string CodePrefix = "RegisterNewPartner";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
        }
        public static class SignupCode
        {
            public const string CodePrefix = "SignupCode";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_GenerateNewCode_Code = CodePrefix + "GenerateSignUpCode";
        }
        public static class ManagePartnerUser 
        {
            public const string CodePrefix = "ManagePartnerUser";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_ViewDetail_Code = CodePrefix + "ViewDetail";
            public const string Action_InviteUser_Code = CodePrefix + "InviteUser";
            public const string Action_EditUser_Code = CodePrefix + "EditUser";
            public const string Action_ResendVerificationEmail_Code = CodePrefix + "ResendVerificationEmail";
        }
        public static class ManageExternalRoles 
        {
            public const string CodePrefix = "ManageExternalRoles";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Edit_Code = CodePrefix + "Edit";
            public const string Action_Add_Code = CodePrefix + "Add";
            public const string Action_Disable_Code = CodePrefix + "Disable";
        }
        #endregion

        #region API Settings

        public static class APISettings 
        {
            public const string CodePrefix = "APISettings";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Action_Code = CodePrefix + "Action";
        }
        #endregion

        #region Tranglo User
        public static class ManageAdminUser
        {
            public const string CodePrefix = "ManageAdminUser";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Add_Code = CodePrefix + "Add";
            public const string Action_Edit_Code = CodePrefix + "Edit";
            public const string Action_Deactivate_Code = CodePrefix + "Deactivate";
            public const string Action_Block_Code = CodePrefix + "Block";
        }
        #endregion

        #region Administration 
        public static class ManageRoles
        {
            public const string CodePrefix = "ManageRoles";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Add_Code = CodePrefix + "Add";
            public const string Action_Edit_Code = CodePrefix + "Edit";
            public const string Action_EnableDisable = CodePrefix + "EnableDisable";
        }

        #endregion

        #region Agreement
        public static class Agreement
        {
            public const string CodePrefix = "Agreement";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Add_Code = CodePrefix + "Add";
            public const string Action_Upload_Code = CodePrefix + "Upload";
            public const string Action_Remove_Code = CodePrefix + "Remove";

        }
        #endregion

        #region Team 
        public static class ManageStagingUser
        {
            public const string CodePrefix = "ManageStagingUser";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_InviteUser_Code = CodePrefix + "InviteUser";
            public const string Action_Update_Code = CodePrefix + "Update";
            public const string Action_Block_Code = CodePrefix + "Block";

        }
        #endregion

        #region Home
        public static class RequestGoLiveButton
        {
            public const string CodePrefix = "RequestGoLiveButton";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_RequestGoLive_Code = CodePrefix + "RequestGoLive";
        }
        #endregion

        #region Comments
        public static class Comments
        {
            public const string CodePrefix = "Comments";
            public const string Action_View_Code = CodePrefix + "View";
            public const string Action_Add_Code = CodePrefix + "Add";
            public const string Action_Edit_Code = CodePrefix + "Edit";
        }
        #endregion

    }

    public enum PortalCode
    {
        Admin = 0,
        Connect = 1,
        Business = 2,
    }
}
