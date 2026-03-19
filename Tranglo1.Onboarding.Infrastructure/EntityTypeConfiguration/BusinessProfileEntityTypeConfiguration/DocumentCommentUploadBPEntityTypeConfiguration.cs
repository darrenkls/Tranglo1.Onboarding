using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Tranglo1.Onboarding.Domain.Entities;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class DocumentCommentUploadBPEntityTypeConfiguration : BaseEntityTypeConfiguration<DocumentCommentUploadBP>
    {
        protected override void Configure(EntityTypeBuilder<DocumentCommentUploadBP> builder)
        {
            builder.ToTable("DocumentCommentUploadBPs", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasOne(o => o.DocumentCommentBP)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("DocumentCommentBPCode")
                .OnDelete(DeleteBehavior.Cascade);

            //Primary Key
            builder.HasKey(kyc => new { kyc.DocumentCommentBPCode, kyc.DocumentId });
        }
    }
}
