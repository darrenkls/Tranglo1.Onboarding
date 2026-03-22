using System.Collections.Generic;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface IStaffEntityQueryService
    {
        Task<List<TrangloStaffEntityAssignment>> GetTrangloStaffEntityAssignmentById(string loginId);
    }
}
