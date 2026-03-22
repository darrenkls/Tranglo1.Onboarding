using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using System;

namespace Tranglo1.Onboarding.Application.Services.Identity
{
    public interface ITrangloEntityContext
    {
        Maybe<string> CurrentTrangloEntity { get; }
    }

    internal class DefaultTrangloEntityContext : ITrangloEntityContext
    {
        public const string TrangloEntityId = "Entity";

        public DefaultTrangloEntityContext(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentException(nameof(httpContextAccessor));
            }

            HttpContextAccessor = httpContextAccessor;
        }

        public Maybe<string> CurrentTrangloEntity
        {
            get
            {
                var _Id = HttpContextAccessor.HttpContext?.Items[TrangloEntityId];

                if (_Id == null)
                {
                    return Maybe<string>.None;
                }

                return Maybe<string>.From((string)_Id);
            }
        }

        public IHttpContextAccessor HttpContextAccessor { get; }
    }
}
