using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EMPS.Core.Entities;
using System.Text.Json;

namespace EMPS.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Payslip> Payslips { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Department Configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Designation Configuration
            modelBuilder.Entity<Designation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SalaryGrade).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BasicSalary).HasPrecision(18, 2);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(d => d.Department)
                    .WithMany(dp => dp.Designations)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Employee Configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.BasicSalary).HasPrecision(18, 2);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Designation)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DesignationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithOne(u => u.Employee)
                    .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Attendance Configuration
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(a => a.Employee)
                    .WithMany(e => e.Attendances)
                    .HasForeignKey(a => a.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // LeaveRequest Configuration
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(l => l.Employee)
                    .WithMany(e => e.LeaveRequests)
                    .HasForeignKey(l => l.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payroll Configuration
            modelBuilder.Entity<Payroll>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BasicSalary).HasPrecision(18, 2);
                entity.Property(e => e.Allowances).HasPrecision(18, 2);
                entity.Property(e => e.Bonuses).HasPrecision(18, 2);
                entity.Property(e => e.OvertimePay).HasPrecision(18, 2);
                entity.Property(e => e.TaxDeductions).HasPrecision(18, 2);
                entity.Property(e => e.OtherDeductions).HasPrecision(18, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(p => p.Employee)
                    .WithMany(e => e.Payrolls)
                    .HasForeignKey(p => p.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payslip Configuration
            modelBuilder.Entity<Payslip>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PayslipCode).IsRequired().HasMaxLength(50);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(ps => ps.Payroll)
                    .WithOne(p => p.Payslip)
                    .HasForeignKey<Payslip>(ps => ps.PayrollId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Notification Configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Message).IsRequired();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // AuditLog Configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
            });
        }

        public async Task<int> SaveChangesAsync(string userId = "System", CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges(userId);
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChanges(auditEntries);
            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Entity.GetType().Name,
                    UserId = userId
                };
                auditEntries.Add(auditEntry);

                // Handle BaseEntity auditing fields
                if (entry.Entity is BaseEntity baseEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        baseEntity.CreatedAt = DateTime.UtcNow;
                        baseEntity.CreatedBy = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        baseEntity.UpdatedAt = DateTime.UtcNow;
                        baseEntity.UpdatedBy = userId;
                    }
                }

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = "Create";
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = "Delete";
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.AuditType = "Update";
                                auditEntry.OldValues[propertyName] = property.OriginalValue!;
                                auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            }
                            break;
                    }
                }
            }

            foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
            {
                AuditLogs.Add(auditEntry.ToAudit());
            }

            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || !auditEntries.Any())
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue!;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue!;
                    }
                }

                AuditLogs.Add(auditEntry.ToAudit());
            }

            return base.SaveChangesAsync();
        }
    }

    internal class AuditEntry
    {
        public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            Entry = entry;
        }

        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
        public string? UserId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string AuditType { get; set; } = string.Empty;
        public Dictionary<string, object> KeyValues { get; } = new();
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public List<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> TemporaryProperties { get; } = new();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditLog ToAudit()
        {
            var audit = new AuditLog
            {
                UserId = UserId,
                Action = AuditType,
                EntityName = TableName,
                Timestamp = DateTime.UtcNow,
                EntityId = JsonSerializer.Serialize(KeyValues),
                OldValues = OldValues.Any() ? JsonSerializer.Serialize(OldValues) : null,
                NewValues = NewValues.Any() ? JsonSerializer.Serialize(NewValues) : null
            };
            return audit;
        }
    }
}
