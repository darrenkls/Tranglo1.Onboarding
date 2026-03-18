using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration
{
    public class CustomerBusinessDeclaration : Entity
    {
        public int BusinessProfileCode { get; set; }
        public BusinessDeclarationStatus BusinessDeclarationStatus { get; set; }
        public int RedoCount { get; set; }
        public bool? IsRedoBusinessDeclaration { get; set; } // FE uses this flag to display message on user log in

        public CustomerBusinessDeclaration() { }

        public CustomerBusinessDeclaration(int businessProfileCode)
        {
            this.BusinessProfileCode = businessProfileCode;
            this.BusinessDeclarationStatus = BusinessDeclarationStatus.Pending;
            this.RedoCount = 3; // redo limit
        }
    }
}