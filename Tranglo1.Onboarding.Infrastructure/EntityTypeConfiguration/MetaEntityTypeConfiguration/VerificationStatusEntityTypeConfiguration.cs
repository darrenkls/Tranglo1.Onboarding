using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    class VerificationStatusEntityTypeConfiguration : BaseEntityTypeConfiguration<VerificationStatus>
    {
        protected override void Configure(EntityTypeBuilder<VerificationStatus> builder)
        {
            builder.ToTable("VerificationStatus", BusinessProfileDbContext.META_SCHEMA);

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .IsRequired()
                   .HasColumnName("VerificationStatusCode");

            builder.Property(a => a.Name)
                   .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<VerificationStatus>());

        }
    }
}
