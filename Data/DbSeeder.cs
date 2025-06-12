using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;

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

        public static void SeedSampleData(ApplicationDbContext context)
        {
            // Seed UserRoles if empty
            if (!context.UserRoles.Any())
            {
                context.UserRoles.AddRange(
                    new UserRole { RoleName = "Admin", Description = "Quản trị hệ thống", IsActive = true },
                    new UserRole { RoleName = "Moderator", Description = "Người kiểm duyệt", IsActive = true },
                    new UserRole { RoleName = "Business", Description = "Doanh nghiệp/Nhà tuyển dụng", IsActive = true },
                    new UserRole { RoleName = "Student", Description = "Sinh viên", IsActive = true }
                );
                context.SaveChanges();
            }

            // Seed Categories if empty
            if (!context.Categories.Any())
            {
                var parentCategories = new[]
                {
                    new Category { CategoryName = "Công nghệ thông tin", CategoryType = "Field", Description = "Lĩnh vực công nghệ thông tin", IsActive = true },
                    new Category { CategoryName = "Marketing", CategoryType = "Field", Description = "Lĩnh vực marketing", IsActive = true },
                    new Category { CategoryName = "Thiết kế", CategoryType = "Field", Description = "Lĩnh vực thiết kế", IsActive = true }
                };
                context.Categories.AddRange(parentCategories);
                context.SaveChanges();

                var subCategories = new[]
                {
                    new Category { CategoryName = "Lập trình web", CategoryType = "Skill", Description = "Phát triển website", ParentCategoryID = parentCategories[0].CategoryID, IsActive = true },
                    new Category { CategoryName = "Lập trình mobile", CategoryType = "Skill", Description = "Phát triển ứng dụng di động", ParentCategoryID = parentCategories[0].CategoryID, IsActive = true },
                    new Category { CategoryName = "Digital Marketing", CategoryType = "Skill", Description = "Marketing số", ParentCategoryID = parentCategories[1].CategoryID, IsActive = true },
                    new Category { CategoryName = "Content Marketing", CategoryType = "Skill", Description = "Sáng tạo nội dung", ParentCategoryID = parentCategories[1].CategoryID, IsActive = true },
                    new Category { CategoryName = "UI/UX Design", CategoryType = "Skill", Description = "Thiết kế giao diện người dùng", ParentCategoryID = parentCategories[2].CategoryID, IsActive = true },
                    new Category { CategoryName = "Graphic Design", CategoryType = "Skill", Description = "Thiết kế đồ họa", ParentCategoryID = parentCategories[2].CategoryID, IsActive = true }
                };
                context.Categories.AddRange(subCategories);
                context.SaveChanges();
            }

            // Seed Skills if empty
            if (!context.Skills.Any())
            {
                var webDevCategory = context.Categories.First(c => c.CategoryName == "Lập trình web");
                var mobileDevCategory = context.Categories.First(c => c.CategoryName == "Lập trình mobile");
                var digitalMarketingCategory = context.Categories.First(c => c.CategoryName == "Digital Marketing");
                var contentMarketingCategory = context.Categories.First(c => c.CategoryName == "Content Marketing");
                var uiuxCategory = context.Categories.First(c => c.CategoryName == "UI/UX Design");
                var graphicCategory = context.Categories.First(c => c.CategoryName == "Graphic Design");

                context.Skills.AddRange(
                    // Web Development Skills
                    new Skill { SkillName = "HTML/CSS", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "JavaScript", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "React", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Angular", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Node.js", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = ".NET", CategoryID = webDevCategory.CategoryID, IsActive = true },

                    // Mobile Development Skills
                    new Skill { SkillName = "Android (Java/Kotlin)", CategoryID = mobileDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "iOS (Swift)", CategoryID = mobileDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "React Native", CategoryID = mobileDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Flutter", CategoryID = mobileDevCategory.CategoryID, IsActive = true },

                    // Digital Marketing Skills
                    new Skill { SkillName = "SEO", CategoryID = digitalMarketingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Google Ads", CategoryID = digitalMarketingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Facebook Ads", CategoryID = digitalMarketingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Email Marketing", CategoryID = digitalMarketingCategory.CategoryID, IsActive = true },

                    // Content Marketing Skills
                    new Skill { SkillName = "Copywriting", CategoryID = contentMarketingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Content Strategy", CategoryID = contentMarketingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Social Media Content", CategoryID = contentMarketingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Blog Writing", CategoryID = contentMarketingCategory.CategoryID, IsActive = true },

                    // UI/UX Design Skills
                    new Skill { SkillName = "Figma", CategoryID = uiuxCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Adobe XD", CategoryID = uiuxCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Sketch", CategoryID = uiuxCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Wireframing", CategoryID = uiuxCategory.CategoryID, IsActive = true },

                    // Graphic Design Skills
                    new Skill { SkillName = "Adobe Photoshop", CategoryID = graphicCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Adobe Illustrator", CategoryID = graphicCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "CorelDRAW", CategoryID = graphicCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Logo Design", CategoryID = graphicCategory.CategoryID, IsActive = true }
                );
                context.SaveChanges();
            }

            // Seed Users if empty
            if (!context.Users.Any())
            {
                var adminRole = context.UserRoles.First(r => r.RoleName == "Admin");
                var moderatorRole = context.UserRoles.First(r => r.RoleName == "Moderator");
                var businessRole = context.UserRoles.First(r => r.RoleName == "Business");
                var studentRole = context.UserRoles.First(r => r.RoleName == "Student");
                var activeStatus = context.AccountStatuses.First(s => s.StatusName == "Hoạt động");

                context.Users.AddRange(
                    new User
                    {
                        Email = "admin@example.com",
                        Username = "admin",
                        PasswordHash = "AQAAAAIAAYagAAAAEPbewIEG0gP4Zz9pC2dIGqNFWHtHEHqE5GMRHc+GiZ+tBYFVXOhOdCjXFd+TZpWXwA==", // Password: Admin@123
                        FullName = "Admin",
                        CompanyName = "StudentFreelance",
                        Industry = "Công nghệ thông tin",
                        Major = "Quản trị hệ thống",
                        PhoneNumber = "0123456789",
                        University = "FPT University",
                        WalletBalance = 0,
                        VipStatus = true,
                        ProfileStatus = true,
                        AverageRating = 5,
                        TotalProjects = 0,
                        TotalProjectsPosted = 0,
                        ProfilePicturePath = "default-avatar.png",
                        Avatar = "default-avatar.png",
                        RoleID = adminRole.RoleID,
                        StatusID = activeStatus.StatusID,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsActive = true
                    },
                    new User
                    {
                        Email = "moderator@example.com",
                        Username = "moderator",
                        PasswordHash = "AQAAAAIAAYagAAAAEPbewIEG0gP4Zz9pC2dIGqNFWHtHEHqE5GMRHc+GiZ+tBYFVXOhOdCjXFd+TZpWXwA==", // Password: Moderator@123
                        FullName = "Moderator",
                        CompanyName = "StudentFreelance",
                        Industry = "Công nghệ thông tin",
                        Major = "Kiểm duyệt nội dung",
                        PhoneNumber = "0123456788",
                        University = "FPT University",
                        WalletBalance = 0,
                        VipStatus = true,
                        ProfileStatus = true,
                        AverageRating = 5,
                        TotalProjects = 0,
                        TotalProjectsPosted = 0,
                        ProfilePicturePath = "default-avatar.png",
                        Avatar = "default-avatar.png",
                        RoleID = moderatorRole.RoleID,
                        StatusID = activeStatus.StatusID,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsActive = true
                    },
                    new User
                    {
                        Email = "business@example.com",
                        Username = "business",
                        PasswordHash = "AQAAAAIAAYagAAAAEPbewIEG0gP4Zz9pC2dIGqNFWHtHEHqE5GMRHc+GiZ+tBYFVXOhOdCjXFd+TZpWXwA==", // Password: Business@123
                        FullName = "Công ty TNHH ABC",
                        CompanyName = "Công ty TNHH ABC",
                        Industry = "Công nghệ thông tin",
                        Major = "Phát triển phần mềm",
                        PhoneNumber = "0123456787",
                        WalletBalance = 0,
                        VipStatus = true,
                        ProfileStatus = true,
                        AverageRating = 0,
                        TotalProjects = 0,
                        TotalProjectsPosted = 0,
                        ProfilePicturePath = "default-avatar.png",
                        Avatar = "default-avatar.png",
                        RoleID = businessRole.RoleID,
                        StatusID = activeStatus.StatusID,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsActive = true
                    },
                    new User
                    {
                        Email = "student@example.com",
                        Username = "student",
                        PasswordHash = "AQAAAAIAAYagAAAAEPbewIEG0gP4Zz9pC2dIGqNFWHtHEHqE5GMRHc+GiZ+tBYFVXOhOdCjXFd+TZpWXwA==", // Password: Student@123
                        FullName = "Nguyễn Văn A",
                        CompanyName = "Sinh viên",
                        Industry = "Công nghệ thông tin",
                        Major = "Kỹ thuật phần mềm",
                        PhoneNumber = "0123456786",
                        University = "FPT University",
                        WalletBalance = 0,
                        VipStatus = false,
                        ProfileStatus = true,
                        AverageRating = 0,
                        TotalProjects = 0,
                        TotalProjectsPosted = 0,
                        ProfilePicturePath = "default-avatar.png",
                        Avatar = "default-avatar.png",
                        RoleID = studentRole.RoleID,
                        StatusID = activeStatus.StatusID,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // Seed Projects if empty
            if (!context.Projects.Any())
            {
                var business = context.Users.First(u => u.Email == "business@example.com");
                var webDevCategory = context.Categories.First(c => c.CategoryName == "Lập trình web");
                var openStatus = context.ProjectStatuses.First(s => s.StatusName == "Đang tuyển");
                var fullTimeType = context.ProjectTypes.First(t => t.TypeName == "Toàn thời gian");

                context.Projects.AddRange(
                    new Project
                    {
                        Title = "Phát triển website thương mại điện tử",
                        Description = "Xây dựng website thương mại điện tử sử dụng công nghệ .NET Core và React",
                        Budget = 50000000,
                        StartDate = DateTime.Now.AddDays(7),
                        EndDate = DateTime.Now.AddMonths(3),
                        BusinessID = business.UserID,
                        CategoryID = webDevCategory.CategoryID,
                        StatusID = openStatus.StatusID,
                        TypeID = fullTimeType.TypeID,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // Seed ProjectSkillsRequired if empty
            if (!context.ProjectSkillsRequired.Any())
            {
                var project = context.Projects.First();
                var dotnetSkill = context.Skills.First(s => s.SkillName == ".NET");
                var reactSkill = context.Skills.First(s => s.SkillName == "React");
                var requiredLevel = context.ImportanceLevels.First(l => l.LevelName == "Bắt buộc");
                var importantLevel = context.ImportanceLevels.First(l => l.LevelName == "Quan trọng");

                context.ProjectSkillsRequired.AddRange(
                    new ProjectSkillRequired
                    {
                        ProjectID = project.ProjectID,
                        SkillID = dotnetSkill.SkillID,
                        ImportanceLevelID = requiredLevel.LevelID,
                        IsActive = true
                    },
                    new ProjectSkillRequired
                    {
                        ProjectID = project.ProjectID,
                        SkillID = reactSkill.SkillID,
                        ImportanceLevelID = importantLevel.LevelID,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // Seed StudentSkills if empty
            if (!context.StudentSkills.Any())
            {
                var student = context.Users.First(u => u.Email == "student@example.com");
                var dotnetSkill = context.Skills.First(s => s.SkillName == ".NET");
                var reactSkill = context.Skills.First(s => s.SkillName == "React");
                var intermediateLevel = context.ProficiencyLevels.First(l => l.LevelName == "Trung cấp");
                var advancedLevel = context.ProficiencyLevels.First(l => l.LevelName == "Thành thạo");

                context.StudentSkills.AddRange(
                    new StudentSkill
                    {
                        UserID = student.UserID,
                        SkillID = dotnetSkill.SkillID,
                        ProficiencyLevelID = advancedLevel.LevelID,
                        IsActive = true
                    },
                    new StudentSkill
                    {
                        UserID = student.UserID,
                        SkillID = reactSkill.SkillID,
                        ProficiencyLevelID = intermediateLevel.LevelID,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // Seed StudentApplications if empty
            if (!context.StudentApplications.Any())
            {
                var student = context.Users.First(u => u.Email == "student@example.com");
                var project = context.Projects.First();

                context.StudentApplications.AddRange(
                    new StudentApplication
                    {
                        ProjectID = project.ProjectID,
                        UserID = student.UserID,
                        CoverLetter = "Em rất mong muốn được tham gia dự án này...",
                        Salary = 5000000,
                        Status = "Pending",
                        DateApplied = DateTime.Now,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // Seed Messages if empty
            if (!context.Messages.Any())
            {
                var student = context.Users.First(u => u.Email == "student@example.com");
                var business = context.Users.First(u => u.Email == "business@example.com");
                var project = context.Projects.First();

                context.Messages.AddRange(
                    new Message
                    {
                        SenderID = student.UserID,
                        ReceiverID = business.UserID,
                        ProjectID = project.ProjectID,
                        Content = "Xin chào, tôi muốn trao đổi thêm về dự án",
                        SentAt = DateTime.Now,
                        IsRead = false,
                        IsActive = true
                    },
                    new Message
                    {
                        SenderID = business.UserID,
                        ReceiverID = student.UserID,
                        ProjectID = project.ProjectID,
                        Content = "Chào bạn, bạn có thể cho biết thêm về kinh nghiệm làm việc với .NET không?",
                        SentAt = DateTime.Now.AddMinutes(5),
                        IsRead = false,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // Seed Transactions if empty
            if (!context.Transactions.Any())
            {
                var student = context.Users.First(u => u.Email == "student@example.com");
                var business = context.Users.First(u => u.Email == "business@example.com");
                var project = context.Projects.First();
                var depositType = context.TransactionTypes.First(t => t.TypeName == "Nạp tiền");
                var paymentType = context.TransactionTypes.First(t => t.TypeName == "Thanh toán");
                var completedStatus = context.TransactionStatuses.First(s => s.StatusName == "Thành công");

                context.Transactions.AddRange(
                    new Transaction
                    {
                        UserID = business.UserID,
                        Amount = 50000000,
                        TypeID = depositType.TypeID,
                        TransactionDate = DateTime.Now.AddDays(-1),
                        Description = "Nạp tiền vào tài khoản",
                        StatusID = completedStatus.StatusID,
                        IsActive = true
                    },
                    new Transaction
                    {
                        UserID = business.UserID,
                        ProjectID = project.ProjectID,
                        Amount = 15000000,
                        TypeID = paymentType.TypeID,
                        TransactionDate = DateTime.Now,
                        Description = "Thanh toán cho sinh viên",
                        StatusID = completedStatus.StatusID,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // Seed Ratings if empty
            if (!context.Ratings.Any())
            {
                var project1 = context.Projects.First();
                var businessRole = context.UserRoles.First(r => r.RoleName == "Business");
                var studentRole = context.UserRoles.First(r => r.RoleName == "Student");
                var business1 = context.Users.First(u => u.RoleID == businessRole.RoleID);
                var student1 = context.Users.First(u => u.RoleID == studentRole.RoleID);

                context.Ratings.AddRange(
                    new Rating
                    {
                        ProjectID = project1.ProjectID,
                        ReviewerID = business1.UserID,
                        RevieweeID = student1.UserID,
                        Score = 4.5M,
                        Comment = "Làm việc chuyên nghiệp, đúng tiến độ",
                        DateRated = DateTime.Now,
                        IsActive = true
                    },
                    new Rating
                    {
                        ProjectID = project1.ProjectID,
                        ReviewerID = student1.UserID,
                        RevieweeID = business1.UserID,
                        Score = 5.0M,
                        Comment = "Khách hàng dễ tính, thanh toán đúng hạn",
                        DateRated = DateTime.Now,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // Seed Reports if empty
            if (!context.Reports.Any())
            {
                var student = context.Users.First(u => u.Email == "student@example.com");
                var business = context.Users.First(u => u.Email == "business@example.com");
                var project = context.Projects.First();
                var spamType = context.ReportTypes.First(t => t.TypeName == "Spam");
                var pendingStatus = context.ReportStatuses.First(s => s.StatusName == "Đang xử lý");

                context.Reports.AddRange(
                    new Report
                    {
                        ReporterID = student.UserID,
                        ReportedUserID = business.UserID,
                        ProjectID = project.ProjectID,
                        TypeID = spamType.TypeID,
                        Description = "Doanh nghiệp gửi quá nhiều tin nhắn",
                        StatusID = pendingStatus.StatusID,
                        ReportDate = DateTime.Now,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // Seed Notifications if empty
            if (!context.Notifications.Any())
            {
                var student = context.Users.First(u => u.Email == "student@example.com");
                var business = context.Users.First(u => u.Email == "business@example.com");
                var project = context.Projects.First();
                var projectType = context.NotificationTypes.First(t => t.TypeName == "Dự án");
                var messageType = context.NotificationTypes.First(t => t.TypeName == "Tin nhắn");

                context.Notifications.AddRange(
                    new Notification
                    {
                        UserID = business.UserID,
                        Title = "Có ứng viên mới",
                        Content = "Có sinh viên mới ứng tuyển vào dự án của bạn",
                        TypeID = projectType.TypeID,
                        RelatedID = project.ProjectID,
                        NotificationDate = DateTime.Now,
                        IsRead = false,
                        IsActive = true
                    },
                    new Notification
                    {
                        UserID = student.UserID,
                        Title = "Tin nhắn mới",
                        Content = "Bạn có tin nhắn mới từ doanh nghiệp",
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