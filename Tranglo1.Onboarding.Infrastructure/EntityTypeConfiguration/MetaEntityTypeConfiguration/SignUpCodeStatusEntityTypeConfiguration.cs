using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.SignUpCodes;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    class SignUpCodeStatusEntityTypeConfiguration : BaseEntityTypeConfiguration<SignUpAccountStatus>

    {
        protected override void Configure(EntityTypeBuilder<SignUpAccountStatus> builder)
        {
            builder.ToTable("SignUpAccountStatus", SignUpCodeDBContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("SignUpAccountStatusCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<SignUpAccountStatus>());
        }
    }
}
