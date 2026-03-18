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
    class ApprovalStatusEntityTypeConfiguration : BaseEntityTypeConfiguration<ApprovalStatus>
    {
        protected override void Configure(EntityTypeBuilder<ApprovalStatus> builder)
        {
            builder.ToTable("ApprovalStatus", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("ApprovalStatusCode");

            builder.Property(o => o.Name)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<ApprovalStatus>());
        }
    }
}
