using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration
{
    public class DeclarationQuestion : Entity
    {
        public string DeclarationQuestionDescription { get; set; }
        public DeclarationQuestionType DeclarationQuestionType { get; set; }
        public CustomerType CustomerType { get; set; }
        public bool HasDocumentUpload { get; set; }
        public int SequenceNo { get; set; }
        public int? SectionCode { get; set; }
        public string QuestionLabel { get; set; }
        public bool? IsMandatory { get; set; }
    }
}