using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    class CompanyUserBlockStatusEntityTypeConfiguration : BaseEntityTypeConfiguration<CompanyUserBlockStatus>
    {
        protected override void Configure(EntityTypeBuilder<CompanyUserBlockStatus> builder)
        {
            builder.ToTable("CompanyUserBlockStatus", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("CompanyUserBlockStatusCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<CompanyUserBlockStatus>());
        }
    }
}
