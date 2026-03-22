using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using System;

namespace Tranglo1.Onboarding.Application.Services.Identity
{
    public interface IRoleCodeContext
    {
        Maybe<string> CurrentRoleCode { get; }
    }

    internal class DefaultRoleCodeContext : IRoleCodeContext
    {
        public const string RoleCode = "RoleCode";

        public DefaultRoleCodeContext(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentException(nameof(httpContextAccessor));
            }

            HttpContextAccessor = httpContextAccessor;
        }

        public Maybe<string> CurrentRoleCode
        {
            get
            {
                var _Id = HttpContextAccessor.HttpContext?.Items[RoleCode];

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
