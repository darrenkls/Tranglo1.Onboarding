using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tranglo1.Onboarding.Application.AspNetCore.ModelBinding;

namespace Microsoft.AspNetCore.Mvc
{
    public class RoleCodeAttribute : ModelBinderAttribute
    {
        public RoleCodeAttribute()
        {
            base.BinderType = typeof(RoleCodeModelBinder);
        }

        public override BindingSource BindingSource { get => null; protected set => base.BindingSource = value; }
    }
}
