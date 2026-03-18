using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.ApprovalWorkflowEngine.Models;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.RequisitionEntityTypeConfiguration
{
    class RequisitionEditHistoryEntityTypeConfiguration : BaseEntityTypeConfiguration<RequisitionEditHistory>
    {
        protected override void Configure(EntityTypeBuilder<RequisitionEditHistory> builder)
        {
            builder.ToTable("RequisitionEditHistory", KYCPartnerStatusRequisitionDbContext.DEFAULT_SCHEMA);

            builder.Property(kyc => kyc.RequisitionCode)
                   .IsRequired();

            builder.HasNoKey();
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