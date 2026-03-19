using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class BusinessDocumentGroupCategoryEntityTypeConfiguration : BaseEntityTypeConfiguration<BusinessDocumentGroupCategory>
    {
        protected override void Configure(EntityTypeBuilder<BusinessDocumentGroupCategory> builder)
        {
            builder.ToTable("BusinessDocumentGroupCategories", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("BusinessDocumentGroupCategories");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("BusinessDocumentGroupCategoryCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(e => e.GroupCategoryDescription)
                .HasColumnName("GroupCategoryDescription");


            builder.Property(o => o.TooltipDescription)
               .HasColumnName("TooltipDescription");
        }
    }
}
