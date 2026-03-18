using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration;
using Tranglo1.Onboarding.Infrastructure.Event;
using Tranglo1.Onboarding.Infrastructure.Services;
using System.Security.Claims;
using Tranglo1.Onboarding.Domain.DomainServices;

namespace Tranglo1.Onboarding.Infrastructure.Persistence
{
	public abstract class BaseDbContext : DbContext
	{
		private static readonly Type EnumerationType = typeof(Enumeration);
		private readonly IIdentityContext identityContext;
		private List<EntityEntry<IAggregateRoot>> _AggregateRoots;
		private List<DomainEvent> _PendingEvents;

		public IReadOnlyList<DomainEvent> PendingEvents => _PendingEvents.AsReadOnly();
		public IEventDispatcher Dispatcher { get; }

		protected BaseDbContext(DbContextOptions dbContextOptions, IEventDispatcher dispatcher,
			IUnitOfWork unitOfWorkContext,
			IIdentityContext identityContext)
			: base(dbContextOptions)
		{
			Dispatcher = dispatcher;

			if (unitOfWorkContext != null)
			{
				var _ThisConnection = this.Database.GetConnectionString();
				var _ThatConnection = unitOfWorkContext.Connection.ConnectionString;

				if (string.Equals(_ThisConnection, _ThatConnection, StringComparison.OrdinalIgnoreCase))
				{
					this.Database.SetDbConnection(unitOfWorkContext.Connection);
					this.Database.UseTransaction(unitOfWorkContext.Transaction);
				}
			}

			//Default behaviour, in case this is triggered from a backend application.
			//User facing application should aware and register an appropriate implementation
			//of IIdentityContext
			if (identityContext == null)
			{
				identityContext = new BackendIdentityContext();
			}

			this.identityContext = identityContext;
			this._PendingEvents = new List<DomainEvent>();
			this._AggregateRoots = new List<EntityEntry<IAggregateRoot>>();
		}

		protected void ClearDomainEvents()
		{
			this._PendingEvents.Clear();
			ClearAggregateEvents();
		}

		protected void ExcludeEnumerations()
		{
			IEnumerable<EntityEntry> enumerationEntries = ChangeTracker.Entries()
				.Where(x => EnumerationType.IsAssignableFrom(x.Entity.GetType()));

			foreach (EntityEntry enumerationEntry in enumerationEntries)
			{
				enumerationEntry.State = EntityState.Unchanged;
			}
		}

		protected void DispatchEvents()
		{
			if (this.Dispatcher == null)
			{
				return;
			}

			Task[] dispatchTasks = new Task[this._PendingEvents.Count];

			for (int i = 0; i < this._PendingEvents.Count; i++)
			{
				try
				{
					dispatchTasks[i] = this.Dispatcher.DispatchAsync(this._PendingEvents[i]);
				}
				catch (Exception)
				{
					//TODO: What to do here?
				}
			}

			Task.WaitAll(dispatchTasks);
		}

		protected async Task DispatchEventsAsync()
		{
			if (this.Dispatcher == null)
			{
				return;
			}

			Task[] dispatchTasks = new Task[this._PendingEvents.Count];

			for (int i = 0; i < this._PendingEvents.Count; i++)
			{
				try
				{
					dispatchTasks[i] = this.Dispatcher.DispatchAsync(this._PendingEvents[i]);
				}
				catch (Exception)
				{
					//TODO: What to do here?
				}
			}

			await Task.WhenAll(dispatchTasks);
		}

		/// <summary>
		/// Reset domain events hold by aggregate roots, and clear the internal list
		/// that holding aggretate entities
		/// </summary>
		protected virtual void ClearAggregateEvents()
		{
			foreach (var item in this._AggregateRoots)
			{
				item.Entity.ClearDomainEvents();
			}

			this._AggregateRoots.Clear();
		}

		/// <summary>
		/// All aggregate roots will be collected into an internal list, and harvest all
		/// domain events raised by aggregate root
		/// </summary>
		protected virtual void CollectDomainEvents()
		{
			/*
			var tmpListOfAggregrateRoots = this.ChangeTracker.Entries().Where(x => x.GetType().IsAssignableFrom(typeof(AggregateRoot<>)));

			foreach (var entity in tmpListOfAggregrateRoots)
			{
				var test = entity.
				this._PendingEvents.AddRange(entity.Entity.DomainEvents);
			}
			*/
			//var tmp = this.ChangeTracker.Entries().ToList().First().Entity.GetType().IsAssignableFrom(typeof(AggregateRoot<>));
			//var test = this.ChangeTracker.Entries<AggregateRoot>();
			this._AggregateRoots.AddRange(this.ChangeTracker.Entries<IAggregateRoot>());
			//this._AggregateRoots.AddRange(tmp);
			foreach (var e in this._AggregateRoots)
			{
				this._PendingEvents.AddRange(e.Entity.DomainEvents);
			}

			//var DomainEvent = this.ChangeTracker.Entries<DomainEventService>();

			//if (DomainEvent.Any())
			//{
			//	foreach (var root in DomainEvent.ToList())
			//	{
			//		var _Events = root.Entity.DomainEvents;
			//		foreach (var e in _Events)
			//		{
			//			//Fill domain events with computed properties from aggregate/entities 
			//			e.MaterializeEvent();
			//			this.Attach(e);
			//		}
			//		//await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
			//	}

			//	//This will save all attached events (if any)

			//}
		}

