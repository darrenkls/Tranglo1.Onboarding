using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.KYCEmailReminderScheduler.Notification
{
    public class SendNotificationInput
    {
        public List<RecipientsInputDTO> recipients { get; set; }

        public List<RecipientsInputDTO> bcc { get; set; }

        public List<RecipientsInputDTO> cc { get; set; }

        public string subject { get; set; }

        public string body { get; set; }
        public string Module { get; set; }
        public string SubModule { get; set; }
    }

    public class RecipientsInputDTO
    {
        public string email { get; set; }
        public string name { get; set; }
        public string fullname { get; set; }
    }

    public class EmailRequestResponse
    {
        public string RequetId { get; set; }
    }

    public class EmailNotificationInputDTO
    {
        public List<RecipientsInputDTO> recipients { get; set; }

        public List<RecipientsInputDTO> bcc { get; set; }

        public List<RecipientsInputDTO> cc { get; set; }

        public string subject { get; set; }

        public string body { get; set; }

        public string Module { get; set; }

        public string SubModule { get; set; }

        public NotificationTypes NotificationType { get; set; }
        public string NotificationTemplate { get; set; }
        public IFormFile[] Attachments { get; set; }
        public Solution Solution { get; set; }
        public string SolutionName { get; set; }
        public string Url { get; set; }
        public string ConfirmationPlaceHolder { get; set; }
        public string Token { get; set; }

        public string RecipientName { get; set; }
        public string Otp { get; set; }

        //Sign Up Verification
        //public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime? RegisteredDate { get; set; }
        //public int? SolutionCode { get; set; }
        public bool IsMultipleSolutions { get; set; }

        // New User Invitation
        public string UserId { get; set; }
        public string Entity { get; set; }

        // Current User Invitation
        public string CurrentUserName { get; set; }
        public string InviterCompanyName { get; set; }

        //Customer Invitation Submit
        public string Inviter { get; set; }
        public string CompanyName { get; set; }
        public int? BusinessProfileCode { get; set; }
        public string UnsubscribeUrl { get; set; }
        public long PartnerSubscriptionCode { get; set; }

        public int AutoRejectPartnerApplicationExpiredDays { get; set; }
    }
}
