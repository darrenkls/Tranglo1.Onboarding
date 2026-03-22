using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tranglo1.Onboarding.Application.AspNetCore.ModelBinding;

namespace Microsoft.AspNetCore.Mvc
{
    public class TrangloEntityIdAttribute : ModelBinderAttribute
    {
        public TrangloEntityIdAttribute()
        {
            base.BinderType = typeof(TrangloEntityModelBinder);
        }

        public override BindingSource BindingSource { get => null; protected set => base.BindingSource = value; }
    }
}
