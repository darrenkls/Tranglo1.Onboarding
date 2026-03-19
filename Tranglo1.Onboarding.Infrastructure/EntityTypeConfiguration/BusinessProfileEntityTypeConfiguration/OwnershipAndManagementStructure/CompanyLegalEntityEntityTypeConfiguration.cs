using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    
    class CompanyLegalEntityEntityTypeConfiguration : BaseEntityTypeConfiguration<CompanyLegalEntity>
    {
        protected override void Configure(EntityTypeBuilder<CompanyLegalEntity> builder)
        {
            builder.ToTable("CompanyLegalEntities", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("CompanyLegalEntities");
            });


            builder.HasOne(o => o.Country)
               .WithMany()
               .HasForeignKey("CountryCode")
               .IsRequired(false);
        }
    }
    
}
