using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    
    class LegalEntityEntityTypeConfiguration : BaseEntityTypeConfiguration<LegalEntity>
    {
        protected override void Configure(EntityTypeBuilder<LegalEntity> builder)
        {
            builder.ToTable("LegalEntities", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("LegalEntities");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("LegalEntityCode");
            builder.HasKey(kyc => kyc.Id);


            builder.Property(e => e.CompanyName)
                    .HasMaxLength(150);

            builder.Property(e => e.CompanyRegNo)
                    .HasMaxLength(150);

            builder.Property(e => e.NameOfSharesAboveTenPercent)
                    .HasMaxLength(150);

            builder.Property(e => e.EffectiveShareholding)
                    .HasMaxLength(150);

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");

        }
    }
    
}
