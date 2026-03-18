using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class Question: Entity
    {
        public QuestionInputType QuestionInputType { get; set; }
        public long QuestionInputTypeCode { get; set; }
        public string Description { get; set; }
        public QuestionSection QuestionSection { get; set; }
        public AnswerChoice ParentAnswerChoice { get; set; }
        public Question ParentQuestionCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsOptional { get; set; }
        public int SequenceNo { get; set; }

        private Question()
        {

        }

        public Question(QuestionInputType questionInputType, string description, QuestionSection questionSection, 
            AnswerChoice parentAnswerChoice, Question parentQuestionCode, bool isOptional, int sequenceNo)
        {
            QuestionInputType = questionInputType;
            Description = description;
            QuestionSection = questionSection;
            ParentAnswerChoice = parentAnswerChoice;
            ParentQuestionCode = parentQuestionCode;
            IsActive = true;
            IsOptional = isOptional;
            SequenceNo = sequenceNo;
        }
    }
}
