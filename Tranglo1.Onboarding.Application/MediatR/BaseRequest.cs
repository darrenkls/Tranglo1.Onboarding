using MediatR;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.MediatR
{
	/// <summary>
	/// A base class representing a command or a query. Do not direct inherit from this 
	/// class. To create a command or query, please use <seealso cref="BaseCommand{TResponse}"/> or
	/// <seealso cref="BaseQuery{TResponse}"/>
	/// </summary>
	/// <typeparam name="TResponse"></typeparam>
	public abstract class BaseRequest<TResponse> : IRequest<TResponse>
	{
		/// <summary>
		/// Override this function to provide a meaningful audit log message. This
		/// function will be called after execution of the command.
		/// </summary>
		/// <param name="result">The returned result after this command is executed.</param>
		/// <returns>Return an audit log message that describing current 
		/// <seealso cref="BaseRequest{TResponse}"/>. Note that the returned message 
		/// does not need to mention current user's name/email...etc </returns>
		public virtual Task<string> GetAuditLogAsync(TResponse result)
		{
			return Task.FromResult<string>(null);
		}
	}
}
