using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc
{
    public class PartnerCodeAttribute : ModelBinderAttribute
    {
        public PartnerCodeAttribute()
        {
            base.BinderType = typeof(PartnerCodeModelBinder);
        }

        public override BindingSource BindingSource { get => null; protected set => base.BindingSource = value; }
    }
}
