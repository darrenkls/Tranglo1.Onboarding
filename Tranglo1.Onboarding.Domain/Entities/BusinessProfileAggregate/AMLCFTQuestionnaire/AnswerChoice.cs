using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class AnswerChoice: Entity
    {
        public string Description { get; set; }
        public Question Question { get; set; }
        public long? SequenceNumber { get; set; }

        public AnswerChoice()
        {

        }

        public AnswerChoice(string description, Question question, long? sequenceNumber)
        {
            Description = description;
            Question = question;
            SequenceNumber = sequenceNumber;
        }
    }
}
