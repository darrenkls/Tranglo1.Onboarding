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
    class CompanyUserAccountStatusEntityTypeConfiguration : BaseEntityTypeConfiguration<CompanyUserAccountStatus> 
    {
        protected override void Configure(EntityTypeBuilder<CompanyUserAccountStatus> builder)
        {
            builder.ToTable("CompanyUserAccountStatus", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("CompanyUserAccountStatusCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<CompanyUserAccountStatus>());
        }
    }
}
