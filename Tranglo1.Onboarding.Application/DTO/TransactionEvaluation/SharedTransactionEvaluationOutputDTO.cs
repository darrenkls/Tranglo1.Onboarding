using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.TransactionEvaluation
{
    public class SharedTransactionEvaluationDTO
    {
    }
    public class SharedQuestionDTO
    {
        public int QuestionCode { get; set; }
        public int QuestionInputTypeCode { get; set; }
        public string MetaName { get; set; } //To indicate if the list of dropdown should come from a separate API call
    }

    public class SharedAnswerDTO
    {
        public int? AnswerCode { get; set; }
        public int? AnswerChoiceCode { get; set; }
        public bool IsAnswered { get; set; }
        public string AnswerMetaCode { get; set; }
        public string AnswerRemark { get; set; }
    }
}
