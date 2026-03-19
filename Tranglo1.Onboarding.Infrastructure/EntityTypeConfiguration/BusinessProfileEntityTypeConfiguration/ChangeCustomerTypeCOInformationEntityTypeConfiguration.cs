using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class ChangeCustomerTypeCOInformationEntityTypeConfiguration : BaseEntityTypeConfiguration<ChangeCustomerTypeCOInformation>
    {
        protected override void Configure(EntityTypeBuilder<ChangeCustomerTypeCOInformation> builder)
        {
            builder.ToTable("ChangeCustomerTypeCOInformations", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("ChangeCustomerTypeCOInformations");
            });


            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("ChangeCustomerTypeCOInformationCode");
        }
    }
}