using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    
    class CompanyShareholderEntityTypeConfiguration : BaseEntityTypeConfiguration<CompanyShareholder>
    {
        protected override void Configure(EntityTypeBuilder<CompanyShareholder> builder)
        {
            builder.ToTable("CompanyShareholders", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("CompanyShareholders");
            });

            builder.Property(e => e.CompanyName)
                    .HasMaxLength(150);

            builder.Property(e => e.CompanyRegNo)
                    .HasMaxLength(150);

            builder.HasOne(o => o.Country)
              .WithMany()
              .HasForeignKey("CountryCode")
              .IsRequired(false);
        }
    }
    
}
