using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Application.DTO.EmailNotification
{
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
        public string Email { get; set; }
        public DateTime? RegisteredDate { get; set; }
        public bool IsMultipleSolutions { get; set; }
        public string UserId { get; set; }
        public string Entity { get; set; }
        public string CurrentUserName { get; set; }
        public string InviterCompanyName { get; set; }
        public string Inviter { get; set; }
        public string CompanyName { get; set; }
        public int? BusinessProfileCode { get; set; }
        public string UnsubscribeUrl { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public int AutoRejectPartnerApplicationExpiredDays { get; set; }
        public string LoginUrl { get; set; }
        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public string Typer { get; set; }
    }
}
