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
    class DocumentTemplateTypeEntityTypeConfiguration : BaseEntityTypeConfiguration<DocumentTemplateType>
    {
        protected override void Configure(EntityTypeBuilder<DocumentTemplateType> builder)
        {
            builder.ToTable("DocumentTemplateTypes", BusinessProfileDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("DocumentTemplateTypeCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("DocumentTemplateTypeDescription");

            builder.HasData(Enumeration.GetAll<DocumentTemplateType>());
        }
    }
}
