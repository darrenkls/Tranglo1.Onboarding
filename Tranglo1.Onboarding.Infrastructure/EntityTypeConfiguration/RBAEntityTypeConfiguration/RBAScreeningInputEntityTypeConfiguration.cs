using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.RBAEntityTypeConfiguration
{
    class RBAScreeningInputEntityTypeConfiguration : BaseEntityTypeConfiguration<RBAScreeningInput>
    {
      
        protected override void Configure(EntityTypeBuilder<RBAScreeningInput> builder)
        {
            builder.ToTable("RBAScreeningInputs", RBADBContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(RBADBContext.HISTORY_SCHEMA);
                config.HistoryTable("RBAScreeningInputs");
            });

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("RBAScreeningInputCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Ignore(c => c.DomainEvents);

        }
    }
}