		protected virtual void HandleCreationAndModification()
		{
			DateTime _Now = DateTime.UtcNow;
			Maybe<int> _CurrentUser = this.identityContext.CurrentUser.GetUserId();
			UserType _CurrentUserType = this.identityContext.CurrentUser.GetUserType();

			int _UserId = 0;
			if (_CurrentUser.HasValue)
			{
				_UserId = _CurrentUser.Value;
			}

			foreach (var e in this.ChangeTracker.Entries())
			{
				if (e.State == EntityState.Added)
				{
					var _HasCreationInfo = e.Metadata.FindAnnotation("HasCreationInfo");
					if (_HasCreationInfo != null && 
						string.Equals(bool.TrueString, _HasCreationInfo.Value.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						e.Property(BaseEntityTypeConfiguration<Entity>.DateCreatedProperty).CurrentValue = _Now;
						e.Property(BaseEntityTypeConfiguration<Entity>.CreatedByProperty).CurrentValue = _UserId;
						e.Property(BaseEntityTypeConfiguration<Entity>.CreatedByUserTypeProperty).CurrentValue = _CurrentUserType;
					}
				}
				else if (e.State == EntityState.Modified)
				{
					var _HasModificationInfo = e.Metadata.FindAnnotation("HasModificationInfo");
					if (_HasModificationInfo != null &&
						string.Equals(bool.TrueString, _HasModificationInfo.Value.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						e.Property(BaseEntityTypeConfiguration<Entity>.LastModifiedDateProperty).CurrentValue = _Now;
						e.Property(BaseEntityTypeConfiguration<Entity>.LastModifiedByProperty).CurrentValue = _UserId;
						e.Property(BaseEntityTypeConfiguration<Entity>.LastModifiedByUserTypeProperty).CurrentValue = _CurrentUserType;
					}
				}
				else if (e.State == EntityState.Deleted)
				{
					var _IsDeleted = FindProperty(e, BaseEntityTypeConfiguration<Entity>.IsDeletedProperty);

					//This entity do not have soft delete, so we just skip it
					if (_IsDeleted == null)
					{
						continue;
					}

					//We mark the entity become Unchanged, and all properties of the soft delete entity become un-modified,
					//hence EF will not generate UPDATE- statement for those columns later.
					//Note that if there is any property changed on soft deleted entities, those changes are ignored.
					//The entity state will become Modified again once we change the "IsDeleted" property to "true"
					e.State = EntityState.Unchanged;

					//We will update the last modified date when something is soft deleted.
					var _HasModificationInfo = e.Metadata.FindAnnotation("HasModificationInfo");
					if (_HasModificationInfo != null &&
						string.Equals(bool.TrueString, _HasModificationInfo.Value.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						e.Property(BaseEntityTypeConfiguration<Entity>.LastModifiedDateProperty).CurrentValue = _Now;
						e.Property(BaseEntityTypeConfiguration<Entity>.LastModifiedByProperty).CurrentValue = _UserId;
						e.Property(BaseEntityTypeConfiguration<Entity>.LastModifiedByUserTypeProperty).CurrentValue = _CurrentUserType;
					}

					//.References will give us either ValueObjects, or the other entities that are reachable 
					//from current entity.
					//When we remove an entity from DbContext, EF will mark all reference state as Modified (not deleted)
					//so we have to go through all of them, and make them become Unchanged and not modified.
					foreach (var reference in e.References)
					{
						if (reference.TargetEntry == null)
						{
							continue;
						}

						reference.TargetEntry.State = EntityState.Unchanged;
						reference.IsModified = false;
					}

					foreach (var property in e.Properties)
					{
						property.IsModified = false;
					}

					//TODO: How about Collections?

					//This will cause EF to produce the following query:
					//	UPDATE .... SET [IsDeleted] = 'TRUE'
					e.Property(BaseEntityTypeConfiguration<Entity>.IsDeletedProperty).CurrentValue = true;

				}
			}
		}

		private void BeforeSaveChanges()
		{
			CollectDomainEvents();
			ExcludeEnumerations();
			HandleCreationAndModification();
		}

		private void AfterSaveChanges()
		{
			ClearDomainEvents();
		}

		private PropertyEntry FindProperty(EntityEntry entry, string name)
		{
			return (from p in entry.Properties
					where p.Metadata.Name == name
					select p).FirstOrDefault();
		}

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			//Clear the events if it is 2nd run
			if(this._PendingEvents.Count > 0)
            {
				AfterSaveChanges();
			}

			BeforeSaveChanges();
			try
			{
				int _SaveChanges = base.SaveChanges(acceptAllChangesOnSuccess);
				DispatchEvents();
				return _SaveChanges;
			}
			finally
			{
				AfterSaveChanges();
			}
		}

		public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		{
			//Clear the events if it is 2nd run
			if (this._PendingEvents.Count > 0)
			{
				AfterSaveChanges();
			}
			BeforeSaveChanges();
			try
			{
				int _SaveChangesAsync = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

				//var AggregateRoots = this.ChangeTracker.Entries<DomainEventService>();

				//if (AggregateRoots.Any())
				//{
				//	foreach (var root in AggregateRoots)
				//	{
				//		var _Events = root.Entity.DomainEvents;
				//		foreach (var e in _Events)
				//		{
				//			//Fill domain events with computed properties from aggregate/entities 
				//			e.MaterializeEvent();
				//			this.Attach(e);
				//		}
				//		await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
				//		root.Entity.ClearDomainEvents();
				//	}
				//}
				await DispatchEventsAsync();
				return _SaveChangesAsync;


			}
			finally
			{
				AfterSaveChanges();
			}
		}
		
	}
}
