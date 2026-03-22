using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc
{
    public class BusinessProfileIdAttribute : ModelBinderAttribute
    {
        public BusinessProfileIdAttribute()
        {
            base.BinderType = typeof(BusinessProfileIdModelBinder);
        }

        public override BindingSource BindingSource { get => null; protected set => base.BindingSource = value; }
    }
}
