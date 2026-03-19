using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class BusinessDocumentCategoryEntityTypeConfiguration : BaseEntityTypeConfiguration<BusinessDocumentCategory>
    {
        protected override void Configure(EntityTypeBuilder<BusinessDocumentCategory> builder)
        {
            builder.ToTable("BusinessDocumentCategories", BusinessProfileDbContext.DEFAULT_SCHEMA);

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("BusinessDocumentCategoryCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(e => e.BusinessDocumentGroupCategory)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessDocumentGroupCategoryCode");


            builder.HasOne(o => o.DocumentCategoryGroup)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("DocumentCategoryGroupCode");

            builder.HasOne(o => o.DocumentCategory)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("DocumentCategoryCode");

        }
    }
}