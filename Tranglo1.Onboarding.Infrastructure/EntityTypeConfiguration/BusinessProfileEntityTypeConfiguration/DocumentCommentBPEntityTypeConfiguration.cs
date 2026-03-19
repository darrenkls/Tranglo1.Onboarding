using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class DocumentCommentBPEntityTypeConfiguration : BaseEntityTypeConfiguration<DocumentCommentBP>
    {
        protected override void Configure(EntityTypeBuilder<DocumentCommentBP> builder)
        {
            builder.ToTable("DocumentCommentBPs", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("DocumentCommentBPs");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("DocumentCommentBPCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(o => o.DocumentCategoryBP)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("DocumentCategoryBPCode");

            builder.Property(e => e.Comment)
               .HasColumnType("nvarchar(MAX)");

        }
    }
}
