using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Common.ModelBinder
{
    public class EndDateBinderAttribute : ValidationAttribute
    {
        
        
        private const string DefaultErrorMessage = "Not allowed to choose current day date for Agreement End Date";
        protected override ValidationResult IsValid(object objValue,
                                                       ValidationContext validationContext)
        {
            var dateValue = objValue as DateTime? ?? new DateTime();

            //alter this as needed. I am doing the date comparison if the value is not null

            if (dateValue != null && dateValue.Date == DateTime.UtcNow.Date)
            {
                return new ValidationResult(DefaultErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
