using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class QuestionSection: Entity
    {
        public Questionnaire Questionnaire { get; set; }
        public string Description { get; set; }
        public int SequenceNo { get; set; }
        public bool IsActive { get; set; }

        public QuestionSection()
        {

        }

        public QuestionSection(Questionnaire questionnaire, string description, int sequenceNo)
        {
            Questionnaire = questionnaire;
            Description = description;
            SequenceNo = sequenceNo;
            IsActive = true;
        }
    }
}
