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
    class ScreeningTypeListEntityTypeConfiguration : IEntityTypeConfiguration<ScreeningTypeList>
    {
        public void Configure(EntityTypeBuilder<ScreeningTypeList> builder)
        {
            builder.ToTable("ScreeningTypeList", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("ScreeningTypeListCode");

            builder.Property(o => o.Name)
                .HasMaxLength(150)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<ScreeningTypeList>());
        }
    }
}
