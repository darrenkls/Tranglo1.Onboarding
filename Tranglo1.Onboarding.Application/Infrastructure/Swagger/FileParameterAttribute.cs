using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Infrastructure.Swagger
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FileParameterAttribute : System.Attribute
    {
        public FileParameterAttribute()
        {
            this.ParamName = "file";
        }

        public string ParamName { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
    }
}
