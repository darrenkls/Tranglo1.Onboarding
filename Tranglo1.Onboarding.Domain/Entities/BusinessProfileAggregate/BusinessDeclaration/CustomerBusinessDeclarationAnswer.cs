using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration
{
    public class CustomerBusinessDeclarationAnswer : Entity
    {
        public long CustomerBusinessDeclarationCode { get; set; }
        public long DeclarationQuestionCode { get; set; }
        public bool? DeclarationAnswer { get; set; }
        public string DocumentName { get; set; }
        public Guid? DocumentId { get; set; }

        public CustomerBusinessDeclarationAnswer(long customerBusinessDeclarationCode, long declarationQuestionCode)
        {
            this.CustomerBusinessDeclarationCode = customerBusinessDeclarationCode;
            this.DeclarationQuestionCode = declarationQuestionCode;
        }
    }
}