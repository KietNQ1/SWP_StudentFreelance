// File: DbContext/ApplicationDbContext.cs

using Microsoft.EntityFrameworkCore;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace StudentFreelance.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet declarations
      
      
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<StudentSkill> StudentSkills { get; set; }
        public DbSet<StudentApplication> StudentApplications { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectSkillRequired> ProjectSkillsRequired { get; set; }
        public DbSet<ProjectAttachment> ProjectAttachments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserAccountHistory> UserAccountHistories { get; set; }

        // Enum DbSets
        public DbSet<ProjectStatus> ProjectStatuses { get; set; }
        public DbSet<ProjectType> ProjectTypes { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<TransactionStatus> TransactionStatuses { get; set; }
        public DbSet<ReportType> ReportTypes { get; set; }
        public DbSet<ReportStatus> ReportStatuses { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<AccountStatus> AccountStatuses { get; set; }
        public DbSet<ProficiencyLevel> ProficiencyLevels { get; set; }
        public DbSet<ImportanceLevel> ImportanceLevels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique indexes
            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.CategoryName, c.CategoryType })
                .IsUnique();
            modelBuilder.Entity<StudentSkill>()
                .HasIndex(ss => new { ss.UserID, ss.SkillID })
                .IsUnique();
            modelBuilder.Entity<ProjectSkillRequired>()
                .HasIndex(ps => new { ps.ProjectID, ps.SkillID })
                .IsUnique();
            modelBuilder.Entity<StudentApplication>()
                .HasIndex(sa => new { sa.ProjectID, sa.UserID })
                .IsUnique();
            modelBuilder.Entity<Rating>()
                .HasIndex(r => new { r.ProjectID, r.ReviewerID, r.RevieweeID })
                .IsUnique();

            // Relationships

            // Provinces → Districts
            modelBuilder.Entity<District>()
                .HasOne(d => d.Province)
                .WithMany(p => p.Districts)
                .HasForeignKey(d => d.ProvinceID)
                .OnDelete(DeleteBehavior.Restrict);

            // Districts → Wards
            modelBuilder.Entity<Ward>()
                .HasOne(w => w.District)
                .WithMany(d => d.Wards)
                .HasForeignKey(w => w.DistrictID)
                .OnDelete(DeleteBehavior.Restrict);

            // Addresses → Province/District/Ward
            modelBuilder.Entity<Address>()
                .HasOne(a => a.Province)
                .WithMany(p => p.Addresses)
                .HasForeignKey(a => a.ProvinceID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Address>()
                .HasOne(a => a.District)
                .WithMany(d => d.Addresses)
                .HasForeignKey(a => a.DistrictID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Address>()
                .HasOne(a => a.Ward)
                .WithMany(w => w.Addresses)
                .HasForeignKey(a => a.WardID)
                .OnDelete(DeleteBehavior.Restrict);

            // Users → UserRole
          

            // Users → Address
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Address)
                .WithMany()   // no Address.Users collection
                .HasForeignKey(u => u.AddressID)
                .OnDelete(DeleteBehavior.Restrict);

            // Category hierarchy
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Project → Business(User)
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Business)
                .WithMany()   // no User.Projects collection
                .HasForeignKey(p => p.BusinessID)
                .OnDelete(DeleteBehavior.Restrict);

            // Project → Category
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Project → Address
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Address)
                .WithMany()   // no Address.Projects collection
                .HasForeignKey(p => p.AddressID)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectSkillRequired
            modelBuilder.Entity<ProjectSkillRequired>()
                .HasOne(ps => ps.Project)
                .WithMany(p => p.ProjectSkillsRequired)
                .HasForeignKey(ps => ps.ProjectID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProjectSkillRequired>()
                .HasOne(ps => ps.Skill)
                .WithMany(s => s.ProjectSkillsRequired)
                .HasForeignKey(ps => ps.SkillID)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectAttachment
            modelBuilder.Entity<ProjectAttachment>()
                .HasOne(pa => pa.Project)
                .WithMany(p => p.ProjectAttachments)
                .HasForeignKey(pa => pa.ProjectID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProjectAttachment>()
                .HasOne(pa => pa.UploadedByUser)
                .WithMany()   // no User.ProjectAttachments collection
                .HasForeignKey(pa => pa.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentApplication
            modelBuilder.Entity<StudentApplication>()
                .HasOne(sa => sa.Project)
                .WithMany(p => p.StudentApplications)
                .HasForeignKey(sa => sa.ProjectID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StudentApplication>()
                .HasOne(sa => sa.User)
                .WithMany()   // no User.StudentApplications collection
                .HasForeignKey(sa => sa.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentSkill
            modelBuilder.Entity<StudentSkill>()
                .HasOne(ss => ss.User)
                .WithMany()   // no User.StudentSkills collection
                .HasForeignKey(ss => ss.UserID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StudentSkill>()
                .HasOne(ss => ss.Skill)
                .WithMany(s => s.StudentSkills)
                .HasForeignKey(ss => ss.SkillID)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()   // no User.Transactions collection
                .HasForeignKey(t => t.UserID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Transactions)
                .HasForeignKey(t => t.ProjectID)
                .OnDelete(DeleteBehavior.Restrict);

            // Rating
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Project)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.ProjectID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Reviewer)
                .WithMany()   // no User.RatingsGiven collection
                .HasForeignKey(r => r.ReviewerID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Reviewee)
                .WithMany()   // no User.RatingsReceived collection
                .HasForeignKey(r => r.RevieweeID)
                .OnDelete(DeleteBehavior.Restrict);

            // Report
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()   // no User.ReportsMade collection
                .HasForeignKey(r => r.ReporterID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReportedUser)
                .WithMany()   // no User.ReportsReceived collection
                .HasForeignKey(r => r.ReportedUserID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Project)
                .WithMany(p => p.Reports)
                .HasForeignKey(r => r.ProjectID)
                .OnDelete(DeleteBehavior.Restrict);

            // Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()   // no User.MessagesSent collection
                .HasForeignKey(m => m.SenderID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()   // no User.MessagesReceived collection
                .HasForeignKey(m => m.ReceiverID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Project)
                .WithMany(p => p.Messages)
                .HasForeignKey(m => m.ProjectID)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()   // no User.Notifications collection
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // UserAccountHistory
            modelBuilder.Entity<UserAccountHistory>()
                .HasOne(h => h.User)
                .WithMany()   // no User.UserAccountHistories collection
                .HasForeignKey(h => h.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Enum configurations
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Status)
                .WithMany(s => s.Projects)
                .HasForeignKey(p => p.StatusID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Type)
                .WithMany(t => t.Projects)
                .HasForeignKey(p => p.TypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Type)
                .WithMany(tt => tt.Transactions)
                .HasForeignKey(t => t.TypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Status)
                .WithMany(ts => ts.Transactions)
                .HasForeignKey(t => t.StatusID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Type)
                .WithMany(rt => rt.Reports)
                .HasForeignKey(r => r.TypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Status)
                .WithMany(rs => rs.Reports)
                .HasForeignKey(r => r.StatusID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Type)
                .WithMany(nt => nt.Notifications)
                .HasForeignKey(n => n.TypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Status)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.StatusID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentSkill>()
                .HasOne(ss => ss.ProficiencyLevel)
                .WithMany(pl => pl.StudentSkills)
                .HasForeignKey(ss => ss.ProficiencyLevelID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectSkillRequired>()
                .HasOne(psr => psr.ImportanceLevel)
                .WithMany(il => il.ProjectSkillsRequired)
                .HasForeignKey(psr => psr.ImportanceLevelID)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filters for soft-delete
            
            modelBuilder.Entity<Address>().HasQueryFilter(a => a.IsActive);
            modelBuilder.Entity<Category>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<Skill>().HasQueryFilter(s => s.IsActive);
            modelBuilder.Entity<StudentSkill>().HasQueryFilter(ss => ss.IsActive);
            modelBuilder.Entity<ProjectSkillRequired>().HasQueryFilter(ps => ps.IsActive);
            modelBuilder.Entity<ProjectAttachment>().HasQueryFilter(pa => pa.IsActive);
            modelBuilder.Entity<Transaction>().HasQueryFilter(t => t.IsActive);
            modelBuilder.Entity<Rating>().HasQueryFilter(r => r.IsActive);
            modelBuilder.Entity<Report>().HasQueryFilter(r => r.IsActive);
            modelBuilder.Entity<Message>().HasQueryFilter(m => m.IsActive);
            modelBuilder.Entity<Notification>().HasQueryFilter(n => n.IsActive);
            modelBuilder.Entity<UserAccountHistory>().HasQueryFilter(h => h.IsActive);
            modelBuilder.Entity<ProjectStatus>().HasQueryFilter(e => e.IsActive);
            modelBuilder.Entity<ProjectType>().HasQueryFilter(e => e.IsActive);
            modelBuilder.Entity<TransactionType>().HasQueryFilter(e => e.IsActive);
            modelBuilder.Entity<TransactionStatus>().HasQueryFilter(e => e.IsActive);
            modelBuilder.Entity<ReportType>().HasQueryFilter(e => e.IsActive);
            modelBuilder.Entity<ReportStatus>().HasQueryFilter(e => e.IsActive);
            modelBuilder.Entity<NotificationType>().HasQueryFilter(e => e.IsActive);
            modelBuilder.Entity<AccountStatus>().HasQueryFilter(e => e.IsActive);
            modelBuilder.Entity<ProficiencyLevel>().HasQueryFilter(e => e.IsActive);
            modelBuilder.Entity<ImportanceLevel>().HasQueryFilter(e => e.IsActive);
        }
    }
}
