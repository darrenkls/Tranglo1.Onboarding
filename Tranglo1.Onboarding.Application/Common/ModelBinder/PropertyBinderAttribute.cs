using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace System.Web.Http.ModelBinding
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public class PropertyBinderAttribute : Attribute
    {
        public PropertyBinderAttribute()
        {
        }

        public PropertyBinderAttribute(Type binderType)
        {
            BinderType = binderType;
        }

        public Type BinderType { get; set; }
        public virtual BindingSource BindingSource { get; protected set; }
        public string Name { get; set; }
    }
}
