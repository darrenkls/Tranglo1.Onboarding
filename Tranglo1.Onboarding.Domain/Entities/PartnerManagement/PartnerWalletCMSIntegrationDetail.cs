using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
	public class PartnerWalletCMSIntegrationDetail : Entity
	{
		public long PartnerCode { get; set; }
		public string TrnxID { get; set; }
		public string RcCode { get; set; }
		public string CMSStatus { get; set; }
		public long? PartnerSubscriptionCode { get; set; }
	}
}
