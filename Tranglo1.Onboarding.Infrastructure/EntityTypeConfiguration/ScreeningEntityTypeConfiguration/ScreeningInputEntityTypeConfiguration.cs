using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class ScreeningInputEntityTypeConfiguration : BaseEntityTypeConfiguration<ScreeningInput>
    {
        protected override void Configure(EntityTypeBuilder<ScreeningInput> builder)
        {
            builder.ToTable("ScreeningInput", ScreeningDBContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(ScreeningDBContext.HISTORY_SCHEMA);
                config.HistoryTable("ScreeningInput");
            });

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("ScreeningInputCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasMany(o => o.Screenings)
                .WithOne(s => s.ScreeningInput)
                .HasForeignKey(s => s.ScreeningInputCode);

            builder.HasOne(o => o.OwnershipStrucureType)
                .WithMany()
                .HasForeignKey(o => o.OwnershipStrucureTypeId);

            builder.HasOne(o => o.ScreeningEntityType)
                .WithMany()
                .HasForeignKey(o => o.ScreeningEntityTypeId);

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .HasForeignKey(o => o.BusinessProfileId);

            builder.HasOne(o => o.NationalityMeta)
                .WithMany()
                .HasForeignKey(o => o.NationalityCountryCode);

            builder.HasOne(o => o.WatchlistStatus)
                .WithMany()
                .HasForeignKey(o => o.WatchlistStatusId);

            builder.HasOne(o => o.ScreeningInputEnforcementAction)
                .WithMany()
                .HasForeignKey(o => o.ScreeningInputEnforcementActionCode);

            builder.Property(x => x.DateOfBirth)
                .HasColumnType("date");

            builder.Ignore(c => c.DomainEvents);
        }
    }
}
