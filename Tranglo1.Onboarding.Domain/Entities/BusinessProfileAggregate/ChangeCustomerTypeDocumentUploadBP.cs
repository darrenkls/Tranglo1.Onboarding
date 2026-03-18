using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    //Required for #50693
    public class ChangeCustomerTypeDocumentUploadBP : Entity
    {
        public int? BusinessProfileCode { get; set; }
        public long? DocumentCategoryBPCode { get; set; }
        public int? AdminUserID { get; set; }
        public Guid? DocumentID { get; set; }


        private ChangeCustomerTypeDocumentUploadBP() { }

        public ChangeCustomerTypeDocumentUploadBP(int? businessProfileCode, long? documentCategoryBPCode, int? adminUserID, Guid? documentID)
        {
            this.BusinessProfileCode = businessProfileCode;
            this.DocumentCategoryBPCode = documentCategoryBPCode;
            this.AdminUserID = adminUserID;
            this.DocumentID = documentID;
        }
    }
}