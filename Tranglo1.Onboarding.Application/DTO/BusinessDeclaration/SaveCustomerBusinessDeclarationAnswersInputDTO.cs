using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.BusinessDeclaration
{
    public class SaveCustomerBusinessDeclarationAnswersInputDTO
    {
        public int BusinessProfileCode { get; set; }
        public long CustomerBusinessDeclarationCode { get; set; }
        public long BusinessDeclarationStatusCode { get; set; }
        public string BusinessDeclarationStatusDescription { get; set; }
        public List<SaveCustomerBusinessDeclarationAnswers> SaveCustomerBusinessDeclarationAnswers { get; set; }
    }

    public class SaveCustomerBusinessDeclarationAnswers
    {
        public long CustomerBusinessDeclarationAnswerCode { get; set; }
        public bool? DeclarationAnswer { get; set; }
    }
}