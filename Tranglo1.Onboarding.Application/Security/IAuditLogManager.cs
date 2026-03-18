using System.Threading;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Security
{
	public interface IAuditLogManager
	{
		Task LogAsync(AuditLog auditLog, CancellationToken cancellationToken);
	}
}
