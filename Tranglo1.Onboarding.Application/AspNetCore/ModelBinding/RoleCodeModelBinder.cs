using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Services.Identity;

namespace Tranglo1.Onboarding.Application.AspNetCore.ModelBinding
{
    public class RoleCodeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            if (!(value is string))
            {
                bindingContext.ModelState.TryAddModelError(modelName, "RoleCode must be a string.");
                return Task.CompletedTask;
            }

            bindingContext.HttpContext.Items[DefaultRoleCodeContext.RoleCode] = value;
            bindingContext.Result = ModelBindingResult.Success(value);
            return Task.CompletedTask;
        }
    }
}
