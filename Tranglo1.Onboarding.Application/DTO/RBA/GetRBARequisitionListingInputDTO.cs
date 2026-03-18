using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.RBA
{
    public class GetRBARequisitionListingInputDTO
    {
        
        public string RequisitionCode { get; set; }
        public long SolutionCode { get; set; }
        public long? ComplianceSettingTypeCode { get; set; }
        public long? ComplianceRequisitionTypeCode { get; set; }

        public string RequestedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string EditedBy { get; set; }
        public long? FinalApprovalStatusCode { get; set; }

        public DateTime? RequestedDurationFrom { get; set; }
        public DateTime? RequestedDurationTo { get; set; }
        public DateTime? ApprovedDurationFrom { get; set; }
        public DateTime? ApprovedDurationTo { get; set; }
        public DateTime? EditedDurationFrom { get; set; }
        public DateTime? EditedDurationTo { get; set; }

    }
}

