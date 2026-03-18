using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
	public class KYCSummaryFeedback : Entity
	{
		public KYCCategory KYCCategory { get; set; }
		public BusinessProfile BusinessProfile { get; set; }
		public string IncorrectItem { get; set; }
		public string InternalRemarks { get; set; }
		public string FeedbackToUser { get; set; }
		public long? SolutionCode { get; set; }
		public bool IsResolved {  get; set; }

		public KYCSummaryFeedback()
		{
			
		}

		public KYCSummaryFeedback(int id, BusinessProfile bp, KYCCategory category)
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
