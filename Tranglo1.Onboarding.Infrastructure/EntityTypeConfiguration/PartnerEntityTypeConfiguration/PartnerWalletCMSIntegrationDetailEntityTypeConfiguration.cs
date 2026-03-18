using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.PartnerEntityTypeConfiguration
{
	class PartnerWalletCMSIntegrationDetailEntityTypeConfiguration : BaseEntityTypeConfiguration<PartnerWalletCMSIntegrationDetail>
	{
		protected override void Configure(EntityTypeBuilder<PartnerWalletCMSIntegrationDetail> builder)
		{
			builder.ToTable("PartnerWalletIntegrationDetails", PartnerDBContext.DEFAULT_SCHEMA);

			builder.HasKey(o => o.Id);

			builder.Property(o => o.Id)
				.IsRequired()
				.HasColumnName("PartnerWalletIntegrationId");

			builder.Property(o => o.PartnerSubscriptionCode)
				.IsRequired(false)
				.HasColumnName("PartnerSubscriptionCode");
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
