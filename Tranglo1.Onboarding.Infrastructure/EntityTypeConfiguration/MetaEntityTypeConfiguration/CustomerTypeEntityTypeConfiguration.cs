using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class CustomerTypeEntityTypeConfiguration : BaseEntityTypeConfiguration<CustomerType>
    {
        protected override void Configure(EntityTypeBuilder<CustomerType> builder)
        {
            builder.ToTable("CustomerTypes", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .IsRequired()
                .HasColumnName("CustomerTypeCode");

            builder.Property(a => a.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<CustomerType>());
        }
    }
}
