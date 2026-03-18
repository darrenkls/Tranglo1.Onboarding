using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Infrastructure.Persistance;

namespace Tranglo1.Onboarding.Application.Security
{
	internal class AuditLogManager : IAuditLogManager
	{
		public AuditLogManager(AuditLogDbContext auditLogDbContext)
		{
			AuditLogDbContext = auditLogDbContext;
		}

		public AuditLogDbContext AuditLogDbContext { get; }

		public async Task LogAsync(AuditLog auditLog, CancellationToken cancellationToken)
		{
			this.AuditLogDbContext.AuditLogs.Add(auditLog);
			await this.AuditLogDbContext.SaveChangesAsync();
		}
	}
}
