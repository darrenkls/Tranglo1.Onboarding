using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Tranglo1.Onboarding.Infrastructure.Services
{
	/// <summary>
	/// The implementation of this interface should provide access to current
	/// logged in user info. If current execution context is not under a human user,
	/// <seealso cref="ClaimsPrincipal.Current"/> will be returned.
	/// </summary>
	public interface IIdentityContext
	{
		ClaimsPrincipal CurrentUser { get; }
	}
}
