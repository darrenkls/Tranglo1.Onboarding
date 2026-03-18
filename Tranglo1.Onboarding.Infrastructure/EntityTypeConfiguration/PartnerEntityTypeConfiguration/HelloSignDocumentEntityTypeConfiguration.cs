using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class HelloSignDocumentEntityTypeConfiguration:BaseEntityTypeConfiguration<HelloSignDocument>
    {
        protected override void Configure(EntityTypeBuilder<HelloSignDocument> builder)
        {
            builder.ToTable("HelloSignDocuments", PartnerDBContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("HelloSignDocumentId");

            builder.HasOne(a => a.PartnerRegistration)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("PartnerCode");

            builder.Property(a => a.DocumentName)
                .HasMaxLength(150);
        }
    }
}
