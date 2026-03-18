using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.OwnershipAndManagementStructure
{
    class ShareholderCompanyLegalEntityEntityTypeConfiguration
    : BaseEntityTypeConfiguration<ShareholderCompanyLegalEntity>
    {
        protected override void Configure(EntityTypeBuilder<ShareholderCompanyLegalEntity> builder)
        {
            builder.ToTable("ShareholderCompanyLegalEntities", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("ShareholderCompanyLegalEntities");
            });

      

            builder.HasOne(o => o.Shareholder)
             .WithMany()
             .HasForeignKey("ShareholderCode")
             .IsRequired(true);

            builder.HasOne(o => o.Country)
               .WithMany()
               .HasForeignKey("CountryCode")
               .IsRequired(false);

        }
    }

}