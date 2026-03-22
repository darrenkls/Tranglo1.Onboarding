using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Services.Identity;

namespace Microsoft.AspNetCore.Mvc
{
    public class PartnerCodeModelBinder : IModelBinder
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

            if (!long.TryParse(value, out var id))
            {
                bindingContext.ModelState.TryAddModelError(modelName, "Partner code must be an integer.");
                return Task.CompletedTask;
            }

            bindingContext.HttpContext.Items[DefaultPartnerContext.PartnerCode] = id;
            bindingContext.Result = ModelBindingResult.Success(id);
            return Task.CompletedTask;
        }
    }
}
