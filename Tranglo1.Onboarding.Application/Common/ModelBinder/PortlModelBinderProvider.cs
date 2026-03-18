using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.AspNetCore.ModelBinding;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerRegistration;

namespace Tranglo1.Onboarding.Application.Common.ModelBinder
{
    public class PortlModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            // our binders here
            //if (context.Metadata.ModelType == typeof(UpdatePartnerInputDTO))
            //{
            //    return new BinderTypeModelBinder(typeof(TrangloEntityModelBinder));
            //}

            // your maybe have more binders?
            // ....

            // this provider does not provide any binder for given type
            //   so we return null
            return null;
        }
    }
}
