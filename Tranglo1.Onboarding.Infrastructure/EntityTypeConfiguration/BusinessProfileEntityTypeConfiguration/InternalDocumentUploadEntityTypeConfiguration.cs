using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class InternalDocumentUploadEntityTypeConfiguration : BaseEntityTypeConfiguration<InternalDocumentUpload>
    {
        protected override void Configure(EntityTypeBuilder<InternalDocumentUpload> builder)
        {
            builder.ToTable("InternalDocumentUploads", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("InternalDocumentUploadId");

            builder.HasOne(a => a.BusinessProfile)
               .WithMany()
               .IsRequired(true)
               .HasForeignKey("BusinessProfileCode");
        }
    }
}
