using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.ExternalUserRoleAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.Repositories
{
    public class ExternalUserRoleRepository : IExternalUserRoleRepository
    {
        private readonly ExternalUserRoleDbContext _dbContext;

        public ExternalUserRoleRepository(ExternalUserRoleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ExternalUserRole> GetInitialRoleAsync(int solutionCode)
        {
			var query = await _dbContext.ExternalUserRoles
				.Include(x => x.Status)
				.Include(x => x.Solution)
				.Where(x => x.ExternalUserRoleName == "System Admin" && x.Solution.Id == solutionCode)
				.FirstOrDefaultAsync();

			return query;
		}

        public async Task<ExternalUserRole> GetExternalRoleByRoleCodeAsync(string roleCode)
        {
			return await _dbContext.ExternalUserRoles
				.Include(r => r.Solution)
				.FirstOrDefaultAsync(x => x.RoleCode == roleCode);
		}

		public async Task<List<ExternalUserRole>> GetAllExternalUserRolesBySolution(long solutionCode)
		{
			var query = await _dbContext.ExternalUserRoles
				.Where(x => x.Solution.Id == solutionCode)
				.ToListAsync();

			return query;
		}
	}
}
