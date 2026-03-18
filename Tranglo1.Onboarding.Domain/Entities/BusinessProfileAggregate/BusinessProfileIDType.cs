using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class BusinessProfileIDType : Enumeration
    {
        public BusinessProfileIDType() : base()
        {
        }

        public BusinessProfileIDType(int id, string name)
            : base(id, name)
        {

        }

        //it only appear if CustomerType = Individual
        public static readonly BusinessProfileIDType Identity_Card = new BusinessProfileIDType(1, "Identity Card"); //Default value 
        public static readonly BusinessProfileIDType International_Passport = new BusinessProfileIDType(2, "International Passport");
        public static readonly BusinessProfileIDType Goverment_Issued_ID = new BusinessProfileIDType(3, "Government Issued ID");
    }
}