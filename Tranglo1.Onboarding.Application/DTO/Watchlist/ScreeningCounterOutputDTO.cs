using System;

namespace Tranglo1.Onboarding.Application.DTO.Watchlist
{
    public class ScreeningCounterOutputDTO
    {
        public int ScreeningCode { get; set; }
        public Guid ReferenceCode { get; set; }
        public int PEPCounter { get; set; }
        public int SOECounter { get; set; }
        public int SanctionCounter { get; set; }
        public int AdverseMediaCounter { get; set; }
        public int EnforcementCounter { get; set; }
    }
}
