using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine.EntityFramework.DBContexts;
using Tranglo1.ApprovalWorkflowEngine.Models;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Infrastructure.Event;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Infrastructure.Persistence
{
    public class RBARequisitionDBContext : BaseDbContext, IApprovalWorkflowManagerDbContext<RBARequisition>
    {
        public RBARequisitionDBContext(
            DbContextOptions<RBARequisitionDBContext> options, IEventDispatcher dispatcher,
            IUnitOfWork unitOfWorkContext, IIdentityContext identityContext)
            : base(options, dispatcher, unitOfWorkContext, identityContext)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }

        public const string DEFAULT_SCHEMA = "dbo";
        public const string META_SCHEMA = "meta";
        public const string HISTORY_SCHEMA = "history";

        public DbSet<RBARequisition> RequisitionDetails { get; set; }
        public DbSet<ApprovalHistory<RBARequisition>> ApprovalHistories { get; set; }
        public DbSet<RequisitionEditHistory> RequisitionEditHistories { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }
    }
}
