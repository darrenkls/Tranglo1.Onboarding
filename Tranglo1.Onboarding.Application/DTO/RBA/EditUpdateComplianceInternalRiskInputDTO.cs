using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.RBA
{
    public class EditUpdateComplianceInternalRiskInputDTO
    {
        [Required(ErrorMessage = "The RBA compliance rating update cannot proceed because the remarks field is empty or missing.")]
        public string Remarks { get; set; }

        [Required(ErrorMessage = "The RBA compliance rating update cannot proceed because the risk ranking field is empty or missing.")]
        public long RiskRankingCode { get; set; }

    }
}
