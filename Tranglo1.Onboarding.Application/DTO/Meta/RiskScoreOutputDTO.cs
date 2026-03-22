using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Meta
{
    public class RiskScoreOutputDTO
    {
        public long? RiskScoreCode { get; set; }
        public string RiskScoreDescription { get; set; }
        public double LowRange { get; set; }
        public double HighRange { get; set; }
    }
}
