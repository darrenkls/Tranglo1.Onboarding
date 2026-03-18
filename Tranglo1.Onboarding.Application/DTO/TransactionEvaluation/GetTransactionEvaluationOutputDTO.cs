using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.TransactionEvaluation
{
    public class GetTransactionEvaluationOutputDTO : SharedTransactionEvaluationDTO
    {
        public int CustomerTypeCode { get; set; }
        public string CustomerTypeDescription { get; set; }
        public Guid? TransactionEvalConcurrencyToken { get; set; }
        public List<TransactionEvaluationInfoDTO> TransactionEvaluationInfos { get; set; }
        public List<QuestionDTO> Questions { get; set; }
    }
    public class TransactionEvaluationInfoDTO
    {
        public int TransactionEvaluationInfoCode { get; set; }
        public string TransactionEvaluationInfoDescription { get; set; }
    }
    public class QuestionDTO : SharedQuestionDTO
    {
        public string QuestionDescription { get; set; }
        public string QuestionInputTypeDescription { get; set; }
        public int SequenceNo { get; set; }
        public bool? IsOptional { get; set; }
        public List<AnswerDTO> Answers { get; set; }
    }

    public class AnswerDTO : SharedAnswerDTO
    {
        public string AnswerChoiceDescription { get; set; }
        public string AnswerMetaDescription { get; set; }
        public int QuestionCode { get; set; }
    }
}
