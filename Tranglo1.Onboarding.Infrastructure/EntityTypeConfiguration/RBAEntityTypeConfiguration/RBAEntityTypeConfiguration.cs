using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.RBAEntityTypeConfiguration
{
    class RBAEntityTypeConfiguration : BaseEntityTypeConfiguration<RBA>
    {
      
        protected override void Configure(EntityTypeBuilder<RBA> builder)
        {
            builder.ToTable("RBA", RBADBContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(RBADBContext.HISTORY_SCHEMA);
                config.HistoryTable("RBA");
            });

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("RBACode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(r => r.Solution)
            .WithMany()
            .HasForeignKey("SolutionCode");

            builder.HasOne(r => r.PartnerType)
                .WithMany()
                .HasForeignKey("PartnerTypeCode");

            builder.HasOne(r => r.ScreeningEntityType)
                .WithMany()
                .HasForeignKey("ScreeningEntityTypeCode");

        }
    }
}
