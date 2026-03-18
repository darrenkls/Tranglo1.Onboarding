using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.RBA
{
    public class RejectRBARequisitionInputDTO
    {
        public string OTP { get; set; }
        public Guid RequisitionGroupId { get; set; }
        public List<Requisition> Requisitions { get; set; }
    }

    public class RejectRequisition
    {
        public string RequisitionCode { get; set; }
        public long ComplianceRequisitionTypeCode { get; set; }
    }
}
