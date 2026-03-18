using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    //Required for #50693
    public class ChangeCustomerTypeCOInformation : Entity
    {
        public int? BusinessProfileCode { get; set; }
        public int? AdminUserID { get; set; }
        public Guid? DocumentID { get; set; }


        private ChangeCustomerTypeCOInformation() { }

        public ChangeCustomerTypeCOInformation(int? businessProfileCode, int? adminUserID, Guid? documentID)
        {
            this.BusinessProfileCode = businessProfileCode;
            this.AdminUserID = adminUserID;
            this.DocumentID = documentID;
        }
    }
}