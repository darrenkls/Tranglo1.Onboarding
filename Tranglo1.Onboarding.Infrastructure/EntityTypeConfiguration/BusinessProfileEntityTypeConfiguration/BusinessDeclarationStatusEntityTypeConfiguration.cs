using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class BusinessDeclarationStatusEntityTypeConfiguration : BaseEntityTypeConfiguration<BusinessDeclarationStatus>
    {
        protected override void Configure(EntityTypeBuilder<BusinessDeclarationStatus> builder)
        {
            builder.ToTable("BusinessDeclarationStatuses", BusinessProfileDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("BusinessDeclarationStatusCode");

            builder.Property(o => o.Name)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<BusinessDeclarationStatus>());
        }
    }
}
