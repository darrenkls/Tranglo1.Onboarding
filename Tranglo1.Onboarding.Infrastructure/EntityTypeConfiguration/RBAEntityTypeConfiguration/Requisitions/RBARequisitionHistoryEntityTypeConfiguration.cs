using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.ApprovalWorkflowEngine.Models;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.RBAEntityTypeConfiguration.Requisitions
{
    class RBARequisitionHistoryEntityTypeConfiguration : BaseEntityTypeConfiguration<ApprovalHistory<RBARequisition>>
    {
        protected override void Configure(EntityTypeBuilder<ApprovalHistory<RBARequisition>> builder)
        {
            builder.ToTable("RBARequisitionHistories", RBARequisitionDBContext.DEFAULT_SCHEMA);

            builder.Property(a => a.Id)
                   .HasColumnName("RBARequisitionHistoryCode");
            builder.HasKey(a => a.Id);
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
