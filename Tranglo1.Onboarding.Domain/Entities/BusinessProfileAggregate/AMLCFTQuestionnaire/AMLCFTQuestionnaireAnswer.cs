using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class AMLCFTQuestionnaireAnswer : Entity<int>
    {
        public AMLCFTQuestionnaire AMLCFTQuestionnaire { get; set; }
        public AnswerChoice AnswerChoice { get; set; }
        public string AnswerRemark { get; set; }

        private AMLCFTQuestionnaireAnswer()
        {

        }

        public AMLCFTQuestionnaireAnswer(AMLCFTQuestionnaire amlCFTQuestionnaire, AnswerChoice answerChoice, string answerRemark)
        {
            AMLCFTQuestionnaire = amlCFTQuestionnaire;
            AnswerChoice = answerChoice;
            AnswerRemark = answerRemark;
        }
    }
}
