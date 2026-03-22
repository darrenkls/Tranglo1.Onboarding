using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class NotificationTemplate : Enumeration
    {
        public NotificationTemplate() : base() { }

        public NotificationTemplate(int id, string name) : base(id, name) { }

        public static readonly NotificationTemplate SignUpVerification = new NotificationTemplate(1, "SignUpVerification");
        public static readonly NotificationTemplate SignUpSuccessful = new NotificationTemplate(2, "SignUpSuccessful");
        public static readonly NotificationTemplate PasswordReset = new NotificationTemplate(3, "PasswordReset");
        public static readonly NotificationTemplate InviteUserExistingUser = new NotificationTemplate(4, "InviteUserExistingUser");
        public static readonly NotificationTemplate InviteUserNewUser = new NotificationTemplate(5, "InviteUserNewUser");
        public static readonly NotificationTemplate ApproveReviewResult = new NotificationTemplate(6, "ApproveReviewResult");
        public static readonly NotificationTemplate RejectReviewResult = new NotificationTemplate(7, "RejectReviewResult");
        public static readonly NotificationTemplate PendingReviewResult = new NotificationTemplate(8, "PendingReviewResult");
        public static readonly NotificationTemplate IncompleteReviewResult = new NotificationTemplate(9, "IncompleteReviewResult");
        public static readonly NotificationTemplate ResubmissionReview = new NotificationTemplate(10, "ResubmissionReview");
        public static readonly NotificationTemplate SubmissionReview = new NotificationTemplate(11, "SubmissionReview");
        public static readonly NotificationTemplate DocumentReleased = new NotificationTemplate(12, "DocumentReleased");
        public static readonly NotificationTemplate WatchlistReview = new NotificationTemplate(13, "WatchlistReview");
        public static readonly NotificationTemplate ActivePartnerAccountStatus = new NotificationTemplate(14, "ActivePartnerAccountStatus");
        public static readonly NotificationTemplate InactivePartnerAccountStatus = new NotificationTemplate(15, "InactivePartnerAccountStatus");
        public static readonly NotificationTemplate PartnerOnboaringStatus = new NotificationTemplate(16, "PartnerOnboaringStatus");
        public static readonly NotificationTemplate PartnerOnboaringStatusGoLive = new NotificationTemplate(17, "PartnerOnboaringStatusGoLive");
        public static readonly NotificationTemplate IPWhitelisted = new NotificationTemplate(18, "IPWhitelisted");
        public static readonly NotificationTemplate CallbackURLConfigured = new NotificationTemplate(19, "CallbackURLConfigured");
        public static readonly NotificationTemplate IPAddressSubmitted = new NotificationTemplate(20, "IPAddressSubmitted");
        public static readonly NotificationTemplate CallbackURLSubmitted = new NotificationTemplate(21, "CallbackURLSubmitted");
        public static readonly NotificationTemplate PartnerConfirmation = new NotificationTemplate(22, "PartnerConfirmation");
        public static readonly NotificationTemplate AdminApprovePartnrGoLive = new NotificationTemplate(23, "AdminApprovePartnrGoLive");
        public static readonly NotificationTemplate PartnerRegistration = new NotificationTemplate(24, "PartnerRegistration");
        public static readonly NotificationTemplate SupplyPartnerDocumentReleased = new NotificationTemplate(25, "SupplyPartnerDocumentReleased");
        public static readonly NotificationTemplate KYCPartnerKYCRequisitionNotification = new NotificationTemplate(26, "KYCPartnerKYCRequisitionNotification");
        public static readonly NotificationTemplate AddSubscription = new NotificationTemplate(27, "AddSubscription");
        public static readonly NotificationTemplate AdminEKYCVerificationStatus = new NotificationTemplate(28, "AdminEKYCVerificationStatus");
        public static readonly NotificationTemplate CustomerEKYCVerificationStatus = new NotificationTemplate(29, "CustomerEKYCVerificationStatus");
        public static readonly NotificationTemplate AdminKYCVerifiedStatus = new NotificationTemplate(30, "AdminKYCVerifiedStatus");
        public static readonly NotificationTemplate ImcompleteTBKYCReminder = new NotificationTemplate(31, "ImcompleteTBKYCReminder");
        public static readonly NotificationTemplate PartnerNameChangeNotification = new NotificationTemplate(32, "PartnerNameChangeNotification");
        public static readonly NotificationTemplate AutoRejectPartnerApplication = new NotificationTemplate(33, "AutoRejectPartnerApplication");
        public static readonly NotificationTemplate ManualRejectPartnerApplication = new NotificationTemplate(34, "ManualRejectPartnerApplication");
        public static readonly NotificationTemplate ReactivatePartnerAccount = new NotificationTemplate(35, "ReactivatePartnerAccount");
        public static readonly NotificationTemplate Reset2faRequest = new NotificationTemplate(36, "Reset2faRequest");
        public static readonly NotificationTemplate Reset2faSuccessful = new NotificationTemplate(37, "Reset2faSuccessful");
        public static readonly NotificationTemplate Register2faSuccessful = new NotificationTemplate(38, "Register2faSuccessful");
    }
}
