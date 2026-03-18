using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class ParentHoldingCompanyEntityTypeConfiguration : BaseEntityTypeConfiguration<ParentHoldingCompany>
    {
        protected override void Configure(EntityTypeBuilder<ParentHoldingCompany> builder)
        {
            builder.ToTable("ParentHoldingCompanies", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("ParentHoldingCompanies");
            });

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("ParentHoldingCompanyCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(e => e.NameOfListedParentHoldingCompany)
                    .HasMaxLength(150);

            builder.HasOne(o => o.Country)
                .WithMany()
                .HasForeignKey("CountryCode")
                .IsRequired(false);

            builder.Property(e => e.NameOfStockExchange)
                    .HasMaxLength(150);

            builder.Property(e => e.StockCode)
                    .HasMaxLength(150);

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");

        }
    }
}
