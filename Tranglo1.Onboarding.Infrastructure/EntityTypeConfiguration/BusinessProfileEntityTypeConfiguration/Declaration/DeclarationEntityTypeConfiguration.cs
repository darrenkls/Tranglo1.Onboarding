using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class DeclarationEntityTypeConfiguration : BaseEntityTypeConfiguration<Declaration>
    {
        protected override void Configure(EntityTypeBuilder<Declaration> builder)
        {
            builder.ToTable("Declarations", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("Declarations");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("DeclarationCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");

            builder.Property(o => o.Designation)
             .HasMaxLength(150);

            builder.Property(o => o.SigneeName)
            .HasMaxLength(150);

            builder.Property(o => o.IsShowOldUI)
                .IsRequired();
        }
    }
}
