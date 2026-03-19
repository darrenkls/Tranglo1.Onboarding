using System;
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

        public Task<ExternalUserRole> GetInitialRoleAsync(int solutionCode)
        {
            throw new NotImplementedException();
        }

        public Task<ExternalUserRole> GetExternalRoleByRoleCodeAsync(string roleCode)
        {
            throw new NotImplementedException();
        }
    }
}
