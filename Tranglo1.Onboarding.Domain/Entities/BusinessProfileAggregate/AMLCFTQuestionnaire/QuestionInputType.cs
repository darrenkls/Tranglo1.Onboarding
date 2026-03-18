using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class QuestionInputType : Enumeration
    {
        public QuestionInputType() : base()
        {
        }

        public QuestionInputType(int id, string name)
            : base(id, name)
        {

        }

        public static readonly QuestionInputType SingleChoice = new QuestionInputType(1, "Single Selection"); //Default value 
        public static readonly QuestionInputType MultipleChoice = new QuestionInputType(2, "Multiple Selection");
        public static readonly QuestionInputType FreeText = new QuestionInputType(3, "Open Question");
        public static readonly QuestionInputType SubQuestion = new QuestionInputType(4, "Title Question");
    }
}
