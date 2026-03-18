using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Common.Exceptions
{
    public class ForbiddenException: Exception
    {
        public string Type { get; set; }
        public string Detail { get; set; }
        public string Title { get; set; }
        //public string Instance { get; set; }
        public ForbiddenException(string detail) : base(detail)
        {
            Type = "https://httpstatuses.com/403";
            Title = "User is not authorized to perform the requested action";
            //Instance = instance;
        }
    }
}
