using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class DocumentCategoryGroupEntityTypeConfiguration : BaseEntityTypeConfiguration<DocumentCategoryGroup>
    {
        protected override void Configure(EntityTypeBuilder<DocumentCategoryGroup> builder)
        {
            builder.ToTable("DocumentCategoryGroups", BusinessProfileDbContext.DEFAULT_SCHEMA);

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("DocumentCategoryGroupCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(e => e.DocumentCategoryGroupDescription)
                    .HasMaxLength(150);

            builder.HasOne(o => o.Solution)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("SolutionCode");

            builder.Property(a => a.CustomerTypeGroupCode);
                

        }
    }
}
