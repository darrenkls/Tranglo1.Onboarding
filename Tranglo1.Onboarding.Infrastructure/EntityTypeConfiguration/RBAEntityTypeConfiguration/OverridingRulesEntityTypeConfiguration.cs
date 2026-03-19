using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class OverridingRulesEntityTypeConfiguration : BaseEntityTypeConfiguration<OverridingRules>
    {
        protected override void Configure(EntityTypeBuilder<OverridingRules> builder)
        {
            builder.ToTable("OverridingRules", RBADBContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(RBADBContext.HISTORY_SCHEMA);
                config.HistoryTable("OverridingRules");
            });


            // Primary Key
            builder.Property(er => er.Id)
                .HasColumnName("OverridingRulesCode");
            builder.HasKey(er => er.Id);

            // Relationships
            builder.HasOne(er => er.RBA)
                   .WithMany(rba => rba.OverridingRules)
                   .HasForeignKey("RBACode");
        }
    }
}
