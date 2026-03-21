namespace Tranglo1.Onboarding.Domain.Entities
{
    public class TrangloStaffEntityAssignment
    {
        public string LoginId { get; set; }
        public string TrangloEntity { get; set; }
        public CompanyUserBlockStatus BlockStatus { get; set; }
        public CompanyUserAccountStatus AccountStatus { get; set; }
        public int UserId { get; set; }

        public TrangloStaffEntityAssignment(string entity, string loginId)
        {
            TrangloEntity = entity;
            LoginId = loginId;
        }

        private TrangloStaffEntityAssignment() { }
    }
}
