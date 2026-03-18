using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Command
{
	/// <summary>
	/// A marker class to represent a command.
	/// </summary>
	/// <typeparam name="TResponse"></typeparam>
	public abstract class BaseCommand<TResponse> : BaseRequest<TResponse>
	{

	}
}
