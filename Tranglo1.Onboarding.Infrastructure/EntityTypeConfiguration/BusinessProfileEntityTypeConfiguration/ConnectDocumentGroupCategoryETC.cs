using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class ConnectDocumentGroupCategoryETC : BaseEntityTypeConfiguration<ConnectDocumentGroupCategory>
    {
        protected override void Configure(EntityTypeBuilder<ConnectDocumentGroupCategory> builder)
        {
            builder.ToTable("ConnectDocumentGroupCategories", BusinessProfileDbContext.META_SCHEMA);

            builder.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("ConnectDocumentGroupCategoryCode");

            builder.Property(e => e.Name)
               .IsRequired()
               .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<ConnectDocumentGroupCategory>());

        }
    }
}
