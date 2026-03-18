using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class WhitelistIPAddressInputDTO
    {
        public List<IPAddress> Staging { get; set; }
        public List<IPAddress> Production { get; set; }
    }
}
