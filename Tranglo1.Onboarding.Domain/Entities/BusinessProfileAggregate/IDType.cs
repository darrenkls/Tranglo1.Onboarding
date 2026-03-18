using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class IDType : Enumeration
    {
        public IDType() : base()
        {
        }

        public IDType(int id, string name)
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

        public static readonly IDType Driving_License = new IDType(1, "Driving License"); //Default value 
        public static readonly IDType Identification_Card = new IDType(2, "Identification Card");
        public static readonly IDType International_Passport = new IDType(3, "International Passport");
        public static readonly IDType Permanent_Resident_Card = new IDType(4, "Permanent Resident Card");
        public static readonly IDType Work_Permit = new IDType(5, "Work Permit");

    }
}
