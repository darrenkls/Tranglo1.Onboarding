using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    class RiskScoreEntityTypeConfiguration : BaseEntityTypeConfiguration<RiskScore>
    {
        protected override void Configure(EntityTypeBuilder<RiskScore> builder)
        {
            builder.ToTable("RiskScores", BusinessProfileDbContext.META_SCHEMA);

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .IsRequired()
                   .HasColumnName("RiskScoreCode");

            builder.Property(a => a.Name)
                   .HasColumnName("Description");

            builder.Property(a => a.LowRange)
                   .IsRequired()
                   .HasColumnName("LowRange")
                   .HasColumnType("decimal(18,2)") 
                   .HasPrecision(18, 2); 

            builder.Property(a => a.HighRange)
                    .IsRequired()
                    .HasColumnName("HighRange")
                    .HasColumnType("decimal(18,2)")
                    .HasPrecision(18, 2);

            builder.HasData(
                RiskScore.Low_Risk,
                RiskScore.Medium_Risk,
                RiskScore.High_Risk
            );

        }
    }
}
