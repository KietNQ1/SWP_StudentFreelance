using Microsoft.AspNetCore.Identity;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace StudentFreelance.Data
{
    public static class DbSeeder
    {
        public static void SeedEnums(ApplicationDbContext context)
        {
            if (!context.ProjectStatuses.Any())
            {
                context.ProjectStatuses.AddRange(
                    new ProjectStatus { StatusName = "Đang tuyển", IsActive = true },
                    new ProjectStatus { StatusName = "Đã đóng", IsActive = true },
                    new ProjectStatus { StatusName = "Đã hoàn thành", IsActive = true },
                    new ProjectStatus { StatusName = "Đã hủy", IsActive = true }
                );
            }

            if (!context.ProjectTypes.Any())
            {
                context.ProjectTypes.AddRange(
                    new ProjectType { TypeName = "Toàn thời gian", IsActive = true },
                    new ProjectType { TypeName = "Bán thời gian", IsActive = true },
                    new ProjectType { TypeName = "Theo dự án", IsActive = true }
                );
            }

            if (!context.TransactionTypes.Any())
            {
                context.TransactionTypes.AddRange(
                    new TransactionType { TypeName = "Nạp tiền", IsActive = true },
                    new TransactionType { TypeName = "Rút tiền", IsActive = true },
                    new TransactionType { TypeName = "Thanh toán", IsActive = true },
                    new TransactionType { TypeName = "Hoàn tiền", IsActive = true }
                );
            }

            if (!context.TransactionStatuses.Any())
            {
                context.TransactionStatuses.AddRange(
                    new TransactionStatus { StatusName = "Đang xử lý", IsActive = true },
                    new TransactionStatus { StatusName = "Thành công", IsActive = true },
                    new TransactionStatus { StatusName = "Thất bại", IsActive = true },
                    new TransactionStatus { StatusName = "Đã hủy", IsActive = true }
                );
            }

            if (!context.ReportTypes.Any())
            {
                context.ReportTypes.AddRange(
                    new ReportType { TypeName = "Spam", IsActive = true },
                    new ReportType { TypeName = "Lừa đảo", IsActive = true },
                    new ReportType { TypeName = "Nội dung không phù hợp", IsActive = true },
                    new ReportType { TypeName = "Khác", IsActive = true }
                );
            }

            if (!context.ReportStatuses.Any())
            {
                context.ReportStatuses.AddRange(
                    new ReportStatus { StatusName = "Đang xử lý", IsActive = true },
                    new ReportStatus { StatusName = "Đã xử lý", IsActive = true },
                    new ReportStatus { StatusName = "Đã hủy", IsActive = true }
                );
            }

            if (!context.NotificationTypes.Any())
            {
                context.NotificationTypes.AddRange(
                    new NotificationType { TypeName = "Hệ thống", IsActive = true },
                    new NotificationType { TypeName = "Dự án", IsActive = true },
                    new NotificationType { TypeName = "Tin nhắn", IsActive = true },
                    new NotificationType { TypeName = "Giao dịch", IsActive = true }
                );
            }

            if (!context.AccountStatuses.Any())
            {
                context.AccountStatuses.AddRange(
                    new AccountStatus { StatusName = "Hoạt động", IsActive = true },
                    new AccountStatus { StatusName = "Tạm khóa", IsActive = true },
                    new AccountStatus { StatusName = "Đã khóa", IsActive = true }
                );
            }

            if (!context.ProficiencyLevels.Any())
            {
                context.ProficiencyLevels.AddRange(
                    new ProficiencyLevel { LevelName = "Mới bắt đầu", IsActive = true },
                    new ProficiencyLevel { LevelName = "Trung cấp", IsActive = true },
                    new ProficiencyLevel { LevelName = "Thành thạo", IsActive = true },
                    new ProficiencyLevel { LevelName = "Chuyên gia", IsActive = true }
                );
            }

            if (!context.ImportanceLevels.Any())
            {
                context.ImportanceLevels.AddRange(
                    new ImportanceLevel { LevelName = "Bắt buộc", IsActive = true },
                    new ImportanceLevel { LevelName = "Quan trọng", IsActive = true },
                    new ImportanceLevel { LevelName = "Ưu tiên", IsActive = true }
                );
            }

            context.SaveChanges();
        }

        public static async Task SeedSampleDataAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            // 1. Roles
            string[] roleNames = { "Admin", "Moderator", "Business", "Student" };
            foreach (var role in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // 2. AccountStatus
            var activeStatus = context.AccountStatuses.FirstOrDefault(s => s.StatusName == "Hoạt động");
            if (activeStatus == null)
            {
                activeStatus = new AccountStatus { StatusName = "Hoạt động", IsActive = true };
                context.AccountStatuses.Add(activeStatus);
                context.SaveChanges();
            }

            // 3. Default Users
            var usersToSeed = new List<(string Email, string Password, string Role, string FullName)>
            {
                ("admin@example.com", "Admin@123", "Admin", "Admin"),
                ("moderator@example.com", "Moderator@123", "Moderator", "Moderator"),
                ("business@example.com", "Business@123", "Business", "Công ty TNHH ABC"),
                ("student@example.com", "Student@123", "Student", "Nguyễn Văn A")
            };

            foreach (var (email, password, role, fullName) in usersToSeed)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = fullName,
                        PhoneNumber = "0123456789",
                        University = "FPT University",
                        Major = "CNTT",
                        CompanyName = "StudentFreelance",
                        Industry = "Công nghệ",
                        WalletBalance = 0,
                        VipStatus = role == "Admin" || role == "Moderator",
                        ProfileStatus = true,
                        AverageRating = 5,
                        TotalProjects = 0,
                        TotalProjectsPosted = 0,
                        ProfilePicturePath = "default-avatar.png",
                        Avatar = "default-avatar.png",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsActive = true,
                        StatusID = activeStatus.StatusID
                    };

                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                    else
                    {
                        throw new Exception($"Không thể tạo user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // 4. Seed Categories if empty
            if (!context.Categories.Any())
            {
                var parentCategories = new[]
                {
        new Category { CategoryName = "Công nghệ thông tin", CategoryType = "Field", Description = "Lĩnh vực CNTT", IsActive = true },
        new Category { CategoryName = "Marketing", CategoryType = "Field", Description = "Lĩnh vực marketing", IsActive = true }
    };
                context.Categories.AddRange(parentCategories);
                context.SaveChanges();

                var subCategories = new[]
                {
        new Category { CategoryName = "Lập trình web", CategoryType = "Skill", Description = "Phát triển website", ParentCategoryID = parentCategories[0].CategoryID, IsActive = true },
        new Category { CategoryName = "SEO", CategoryType = "Skill", Description = "Tối ưu hóa công cụ tìm kiếm", ParentCategoryID = parentCategories[1].CategoryID, IsActive = true }
    };
                context.Categories.AddRange(subCategories);
                context.SaveChanges();
            }

            // 5. Seed Skills
            if (!context.Skills.Any())
            {
                var webDevCategory = context.Categories.First(c => c.CategoryName == "Lập trình web");
                var seoCategory = context.Categories.First(c => c.CategoryName == "SEO");

                context.Skills.AddRange(
                    new Skill { SkillName = ".NET", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "React", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "SEO", CategoryID = seoCategory.CategoryID, IsActive = true }
                );
                context.SaveChanges();
            }

            // 6. Seed Project
            if (!context.Projects.Any())
            {
                var business = await userManager.FindByEmailAsync("business@example.com");
                var category = context.Categories.First(c => c.CategoryName == "Lập trình web");
                var status = context.ProjectStatuses.First(s => s.StatusName == "Đang tuyển");
                var type = context.ProjectTypes.First(t => t.TypeName == "Toàn thời gian");

                var project = new Project
                {
                    Title = "Phát triển website bán hàng",
                    Description = "Cần lập trình viên có kinh nghiệm về .NET và React",
                    Budget = 50000000,
                    StartDate = new DateTime(2025, 6, 22),
                    EndDate = new DateTime(2025, 9, 13),
                    BusinessID = business.Id,
                    CategoryID = category.CategoryID,
                    StatusID = status.StatusID,
                    TypeID = type.TypeID,
                    IsActive = true
                };
                context.Projects.Add(project);
                context.SaveChanges();
            }

            // 7. Seed ProjectSkillRequired
            if (!context.ProjectSkillsRequired.Any())
            {
                var project = context.Projects.First();
                var dotnet = context.Skills.First(s => s.SkillName == ".NET");
                var react = context.Skills.First(s => s.SkillName == "React");
                var required = context.ImportanceLevels.First(l => l.LevelName == "Bắt buộc");

                context.ProjectSkillsRequired.AddRange(
                    new ProjectSkillRequired { ProjectID = project.ProjectID, SkillID = dotnet.SkillID, ImportanceLevelID = required.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = project.ProjectID, SkillID = react.SkillID, ImportanceLevelID = required.LevelID, IsActive = true }
                );
                context.SaveChanges();
            }

            // 8. Seed StudentSkills
            if (!context.StudentSkills.Any())
            {
                var student = await userManager.FindByEmailAsync("student@example.com");
                var dotnet = context.Skills.First(s => s.SkillName == ".NET");
                var level = context.ProficiencyLevels.First(l => l.LevelName == "Thành thạo");

                context.StudentSkills.Add(new StudentSkill
                {
                    UserID = student.Id,
                    SkillID = dotnet.SkillID,
                    ProficiencyLevelID = level.LevelID,
                    IsActive = true
                });
                context.SaveChanges();
            }

            // 9. Seed StudentApplications
            if (!context.StudentApplications.Any())
            {
                var student = await userManager.FindByEmailAsync("student@example.com");
                var project = context.Projects.First();

                context.StudentApplications.Add(new StudentApplication
                {
                    ProjectID = project.ProjectID,
                    UserID = student.Id,
                    CoverLetter = "Tôi rất phù hợp dự án này.",
                    Salary = 5000000,
                    Status = "Pending",
                    DateApplied = DateTime.Now,
                    IsActive = true
                });
                context.SaveChanges();
            }
            // 10. Seed Messages
            if (!context.Messages.Any())
            {
                var student = await userManager.FindByEmailAsync("student@example.com");
                var business = await userManager.FindByEmailAsync("business@example.com");
                var project = context.Projects.First();

                context.Messages.AddRange(
                    new Message
                    {
                        SenderID = student.Id,
                        ReceiverID = business.Id,
                        ProjectID = project.ProjectID,
                        Content = "Em muốn trao đổi thêm về dự án",
                        SentAt = DateTime.Now,
                        IsRead = false,
                        IsActive = true
                    },
                    new Message
                    {
                        SenderID = business.Id,
                        ReceiverID = student.Id,
                        ProjectID = project.ProjectID,
                        Content = "Bạn có thể cho biết thêm kinh nghiệm .NET?",
                        SentAt = DateTime.Now.AddMinutes(5),
                        IsRead = false,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // 11. Seed Transactions
            if (!context.Transactions.Any())
            {
                var student = await userManager.FindByEmailAsync("student@example.com");
                var business = await userManager.FindByEmailAsync("business@example.com");
                var project = context.Projects.First();
                var deposit = context.TransactionTypes.First(t => t.TypeName == "Nạp tiền");
                var payment = context.TransactionTypes.First(t => t.TypeName == "Thanh toán");
                var success = context.TransactionStatuses.First(s => s.StatusName == "Thành công");

                context.Transactions.AddRange(
                    new Transaction
                    {
                        UserID = business.Id,
                        Amount = 50000000,
                        TypeID = deposit.TypeID,
                        TransactionDate = DateTime.Now.AddDays(-1),
                        Description = "Nạp tiền vào tài khoản",
                        StatusID = success.StatusID,
                        IsActive = true
                    },
                    new Transaction
                    {
                        UserID = business.Id,
                        ProjectID = project.ProjectID,
                        Amount = 15000000,
                        TypeID = payment.TypeID,
                        TransactionDate = DateTime.Now,
                        Description = "Thanh toán cho freelancer",
                        StatusID = success.StatusID,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // 12. Seed Ratings
            if (!context.Ratings.Any())
            {
                var project = context.Projects.First();
                var student = await userManager.FindByEmailAsync("student@example.com");
                var business = await userManager.FindByEmailAsync("business@example.com");

                context.Ratings.AddRange(
                    new Rating
                    {
                        ProjectID = project.ProjectID,
                        ReviewerID = business.Id,
                        RevieweeID = student.Id,
                        Score = 4.5M,
                        Comment = "Làm việc đúng hạn, chuyên nghiệp",
                        DateRated = DateTime.Now,
                        IsActive = true
                    },
                    new Rating
                    {
                        ProjectID = project.ProjectID,
                        ReviewerID = student.Id,
                        RevieweeID = business.Id,
                        Score = 5.0M,
                        Comment = "Khách hàng dễ tính, thanh toán nhanh",
                        DateRated = DateTime.Now,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // 13. Seed Reports
            if (!context.Reports.Any())
            {
                var student = await userManager.FindByEmailAsync("student@example.com");
                var business = await userManager.FindByEmailAsync("business@example.com");
                var project = context.Projects.First();
                var spam = context.ReportTypes.First(t => t.TypeName == "Spam");
                var pending = context.ReportStatuses.First(s => s.StatusName == "Đang xử lý");

                context.Reports.Add(new Report
                {
                    ReporterID = student.Id,
                    ReportedUserID = business.Id,
                    ProjectID = project.ProjectID,
                    TypeID = spam.TypeID,
                    Description = "Gửi quá nhiều tin nhắn rác",
                    StatusID = pending.StatusID,
                    ReportDate = DateTime.Now,
                    IsActive = true
                });
                context.SaveChanges();
            }

            // 14. Seed Notifications
            if (!context.Notifications.Any())
            {
                var student = await userManager.FindByEmailAsync("student@example.com");
                var business = await userManager.FindByEmailAsync("business@example.com");
                var project = context.Projects.First();
                var projectType = context.NotificationTypes.First(t => t.TypeName == "Dự án");
                var messageType = context.NotificationTypes.First(t => t.TypeName == "Tin nhắn");

                context.Notifications.AddRange(
                    new Notification
                    {
                        UserID = business.Id,
                        Title = "Có ứng viên mới",
                        Content = "Một sinh viên vừa ứng tuyển vào dự án của bạn.",
                        TypeID = projectType.TypeID,
                        RelatedID = project.ProjectID,
                        NotificationDate = DateTime.Now,
                        IsRead = false,
                        IsActive = true
                    },
                    new Notification
                    {
                        UserID = student.Id,
                        Title = "Tin nhắn mới",
                        Content = "Bạn nhận được tin nhắn từ doanh nghiệp.",
                        TypeID = messageType.TypeID,
                        RelatedID = project.ProjectID,
                        NotificationDate = DateTime.Now,
                        IsRead = false,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

        }
    }
}