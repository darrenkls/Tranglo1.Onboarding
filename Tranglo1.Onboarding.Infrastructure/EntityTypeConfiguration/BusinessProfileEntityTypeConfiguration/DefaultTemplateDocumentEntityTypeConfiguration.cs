using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class DefaultTemplateDocumentEntityTypeConfiguration : BaseEntityTypeConfiguration<DefaultTemplateDocument>
    {
        protected override void Configure(EntityTypeBuilder<DefaultTemplateDocument> builder)
        {
            builder.ToTable("DefaultTemplateDocuments", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("DefaultTemplateDocuments");
            });

            builder.Property(o => o.Id)
               .IsRequired()
               .HasColumnName("DefaultTemplateDocumentCode");

            builder.HasOne(o => o.DefaultTemplate)
                .WithMany()
                .HasForeignKey("DefaultTemplateCode")
                .IsRequired(false);
        }
    }
}