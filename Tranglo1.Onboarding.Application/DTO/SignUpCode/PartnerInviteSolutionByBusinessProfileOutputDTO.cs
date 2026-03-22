namespace Tranglo1.Onboarding.Application.DTO.SignUpCode
{
    public class PartnerInviteSolutionByBusinessProfileOutputDTO
    {
        public string CompanyName { get; set; }
        public long BusinessProfileCode { get; set; }
        public bool IsTrangloConnectExist { get; set; }
        public bool IsTrangloBusinessExist { get; set; }
    }
}
