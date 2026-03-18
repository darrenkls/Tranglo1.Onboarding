using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class ScreeningEntityTypeEntityTypeConfiguration : IEntityTypeConfiguration<ScreeningEntityType>
    {
        public void Configure(EntityTypeBuilder<ScreeningEntityType> builder)
        {
            builder.ToTable("ScreeningEntityType", ScreeningDBContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("ScreeningEntityTypeCode");

            builder.Property(o => o.Name)
                .HasMaxLength(150)
                .IsRequired()
                .HasColumnName("Description");


            builder.Property(o => o.ExternalDescription)
                .HasMaxLength(150)
                .HasColumnName("ExternalDescription");

            builder.HasData(Enumeration.GetAll<ScreeningEntityType>());
        }
    }
}
