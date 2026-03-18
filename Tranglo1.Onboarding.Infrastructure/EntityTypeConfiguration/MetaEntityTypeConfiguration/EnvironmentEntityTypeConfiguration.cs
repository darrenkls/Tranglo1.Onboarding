using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class EnvironmentEntityTypeConfiguration : BaseEntityTypeConfiguration<Environment>
    {
        protected override void Configure(EntityTypeBuilder<Environment> builder)
        {
            builder.ToTable("Environments", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("EnvironmentCode");

            builder.Property(o => o.Name)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<Environment>());
        }
    }
}
