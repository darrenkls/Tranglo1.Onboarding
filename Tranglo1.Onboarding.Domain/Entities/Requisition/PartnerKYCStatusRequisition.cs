using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.ApprovalWorkflowEngine.Models;

namespace Tranglo1.Onboarding.Domain.Entities.Requisition
{
    public class PartnerKYCStatusRequisition : RequisitionDetail
    {
        public long KYCStatusCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public string Remarks { get; set; }
        public int SolutionCode { get; set; }
    }
}
