using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DocumentCategoryBPStatus : Enumeration
    {
        public DocumentCategoryBPStatus() : base()
        {
        }

        public DocumentCategoryBPStatus(int id, string name)
            : base(id, name)
        {

        }

        public static readonly DocumentCategoryBPStatus PendingUpload = new DocumentCategoryBPStatus(1, "Pending Upload");
        public static readonly DocumentCategoryBPStatus PendingSubmission = new DocumentCategoryBPStatus(2, "Pending Submission");
        public static readonly DocumentCategoryBPStatus PendingReview = new DocumentCategoryBPStatus(3, "Pending Review");
        public static readonly DocumentCategoryBPStatus Pass = new DocumentCategoryBPStatus(4, "Pass");
        public static readonly DocumentCategoryBPStatus Rejected = new DocumentCategoryBPStatus(5, "Rejected");
        public static readonly DocumentCategoryBPStatus NotApplicable = new DocumentCategoryBPStatus(6, "Not Applicable");
        public static readonly DocumentCategoryBPStatus IncompleteDocument = new DocumentCategoryBPStatus(7, "Incomplete Document");
    }
}
