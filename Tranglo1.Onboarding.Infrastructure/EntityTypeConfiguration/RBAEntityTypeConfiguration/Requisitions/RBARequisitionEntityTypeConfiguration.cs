using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.RBAEntityTypeConfiguration.Requisitions
{
    class RBARequisitionEntityTypeConfiguration : BaseEntityTypeConfiguration<RBARequisition>
    {
        protected override void Configure(EntityTypeBuilder<RBARequisition> builder)
        {
            builder.ToTable("RBARequisitions",
                RBADBContext.DEFAULT_SCHEMA, a =>
                {

                });

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("RBARequisitionsCode");

      
            builder.HasOne(o => o.Solution)
         .WithMany()
         .HasForeignKey("SolutionCode");

                builder.HasOne(o => o.ComplianceRequisitionType)
                .WithMany()
                .IsRequired()
                .HasForeignKey("ComplianceRequisitionTypeCode");


        }
        protected override bool HasCreationInfo()
        {
            return false;
        }

        protected override bool HasLastModificationInfo()
        {
            return false;
        }
    }
}
