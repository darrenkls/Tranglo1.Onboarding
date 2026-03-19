using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.CustomerUserVerification;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class JumioUploadEntityTypeConfiguration : BaseEntityTypeConfiguration<JumioUpload>
    {
        protected override void Configure(EntityTypeBuilder<JumioUpload> builder)
        {
            builder.ToTable("JumioUploads", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.Property(a => a.Id)
               .HasColumnName("JumioUploadCode");
            builder.HasKey(a => a.Id);
        }
    }
}
