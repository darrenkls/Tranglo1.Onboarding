using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class ScreeningDetailsEntityTypeConfiguration : BaseEntityTypeConfiguration<ScreeningDetail>
    {
        protected override void Configure(EntityTypeBuilder<ScreeningDetail> builder)
        {
            builder.ToTable("ScreeningDetails", ScreeningDBContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(ScreeningDBContext.HISTORY_SCHEMA);
                config.HistoryTable("ScreeningDetails");
            });

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("ScreeningDetailsCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(o => o.Screening)
                .WithMany()
                .HasForeignKey("ScreeningCode");

            builder.HasOne(o => o.ScreeningListSource)
                .WithMany()
                .HasForeignKey("ScreeningListSourceCode");
        }
    }
}
