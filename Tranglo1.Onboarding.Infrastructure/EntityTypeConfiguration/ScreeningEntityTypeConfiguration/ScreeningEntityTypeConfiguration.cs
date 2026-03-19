using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class ScreeningEntityTypeConfiguration : BaseEntityTypeConfiguration<Screening>
    {
        protected override void Configure(EntityTypeBuilder<Screening> builder)
        {
            builder.ToTable("Screening", ScreeningDBContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(ScreeningDBContext.HISTORY_SCHEMA);
                config.HistoryTable("Screening");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("ScreeningCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(o => o.ScreeningInput)
                .WithMany()
                .HasForeignKey("ScreeningInputCode");

            builder.HasMany(o => o.ScreeningDetails)
                .WithOne(d => d.Screening)
                .HasForeignKey("ScreeningCode");
        }
    }
}
