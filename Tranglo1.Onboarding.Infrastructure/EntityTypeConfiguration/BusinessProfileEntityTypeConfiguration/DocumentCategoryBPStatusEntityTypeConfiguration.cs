using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class DocumentCategoryBPStatusEntityTypeConfiguration : BaseEntityTypeConfiguration<DocumentCategoryBPStatus>
    {
        protected override void Configure(EntityTypeBuilder<DocumentCategoryBPStatus> builder)
        {
            builder.ToTable("DocumentCategoryBPStatus", BusinessProfileDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("DocumentCategoryBPStatusCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("DocumentCategoryBPStatusDescription");

            builder.HasData(Enumeration.GetAll<DocumentCategoryBPStatus>());
        }
    }
}
