using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class KYCStatus : Enumeration
    {
        public KYCStatus() : base()
        {
        }

        public KYCStatus(int id, string name)
            : base(id, name)
        {

        }
        /*
        Derive data from Compliance Officer’s review 
        •	Insufficient / Incomplete
        User need to take action to complete KYC based on Compliance Officer’s feedback and review results
        •	Complete
        Compliance Officer has approved user’s KYC
        */

        public static readonly KYCStatus Insufficient_Incomplete = new KYCStatus(1, "Insufficient / Incomplete");
        public static readonly KYCStatus Verified = new KYCStatus(2, "Verified");
        public static readonly KYCStatus Rejected = new KYCStatus(3, "Rejected");
        public static readonly KYCStatus Keep_In_View = new KYCStatus(4, "Keep in view");
        public static readonly KYCStatus Terminated = new KYCStatus(5, "Terminated");
        public static readonly KYCStatus Pending_Higher_Approval = new KYCStatus(6, "Pending higher approval");
        public static readonly KYCStatus Deactivated = new KYCStatus(7, "Deactivated");


    }
}
