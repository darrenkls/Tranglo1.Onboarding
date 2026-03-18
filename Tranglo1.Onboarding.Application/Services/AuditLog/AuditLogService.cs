using System;
using System.Net;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Security;
using Tranglo1.Onboarding.Infrastructure.Services;
using System.Security.Claims;
using System.Threading;

namespace Tranglo1.Onboarding.Application.Services
{
    internal class AuditLogService : IAuditLogService
    {
        private IAuditLogManager _manager { get; }
        private IIdentityContext _identityContext { get; }
        public AuditLogService(IAuditLogManager auditLogManager, IIdentityContext identityContext) 
        {
            _manager = auditLogManager ?? throw new ArgumentNullException(nameof(auditLogManager));
            _identityContext = identityContext ?? throw new ArgumentNullException(nameof(identityContext));
        }

        public async Task PersistAuditLogAsync(
            DateTime eventDate, string actionDescription, 
            IPAddress ipAddress, string correlationId) 
        {

            var auditLog = new AuditLog()
            {
                Username = _identityContext.CurrentUser.GetSubjectId().HasValue ? _identityContext.CurrentUser?.GetSubjectId().Value : "System",
                UserType = _identityContext.CurrentUser.GetUserType(),
                EventDate = eventDate,
                ActionDescription = actionDescription,
                ClientAddress = ipAddress,
                CorrelationId = correlationId
            };
                
            await _manager.LogAsync(auditLog, CancellationToken.None);
        }
    }
}
