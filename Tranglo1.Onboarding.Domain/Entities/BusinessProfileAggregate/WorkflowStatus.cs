using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class WorkflowStatus : Enumeration
    {
        public WorkflowStatus() : base()
        {
        }

        public WorkflowStatus(int id, string name)
            : base(id, name)
        {

        }

        /*
        •	Pending email confirmation
        User has not verified account from email verification at Sign Up stage
        •	Registration completed
        User has completed verification account from Sign Up stage BUT has not completed Business Profile
        •	Document pending review
        User has uploaded documents
        •	Compliance review in progress
        Compliance Officer reviewing documents
        •	Agreement pending sign
        User has not completed Agreement sign off stage
        */

        public static readonly WorkflowStatus Compliance_Pending_Review = new WorkflowStatus(1, "Compliance Pending Review");
        public static readonly WorkflowStatus Compliance_Review_In_Progress = new WorkflowStatus(2, "Compliance Review In Progress");
        public static readonly WorkflowStatus Compliance_Approved = new WorkflowStatus(3, "Compliance Approved");
        public static readonly WorkflowStatus Compliance_Reject = new WorkflowStatus(4, "Compliance Rejected");
        public static readonly WorkflowStatus KYC_Operations_Pending_Review = new WorkflowStatus(5, "KYC Operations Pending Review");
        public static readonly WorkflowStatus KYC_Operations_In_Progress = new WorkflowStatus(6, "KYC Operations Review In Progress");
        public static readonly WorkflowStatus KYC_Operations_Approved = new WorkflowStatus(7, "KYC Operations Approved");
        public static readonly WorkflowStatus KYC_Operations_Reject = new WorkflowStatus(8, "KYC Operations Reject");
        public static readonly WorkflowStatus System_Reject = new WorkflowStatus(9, "System Reject");

    }
}
