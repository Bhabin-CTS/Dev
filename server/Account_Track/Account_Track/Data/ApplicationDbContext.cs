using Account_Track.DTOs.AccountDto;
using Account_Track.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Account_Track.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<t_User> Users { get; set; }
        public DbSet<t_Account> Accounts { get; set; }
        public DbSet<t_Transaction> Transactions { get; set; }
        public DbSet<t_Approval> Approvals { get; set; }
        public DbSet<t_Notification> Notifications { get; set; }
        public DbSet<t_Report> Reports { get; set; }
        public DbSet<t_LoginLog> LoginLogs { get; set; }
        public DbSet<t_AuditLog> AuditLogs { get; set; }
        public DbSet<t_Branch> Branches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountListItemDto>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("vw_AccountList");
            });

            // ENUM CONVERSIONS (INT)
            modelBuilder.Entity<t_User>().Property(p => p.Role).HasConversion<int>();
            modelBuilder.Entity<t_User>().Property(p => p.Status).HasConversion<int>();

            modelBuilder.Entity<t_Account>().Property(p => p.AccountType).HasConversion<int>();
            modelBuilder.Entity<t_Account>().Property(p => p.Status).HasConversion<int>();

            modelBuilder.Entity<t_Transaction>().Property(p => p.Type).HasConversion<int>();
            modelBuilder.Entity<t_Transaction>().Property(p => p.Status).HasConversion<int>();

            modelBuilder.Entity<t_Approval>().Property(p => p.Decision).HasConversion<int>();

            modelBuilder.Entity<t_Notification>().Property(p => p.Status).HasConversion<int>();
            modelBuilder.Entity<t_Notification>().Property(p => p.Type).HasConversion<int>();

            //Relation Rule 
            // User -> Branch
            modelBuilder.Entity<t_User>()
                .HasOne(u => u.Branches)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Account -> Branch
            modelBuilder.Entity<t_Account>()
                .HasOne(a => a.Branch)
                .WithMany(b => b.Accounts)
                .HasForeignKey(a => a.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Account -> CreatedByUser
            modelBuilder.Entity<t_Account>()
                .HasOne(a => a.CreatedByUser)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction -> FromAccount
            modelBuilder.Entity<t_Transaction>()
                .HasOne(t => t.FromAccount)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction -> BranchId
            modelBuilder.Entity<t_Transaction>()
                .HasOne(t => t.Branch)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction -> ToAccount (Optional)
            modelBuilder.Entity<t_Transaction>()
                .HasOne(t => t.ToAccount)
                .WithMany()
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            //Transaction -> CreatedByUser
            modelBuilder.Entity<t_Transaction>()
                .HasOne(t => t.CreatedByUser)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Approval -> Transaction
            modelBuilder.Entity<t_Approval>()
                .HasOne(a => a.Transaction)
                .WithMany(t => t.Approvals)
                .HasForeignKey(a => a.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Approval -> Reviewer(User)
            modelBuilder.Entity<t_Approval>()
                .HasOne(a => a.Reviewer)
                .WithMany(u => u.Approvals)
                .HasForeignKey(a => a.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification -> User
            modelBuilder.Entity<t_Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // LoginLog -> User
            modelBuilder.Entity<t_LoginLog>()
                .HasOne(l => l.User)
                .WithMany(u => u.LoginLogs)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AuditLog -> User
            modelBuilder.Entity<t_AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // AuditLog -> LoginLog
            modelBuilder.Entity<t_AuditLog>()
                .HasOne(a => a.LoginLog)
                .WithMany(l => l.AuditLogs)
                .HasForeignKey(a => a.LoginId)
                .OnDelete(DeleteBehavior.NoAction);

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            modelBuilder.Entity<t_Report>()
                .Property(r => r.Scope)
                .HasConversion(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<Scope>(v, options)!);

            modelBuilder.Entity<t_Report>()
                .Property(r => r.Metrics)
                .HasConversion(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<Metrics>(v, options)!);

        }
    }
}
