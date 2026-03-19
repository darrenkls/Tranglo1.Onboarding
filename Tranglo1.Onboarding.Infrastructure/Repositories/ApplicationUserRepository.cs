using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.Repositories
{
    public class ApplicationUserRepository : IApplicationUserRepository, IStaffEntityQueryService
    {
        private readonly ApplicationUserDbContext _dbContext;

        public ApplicationUserRepository(ApplicationUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<IEnumerable<TrangloStaffEntityAssignment>> GetTrangloStaffEntityAssignmentById(string loginId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TrangloStaffEntityAssignment>> GetTrangloStaffEntityAssignmentByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> GetTrangloUserByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> GetApplicationUserByLoginId(string loginId)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> GetApplicationUserByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerUserRegistration> GetCustomerUserRegistrationsByLoginIdAsync(string loginId)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerUserRegistration> GetCustomerUserRegistrationsByCompanyNameAsync(string companyName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCustomerUserRegistrationsAsync(CustomerUserRegistration registration)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> UpdateApplicationUser(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TrangloEntity> GetTrangloEntityByCodeAsync(string entityCode)
        {
            throw new NotImplementedException();
        }
    }
}
