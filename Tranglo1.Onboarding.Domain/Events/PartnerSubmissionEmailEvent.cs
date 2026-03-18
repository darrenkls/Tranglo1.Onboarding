using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Events
{
    public class PartnerSubmissionEmailEvent : DomainEvent
    {
        public string PICName { get; set; }
        public string CustomerSolution {  get; set; }
        public int BusinessProfileCode { get; set; }
        public int UserId { get; set; }

        public PartnerSubmissionEmailEvent(string PICName, string CustomerSolution, int businessProfileCode, int userId)
        {
            this.PICName = PICName;
            this.CustomerSolution = CustomerSolution;
            BusinessProfileCode = businessProfileCode;
            UserId = userId;
        }
    }
}
