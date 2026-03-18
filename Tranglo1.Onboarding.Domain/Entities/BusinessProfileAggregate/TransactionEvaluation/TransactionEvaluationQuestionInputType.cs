using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.TransactionEvaluation
{
    public class TransactionEvaluationQuestionInputType : Enumeration
    {
        public TransactionEvaluationQuestionInputType() : base()
        {
        }

        public TransactionEvaluationQuestionInputType(int id, string name)
            : base(id, name)
        {

        }

        public static readonly TransactionEvaluationQuestionInputType RadioButton = new TransactionEvaluationQuestionInputType(1, "Radiobutton"); 
        public static readonly TransactionEvaluationQuestionInputType SingleSelectionDropdown = new TransactionEvaluationQuestionInputType(2, "Single Dropdown");
        public static readonly TransactionEvaluationQuestionInputType MultiSelectionDropdown = new TransactionEvaluationQuestionInputType(3, "Multiple Selection Dropdown");
        public static readonly TransactionEvaluationQuestionInputType FreeText = new TransactionEvaluationQuestionInputType(4, "Free Text");
        public static readonly TransactionEvaluationQuestionInputType Checkbox = new TransactionEvaluationQuestionInputType(5, "Checkbox");

    }
}
