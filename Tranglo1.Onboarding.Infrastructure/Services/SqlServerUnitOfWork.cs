using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Infrastructure.Services
{
	public sealed class SqlServerUnitOfWork : IUnitOfWork
	{
		private DbConnection _Connection;
		private DbTransaction _Transaction;

		public SqlServerUnitOfWork(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty.", nameof(connectionString));
			}

			ConnectionString = connectionString;

			_Connection = new SqlConnection(this.ConnectionString);
			_Connection.Open();
			_Transaction = _Connection.BeginTransaction();
		}

		public DbConnection Connection => _Connection ?? throw new ObjectDisposedException(nameof(Connection));

		public DbTransaction Transaction => _Transaction ?? throw new ObjectDisposedException(nameof(Transaction));

		public string ConnectionString { get; }
		public async Task RollbackAsync()
		{
			if (_Transaction != null)
			{
                try
                {
					await _Transaction.RollbackAsync();
				}
				catch( Exception ex )
				{ 

				}
				_Transaction = null;
			}
		}

		public async Task CommitAsync()
		{
			if (_Transaction != null)
			{
				await _Transaction.CommitAsync();
				_Transaction = null;
			}
		}

		public void Dispose()
		{
			if (_Transaction != null)
			{
				//Note: CommitAsync must be called in order to commit all the changes.
				//If UnitOfWork is disposed without CommentAsync, everything will
				//be rolled back (Similar to TransactionScope bahaviour)
				_Transaction.Rollback();
				_Transaction = null;
			}

			if (_Connection != null)
			{
				_Connection.Dispose();
				_Connection = null;
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (_Transaction != null)
			{
				//Note: CommitAsync must be called in order to commit all the changes.
				//If UnitOfWork is disposed without CommentAsync, everything will
				//be rolled back (Similar to TransactionScope bahaviour)
				await _Transaction.RollbackAsync();
				_Transaction = null;
			}

			if (_Connection != null)
			{
				await _Connection.DisposeAsync();
				_Connection = null;
			}
		}

		//public async Task RollbackAsync()
		//{
		//	if (_Transaction != null)
		//	{
		//		await _Transaction.RollbackAsync();
		//		_Transaction = null;
		//	}
		//}
	}
}
