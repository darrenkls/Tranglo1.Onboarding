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
    class RoleStatusEntityTypeConfiguration : BaseEntityTypeConfiguration<RoleStatus>
    {
        protected override void Configure(EntityTypeBuilder<RoleStatus> builder)
        {
            builder.ToTable("RoleStatus", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("RoleStatusCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<RoleStatus>());
        }
    }
}
