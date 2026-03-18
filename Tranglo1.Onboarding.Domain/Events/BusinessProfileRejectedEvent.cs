using System;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Domain.Events
{
    public class BusinessProfileRejectedEvent : DomainEvent
	{
		public int BusinessProfileCode { get; set; }
		public string CompanyName { get; set; }
		public int AdminSolution { get; set; }

		public BusinessProfileRejectedEvent(int businessProfileCode,string companyName, int adminSolution)
		{
			this.BusinessProfileCode = businessProfileCode;
			this.CompanyName = companyName;
			this.AdminSolution = adminSolution;
		}
	}
}
