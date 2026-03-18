using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class KYCSubmissionStatus : Enumeration
    {
        public KYCSubmissionStatus() : base()
        {
        }

        public KYCSubmissionStatus(int id, string name)
            : base(id, name)
        {

        }

        public static readonly KYCSubmissionStatus Draft = new KYCSubmissionStatus(1, "Draft"); //Default value 
        public static readonly KYCSubmissionStatus Submitted = new KYCSubmissionStatus(2, "Submitted");

    }
}
