using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.CustomerUserVerification;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class JumioAccountCreationEntityTypeConfiguration : BaseEntityTypeConfiguration<JumioAccountCreation>
    {
        protected override void Configure(EntityTypeBuilder<JumioAccountCreation> builder)
        {
            builder.ToTable("JumioAccountCreations", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.Property(a => a.Id)
               .HasColumnName("JumioAccountCreationCode");
            builder.HasKey(a => a.Id);
        }
    }
}