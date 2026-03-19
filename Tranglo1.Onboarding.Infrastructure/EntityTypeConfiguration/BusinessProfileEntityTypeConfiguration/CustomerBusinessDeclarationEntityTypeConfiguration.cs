using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class CustomerBusinessDeclarationEntityTypeConfiguration : BaseEntityTypeConfiguration<CustomerBusinessDeclaration>
    {
        protected override void Configure(EntityTypeBuilder<CustomerBusinessDeclaration> builder)
        {
            builder.ToTable("CustomerBusinessDeclarations", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("CustomerBusinessDeclarationCode");

            builder.HasOne(o => o.BusinessDeclarationStatus)
                .WithMany()
                .HasForeignKey("BusinessDeclarationStatusCode");
        }
    }
}
