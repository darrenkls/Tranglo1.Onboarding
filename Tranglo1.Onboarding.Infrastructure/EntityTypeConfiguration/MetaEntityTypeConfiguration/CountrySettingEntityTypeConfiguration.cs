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
    class CountrySettingEntityTypeConfiguration : BaseEntityTypeConfiguration<CountrySetting>
    {
        protected override void Configure(EntityTypeBuilder<CountrySetting> builder)
        {
            builder.ToTable("CountrySettings", ApplicationUserDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(CountrySettingDbContext.HISTORY_SCHEMA);
                config.HistoryTable("CountrySettings");
            });


            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("CountrySettingCode");

            builder.HasOne(p => p.Country)
                .WithMany()
                .IsRequired()
                .HasForeignKey("CountryCode");

            builder.Property(o => o.IsHighRisk)
                .HasColumnName("IsHighRisk");

            builder.Property(o => o.IsSanction)
                .HasColumnName("IsSanction");

            builder.Property(o => o.IsDisplay)
                .HasColumnName("IsDisplay");

            builder.Property(o => o.IsRejectTransaction)
                .HasColumnName("IsRejectTransaction");
        }
    }
}
