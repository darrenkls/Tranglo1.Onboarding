using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    class TitleEntityTypeConfiguration : BaseEntityTypeConfiguration<Title>
    {
        protected override void Configure(EntityTypeBuilder<Title> builder)
        {
            builder.ToTable("Titles", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("TitleCode");

            builder.Property(o => o.Name)
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<Title>());
        }
    }
}
