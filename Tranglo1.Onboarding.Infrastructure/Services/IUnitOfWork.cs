using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Infrastructure.Services
{
	public interface IUnitOfWork : IDisposable, IAsyncDisposable
	{
		DbConnection Connection { get; }
		DbTransaction Transaction { get; }
		Task CommitAsync();
		Task RollbackAsync();
	}
}
