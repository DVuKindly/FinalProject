using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Data.Entity;
using System;

namespace Project.Data.Data
{
    public class ProjectDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Candidate> CandidateRecords { get; set; }
        public DbSet<SourcingHistory> SourcingHistories { get; set; }
        public DbSet<UpdateHistory> UpdateHistories { get; set; }
        public DbSet<RemarkHistory> RemarkHistories { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Event>()
     .HasOne(e => e.Creator)
     .WithMany(u => u.Events)
     .HasForeignKey(e => e.CreatedBy)
     .OnDelete(DeleteBehavior.SetNull);



            // Cấu hình mối quan hệ giữa Event và CandidateRecord
            builder.Entity<Event>()
                .HasMany(e => e.candidate)
                .WithOne(c => c.Event)
                .HasForeignKey(c => c.EventId);

            // Cấu hình mối quan hệ giữa CandidateRecord và SourcingHistory
            builder.Entity<SourcingHistory>()
                .HasOne(sh => sh.candidate)
                .WithMany(c => c.SourcingHistories)
                .HasForeignKey(sh => sh.CandidateID);

            // Cấu hình mối quan hệ giữa CandidateRecord và UpdateHistory
            builder.Entity<UpdateHistory>()
                .HasOne(uh => uh.candidate)
                .WithMany(c => c.UpdateHistories)
                .HasForeignKey(uh => uh.CandidateID);

            // Cấu hình mối quan hệ giữa CandidateRecord và RemarkHistory
            builder.Entity<RemarkHistory>()
                .HasOne(rh => rh.candidate)
                .WithMany(c => c.RemarkHistories)
                .HasForeignKey(rh => rh.CandidateID);

            // Cấu hình mối quan hệ giữa CandidateRecord và AuditTrail
            builder.Entity<AuditTrail>()
                .HasOne(at => at.candidate)
                .WithMany(c => c.AuditTrails)
                .HasForeignKey(at => at.CandidateID);

            // Cấu hình mối quan hệ User và Role thông qua UserRole
            builder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        }
    }
}
