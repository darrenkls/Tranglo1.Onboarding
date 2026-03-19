using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace Tranglo1.Onboarding.Domain.Common
{
    /// <summary>
    /// Base class for aggregate roots. Domain events raised during a unit of work
    /// are collected here and dispatched by the infrastructure layer before commit.
    ///
    /// Usage — raise an event from within the aggregate:
    ///     base.AddDomainEvent(new SomeEvent(...));
    /// </summary>
    public abstract class AggregateRoot : AggregateRoot<int>
    {
    }

    public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot
    {
        private readonly List<DomainEvent> _domainEvents;
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents?.AsReadOnly();

        protected AggregateRoot()
        {
            _domainEvents = new List<DomainEvent>();
        }

        public void AddDomainEvent(DomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(DomainEvent eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }
    }
}
