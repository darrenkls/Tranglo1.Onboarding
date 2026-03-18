using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
	internal abstract class BaseEntityTypeConfiguration<TEntity> : 
		IEntityTypeConfiguration<TEntity> where TEntity : class
	{
		public const string DateCreatedProperty = "CreatedDate";
		public const string CreatedByProperty = "CreatedBy";
		public const string CreatedByUserTypeProperty = "CreatedByUserType";

		public const string LastModifiedDateProperty = "LastModifiedDate";
		public const string LastModifiedByProperty = "LastModifiedBy";
		public const string LastModifiedByUserTypeProperty = "LastModifiedByUserType";

		public const string IsDeletedProperty = "IsDeleted";

		private Expression<Func<DateTime, DateTime>> DateToDatabase = d => d;
		private Expression<Func<DateTime, DateTime>> DateFromDatabase = 
			(d) => DateTime.SpecifyKind(d, DateTimeKind.Utc);

		Expression<Func<DateTime?, DateTime?>> NullDateToDatabase = (d) => d;
		Expression<Func<DateTime?, DateTime?>> NullDateFromDatabase = 
			(d) => d.HasValue ? (DateTime?)DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null;

		protected abstract void Configure(EntityTypeBuilder<TEntity> builder);

		void IEntityTypeConfiguration<TEntity>.Configure(EntityTypeBuilder<TEntity> builder)
		{
			this.Configure(builder);

			if (HasCreationInfo())
			{
				ConfigureCreationInfo(builder);
			}
			else
			{
				builder.HasAnnotation("HasCreationInfo", false);
			}

			if (HasLastModificationInfo())
			{
				ConfigureModificationInfo(builder);
			}
			else
			{
				builder.HasAnnotation("HasModificationInfo", false);
			}
		}

		protected virtual bool HasCreationInfo()
		{
			return typeof(Enumeration).IsAssignableFrom(typeof(TEntity)) == false;
		}

		protected virtual bool HasLastModificationInfo()
		{
			return typeof(Enumeration).IsAssignableFrom(typeof(TEntity)) == false;
		}

		protected void HasSoftDelete(EntityTypeBuilder<TEntity> builder)
		{
			builder.Property<bool>(IsDeletedProperty)
				.IsRequired()
				.HasColumnName("IsDeleted");

			//Apply deleted flag when EF is querying this TEntity
			builder.HasQueryFilter(entity => EF.Property<bool>(entity, IsDeletedProperty) == false);
		}

		private void ConfigureCreationInfo(EntityTypeBuilder<TEntity> builder)
		{
			builder.HasAnnotation("HasCreationInfo", true);

			builder.Property<DateTime>(DateCreatedProperty)
				.HasConversion(DateToDatabase, DateFromDatabase)
				.IsRequired(true)
				.HasDefaultValueSql("getutcdate()");

			builder.Property<int>(CreatedByProperty)
				.HasColumnName(CreatedByProperty)
				.IsRequired(true)
				.HasDefaultValue(0);

			//If no user type can be determined, then default to System
			builder.Property<UserType>(CreatedByUserTypeProperty)
				.HasColumnName(CreatedByUserTypeProperty)
				.IsRequired(true)
				.HasDefaultValue(UserType.System);
		}

		private void ConfigureModificationInfo(EntityTypeBuilder<TEntity> builder)
		{
			builder.HasAnnotation("HasModificationInfo", true);

			builder.Property<DateTime?>(LastModifiedDateProperty)
				.HasColumnName(LastModifiedDateProperty)
				.HasConversion(NullDateToDatabase, NullDateFromDatabase)
				.IsRequired(false);

			builder.Property<int?>(LastModifiedByProperty)
				.HasColumnName(LastModifiedByProperty)
				.IsRequired(false);

			//If no user type can be determined, then default to System
			builder.Property<UserType>(LastModifiedByUserTypeProperty)
				.HasColumnName(LastModifiedByUserTypeProperty)
				.IsRequired(true)
				.HasDefaultValue(UserType.System);
		}
	}
}
