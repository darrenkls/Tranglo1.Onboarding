using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class UserRoleEntityTypeConfiguration : BaseEntityTypeConfiguration<UserRole>
    {
        protected override void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("UserRoleCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<UserRole>());
        }
    }
}
