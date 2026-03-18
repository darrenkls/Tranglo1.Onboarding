using System;
using System.Collections.Generic;
using System.Text;
using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
	public class PartnerCMSIntegrationDetail : Entity
	{
		public long PartnerCode { get; set; }
		public long CMSPartnerId { get; set; }
		//CMS GUID TrnxID
		public string TrnxID { get; set; }
		public string RcCode { get; set; }
		public string CMSStatus { get; set; }
		public long? PartnerSubscriptionCode { get; set; }
	}
}
