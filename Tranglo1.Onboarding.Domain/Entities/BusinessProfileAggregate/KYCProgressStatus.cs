using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class KYCProgressStatus : Enumeration
    {
        public KYCProgressStatus() : base()
        {
        }

        public KYCProgressStatus(int id, string name)
            : base(id, name)
        {

        }
      
        public static readonly KYCProgressStatus Pending = new KYCProgressStatus(1, "Pending"); //Default value 
        public static readonly KYCProgressStatus Completed = new KYCProgressStatus(2, "Completed");
        
        public static KYCProgressStatus GetStatus(bool isCompleted)
        {
            return isCompleted ? KYCProgressStatus.Completed : KYCProgressStatus.Pending;
        }
    }
}
