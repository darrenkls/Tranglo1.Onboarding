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
    class TrangloDepartmentEntityTypeConfiguration : BaseEntityTypeConfiguration<TrangloDepartment>
    {
        protected override void Configure(EntityTypeBuilder<TrangloDepartment> builder)
        {
            builder.ToTable("TrangloDepartments", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("TrangloDepartmentCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(TrangloDepartment.GetAllTrangloDepartment());
        }
    }
}
