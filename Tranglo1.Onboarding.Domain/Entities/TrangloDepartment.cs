using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class TrangloDepartment : Enumeration
    {
        public TrangloDepartment() : base() { }

        public TrangloDepartment(int id, string name) : base(id, name) { }

        public static IEnumerable<TrangloDepartment> GetAllTrangloDepartment()
        {
            var fields = typeof(TrangloDepartment).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            return fields.Select(f => f.GetValue(null)).Cast<TrangloDepartment>();
        }
    }
}
