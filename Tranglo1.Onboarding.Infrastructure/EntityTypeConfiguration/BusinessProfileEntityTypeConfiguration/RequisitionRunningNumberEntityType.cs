using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class RequisitionRunningNumberEntityType : BaseEntityTypeConfiguration<RequisitionRunningNumber>
    {
        protected override void Configure(EntityTypeBuilder<RequisitionRunningNumber> builder)
        {
            builder.ToTable("RequisitionRunningNumbers", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("RequisitionRunningNumbers");
            });


            builder.Property(o => o.Prefix)
                .HasMaxLength(150)
                .IsRequired(true)
                .HasColumnName("Prefix");

            builder.Property(o => o.RunningNumber)
                .IsRequired(true)
                .HasColumnName("RunningNumber");

            builder.Property(o => o.RequisitionType)
                .HasMaxLength(150)
                .HasColumnName("RequisitionType");
        }
    }
}
