using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.PartnerEntityTypeConfiguration
{
    class PartnerTPNMigrationDetailEntityTypeConfiguration : BaseEntityTypeConfiguration<PartnerTPNMigrationDetail>
    {
		protected override void Configure(EntityTypeBuilder<PartnerTPNMigrationDetail> builder)
		{
			builder.ToTable("PartnerTPNMigrationDetails", PartnerDBContext.DEFAULT_SCHEMA);

			builder.HasTemporalTable(config =>
			{
				config.HistorySchema(PartnerDBContext.HISTORY_SCHEMA);
				config.HistoryTable("PartnerTPNMigrationDetails");
			});

			builder.HasKey(o => o.Id);

			builder.Property(o => o.Id)
				.IsRequired()
				.HasColumnName("PartnerTPNMigrationCode");

			builder.Property(o => o.SessionId)
				.IsRequired(false);

			builder.Property(o => o.SessionDate)
				.IsRequired(false);

			builder.Property(o => o.isDocUploaded)
				.IsRequired(false);

			builder.Property(o => o.isAgreementUploaded)
				.IsRequired(false);
		}
 
		protected override bool HasCreationInfo()
		{
			return false;
		}

		protected override bool HasLastModificationInfo()
		{
			return false;
		}
	}
}
