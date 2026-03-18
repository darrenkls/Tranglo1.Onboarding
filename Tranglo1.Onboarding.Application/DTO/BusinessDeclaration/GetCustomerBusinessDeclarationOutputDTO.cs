using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.BusinessDeclaration
{
    public class GetCustomerBusinessDeclarationOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public long CustomerBusinessDeclarationCode { get; set; }
        public long BusinessDeclarationStatusCode { get; set; }
        public string BusinessDeclarationStatusDescription { get; set; }
        public long CustomerTypeCode { get; set; }
        public string CustomerTypeDescriptionExternal { get; set; }
        public string CustomerTypeDescriptionInternal { get; set; }
        public int RedoCount { get; set; }
        public bool? IsRedoBusinessDeclaration { get; set; }
        public List<CustomerBusinessDeclarationAnswerList> CustomerBusinessDeclarationAnswers { get; set; }
    }

    public class CustomerBusinessDeclarationAnswerList
    {
        public long CustomerBusinessDeclarationAnswerCode { get; set; }
        public long CustomerBusinessDeclarationCode { get; set; }
        public long DeclarationQuestionCode { get; set; }
        public string DeclarationQuestionDescription { get; set; }
        public long DeclarationQuestionTypeCode { get; set; }
        public string DeclarationQuestionTypeDescription { get; set; }
        public bool? DeclarationAnswer { get; set; }
        public bool HasDocumentUpload { get; set; } //Indicates if declaration question requires document upload
        public string DocumentName { get; set; }
        public Guid? DocumentId { get; set; }
        public int? FileSizeMB { get; set; }
        public int SequenceNo { get; set; }
        public int SectionCode { get; set; }
        public bool? IsMandatory { get; set; }
    }
}