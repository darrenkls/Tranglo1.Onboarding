using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface IApplicationUserRepository
    {
        Task<IEnumerable<TrangloStaffEntityAssignment>> GetTrangloStaffEntityAssignmentById(string loginId);
        Task<IEnumerable<TrangloStaffEntityAssignment>> GetTrangloStaffEntityAssignmentByUserId(string userId);
        Task<ApplicationUser> GetTrangloUserByUserId(int userId);
        Task<ApplicationUser> GetApplicationUserByLoginId(string loginId);
        Task<ApplicationUser> GetApplicationUserByUserId(string userId);
        Task<CustomerUserRegistration> GetCustomerUserRegistrationsByLoginIdAsync(string loginId);
        Task<CustomerUserRegistration> GetCustomerUserRegistrationsByCompanyNameAsync(string companyName);
        Task UpdateCustomerUserRegistrationsAsync(CustomerUserRegistration registration);
        Task<ApplicationUser> UpdateApplicationUser(ApplicationUser user, CancellationToken cancellationToken);
        Task<TrangloEntity> GetTrangloEntityByCodeAsync(string entityCode);
    }
}
