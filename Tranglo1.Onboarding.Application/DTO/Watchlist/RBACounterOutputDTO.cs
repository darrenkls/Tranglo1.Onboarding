using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Watchlist
{
    public class RBACounterOutputDTO
    {
        public string RBAResult { get; set; }
        public string RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public DateTime DateAndTime { get; set; }

    }
}
