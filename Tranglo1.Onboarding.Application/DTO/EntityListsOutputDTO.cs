using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class EntityListsOutputDTO
    {
        public string LoginId { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
        public string TrangloEntity { get; set; }
        public string BlockStatus { get; set; }
    }
}
