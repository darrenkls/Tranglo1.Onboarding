using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class AccountStatusEntityTypeConfiguration : BaseEntityTypeConfiguration<AccountStatus>
    {
        protected override void Configure(EntityTypeBuilder<AccountStatus> builder)
        {            
            builder.ToTable("AccountStatus", ApplicationUserDbContext.META_SCHEMA);
            
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("AccountStatusCode");

            builder.Property(o => o.Name)
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<AccountStatus>());
        }
    }
}
