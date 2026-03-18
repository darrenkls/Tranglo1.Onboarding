using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class KYCCategoryEntityTypeConfiguration : BaseEntityTypeConfiguration<KYCCategory>
    {
        protected override void Configure(EntityTypeBuilder<KYCCategory> builder)
        {
            builder.ToTable("KYCCategories", BusinessProfileDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("KYCCategoryCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.Property(o => o.PortalDisplayName)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("PortalDisplayName");

            builder.HasData(Enumeration.GetAll<KYCCategory>());

        }
    }
}
