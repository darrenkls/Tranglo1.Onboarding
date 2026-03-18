using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.PartnerEntityTypeConfiguration
{
	class PartnerCMSIntegrationDetailEntityTypeConfiguration : BaseEntityTypeConfiguration<PartnerCMSIntegrationDetail>
	{
		protected override void Configure(EntityTypeBuilder<PartnerCMSIntegrationDetail> builder)
		{
			builder.ToTable("PartnerCMSIntegrationDetails", PartnerDBContext.DEFAULT_SCHEMA);

			builder.HasKey(o => o.Id);

			builder.Property(o => o.Id)
				.IsRequired()
				.HasColumnName("PartnerCMSIntegrationId");

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
