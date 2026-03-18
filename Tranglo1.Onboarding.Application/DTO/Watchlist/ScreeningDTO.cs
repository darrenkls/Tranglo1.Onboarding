using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Watchlist
{
    public class ScreeningDTO
    {
        public string ClientReference { set; get; }
        public string FullName { set; get; }
        public string Gender { set; get; }
        public int? YearOfBirth { set; get; }
        public string Nationality { set; get; }
        public string CompanyName { set; get; }
    }
}
