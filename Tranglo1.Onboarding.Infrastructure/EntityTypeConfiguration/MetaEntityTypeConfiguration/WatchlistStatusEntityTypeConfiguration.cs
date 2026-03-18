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
    class WatchlistStatusEntityTypeConfiguration : IEntityTypeConfiguration<WatchlistStatus>
    {
        public void Configure(EntityTypeBuilder<WatchlistStatus> builder)
        {
            builder.ToTable("WatchlistStatus", ScreeningDBContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("WatchlistStatusCode");

            builder.Property(o => o.Name)
                .HasMaxLength(150)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<WatchlistStatus>());
        }
    }
}
