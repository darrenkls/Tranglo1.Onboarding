namespace Tranglo1.Onboarding.Domain.Entities
{
    public class TrangloStaffAssignment
    {
        public string LoginId { get; set; }
        public string TrangloEntity { get; set; }
        public string RoleCode { get; set; }
        public TrangloDepartment TrangloDepartment { get; set; }
        public long TrangloDepartmentCode { get; set; }

        public TrangloStaffAssignment(long department, string role, string entity, string loginId)
        {
            TrangloDepartmentCode = department;
            TrangloEntity = entity;
            RoleCode = role;
            LoginId = loginId;
        }

        public TrangloStaffAssignment() { }
    }
}
