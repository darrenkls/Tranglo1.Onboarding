using System;
using System.Collections.Generic;
using System.Text;

namespace CMSIntegrationTask.Services
{
	interface ICorrelationIdProvider
	{
		Guid GetCorrelationId();
	}

	class ScopedCorrelationIdProvider : ICorrelationIdProvider
	{
		public ScopedCorrelationIdProvider(Guid correlationId)
		{
			CorrelationId = correlationId;
		}

		public Guid CorrelationId { get; }

		public Guid GetCorrelationId()
		{
			return CorrelationId;
		}
	}
}
