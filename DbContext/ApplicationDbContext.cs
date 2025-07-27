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
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<UserAccountHistory> UserAccountHistories { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<UserAccountAction> UserAccountActions { get; set; }
        public DbSet<ProjectFlagAction> ProjectFlagActions { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<AdvertisementPackageType> AdvertisementPackageTypes { get; set; }
        public DbSet<AdvertisementStatus> AdvertisementStatuses { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; } //rút tiền


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

        public DbSet<ProjectSubmission> ProjectSubmissions { get; set; }
        public DbSet<ProjectSubmissionAttachment> ProjectSubmissionAttachments { get; set; }

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
                
            // Self-referencing relationships for user verification/flagging
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.VerifiedBy)
                .WithMany()
                .HasForeignKey(u => u.VerifiedByID)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.FlaggedBy)
                .WithMany()
                .HasForeignKey(u => u.FlaggedByID)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Project flagging relationship
            modelBuilder.Entity<Project>()
                .HasOne(p => p.FlaggedBy)
                .WithMany()
                .HasForeignKey(p => p.FlaggedByID)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships

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

            // Conversation relationships
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Project)
                .WithMany(p => p.Conversations)
                .HasForeignKey(c => c.ProjectID)
                .OnDelete(DeleteBehavior.Restrict);

            // Message relationships
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
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationID)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany(u => u.SentNotifications)
                .HasForeignKey(n => n.SenderID)
                .OnDelete(DeleteBehavior.Restrict);

            // UserNotification
            modelBuilder.Entity<UserNotification>()
                .HasOne(un => un.User)
                .WithMany(u => u.UserNotifications)
                .HasForeignKey(un => un.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserNotification>()
                .HasOne(un => un.Notification)
                .WithMany(n => n.UserNotifications)
                .HasForeignKey(un => un.NotificationID)
                .OnDelete(DeleteBehavior.Restrict);

            // Create a unique index for UserID and NotificationID combination
            modelBuilder.Entity<UserNotification>()
                .HasIndex(un => new { un.UserID, un.NotificationID })
                .IsUnique();

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

            // ProjectSubmission relationships
            modelBuilder.Entity<ProjectSubmission>()
                .HasOne(ps => ps.Application)
                .WithMany()  // no StudentApplication.Submissions collection
                .HasForeignKey(ps => ps.ApplicationID)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectSubmissionAttachment relationships
            modelBuilder.Entity<ProjectSubmissionAttachment>()
                .HasOne(psa => psa.Submission)
                .WithMany(ps => ps.Attachments)
                .HasForeignKey(psa => psa.SubmissionID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectSubmissionAttachment>()
                .HasOne(psa => psa.UploadedByUser)
                .WithMany()  // no User.SubmissionAttachments collection
                .HasForeignKey(psa => psa.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);
                
            // UserAccountActions relationships - prevent cascade delete cycles
            modelBuilder.Entity<UserAccountAction>()
                .HasOne(uaa => uaa.User)
                .WithMany()
                .HasForeignKey(uaa => uaa.UserID)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<UserAccountAction>()
                .HasOne(uaa => uaa.ActionBy)
                .WithMany()
                .HasForeignKey(uaa => uaa.ActionByID)
                .OnDelete(DeleteBehavior.Restrict);
                
            // ProjectFlagActions relationships - prevent cascade delete cycles
            modelBuilder.Entity<ProjectFlagAction>()
                .HasOne(pfa => pfa.Project)
                .WithMany()
                .HasForeignKey(pfa => pfa.ProjectID)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<ProjectFlagAction>()
                .HasOne(pfa => pfa.ActionBy)
                .WithMany()
                .HasForeignKey(pfa => pfa.ActionByID)
                .OnDelete(DeleteBehavior.Restrict);

            // Advertisement relationships
            modelBuilder.Entity<Advertisement>()
                .HasOne(a => a.Business)
                .WithMany()
                .HasForeignKey(a => a.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Advertisement>()
                .HasOne(a => a.ApprovedBy)
                .WithMany()
                .HasForeignKey(a => a.ApprovedById)
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
            modelBuilder.Entity<ProjectSubmission>().HasQueryFilter(ps => ps.IsActive);
            modelBuilder.Entity<ProjectSubmissionAttachment>().HasQueryFilter(psa => psa.IsActive);
            modelBuilder.Entity<Conversation>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<UserAccountAction>().HasQueryFilter(uaa => uaa.IsActive);
            modelBuilder.Entity<ProjectFlagAction>().HasQueryFilter(pfa => pfa.IsActive);
        }
    }
}