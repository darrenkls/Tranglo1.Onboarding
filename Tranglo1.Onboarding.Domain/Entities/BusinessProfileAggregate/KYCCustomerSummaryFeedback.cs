using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
	public class KYCCustomerSummaryFeedback : Entity
	{
		public KYCCategory KYCCategory { get; set; }
		public BusinessProfile BusinessProfile { get; set; }
		public string FeedbackToTranglo { get; set; }
		public long? SolutionCode { get; set; }

		public KYCCustomerSummaryFeedback()
		{
			
		}

		public KYCCustomerSummaryFeedback(int id, BusinessProfile bp, KYCCategory category)
		{
			if (id != 0)
			{
				base.Id = id;
			}
			this.BusinessProfile = bp;
			this.KYCCategory = category;
		}
	}
}
