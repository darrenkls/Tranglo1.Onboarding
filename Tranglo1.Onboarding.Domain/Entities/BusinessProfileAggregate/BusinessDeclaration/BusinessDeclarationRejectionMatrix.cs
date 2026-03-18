using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration
{
    public class BusinessDeclarationRejectionMatrix : Entity
    {
        public long CustomerTypeCode { get; set; }
        public bool A { get; set; }
        public bool B { get; set; }
        public bool C { get; set; }
    }
}