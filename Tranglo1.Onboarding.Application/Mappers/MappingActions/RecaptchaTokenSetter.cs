using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.CustomerUserList.Commands;
using Tranglo1.Onboarding.Application.Models;

namespace Tranglo1.Onboarding.Application.Mappers.MappingActions
{
	internal class RecaptchaTokenSetter : IMappingAction<RegisterInputModel, RegisterCustomerUserCommand>
	{
		private readonly IHttpContextAccessor httpContextAccessor;

		public RecaptchaTokenSetter(IHttpContextAccessor httpContextAccessor)
		{
			this.httpContextAccessor = httpContextAccessor;
		}

		public void Process(RegisterInputModel source, RegisterCustomerUserCommand destination, ResolutionContext context)
		{
			destination.RecaptchaToken = httpContextAccessor.HttpContext.Request.Headers["RecaptchaToken"];
		}
	}
}
