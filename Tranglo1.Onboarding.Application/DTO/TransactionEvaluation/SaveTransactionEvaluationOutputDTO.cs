using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.TransactionEvaluation
{
    public class SaveTransactionEvaluationOutputDTO
    {
        public Guid? TransactionEvalConcurrencyToken { get; set; }
    }
}