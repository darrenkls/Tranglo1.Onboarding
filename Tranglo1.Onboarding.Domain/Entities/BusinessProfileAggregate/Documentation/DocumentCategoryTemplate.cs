using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DocumentCategoryTemplate:Entity
    {
        

        public DocumentCategory DocumentCategory { get; set; }
        public long DocumentCategoryCode { get; set; }
        public Guid DocumentId { get; set; }
        public Questionnaire Questionnaire { get; set; }
        public long? QuestionnaireCode { get; set; }
        

        private DocumentCategoryTemplate()
        {

        }

        public DocumentCategoryTemplate(DocumentCategory documentCategoryInfo, long? questionnaireCode)
        {
            DocumentCategoryCode = documentCategoryInfo.Id;
            QuestionnaireCode = questionnaireCode;
        }

     
    }
}
