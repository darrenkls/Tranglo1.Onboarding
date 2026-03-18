using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.OwnershipAndManagementStructure
{
    class ShareholderIndividualLegalEntityEntityTypeConfiguration : BaseEntityTypeConfiguration<ShareholderIndividualLegalEntity>
    {
        protected override void Configure(EntityTypeBuilder<ShareholderIndividualLegalEntity> builder)
        {
            builder.ToTable("ShareholderIndividualLegalEntities", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("ShareholderIndividualLegalEntities");
            });

            builder.HasOne(e => e.Nationality)
              .WithMany()
              .HasForeignKey("CountryCode")
              .IsRequired(false);
            builder.HasOne(e => e.CountryOfResidence)
            .WithMany()
            .HasForeignKey("CountryResidenceCode")
            .IsRequired(false);

            builder.HasOne(o => o.IDType)
                .WithMany()
                .HasForeignKey("IDTypeCode");

            builder.Property(e => e.IDNumber)
                    .HasMaxLength(150);

            builder.HasOne(o => o.Gender)
                .WithMany()
                .HasForeignKey("GenderCode");

            builder.HasOne(o => o.Shareholder)
            .WithMany()
            .HasForeignKey("ShareholderCode")
            .IsRequired(true);

            builder.HasOne(o => o.Title)
            .WithMany()
            .IsRequired(false)
            .HasForeignKey("TitleCode");

            builder.Property(o => o.TitleOthers)
                .HasMaxLength(50);
        }
    }


}
