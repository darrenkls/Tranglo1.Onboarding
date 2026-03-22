using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using System;

namespace Tranglo1.Onboarding.Application.Services.Identity
{
    public interface IBusinessProfileContext
    {
        Maybe<int> CurrentProfileId { get; }
    }

    internal class DefaultBusinessProfileContext : IBusinessProfileContext
    {
        public const string BusinessProfileId = "BusinessProfileId";

        public DefaultBusinessProfileContext(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentException(nameof(httpContextAccessor));
            }

            HttpContextAccessor = httpContextAccessor;
        }

        public Maybe<int> CurrentProfileId
        {
            get
            {
                var _Id = HttpContextAccessor.HttpContext?.Items[BusinessProfileId];

                if (_Id == null)
                {
                    return Maybe<int>.None;
                }

                return Maybe<int>.From((int)_Id);
            }
        }

        public IHttpContextAccessor HttpContextAccessor { get; }
    }
}
