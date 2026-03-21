using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class PartnerRegistrationLeadsOrigin : Enumeration
    {
        public PartnerRegistrationLeadsOrigin() : base() { }

        public PartnerRegistrationLeadsOrigin(int id, string name) : base(id, name) { }

        public static readonly PartnerRegistrationLeadsOrigin Website = new PartnerRegistrationLeadsOrigin(1, "Website (tranglo.com)");
        public static readonly PartnerRegistrationLeadsOrigin SocialMedia = new PartnerRegistrationLeadsOrigin(2, "Social media (e.g., Facebook, Instagram, LinkedIn, X, TikTok, etc)");
        public static readonly PartnerRegistrationLeadsOrigin GoogleSearch = new PartnerRegistrationLeadsOrigin(3, "Google / search");
        public static readonly PartnerRegistrationLeadsOrigin OnlineAds = new PartnerRegistrationLeadsOrigin(4, "Online ads");
        public static readonly PartnerRegistrationLeadsOrigin EmailNewsletter = new PartnerRegistrationLeadsOrigin(5, "Email newsletter");
        public static readonly PartnerRegistrationLeadsOrigin MediaNewsBlogArticle = new PartnerRegistrationLeadsOrigin(6, "Media news / blog / article");
        public static readonly PartnerRegistrationLeadsOrigin EventWebinar = new PartnerRegistrationLeadsOrigin(7, "Event / webinar");
        public static readonly PartnerRegistrationLeadsOrigin AIChat = new PartnerRegistrationLeadsOrigin(8, "AI chat (e.g., ChatGPT, Copilot, Gemini, etc)");
        public static readonly PartnerRegistrationLeadsOrigin Referral = new PartnerRegistrationLeadsOrigin(9, "Referral (e.g., friend, partner, colleague)");
        public static readonly PartnerRegistrationLeadsOrigin Other = new PartnerRegistrationLeadsOrigin(10, "Other (please specify)");
    }
}
