using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.MediatR
{
	/// <summary>
	/// A marker class to represent a query.
	/// </summary>
	/// <typeparam name="TResponse"></typeparam>
	internal abstract class BaseQuery<TResponse> : BaseRequest<TResponse>
	{

	}
}
