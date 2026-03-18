using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.PartnerEntityTypeConfiguration
{
    class PartnerAPISettingEntityTypeConfiguration : BaseEntityTypeConfiguration<PartnerAPISetting>
    {
        protected override void Configure(EntityTypeBuilder<PartnerAPISetting> builder)
        {
            builder.ToTable("PartnerAPISettings", PartnerDBContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("PartnerAPISettingId");

            builder.HasOne(a => a.PartnerRegistration)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("PartnerCode");

            builder.Property(a => a.PartnerSubscriptionCode)
                .IsRequired(false)
                .HasColumnName("PartnerSubscriptionCode");

            builder.Property(a => a.IsPartnerConfirmWhitelisted)
                .HasDefaultValue(false);
        }
    }
}
