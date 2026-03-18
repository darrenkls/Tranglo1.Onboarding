using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class KYCSubModuleReviewEntityTypeConfiguration : BaseEntityTypeConfiguration<KYCSubModuleReview>
    {
        protected override void Configure(EntityTypeBuilder<KYCSubModuleReview> builder)
        {
            builder.ToTable("KYCSubModuleReview", BusinessProfileDbContext.DEFAULT_SCHEMA);
            /*
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("KYCSubModuleReview");
            });
            */                    
            builder.HasOne(o => o.ReviewResult)
              .WithMany()
              .IsRequired(false)
              .HasForeignKey("ReviewResultCode");

            builder.HasOne(o => o.BusinessProfile)
              .WithMany()
              .IsRequired()
              .HasForeignKey("BusinessProfileCode");

            builder.HasOne(o => o.KYCCategory)
              .WithMany()
              .IsRequired()
              .HasForeignKey("KYCCategoryCode");

            //Primary Key
            builder.HasKey(o => new { o.BusinessProfileCode, o.KYCCategoryCode });

        }
    }
}
