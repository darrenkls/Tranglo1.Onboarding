namespace Tranglo1.Onboarding.Domain.Entities
{
    public class SendNotification
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string UserName { get; set; }
        public string RedirectUri { get; set; }
        public NotificationTypes NotificationTypes { get; set; }
        public NotificationTemplate? NotificationTemplates { get; set; }
    }
}
