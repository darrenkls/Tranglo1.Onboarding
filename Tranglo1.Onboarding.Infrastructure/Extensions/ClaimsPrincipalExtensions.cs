using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Linq;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        private static Claim FindFirst(IEnumerable<Claim> claims, params string[] types)
        {
            return claims?.FirstOrDefault(c =>
                types.Contains(c.Type, StringComparer.OrdinalIgnoreCase));
        }

        public static Maybe<string> GetSolutionCode(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims == null)
            {
                return Maybe<string>.None;
            }

            var _TargetClaim = FindFirst(user.Claims, "solution");

            if (_TargetClaim == null)
            {
                return Maybe<string>.None;
            }
            else
            {
                return Maybe<string>.From(_TargetClaim.Value);
            }
        }

        public static Maybe<string> GetSubjectId(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims == null)
            {
                return Maybe<string>.None;
            }

            var _TargetClaim = FindFirst(user.Claims, "sub");

            if (_TargetClaim == null)
            {
                return Maybe<string>.None;
            }
            else
            {
                return Maybe<string>.From(_TargetClaim.Value);
            }
        }

        public static Maybe<int> GetUserId(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims == null)
            {
                return Maybe<int>.None;
            }

            var _UserIdClaim = user.Claims
                .Where(c => string.Equals(c.Type, "Userid", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (_UserIdClaim == null)
            {
                return Maybe<int>.None;
            }
            else if (int.TryParse(_UserIdClaim.Value, out int _Id))
            {
                return Maybe<int>.From(_Id);
            }
            else
            {
                return Maybe<int>.None;
            }
        }

        public static UserType GetUserType(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims == null)
            {
                return UserType.System;
            }

            var _UserTypeClaim = FindFirst(user.Claims, "type");

            if (_UserTypeClaim == null)
            {
                return UserType.System;
            }
            else if (string.Equals(_UserTypeClaim.Value, "internal", StringComparison.OrdinalIgnoreCase))
            {
                return UserType.Internal;
            }
            else if (string.Equals(_UserTypeClaim.Value, "external", StringComparison.OrdinalIgnoreCase))
            {
                return UserType.External;
            }
            else
            {
                return UserType.System;
            }
        }

        public static Maybe<string> GetUserEmail(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims == null)
            {
                return Maybe<string>.None;
            }

            Claim _Target = FindFirst(user.Claims, "email", ClaimTypes.Email);

            if (_Target == null)
            {
                return Maybe<string>.None;
            }
            else if (string.IsNullOrEmpty(_Target.Value) == false)
            {
                return Maybe<string>.From(_Target.Value);
            }
            else
            {
                return Maybe<string>.None;
            }
        }

        public static Maybe<string> GetJWTId(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims == null)
            {
                return Maybe<string>.None;
            }

            Claim _Target = FindFirst(user.Claims, "jti");

            if (_Target == null)
            {
                return Maybe<string>.None;
            }
            else if (string.IsNullOrEmpty(_Target.Value) == false)
            {
                return Maybe<string>.From(_Target.Value);
            }
            else
            {
                return Maybe<string>.None;
            }
        }

        public static Maybe<bool> IsBusinessProfileActive(this ClaimsPrincipal user, int businessProfileCode)
        {
            if (user == null || user.Claims == null)
            {
                return Maybe<bool>.None;
            }

            var bpClaims = user.Claims.Where(c =>
                string.Equals(c.Type, "connect_solution_business_profile", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Type, "business_solution_business_profile", StringComparison.OrdinalIgnoreCase));

            if (!bpClaims.Any())
            {
                return Maybe<bool>.None;
            }

            var profileCodeStr = businessProfileCode.ToString();
            var isActive = bpClaims.Any(c => c.Value.Split(',').Contains(profileCodeStr));

            return Maybe<bool>.From(isActive);
        }
    }
}
