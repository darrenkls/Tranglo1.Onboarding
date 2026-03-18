using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class RequisitionRunningNumber : Entity
    {
        public string Prefix { get; set; }
        public string RunningNumber { get; set; }
        public string RequisitionType { get; set; }

    }
}
