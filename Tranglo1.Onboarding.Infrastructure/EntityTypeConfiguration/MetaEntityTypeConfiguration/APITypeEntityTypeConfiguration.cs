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
    class APITypeEntityTypeConfiguration : BaseEntityTypeConfiguration<APIType>
    {
        protected override void Configure(EntityTypeBuilder<APIType> builder)
        {
            builder.ToTable("APIType", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("APITypeCode");

            builder.Property(o => o.Name)
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<APIType>());
        }
    }
}
