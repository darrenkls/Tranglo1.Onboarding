using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
    public class OnboardWorkflowStatus : Enumeration
    {
        public OnboardWorkflowStatus() : base() 
        {

        }

        public OnboardWorkflowStatus(int id, string name)
            : base (id,name)
        {

        }

        public static readonly OnboardWorkflowStatus Pending = new OnboardWorkflowStatus(1, "Pending");
        public static readonly OnboardWorkflowStatus In_Progress = new OnboardWorkflowStatus(2, "In Progress");
        public static readonly OnboardWorkflowStatus Approve_Complete = new OnboardWorkflowStatus(3, "Approve/Complete");
        public static readonly OnboardWorkflowStatus Reject = new OnboardWorkflowStatus(4, "Reject");


    }
}
