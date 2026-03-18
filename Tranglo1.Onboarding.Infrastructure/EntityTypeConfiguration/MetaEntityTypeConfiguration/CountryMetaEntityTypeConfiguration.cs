using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class CountryMetaEntityTypeConfiguration : BaseEntityTypeConfiguration<CountryMeta>
    {
        protected override void Configure(EntityTypeBuilder<CountryMeta> builder)
        {
            builder.ToTable("CountryMetas", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("CountryCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.Property(o => o.CountryISO2)
                .HasMaxLength(2)
                .IsRequired();

            builder.Property(o => o.CountryISO3)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(o => o.DialCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasData(Enumeration.GetAll<CountryMeta>());
        }
    }
}
