using System;
using System.Net;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Application.Services
{
    public interface IAuditLogService
    {
        /// <summary>
        /// NOTE : For <see cref="Controllers.AccountController"/> AND <see cref="Controllers.MFA.MFAController"/> as both controllers are not using <see cref="Command.BaseCommand{TResponse}"/>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userType"></param>
        /// <param name="eventDate"></param>
        /// <param name="actionDescription"></param>
        /// <param name="ipAddress"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        Task PersistAuditLogAsync(DateTime eventDate, string actionDescription, IPAddress ipAddress, string correlationId);
    }
}
