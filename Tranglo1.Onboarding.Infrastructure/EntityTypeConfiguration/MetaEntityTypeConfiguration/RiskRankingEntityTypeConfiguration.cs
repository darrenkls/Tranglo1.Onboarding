using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    class RiskRankingEntityTypeConfiguration : BaseEntityTypeConfiguration<RiskRanking>
    {
        protected override void Configure(EntityTypeBuilder<RiskRanking> builder)
        {
            builder.ToTable("RiskRanking", BusinessProfileDbContext.META_SCHEMA);


            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .IsRequired()
                   .HasColumnName("RiskRankingCode");

            builder.Property(a => a.Name)
                   .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<RiskRanking>());
        }
    }
}
