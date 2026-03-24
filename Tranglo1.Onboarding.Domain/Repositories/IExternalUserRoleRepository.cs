using System.Collections.Generic;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.ExternalUserRoleAggregate;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface IExternalUserRoleRepository
    {
        Task<ExternalUserRole> GetInitialRoleAsync(int solutionCode);
        Task<ExternalUserRole> GetExternalRoleByRoleCodeAsync(string roleCode);
		Task<List<ExternalUserRole>> GetAllExternalUserRolesBySolution(long solutionCode);
	}
}
