using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class APIURLOutputDTO
    {
        public long APIURLId { get; set; }
        public string Environment { get; set; }
        public string StringDomain { get; set; }
        public string APIType { get; set; }
        public string APITypeDescription { get; set; }
    }
}
