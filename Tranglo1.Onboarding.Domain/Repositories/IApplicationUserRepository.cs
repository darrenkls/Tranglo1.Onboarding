using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface IApplicationUserRepository
    {
        Task<List<TrangloStaffAssignment>> GetTrangloStaffAssignmentsByTrangloStaffAsync(TrangloStaff trangloStaff);
        Task<ApplicationUser> GetApplicationUserByLoginId(string loginId);
        Task<ApplicationUser> GetApplicationUserByEmail(Email email);
        Task<CustomerUserRegistration> GetCustomerUserRegistrationsByLoginIdAsync(string loginId);
        Task<CustomerUserRegistration> GetCustomerUserRegistrationsByCompanyNameAsync(string companyName);
        Task<CustomerUserRegistration> GetCustomerUserRegistrationsByCompanyNameAndLoginIdAsync(string companyName, string loginId);
        Task<CustomerUserRegistration> GetCustomerUserRegistrationsByCompanyNameAndLoginIdAndSolutionAsync(string companyName, string loginId, int solution);
        Task<CustomerUserRegistration> UpdateCustomerUserRegistrationsAsync(CustomerUserRegistration customerUserRegistration);
        Task<List<TrangloStaffAssignment>> GetTrangloStaffAssignment(string loginId);
        Task<List<TrangloStaffAssignment>> GetTrangloStaffAssignmentByRole(string roleCode);
        Task<List<TrangloStaffAssignment>> GetTrangloStaffAssignmentByIdAndEntity(string loginId, string entity);
        Task SaveTrangloStaffAssignmentChanges(List<TrangloStaffAssignment> trangloStaffAssignments);
        Task<Result<TrangloStaff>> SaveTrangloUserChanges(TrangloStaff trangloStaff);
        Task AddTrangloUser(TrangloStaff trangloStaff);
        Task AddCustomerUserRegistration(CustomerUserRegistration customerUserRegistration);
        Task<TrangloStaff> GetTrangloUserByLoginId(string loginId);
        Task<TrangloStaffEntityAssignment> GetTrangloStaffEntityAssignment(string loginId, string trangloEntity);
        Task<List<TrangloStaffEntityAssignment>> GetTrangloStaffEntityAssignmentById(string loginId);
        Task<TrangloEntity> GetTrangloEntityByCodeAsync(string trangloEntityCode);
        Task UpdateTrangloStaffEntityAssignment(TrangloStaffEntityAssignment trangloStaffEntityAssignment);
        Task<Result<ApplicationUser>> UpdateApplicationUser(ApplicationUser applicationUser, CancellationToken cancellationToken);
        Task<List<TrangloStaffEntityAssignment>> GetTrangloStaffEntityAssignmentByUserId(int userId);
        Task<ApplicationUser> GetTrangloUserByUserId(int userId);
        Task<CustomerUser> GetCustomerUserAsync(string loginId);
        Task<ApplicationUser> GetApplicationUserByUserId(long userId);
        Task<CompanyUserBlockStatus> GetCompanyUserBlockStatusAsync(CompanyUserBlockStatus status);
    }
}
