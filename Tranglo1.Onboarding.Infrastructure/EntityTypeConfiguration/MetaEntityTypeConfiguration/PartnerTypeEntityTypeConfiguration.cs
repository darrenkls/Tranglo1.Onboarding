using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    class PartnerTypeEntityTypeConfiguration : BaseEntityTypeConfiguration<PartnerType>
    {
        protected override void Configure(EntityTypeBuilder<PartnerType> builder)
        {
            builder.ToTable("PartnerTypes", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("PartnerTypeCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<PartnerType>());
        }
    }
}
