using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.Repositories
{
    public class RBARepository : IRBARepository
    {
        private readonly RBADBContext dbContext;

        public RBARepository(RBADBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<RBA>> SaveRiskEvaluationResponseAsync(List<RBA> rbaResponse)
        {
            this.dbContext.AddRange(rbaResponse);
            await dbContext.SaveChangesAsync();
            return rbaResponse;
        }

        public async Task<RBAScreeningInput> SaveRBAScreeningInputAsync(RBAScreeningInput rbaScreeningInput)
        {
            this.dbContext.RBAScreeningInputs.AddRange(rbaScreeningInput);
            await dbContext.SaveChangesAsync();
            return rbaScreeningInput;
        }

        public async Task<RBAScreeningInput> GetLatestRBAScreeningInputByBusinessProfileCodeAsync(long businessProfileCode)
        {
            var query = await this.dbContext.RBAScreeningInputs.Include(x => x.RBAList).ThenInclude(x => x.EvaluationRules).ThenInclude(rule => rule.Parameter)
                .Include(x => x.RBAList).ThenInclude(x => x.OverridingRules).ThenInclude(over => over.Parameter)
                .OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.BusinessProfileCode == businessProfileCode);
            return query;
        }

        public async Task<RBARequisition> GetRBARequisitionsByRBACodeAsync(long id)
        {
            return await dbContext.RBARequisitions.Include(x => x.Solution)
                      .Where(c => c.RBACode == id)
                      .FirstOrDefaultAsync();
        }

        public async Task<RBA> GetRBAByRBACodeAsync(long? rBACode)
        {
            return await dbContext.RBA.Include(x => x.Solution)
                .Where(a => a.Id == rBACode).FirstOrDefaultAsync();
        }

        public async Task<RBARequisition> GetRBARequisitionsByRequisitionCodeAsync(string requisitionCode)
        {
            return await dbContext.RBARequisitions.Include(x => x.Solution).Include(x => x.ComplianceRequisitionType)
                        .Where(c => c.RequisitionCode == requisitionCode)
                        .FirstOrDefaultAsync();
        }

        public async Task<RBA> UpdateRBAAsync(RBA rbaDetail)
        {
            dbContext.RBA.Update(rbaDetail);
            await dbContext.SaveChangesAsync();
            return rbaDetail;
        }
        public async Task<IEnumerable<RiskRanking>> GetAllRiskRatings()
        {
            return await dbContext.RiskRankings.ToListAsync();
        }

        public async Task<IEnumerable<ComplianceSettingType>> GetAllComplianceSetting()
        {
            return await dbContext.ComplianceSettings.ToListAsync();
        }


        public async Task<IEnumerable<ComplianceRequisitionType>> GetAllComplianceRequisitionType()
        {
            return await dbContext.ComplianceRequisitionTypes.ToListAsync();
        }

        public async Task<RBARequisition> GetLatestRBARequisitionsByRBACodeAsync(long rbaCode)
        {
            return await dbContext.RBARequisitions
                            .Where(a => a.RBACode == rbaCode)
                            .OrderByDescending(a => a.CreatedDate)
                            .FirstOrDefaultAsync();
        }

        public async Task<RBARequisition> GetPreviousRBARequisitionByRBACodeAsync(long rbaCode)
        {
            return await dbContext.RBARequisitions
                          .Where(a => a.RBACode == rbaCode)
                          .OrderByDescending(a => a.CreatedDate) 
                          .Skip(1) 
                          .FirstOrDefaultAsync();
        }

        public async Task<List<RBARequisition>> GetAllRequisitionListByRBACodeAsync(long rbaCode)
        {
            var query = this.dbContext.RBARequisitions.Where(x => x.RBACode == rbaCode);

            return await query.ToListAsync();
        }
    }
}
