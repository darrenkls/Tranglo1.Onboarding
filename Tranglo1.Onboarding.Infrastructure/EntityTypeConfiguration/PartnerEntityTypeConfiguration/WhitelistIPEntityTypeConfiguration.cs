using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.PartnerEntityTypeConfiguration
{
    class WhitelistIPEntityTypeConfiguration : BaseEntityTypeConfiguration<WhitelistIP>
    {
        protected override void Configure(EntityTypeBuilder<WhitelistIP> builder)
        {
            builder.ToTable("WhitelistIPs", PartnerDBContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("WhitelistIPId");

            builder.Property(a => a.PartnerSubscriptionCode)
                .IsRequired(false)
                .HasColumnName("PartnerSubscriptionCode");

            builder.HasOne(a => a.PartnerRegistration)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("PartnerCode");
        }
    }
}
