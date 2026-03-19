using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class EmailRecipient : Entity
    {
        public RecipientType RecipientType { get; set; }
        public string Email { get; set; }
        public long? CollectionTierCode { get; set; }
        public long? NotificationTemplateCode { get; set; }
        public long? AuthorityLevelCode { get; set; }
    }
}
