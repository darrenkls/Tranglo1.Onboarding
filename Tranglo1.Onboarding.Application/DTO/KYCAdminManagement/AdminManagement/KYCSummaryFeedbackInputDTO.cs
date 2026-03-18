using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
	public class KYCSummaryFeedbackInputDTO : IValidatableObject
	{
		public int KYCSummaryFeedbackCode { get; set; }
		public long KYCCategoryCode { get; set; }
		public string IncorrectItem { get; set; }
		public string InternalRemarks { get; set; }
		public string FeedbackToUser { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IncorrectItem.Trim().Length == 0 && InternalRemarks.Trim().Length == 0 && FeedbackToUser.Trim().Length == 0)
            {
                yield return new ValidationResult(
                    $"Either one from Incorrect Item, Internal Remarks and Feedback To User cannot be empty.");
            }
        }
    }
}
