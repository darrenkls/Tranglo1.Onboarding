using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface ITrangloRoleRepository
    {
        Task<TrangloRoleResult> GetTrangloRoleByCodeAsync(string roleCode);
    }

    public class TrangloRoleResult
    {
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public bool? IsSuperApprover { get; set; }
    }
}
