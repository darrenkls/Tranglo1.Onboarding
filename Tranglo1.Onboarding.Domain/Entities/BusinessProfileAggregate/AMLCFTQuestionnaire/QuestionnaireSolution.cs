using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{ 
    public class QuestionnaireSolution : Entity
    {
        public Questionnaire Questionnaire { get; set; }
        public Solution Solution { get; set; }

        private QuestionnaireSolution()
        {

        }

        public QuestionnaireSolution(Questionnaire questionnaire, Solution solution)
        {
            Questionnaire = questionnaire;
            Solution = solution;
        }
    }
}
