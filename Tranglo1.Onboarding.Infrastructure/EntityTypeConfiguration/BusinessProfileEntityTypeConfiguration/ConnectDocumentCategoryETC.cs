using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class ConnectDocumentCategoryETC : BaseEntityTypeConfiguration<ConnectDocumentCategory>
    {
        protected override void Configure(EntityTypeBuilder<ConnectDocumentCategory> builder)
        {
            builder.ToTable("ConnectDocumentCategories", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.Ignore(o => o.Id);

            //Foreign Key
            builder.HasOne(e => e.ConnectDocumentGroupCategory)
              .WithMany()
              .IsRequired(true)
              .HasForeignKey("ConnectDocumentGroupCategoryCode");

            builder.HasOne(e => e.DocumentCategory)
              .WithMany()
              .IsRequired(true)
              .HasForeignKey("DocumentCategoryCode");

            builder.Property(e => e.GroupSequence)
                .HasColumnName("GroupSequence")
                .IsRequired();

            builder.HasKey("ConnectDocumentGroupCategoryCode", "DocumentCategoryCode");

        }

        protected override bool HasCreationInfo()
        {
            return false;
        }

        protected override bool HasLastModificationInfo()
        {
            return false;
        }
    }
}
