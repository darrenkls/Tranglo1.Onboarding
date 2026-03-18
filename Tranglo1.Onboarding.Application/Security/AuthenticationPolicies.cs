using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Security
{
	internal class AuthenticationPolicies
	{
		public const string ExternalConnectOnlyPolicy = "ExternalConnectOnlyPolicy";
		public const string ExternalBusinessOnlyPolicy = "ExternalBusinessOnlyPolicy";
		public const string ExternalOnlyPolicy = "ExternalOnlyPolicy";
		public const string InternalOnlyPolicy = "InternalOnlyPolicy";
		public const string InternalOrExternalPolicy = "InternalOrExternalPolicy";

		public static AuthorizationPolicy CreateInternalUserOnlyPolicy() =>
			new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.RequireClaim("type", "internal")
			.Build();

		public static AuthorizationPolicy CreateExternalUserOnlyPolicy() =>
			new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.RequireClaim("type", "external")
			//.RequireClaim("solution", "connect", "business") //Add extra security to only allow connect and business solution
			.Build();

		public static AuthorizationPolicy CreateInternalOrExternalUserPolicy() =>
			new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.RequireClaim("type", "internal", "external")
			.Build();

		public static AuthorizationPolicy CreateExternalConnectUserOnlyPolicy() =>
			new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.RequireClaim("type", "external")
			.RequireClaim("solution", "connect") //Add extra security to only allow connect solution
			.Build();

		public static AuthorizationPolicy CreateExternalBusinessUserOnlyPolicy() =>
			new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.RequireClaim("type", "external")
			.RequireClaim("solution", "business") //Add extra security to only allow business solution
			.Build();
	}
}
