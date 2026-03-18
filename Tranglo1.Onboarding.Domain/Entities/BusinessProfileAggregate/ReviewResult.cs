using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ReviewResult : Enumeration
    {
        public ReviewResult() : base()
        {
        }

        public ReviewResult(int id, string name)
            : base(id, name)
        {

        }
        /*
        Derive data from Compliance Officer’s review 
        •	Insufficient / Incomplete
        •	User need to take action to complete KYC based on Compliance Officer’s feedback and review results
        IF Review Result is Insufficient / Incomplete, Compliance Officer will need to include Feedback
        •	Pass
        Compliance Officer has approved user’s KYC
        */

        public static readonly ReviewResult Insufficient_Incomplete = new ReviewResult(1, "Insufficient / Incomplete");
        public static readonly ReviewResult Complete = new ReviewResult(2, "Complete");
    }
}
