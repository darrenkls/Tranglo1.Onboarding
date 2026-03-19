using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.RBAEntityTypeConfiguration
{
    class OverridingRulesParameterEntityTypeConfiguration : BaseEntityTypeConfiguration<OverridingRulesParameter>
    {
        protected override void Configure(EntityTypeBuilder<OverridingRulesParameter> builder)
        {
            builder.ToTable("OverridingRulesParameter", RBADBContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(RBADBContext.HISTORY_SCHEMA);
                config.HistoryTable("OverridingRulesParameter");
            });


            // Primary Key
            builder.Property(er => er.Id)
                .HasColumnName("OverridingRuleParameterCode");
            builder.HasKey(er => er.Id);
        }
    }
}
