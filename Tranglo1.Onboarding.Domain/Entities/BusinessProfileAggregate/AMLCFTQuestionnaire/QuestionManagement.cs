using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.AMLCFTQuestionnaire
{
    public class QuestionManagement : Entity
    {
        public Questionnaire Questionnaire { get; set; }
        public long? QuestionnaireCode { get; set; }
        public Solution Solution { get; set; }
        public long SolutionCode { get; set; }
        public TrangloEntity TrangloEntity { get; set; }
        public long TrangloEntityCode { get; set; }
        public bool? IsChecked { get; set; }


        public QuestionManagement(long questionnaireCode, long solutionCode, long trangloEntity)
        {
            QuestionnaireCode = questionnaireCode;
            SolutionCode = solutionCode;
            TrangloEntityCode = trangloEntity;
        }

        public QuestionManagement()
        {
        }
    }
}
