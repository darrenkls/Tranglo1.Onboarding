using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    class ScreeningListSourceEntityTypeConfiguration : BaseEntityTypeConfiguration<ScreeningListSource>
    {
        protected override void Configure(EntityTypeBuilder<ScreeningListSource> builder)
        {
            builder.ToTable("ScreeningListSource", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("ScreeningListSourceCode");

            builder.Property(o => o.Name)
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<ScreeningListSource>());
        }
    }
}
