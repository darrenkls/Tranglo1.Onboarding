using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    class VerificationIDTypeSectionEntityTypeConfiguration : BaseEntityTypeConfiguration<VerificationIDTypeSection>
    {
        protected override void Configure(EntityTypeBuilder<VerificationIDTypeSection> builder)
        {
            builder.ToTable("VerificationIDTypeSections", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.Property(a => a.Id)
                .HasColumnName("VerificationIDTypeSectionCode");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Description)
                .HasColumnName("Description");

            builder.HasOne(a => a.VerificationIDType)
                .WithMany()
                .HasForeignKey("VerificationIDTypeCode");
        }
    }
}
