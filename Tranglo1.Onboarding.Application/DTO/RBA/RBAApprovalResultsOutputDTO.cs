using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.RBA
{
    public class RBAApprovalResultsOutputDTO
    {
        public string TrangloEntity { get; set; }
        public string LastSubmittedBy { get; set; }
        public string LastApprovedBy { get; set; }
        public DateTime LastApprovedDate { get; set; }
        public string PreviousRiskLevel { get; set; }
        public string NewRiskLevel { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
}
