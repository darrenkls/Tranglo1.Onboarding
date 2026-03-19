using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.PartnerEntityTypeConfiguration
{
    class PartnerSubscriptionEntityTypeConfiguration : BaseEntityTypeConfiguration<PartnerSubscription>
	{
		protected override void Configure(EntityTypeBuilder<PartnerSubscription> builder)
		{
			builder.ToTable("PartnerSubscriptions", PartnerDBContext.DEFAULT_SCHEMA);
			builder.HasTemporalTable(config =>
			{
				config.HistorySchema(PartnerDBContext.HISTORY_SCHEMA);
				config.HistoryTable("PartnerSubscriptions");
			});


			builder.HasKey(o => o.Id);

			builder.Property(o => o.Id)
				.IsRequired()
				.HasColumnName("PartnerSubscriptionCode");

            builder.HasOne(ps => ps.PartnerRegistration)
                .WithMany(pr => pr.PartnerSubscriptions)
                .HasForeignKey(ps => ps.PartnerCode);

            builder.HasOne(ps => ps.PartnerType)
				.WithMany()
				.IsRequired(false)
				.HasForeignKey(ps => ps.PartnerTypeCode);

			builder.HasOne(ps => ps.Solution)
				.WithMany()
				.IsRequired(false)
				.HasForeignKey(ps => ps.SolutionCode);

			builder.Property(a => a.TrangloEntity)
					.HasMaxLength(50);			

			builder.HasOne(ps => ps.Environment)
				.WithMany()
				.IsRequired(false)
				.HasForeignKey(ps => ps.EnvironmentCode);

			builder.Property(o => o.IsOnboardComplete)
				.IsRequired(false);

			builder.Property(o => o.APIIntegrationOnboardWorkflowStatusCode)
				.IsRequired(false);

			builder.Property(o => o.RspStagingId)
				.IsRequired(false);

			builder.Property(o => o.SupplierPartnerStagingId)
				.IsRequired(false);

			builder.Property(o => o.SettlementCurrencyCode)
				.HasMaxLength(3)
				.IsRequired(false);

			builder.Property(o => o.IsCurrencyCodeAssigned)
				.IsRequired(false);

			builder.Property(o => o.IsPricePackageAssigned)
				.IsRequired(false);

			builder.HasOne(ps => ps.PartnerAccountStatusType)
				.WithMany()
				.IsRequired(false)
				.HasForeignKey(ps => ps.PartnerAccountStatusTypeCode);

			builder.HasOne(ps => ps.KYCReminderSubscription)
				.WithMany()
				.IsRequired(false)
				.HasForeignKey(ps => ps.KYCReminderSubscriptionCode);
        }
	}
}

