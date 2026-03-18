using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Tranglo1.Onboarding.Infrastructure.Services
{
	internal class BackendIdentityContext : IIdentityContext
	{
		public ClaimsPrincipal CurrentUser => ClaimsPrincipal.Current;
	}
}
