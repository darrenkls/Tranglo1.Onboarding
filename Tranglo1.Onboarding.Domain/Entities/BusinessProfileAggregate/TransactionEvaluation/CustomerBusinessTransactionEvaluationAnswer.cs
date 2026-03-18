using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.TransactionEvaluation
{
    public class CustomerBusinessTransactionEvaluationAnswer: Entity
    {
        public int BusinessProfileCode { get; set; }
        public TransactionEvaluationAnswerChoice TransactionEvaluationAnswerChoice { get; set; }
        public TransactionEvaluationQuestion TransactionEvaluationQuestion { get; set; }
        public string MetaTableColumnPKCode { get; set; }
        public string AnswerRemark { get; set; }

        private CustomerBusinessTransactionEvaluationAnswer()
        {

        }

        public CustomerBusinessTransactionEvaluationAnswer(int businessProfileCode, 
                                                            TransactionEvaluationAnswerChoice transactionEvaluationAnswerChoice,
                                                            TransactionEvaluationQuestion transactionEvaluationQuestion,
                                                            string metaTableColumnPKCode,
                                                            string answerRemark)
        {
            this.BusinessProfileCode = businessProfileCode;
            this.TransactionEvaluationAnswerChoice = transactionEvaluationAnswerChoice;
            this.TransactionEvaluationQuestion = transactionEvaluationQuestion;
            this.MetaTableColumnPKCode = metaTableColumnPKCode;
            this.AnswerRemark = answerRemark;
        }
    }
}
