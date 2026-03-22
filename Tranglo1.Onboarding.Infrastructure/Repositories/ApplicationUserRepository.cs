using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.Repositories
{
    public class ApplicationUserRepository : IApplicationUserRepository
    {
        private readonly ApplicationUserDbContext _dbContext;

        public ApplicationUserRepository(ApplicationUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<TrangloStaffAssignment>> GetTrangloStaffAssignmentsByTrangloStaffAsync(TrangloStaff trangloStaff)
        {
            if (trangloStaff != null)
            {
                return await _dbContext.TrangloStaffAssignments
                    .Where(x => x.LoginId == trangloStaff.LoginId)
                    .ToListAsync();
            }

            return await Task.FromResult<List<TrangloStaffAssignment>>(null);
        }

        public async Task<ApplicationUser> GetApplicationUserByLoginId(string loginId)
        {
            if (loginId != null)
            {
                return await _dbContext.ApplicationUsers
                    .Include(x => x.AccountStatus)
                    .FirstOrDefaultAsync(x => x.LoginId == loginId);
            }

            return await Task.FromResult<ApplicationUser>(null);
        }

        public async Task<ApplicationUser> GetApplicationUserByEmail(Email email)
        {
            if (email != null)
            {
                return await _dbContext.ApplicationUsers
                    .FirstOrDefaultAsync(x => x.Email == email);
            }

            return await Task.FromResult<ApplicationUser>(null);
        }

        public async Task<CustomerUserRegistration> GetCustomerUserRegistrationsByLoginIdAsync(string loginId)
        {
            if (loginId != null)
            {
                return await _dbContext.CustomerUserRegistrations
                    .Include(x => x.PartnerRegistrationLeadsOrigin)
                    .FirstOrDefaultAsync(x => x.LoginId == loginId);
            }

            return await Task.FromResult<CustomerUserRegistration>(null);
        }

        public async Task<CustomerUserRegistration> GetCustomerUserRegistrationsByCompanyNameAsync(string companyName)
        {
            return await _dbContext.CustomerUserRegistrations
                .FirstOrDefaultAsync(x => x.CompanyName == companyName);
        }

        public async Task<CustomerUserRegistration> GetCustomerUserRegistrationsByCompanyNameAndLoginIdAsync(string companyName, string loginId)
        {
            return await _dbContext.CustomerUserRegistrations
                .FirstOrDefaultAsync(x => x.CompanyName == companyName && x.LoginId == loginId);
        }

        public async Task<CustomerUserRegistration> GetCustomerUserRegistrationsByCompanyNameAndLoginIdAndSolutionAsync(string companyName, string loginId, int solution)
        {
            return await _dbContext.CustomerUserRegistrations
                .FirstOrDefaultAsync(x => x.CompanyName == companyName && x.LoginId == loginId && x.SolutionCode == solution);
        }

        public async Task<CustomerUserRegistration> UpdateCustomerUserRegistrationsAsync(CustomerUserRegistration customerUserRegistration)
        {
            _dbContext.Update(customerUserRegistration);
            await _dbContext.SaveChangesAsync();

            return customerUserRegistration;
        }

        public async Task<List<TrangloStaffAssignment>> GetTrangloStaffAssignment(string loginId)
        {
            if (loginId != null)
            {
                return await _dbContext.TrangloStaffAssignments
                    .Where(x => x.LoginId == loginId)
                    .ToListAsync();
            }

            return await Task.FromResult<List<TrangloStaffAssignment>>(null);
        }

        public async Task<List<TrangloStaffAssignment>> GetTrangloStaffAssignmentByRole(string roleCode)
        {
            return await _dbContext.TrangloStaffAssignments
                .Where(x => x.RoleCode == roleCode)
                .ToListAsync();
        }

        public async Task<List<TrangloStaffAssignment>> GetTrangloStaffAssignmentByIdAndEntity(string loginId, string entity)
        {
            if (loginId != null)
            {
                return await _dbContext.TrangloStaffAssignments
                    .Where(x => x.LoginId == loginId && x.TrangloEntity == entity)
                    .ToListAsync();
            }

            return await Task.FromResult<List<TrangloStaffAssignment>>(null);
        }

        public async Task SaveTrangloStaffAssignmentChanges(List<TrangloStaffAssignment> trangloStaffAssignments)
        {
            _dbContext.AttachRange(trangloStaffAssignments);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Result<TrangloStaff>> SaveTrangloUserChanges(TrangloStaff trangloStaff)
        {
            _dbContext.Update(trangloStaff);
            await _dbContext.SaveChangesAsync();

            return trangloStaff;
        }

        public async Task AddTrangloUser(TrangloStaff trangloStaff)
        {
            _dbContext.Add(trangloStaff);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddCustomerUserRegistration(CustomerUserRegistration customerUserRegistration)
        {
            _dbContext.Add(customerUserRegistration);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<TrangloStaff> GetTrangloUserByLoginId(string loginId)
        {
            return await _dbContext.TrangloStaffs
                .Include(x => x.TrangloStaffAssignments)
                .Include(x => x.TrangloStaffEntityAssignments)
                .FirstOrDefaultAsync(x => x.LoginId == loginId);
        }

        public async Task<TrangloStaffEntityAssignment> GetTrangloStaffEntityAssignment(string loginId, string trangloEntity)
        {
            return await _dbContext.TrangloStaffEntityAssignment
                .Where(x => x.LoginId == loginId && x.TrangloEntity == trangloEntity)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TrangloStaffEntityAssignment>> GetTrangloStaffEntityAssignmentById(string loginId)
        {
            return await _dbContext.TrangloStaffEntityAssignment
                .Where(x => x.LoginId == loginId)
                .Include(x => x.AccountStatus)
                .Include(x => x.BlockStatus)
                .ToListAsync();
        }

        public async Task<TrangloEntity> GetTrangloEntityByCodeAsync(string trangloEntityCode)
        {
            return await _dbContext.TrangloEntity
                .Where(x => x.TrangloEntityCode == trangloEntityCode)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateTrangloStaffEntityAssignment(TrangloStaffEntityAssignment trangloStaffEntityAssignment)
        {
            _dbContext.AttachRange(trangloStaffEntityAssignment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Result<ApplicationUser>> UpdateApplicationUser(ApplicationUser applicationUser, CancellationToken cancellationToken)
        {
            _dbContext.Update(applicationUser);
            await _dbContext.SaveChangesAsync();

            return applicationUser;
        }

        public async Task<List<TrangloStaffEntityAssignment>> GetTrangloStaffEntityAssignmentByUserId(int userId)
        {
            return await _dbContext.TrangloStaffEntityAssignment
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<ApplicationUser> GetTrangloUserByUserId(int userId)
        {
            return await _dbContext.ApplicationUsers
                .Where(x => x.Id == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<CustomerUser> GetCustomerUserAsync(string loginId)
        {
            return await _dbContext.CustomerUsers
                .Where(x => x.LoginId == loginId)
                .FirstOrDefaultAsync();
        }

        public async Task<ApplicationUser> GetApplicationUserByUserId(long userId)
        {
            if (userId != 0)
            {
                return await _dbContext.ApplicationUsers
                    .FirstOrDefaultAsync(x => x.Id == userId);
            }

            return await Task.FromResult<ApplicationUser>(null);
        }

        public async Task<CompanyUserBlockStatus> GetCompanyUserBlockStatusAsync(CompanyUserBlockStatus status)
        {
            var userBlockStatus = _dbContext.CompanyUserBlockStatus.Local
                .FirstOrDefault(x => x.Id == status.Id);

            if (userBlockStatus == null)
                userBlockStatus = await _dbContext.CompanyUserBlockStatus
                    .FirstOrDefaultAsync(x => x.Id == status.Id);

            return userBlockStatus;
        }
    }
}
