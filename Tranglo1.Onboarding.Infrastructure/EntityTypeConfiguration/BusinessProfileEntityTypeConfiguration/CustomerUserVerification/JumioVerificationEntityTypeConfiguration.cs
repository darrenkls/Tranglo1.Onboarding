using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.CustomerUserVerification;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class JumioVerificationEntityTypeConfiguration : BaseEntityTypeConfiguration<JumioVerification>
    {
        protected override void Configure(EntityTypeBuilder<JumioVerification> builder)
        {
            builder.ToTable("JumioVerifications", BusinessProfileDbContext.EVENT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("JumioVerifications");
            });

            builder.Property(a => a.Id)
               .HasColumnName("JumioVerificationCode");
            builder.HasKey(a => a.Id);
        }
    }
}