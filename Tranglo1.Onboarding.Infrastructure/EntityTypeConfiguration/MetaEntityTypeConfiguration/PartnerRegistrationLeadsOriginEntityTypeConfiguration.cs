using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    internal class PartnerRegistrationLeadsOriginEntityTypeConfiguration : BaseEntityTypeConfiguration<PartnerRegistrationLeadsOrigin>

    {
        protected override void Configure(EntityTypeBuilder<PartnerRegistrationLeadsOrigin> builder)
        {
            builder.ToTable("PartnerRegistrationLeadsOrigins", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("PartnerRegistrationLeadsOriginCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<PartnerRegistrationLeadsOrigin>());
        }
    }
}
