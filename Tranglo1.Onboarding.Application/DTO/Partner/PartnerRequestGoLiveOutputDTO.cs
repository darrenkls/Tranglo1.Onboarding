using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
	public class PartnerRequestGoLiveOutputDTO
	{
		public long PartnerCode { get; set; }
		public long PartnerSubscriptionCode { get; set; }
		public bool Status { get; set; }
	}
}
