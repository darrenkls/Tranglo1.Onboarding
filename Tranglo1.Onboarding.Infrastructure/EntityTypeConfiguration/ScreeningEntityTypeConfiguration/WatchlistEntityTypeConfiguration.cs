using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class WatchlistEntityTypeConfiguration : BaseEntityTypeConfiguration<WatchlistReview>
    {
        protected override void Configure(EntityTypeBuilder<WatchlistReview> builder)
        {
            builder.ToTable("Watchlist", ScreeningDBContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(ScreeningDBContext.HISTORY_SCHEMA);
                config.HistoryTable("Watchlist");
            });

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("WatchlistCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(o => o.ScreeningInput)
                .WithMany()
                .HasForeignKey("ScreeningInputCode");

            builder.HasOne(o => o.WatchlistStatus)
                .WithMany()
                .HasForeignKey("WatchlistStatusCode");            
            
            builder.HasOne(o => o.EnforcementActions)
                .WithMany()
                .HasForeignKey("EnforcementActionsCode")
                .IsRequired(false);
        }
    }
}
