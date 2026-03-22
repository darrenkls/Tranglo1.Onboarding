using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using Tranglo1.Onboarding.Application.Security;

namespace Tranglo1.Onboarding.Application.Infrastructure.Persistance
{
    class AuditLogDbContext : DbContext
    {
        public DbSet<AuditLog> AuditLogs { get; set; }

        public AuditLogDbContext(DbContextOptions<AuditLogDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");

                entity.Property<long>("LogId")
                    .IsRequired(true)
                    .UseHiLo("AuditLogId");

                entity.Property(e => e.Username)
                    .IsRequired(false);

                entity.Property(e => e.ActionDescription)
                    .IsRequired(true)
                    .IsUnicode(true);

                entity.Property(e => e.CorrelationId)
                    .HasMaxLength(36);

                entity.Property(e => e.EventDate)
                    .IsRequired(true)
                    .HasConversion(d => d, d => DateTime.SpecifyKind(d, DateTimeKind.Utc));

                entity
                    .HasKey("LogId").IsClustered(true)
                    .HasName("pk_AuditLogs");

                entity.Property(e => e.ModuleName)
                    .IsRequired(false)
                    .IsUnicode(true)
                    .HasMaxLength(150);

                entity.Property(e => e.ClientAddress)
                    .IsRequired(false)
                    .HasColumnType("varchar(39)")
                    .HasConversion(d => d.ToString(), d => IPAddress.Parse(d));
            });

            modelBuilder.HasSequence("AuditLogId")
                .HasMax(long.MaxValue)
                .HasMin(1)
                .IncrementsBy(20)
                .IsCyclic(false);
        }
    }
}
