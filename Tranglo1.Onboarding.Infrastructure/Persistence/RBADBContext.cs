using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Infrastructure.Event;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Infrastructure.Persistence
{
    public class RBADBContext : BaseDbContext
    {
        public const string DEFAULT_SCHEMA = "dbo";
        public const string META_SCHEMA = "meta";
        public const string HISTORY_SCHEMA = "history";

        public RBADBContext(DbContextOptions<RBADBContext> options, IEventDispatcher dispatcher,
         IUnitOfWork unitOfWorkContext, IIdentityContext identityContext)
         : base(options, dispatcher, unitOfWorkContext, identityContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
        
        public DbSet<RBA> RBA { get;set; }
        public DbSet<RBAScreeningInput> RBAScreeningInputs { get; set; }
        public DbSet<EvaluationRules> EvaluationRules { get; set; }
        public DbSet<OverridingRules> OverridingRules { get; set; }
        public DbSet<EvaluationRulesParameter> EvaluationRulesParameters { get; set; }
        public DbSet<OverridingRulesParameter> OverridingRulesParameters { get; set; }
        public DbSet<RBARequisition> RBARequisitions { get; set; }
        public DbSet<RiskRanking> RiskRankings { get; set; }
        public DbSet<ComplianceSettingType> ComplianceSettings { get; set; }

        // Compliance
        public DbSet<ComplianceRequisitionType> ComplianceRequisitionTypes { get; set; }
        public DbSet<ComplianceSettingType> ComplianceSettingTypes { get; set; }

    }
}
