using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface IRBARepository
    {
        Task<List<RBA>> SaveRiskEvaluationResponseAsync(List<RBA> rbaResponse);
        Task<RBAScreeningInput> SaveRBAScreeningInputAsync(RBAScreeningInput rbaScreeningInput);
        Task<RBAScreeningInput> GetLatestRBAScreeningInputByBusinessProfileCodeAsync(long businessProfileCode);
        Task<RBARequisition> GetRBARequisitionsByRBACodeAsync(long id);
        Task<RBA> GetRBAByRBACodeAsync(long? rBACode);
        Task<RBARequisition> GetRBARequisitionsByRequisitionCodeAsync(string requisitionCode);
        Task<RBA> UpdateRBAAsync(RBA rbaDetail);
        Task<IEnumerable<RiskRanking>> GetAllRiskRatings();
        Task<IEnumerable<ComplianceSettingType>> GetAllComplianceSetting();
        Task<IEnumerable<ComplianceRequisitionType>> GetAllComplianceRequisitionType();
        Task<RBARequisition> GetLatestRBARequisitionsByRBACodeAsync(long rbaCode);
        Task<RBARequisition> GetPreviousRBARequisitionByRBACodeAsync(long rbaCode);
        Task<List<RBARequisition>> GetAllRequisitionListByRBACodeAsync(long rbaCode);
    }
}
