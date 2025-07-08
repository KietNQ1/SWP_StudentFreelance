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
                    new ProjectStatus { StatusName = "Đang tiến hành", IsActive = true },
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
                ("student@example.com", "Student@123", "Student", "Nguyễn Văn A"),
                
                // Additional business accounts
                ("techsolutions@example.com", "Tech@123", "Business", "Tech Solutions"),
                ("marketingpro@example.com", "Marketing@123", "Business", "Marketing Pro"),
                ("designstudio@example.com", "Design@123", "Business", "Design Studio"),
                ("creativeagency@example.com", "Creative@123", "Business", "Creative Agency"),
                ("itconsulting@example.com", "ITConsult@123", "Business", "IT Consulting Services"),
                
                // Additional student accounts
                ("student1@example.com", "Student1@123", "Student", "Trần Thị B"),
                ("student2@example.com", "Student2@123", "Student", "Lê Minh C"),
                ("student3@example.com", "Student3@123", "Student", "Phạm Hoàng D"),
                ("student4@example.com", "Student4@123", "Student", "Vũ Anh E"),
                ("student5@example.com", "Student5@123", "Student", "Đỗ Thu F"),
                ("student6@example.com", "Student6@123", "Student", "Hoàng Lan G"),
                ("student7@example.com", "Student7@123", "Student", "Ngô Thanh H")
            };

            foreach (var (email, password, role, fullName) in usersToSeed)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    // Create address for user with default values
                    var address = new Address
                    {
                        ProvinceID = 1,
                        DistrictID = 1,
                        WardID = 1,
                        DetailAddress = "Số nhà mặc định",
                        FullAddress = "Địa chỉ mặc định",
                        IsActive = true
                    };

                    context.Addresses.Add(address);
                    context.SaveChanges();

                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = fullName,
                        PhoneNumber = role == "Business" ? "0987654321" : "0123456789",
                        University = role == "Student" ? GetRandomUniversity() : null,
                        Major = role == "Student" ? GetRandomMajor() : null,
                        CompanyName = role == "Business" ? fullName : null,
                        Industry = role == "Business" ? GetRandomIndustry() : null,
                        WalletBalance = role == "Business" ? new Random().Next(5, 100) * 1000000 : new Random().Next(1, 10) * 1000000,
                        VipStatus = role == "Admin" || role == "Moderator" || (role == "Business" && new Random().Next(0, 2) == 1),
                        ProfileStatus = true,
                        AverageRating = (decimal)(new Random().NextDouble() * 2 + 3), // Random rating between 3.0 and 5.0
                        TotalProjects = role == "Student" ? new Random().Next(0, 10) : 0,
                        TotalProjectsPosted = role == "Business" ? new Random().Next(1, 15) : 0,
                        ProfilePicturePath = "default-avatar.png",
                        Avatar = "default-avatar.png",
                        CreatedAt = DateTime.Now.AddDays(-new Random().Next(1, 365)), // Random registration date within last year
                        UpdatedAt = DateTime.Now,
                        IsActive = true,
                        StatusID = activeStatus.StatusID,
                        AddressID = address.AddressID // Link to the created address
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

            // Helper methods for random data
            string GetRandomUniversity()
            {
                string[] universities = {
                    "FPT University", "Đại học Bách Khoa Hà Nội", "Đại học Quốc gia Hà Nội",
                    "Đại học Kinh tế TP.HCM", "Đại học Công nghệ TP.HCM", "Đại học Đà Nẵng",
                    "Đại học Cần Thơ", "Đại học Huế", "Đại học Ngoại thương"
                };
                return universities[new Random().Next(0, universities.Length)];
            }

            string GetRandomMajor()
            {
                string[] majors = {
                    "Công nghệ thông tin", "Kỹ thuật phần mềm", "Khoa học máy tính",
                    "Marketing", "Thiết kế đồ họa", "Quản trị kinh doanh",
                    "Kế toán", "Ngôn ngữ Anh", "Truyền thông đa phương tiện"
                };
                return majors[new Random().Next(0, majors.Length)];
            }

            string GetRandomIndustry()
            {
                string[] industries = {
                    "Công nghệ", "Thương mại điện tử", "Fintech",
                    "Giáo dục", "Y tế", "Truyền thông",
                    "Bán lẻ", "Sản xuất", "Dịch vụ"
                };
                return industries[new Random().Next(0, industries.Length)];
            }

            // 4. Seed Categories if empty
            if (!context.Categories.Any())
            {
                var parentCategories = new[]
                {
                    new Category { CategoryName = "Công nghệ thông tin", CategoryType = "Field", Description = "Lĩnh vực CNTT", IsActive = true },
                    new Category { CategoryName = "Marketing", CategoryType = "Field", Description = "Lĩnh vực marketing", IsActive = true },
                    new Category { CategoryName = "Thiết kế", CategoryType = "Field", Description = "Lĩnh vực thiết kế và sáng tạo", IsActive = true },
                    new Category { CategoryName = "Viết lách", CategoryType = "Field", Description = "Lĩnh vực viết và biên tập", IsActive = true },
                    new Category { CategoryName = "Dịch thuật", CategoryType = "Field", Description = "Lĩnh vực dịch thuật", IsActive = true },
                    new Category { CategoryName = "Kinh doanh", CategoryType = "Field", Description = "Lĩnh vực kinh doanh và quản lý", IsActive = true }
                };
                context.Categories.AddRange(parentCategories);
                context.SaveChanges();

                var subCategories = new[]
                {
                    // IT subcategories
                    new Category { CategoryName = "Lập trình web", CategoryType = "Skill", Description = "Phát triển website", ParentCategoryID = parentCategories[0].CategoryID, IsActive = true },
                    new Category { CategoryName = "Lập trình mobile", CategoryType = "Skill", Description = "Phát triển ứng dụng di động", ParentCategoryID = parentCategories[0].CategoryID, IsActive = true },
                    new Category { CategoryName = "Phát triển game", CategoryType = "Skill", Description = "Phát triển trò chơi điện tử", ParentCategoryID = parentCategories[0].CategoryID, IsActive = true },
                    new Category { CategoryName = "Khoa học dữ liệu", CategoryType = "Skill", Description = "Phân tích và xử lý dữ liệu", ParentCategoryID = parentCategories[0].CategoryID, IsActive = true },
                    new Category { CategoryName = "DevOps", CategoryType = "Skill", Description = "Vận hành và triển khai hệ thống", ParentCategoryID = parentCategories[0].CategoryID, IsActive = true },
                    
                    // Marketing subcategories
                    new Category { CategoryName = "SEO", CategoryType = "Skill", Description = "Tối ưu hóa công cụ tìm kiếm", ParentCategoryID = parentCategories[1].CategoryID, IsActive = true },
                    new Category { CategoryName = "Tiếp thị nội dung", CategoryType = "Skill", Description = "Sáng tạo và quản lý nội dung marketing", ParentCategoryID = parentCategories[1].CategoryID, IsActive = true },
                    new Category { CategoryName = "Quảng cáo số", CategoryType = "Skill", Description = "Chạy quảng cáo trên các nền tảng số", ParentCategoryID = parentCategories[1].CategoryID, IsActive = true },
                    new Category { CategoryName = "Social Media", CategoryType = "Skill", Description = "Quản lý mạng xã hội", ParentCategoryID = parentCategories[1].CategoryID, IsActive = true },
                    
                    // Design subcategories
                    new Category { CategoryName = "UI/UX Design", CategoryType = "Skill", Description = "Thiết kế giao diện người dùng", ParentCategoryID = parentCategories[2].CategoryID, IsActive = true },
                    new Category { CategoryName = "Đồ họa", CategoryType = "Skill", Description = "Thiết kế đồ họa", ParentCategoryID = parentCategories[2].CategoryID, IsActive = true },
                    new Category { CategoryName = "Video Editing", CategoryType = "Skill", Description = "Chỉnh sửa video", ParentCategoryID = parentCategories[2].CategoryID, IsActive = true },
                    
                    // Writing subcategories
                    new Category { CategoryName = "Viết nội dung", CategoryType = "Skill", Description = "Sáng tạo nội dung cho website, blog", ParentCategoryID = parentCategories[3].CategoryID, IsActive = true },
                    new Category { CategoryName = "Copywriting", CategoryType = "Skill", Description = "Viết quảng cáo", ParentCategoryID = parentCategories[3].CategoryID, IsActive = true },
                    
                    // Translation subcategories
                    new Category { CategoryName = "Anh - Việt", CategoryType = "Skill", Description = "Dịch Anh - Việt", ParentCategoryID = parentCategories[4].CategoryID, IsActive = true },
                    new Category { CategoryName = "Việt - Anh", CategoryType = "Skill", Description = "Dịch Việt - Anh", ParentCategoryID = parentCategories[4].CategoryID, IsActive = true },
                    
                    // Business subcategories
                    new Category { CategoryName = "Tư vấn kinh doanh", CategoryType = "Skill", Description = "Tư vấn chiến lược kinh doanh", ParentCategoryID = parentCategories[5].CategoryID, IsActive = true },
                    new Category { CategoryName = "Kế toán", CategoryType = "Skill", Description = "Dịch vụ kế toán", ParentCategoryID = parentCategories[5].CategoryID, IsActive = true }
                };
                context.Categories.AddRange(subCategories);
                context.SaveChanges();
            }

            // 5. Seed Skills
            if (!context.Skills.Any())
            {
                // Get categories
                var webDevCategory = context.Categories.First(c => c.CategoryName == "Lập trình web");
                var mobileDevCategory = context.Categories.First(c => c.CategoryName == "Lập trình mobile");
                var gameDevCategory = context.Categories.First(c => c.CategoryName == "Phát triển game");
                var dataScience = context.Categories.First(c => c.CategoryName == "Khoa học dữ liệu");
                var devOpsCategory = context.Categories.First(c => c.CategoryName == "DevOps");
                var seoCategory = context.Categories.First(c => c.CategoryName == "SEO");
                var contentMarketingCategory = context.Categories.First(c => c.CategoryName == "Tiếp thị nội dung");
                var digitalAdsCategory = context.Categories.First(c => c.CategoryName == "Quảng cáo số");
                var socialMediaCategory = context.Categories.First(c => c.CategoryName == "Social Media");
                var uiuxCategory = context.Categories.First(c => c.CategoryName == "UI/UX Design");
                var graphicDesignCategory = context.Categories.First(c => c.CategoryName == "Đồ họa");
                var videoEditingCategory = context.Categories.First(c => c.CategoryName == "Video Editing");
                var contentWritingCategory = context.Categories.First(c => c.CategoryName == "Viết nội dung");
                var copywritingCategory = context.Categories.First(c => c.CategoryName == "Copywriting");
                var engViTranslationCategory = context.Categories.First(c => c.CategoryName == "Anh - Việt");
                var viEngTranslationCategory = context.Categories.First(c => c.CategoryName == "Việt - Anh");
                var businessConsultingCategory = context.Categories.First(c => c.CategoryName == "Tư vấn kinh doanh");
                var accountingCategory = context.Categories.First(c => c.CategoryName == "Kế toán");

                var skills = new List<Skill>
                {
                    // Web Development Skills
                    new Skill { SkillName = ".NET", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "React", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Angular", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Vue.js", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "PHP", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Laravel", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Node.js", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Django", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Ruby on Rails", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "HTML/CSS", CategoryID = webDevCategory.CategoryID, IsActive = true },
                    
                    // Mobile Development Skills
                    new Skill { SkillName = "Android (Java/Kotlin)", CategoryID = mobileDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "iOS (Swift)", CategoryID = mobileDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "React Native", CategoryID = mobileDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Flutter", CategoryID = mobileDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Xamarin", CategoryID = mobileDevCategory.CategoryID, IsActive = true },
                    
                    // Game Development Skills
                    new Skill { SkillName = "Unity", CategoryID = gameDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Unreal Engine", CategoryID = gameDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Godot", CategoryID = gameDevCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Game Design", CategoryID = gameDevCategory.CategoryID, IsActive = true },
                    
                    // Data Science Skills
                    new Skill { SkillName = "Python", CategoryID = dataScience.CategoryID, IsActive = true },
                    new Skill { SkillName = "R", CategoryID = dataScience.CategoryID, IsActive = true },
                    new Skill { SkillName = "Machine Learning", CategoryID = dataScience.CategoryID, IsActive = true },
                    new Skill { SkillName = "Deep Learning", CategoryID = dataScience.CategoryID, IsActive = true },
                    new Skill { SkillName = "Data Visualization", CategoryID = dataScience.CategoryID, IsActive = true },
                    
                    // DevOps Skills
                    new Skill { SkillName = "Docker", CategoryID = devOpsCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Kubernetes", CategoryID = devOpsCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "AWS", CategoryID = devOpsCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Azure", CategoryID = devOpsCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "CI/CD", CategoryID = devOpsCategory.CategoryID, IsActive = true },
                    
                    // SEO Skills
                    new Skill { SkillName = "SEO", CategoryID = seoCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Nghiên cứu từ khóa", CategoryID = seoCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "SEO Onpage", CategoryID = seoCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "SEO Offpage", CategoryID = seoCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Google Analytics", CategoryID = seoCategory.CategoryID, IsActive = true },
                    
                    // Other Marketing Skills
                    new Skill { SkillName = "Content Marketing", CategoryID = contentMarketingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Email Marketing", CategoryID = contentMarketingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Google Ads", CategoryID = digitalAdsCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Facebook Ads", CategoryID = digitalAdsCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Social Media Management", CategoryID = socialMediaCategory.CategoryID, IsActive = true },
                    
                    // Design Skills
                    new Skill { SkillName = "UI Design", CategoryID = uiuxCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "UX Design", CategoryID = uiuxCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Figma", CategoryID = uiuxCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Adobe XD", CategoryID = uiuxCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Photoshop", CategoryID = graphicDesignCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Illustrator", CategoryID = graphicDesignCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Premiere Pro", CategoryID = videoEditingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "After Effects", CategoryID = videoEditingCategory.CategoryID, IsActive = true },
                    
                    // Writing and Translation Skills
                    new Skill { SkillName = "Blog Writing", CategoryID = contentWritingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Technical Writing", CategoryID = contentWritingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Copywriting", CategoryID = copywritingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "English to Vietnamese", CategoryID = engViTranslationCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Vietnamese to English", CategoryID = viEngTranslationCategory.CategoryID, IsActive = true },
                    
                    // Business Skills
                    new Skill { SkillName = "Business Strategy", CategoryID = businessConsultingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Market Research", CategoryID = businessConsultingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Financial Analysis", CategoryID = accountingCategory.CategoryID, IsActive = true },
                    new Skill { SkillName = "Tax Consulting", CategoryID = accountingCategory.CategoryID, IsActive = true }
                };

                context.Skills.AddRange(skills);
                context.SaveChanges();
            }

            // 6. Seed Project
            if (!context.Projects.Any())
            {
                // Get business users
                var businesses = await userManager.GetUsersInRoleAsync("Business");

                // Get categories
                var webCategory = context.Categories.First(c => c.CategoryName == "Lập trình web");
                var mobileCategory = context.Categories.First(c => c.CategoryName == "Lập trình mobile");
                var dataCategory = context.Categories.First(c => c.CategoryName == "Khoa học dữ liệu");
                var uiuxCategory = context.Categories.First(c => c.CategoryName == "UI/UX Design");
                var socialMediaCategory = context.Categories.First(c => c.CategoryName == "Social Media");
                var contentWritingCategory = context.Categories.First(c => c.CategoryName == "Viết nội dung");
                var translationCategory = context.Categories.First(c => c.CategoryName == "Anh - Việt");

                // Get project statuses
                var recruitingStatus = context.ProjectStatuses.First(s => s.StatusName == "Đang tuyển");
                var inProgressStatus = context.ProjectStatuses.First(s => s.StatusName == "Đang tiến hành");
                var completedStatus = context.ProjectStatuses.First(s => s.StatusName == "Đã hoàn thành");
                var cancelledStatus = context.ProjectStatuses.First(s => s.StatusName == "Đã hủy");

                // Get project types
                var fullTimeType = context.ProjectTypes.First(t => t.TypeName == "Toàn thời gian");
                var partTimeType = context.ProjectTypes.First(t => t.TypeName == "Bán thời gian");
                var projectBasedType = context.ProjectTypes.First(t => t.TypeName == "Theo dự án");

                var projects = new List<Project>
                {
                    // Web Development Projects
                    new Project
                    {
                        Title = "Phát triển website bán hàng",
                        Description = "Cần lập trình viên có kinh nghiệm về .NET và React để phát triển website thương mại điện tử với đầy đủ tính năng như giỏ hàng, thanh toán online, quản lý đơn hàng.",
                        Budget = 50000000,
                        StartDate = new DateTime(2025, 6, 22),
                        EndDate = new DateTime(2025, 9, 13),
                        BusinessID = businesses[0].Id,
                        CategoryID = webCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = fullTimeType.TypeID,
                        IsActive = true
                    },
                    new Project
                    {
                        Title = "Xây dựng blog cá nhân",
                        Description = "Cần freelancer thiết kế và phát triển blog cá nhân sử dụng WordPress hoặc các nền tảng tương tự. Blog cần có giao diện đẹp, tối ưu SEO.",
                        Budget = 5000000,
                        StartDate = new DateTime(2025, 7, 1),
                        EndDate = new DateTime(2025, 7, 30),
                        BusinessID = businesses[1].Id,
                        CategoryID = webCategory.CategoryID,
                        StatusID = inProgressStatus.StatusID,
                        TypeID = projectBasedType.TypeID,
                        IsActive = true
                    },
                    new Project
                    {
                        Title = "Nâng cấp hệ thống quản lý nội bộ",
                        Description = "Dự án nâng cấp hệ thống quản lý nội bộ hiện tại, bổ sung các tính năng báo cáo và thống kê. Sử dụng Laravel và VueJS.",
                        Budget = 35000000,
                        StartDate = new DateTime(2025, 5, 15),
                        EndDate = new DateTime(2025, 8, 15),
                        BusinessID = businesses[2].Id,
                        CategoryID = webCategory.CategoryID,
                        StatusID = completedStatus.StatusID,
                        TypeID = fullTimeType.TypeID,
                        IsActive = true
                    },
                    
                    // Mobile Development Projects
                    new Project
                    {
                        Title = "Phát triển ứng dụng di động đặt đồ ăn",
                        Description = "Cần phát triển ứng dụng đặt đồ ăn trên iOS và Android với tính năng tìm kiếm nhà hàng, đánh giá, thanh toán online.",
                        Budget = 70000000,
                        StartDate = new DateTime(2025, 7, 10),
                        EndDate = new DateTime(2025, 10, 30),
                        BusinessID = businesses[3].Id,
                        CategoryID = mobileCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = fullTimeType.TypeID,
                        IsActive = true
                    },
                    new Project
                    {
                        Title = "Ứng dụng theo dõi sức khỏe",
                        Description = "Phát triển ứng dụng theo dõi sức khỏe cá nhân với các tính năng theo dõi bước chân, nhịp tim, chế độ ăn uống, v.v.",
                        Budget = 45000000,
                        StartDate = new DateTime(2025, 6, 15),
                        EndDate = new DateTime(2025, 9, 15),
                        BusinessID = businesses[4].Id,
                        CategoryID = mobileCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = partTimeType.TypeID,
                        IsActive = true
                    },
                    
                    // Data Science Projects
                    new Project
                    {
                        Title = "Phân tích dữ liệu khách hàng",
                        Description = "Cần chuyên gia phân tích dữ liệu để xử lý và phân tích dữ liệu khách hàng, xây dựng mô hình dự đoán hành vi mua hàng.",
                        Budget = 40000000,
                        StartDate = new DateTime(2025, 7, 5),
                        EndDate = new DateTime(2025, 8, 30),
                        BusinessID = businesses[0].Id,
                        CategoryID = dataCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = projectBasedType.TypeID,
                        IsActive = true
                    },
                    
                    // UI/UX Design Projects
                    new Project
                    {
                        Title = "Thiết kế lại giao diện website công ty",
                        Description = "Cần designer thiết kế lại giao diện website công ty theo phong cách hiện đại, tối giản, thân thiện với người dùng.",
                        Budget = 25000000,
                        StartDate = new DateTime(2025, 6, 10),
                        EndDate = new DateTime(2025, 7, 25),
                        BusinessID = businesses[1].Id,
                        CategoryID = uiuxCategory.CategoryID,
                        StatusID = inProgressStatus.StatusID,
                        TypeID = partTimeType.TypeID,
                        IsActive = true
                    },
                    
                    // Social Media Projects
                    new Project
                    {
                        Title = "Quản lý fanpage và chạy quảng cáo",
                        Description = "Cần người có kinh nghiệm quản lý fanpage Facebook, lên kế hoạch nội dung, chạy quảng cáo Facebook Ads.",
                        Budget = 10000000,
                        StartDate = new DateTime(2025, 7, 1),
                        EndDate = new DateTime(2025, 12, 31),
                        BusinessID = businesses[2].Id,
                        CategoryID = socialMediaCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = partTimeType.TypeID,
                        IsActive = true
                    },
                    
                    // Content Writing Projects
                    new Project
                    {
                        Title = "Viết nội dung blog về công nghệ",
                        Description = "Cần freelancer viết 20 bài blog về các chủ đề công nghệ như AI, blockchain, 5G, v.v. Mỗi bài từ 1000-1500 từ.",
                        Budget = 15000000,
                        StartDate = new DateTime(2025, 7, 15),
                        EndDate = new DateTime(2025, 9, 15),
                        BusinessID = businesses[3].Id,
                        CategoryID = contentWritingCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = projectBasedType.TypeID,
                        IsActive = true
                    },
                    
                    // Translation Projects
                    new Project
                    {
                        Title = "Dịch tài liệu kỹ thuật",
                        Description = "Cần dịch tài liệu kỹ thuật về phần mềm từ tiếng Anh sang tiếng Việt, khoảng 50 trang.",
                        Budget = 8000000,
                        StartDate = new DateTime(2025, 8, 1),
                        EndDate = new DateTime(2025, 8, 31),
                        BusinessID = businesses[4].Id,
                        CategoryID = translationCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = projectBasedType.TypeID,
                        IsActive = true
                    }
                };

                context.Projects.AddRange(projects);
                context.SaveChanges();
            }

            // 7. Seed ProjectSkillRequired
            if (!context.ProjectSkillsRequired.Any())
            {
                var projects = context.Projects.ToList();

                // Get skills
                var dotnetSkill = context.Skills.First(s => s.SkillName == ".NET");
                var reactSkill = context.Skills.First(s => s.SkillName == "React");
                var angularSkill = context.Skills.First(s => s.SkillName == "Angular");
                var vueSkill = context.Skills.First(s => s.SkillName == "Vue.js");
                var phpSkill = context.Skills.First(s => s.SkillName == "PHP");
                var laravelSkill = context.Skills.First(s => s.SkillName == "Laravel");
                var androidSkill = context.Skills.First(s => s.SkillName == "Android (Java/Kotlin)");
                var iosSkill = context.Skills.First(s => s.SkillName == "iOS (Swift)");
                var flutterSkill = context.Skills.First(s => s.SkillName == "Flutter");
                var pythonSkill = context.Skills.First(s => s.SkillName == "Python");
                var mlSkill = context.Skills.First(s => s.SkillName == "Machine Learning");
                var uiDesignSkill = context.Skills.First(s => s.SkillName == "UI Design");
                var uxDesignSkill = context.Skills.First(s => s.SkillName == "UX Design");
                var figmaSkill = context.Skills.First(s => s.SkillName == "Figma");
                var socialMediaSkill = context.Skills.First(s => s.SkillName == "Social Media Management");
                var facebookAdsSkill = context.Skills.First(s => s.SkillName == "Facebook Ads");
                var blogSkill = context.Skills.First(s => s.SkillName == "Blog Writing");
                var techWritingSkill = context.Skills.First(s => s.SkillName == "Technical Writing");
                var engViSkill = context.Skills.First(s => s.SkillName == "English to Vietnamese");

                // Get importance levels
                var requiredLevel = context.ImportanceLevels.First(l => l.LevelName == "Bắt buộc");
                var importantLevel = context.ImportanceLevels.First(l => l.LevelName == "Quan trọng");
                var preferredLevel = context.ImportanceLevels.First(l => l.LevelName == "Ưu tiên");

                var projectSkills = new List<ProjectSkillRequired>();

                // Project 1: Website bán hàng
                projectSkills.AddRange(new[]
                {
                    new ProjectSkillRequired { ProjectID = projects[0].ProjectID, SkillID = dotnetSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[0].ProjectID, SkillID = reactSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true }
                });

                // Project 2: Blog cá nhân
                projectSkills.AddRange(new[]
                {
                    new ProjectSkillRequired { ProjectID = projects[1].ProjectID, SkillID = phpSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[1].ProjectID, SkillID = reactSkill.SkillID, ImportanceLevelID = preferredLevel.LevelID, IsActive = true }
                });

                // Project 3: Nâng cấp hệ thống
                projectSkills.AddRange(new[]
                {
                    new ProjectSkillRequired { ProjectID = projects[2].ProjectID, SkillID = laravelSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[2].ProjectID, SkillID = vueSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true }
                });

                // Project 4: Ứng dụng đặt đồ ăn
                projectSkills.AddRange(new[]
                {
                    new ProjectSkillRequired { ProjectID = projects[3].ProjectID, SkillID = androidSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[3].ProjectID, SkillID = iosSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[3].ProjectID, SkillID = flutterSkill.SkillID, ImportanceLevelID = preferredLevel.LevelID, IsActive = true }
                });

                // Project 5: Ứng dụng sức khỏe
                projectSkills.AddRange(new[]
                {
                    new ProjectSkillRequired { ProjectID = projects[4].ProjectID, SkillID = flutterSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[4].ProjectID, SkillID = uiDesignSkill.SkillID, ImportanceLevelID = importantLevel.LevelID, IsActive = true }
                });

                // Project 6: Phân tích dữ liệu
                projectSkills.AddRange(new[]
                {
                    new ProjectSkillRequired { ProjectID = projects[5].ProjectID, SkillID = pythonSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[5].ProjectID, SkillID = mlSkill.SkillID, ImportanceLevelID = importantLevel.LevelID, IsActive = true }
                });

                // Project 7: Thiết kế giao diện
                projectSkills.AddRange(new[]
                {
                    new ProjectSkillRequired { ProjectID = projects[6].ProjectID, SkillID = uiDesignSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[6].ProjectID, SkillID = uxDesignSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[6].ProjectID, SkillID = figmaSkill.SkillID, ImportanceLevelID = importantLevel.LevelID, IsActive = true }
                });

                // Project 8: Quản lý fanpage
                projectSkills.AddRange(new[]
                {
                    new ProjectSkillRequired { ProjectID = projects[7].ProjectID, SkillID = socialMediaSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[7].ProjectID, SkillID = facebookAdsSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true }
                });

                // Project 9: Viết blog
                projectSkills.AddRange(new[]
                {
                    new ProjectSkillRequired { ProjectID = projects[8].ProjectID, SkillID = blogSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true },
                    new ProjectSkillRequired { ProjectID = projects[8].ProjectID, SkillID = techWritingSkill.SkillID, ImportanceLevelID = importantLevel.LevelID, IsActive = true }
                });

                // Project 10: Dịch tài liệu
                projectSkills.AddRange(new[]
                {
                    new ProjectSkillRequired { ProjectID = projects[9].ProjectID, SkillID = engViSkill.SkillID, ImportanceLevelID = requiredLevel.LevelID, IsActive = true }
                });

                context.ProjectSkillsRequired.AddRange(projectSkills);
                context.SaveChanges();
            }

            // 8. Seed StudentSkills
            if (!context.StudentSkills.Any())
            {
                var students = await userManager.GetUsersInRoleAsync("Student");
                var random = new Random();

                // Get skills
                var skills = context.Skills.ToList();

                // Get proficiency levels
                var beginnerLevel = context.ProficiencyLevels.First(l => l.LevelName == "Mới bắt đầu");
                var intermediateLevel = context.ProficiencyLevels.First(l => l.LevelName == "Trung cấp");
                var advancedLevel = context.ProficiencyLevels.First(l => l.LevelName == "Thành thạo");
                var expertLevel = context.ProficiencyLevels.First(l => l.LevelName == "Chuyên gia");

                var levels = new[] { beginnerLevel, intermediateLevel, advancedLevel, expertLevel };
                var webDevSkills = skills.Where(s => s.CategoryID == context.Categories.First(c => c.CategoryName == "Lập trình web").CategoryID).ToList();
                var mobileDevSkills = skills.Where(s => s.CategoryID == context.Categories.First(c => c.CategoryName == "Lập trình mobile").CategoryID).ToList();
                var designSkills = skills.Where(s => s.CategoryID == context.Categories.First(c => c.CategoryName == "UI/UX Design").CategoryID
                    || s.CategoryID == context.Categories.First(c => c.CategoryName == "Đồ họa").CategoryID).ToList();
                var contentSkills = skills.Where(s => s.CategoryID == context.Categories.First(c => c.CategoryName == "Viết nội dung").CategoryID).ToList();

                var studentSkills = new List<StudentSkill>();

                // Assign skills to each student
                foreach (var student in students)
                {
                    // Decide student's main focus area randomly
                    var focus = random.Next(0, 4);
                    var primarySkills = focus switch
                    {
                        0 => webDevSkills,
                        1 => mobileDevSkills,
                        2 => designSkills,
                        _ => contentSkills
                    };

                    // Assign 3-5 primary skills with higher proficiency
                    var numPrimarySkills = random.Next(3, 6);
                    for (int i = 0; i < numPrimarySkills && i < primarySkills.Count; i++)
                    {
                        var skill = primarySkills[random.Next(primarySkills.Count)];
                        // Higher chances of advanced or expert level for primary skills
                        var level = levels[random.Next(2, 4)];

                        // Skip if this skill is already added for this student
                        if (studentSkills.Any(ss => ss.UserID == student.Id && ss.SkillID == skill.SkillID))
                            continue;

                        studentSkills.Add(new StudentSkill
                        {
                            UserID = student.Id,
                            SkillID = skill.SkillID,
                            ProficiencyLevelID = level.LevelID,
                            IsActive = true
                        });
                    }

                    // Assign 1-3 secondary skills with lower proficiency
                    var numSecondarySkills = random.Next(1, 4);
                    var secondarySkills = skills.Except(primarySkills).ToList();
                    for (int i = 0; i < numSecondarySkills && i < secondarySkills.Count; i++)
                    {
                        var skill = secondarySkills[random.Next(secondarySkills.Count)];
                        // Lower chances of beginner or intermediate level for secondary skills
                        var level = levels[random.Next(0, 2)];

                        // Skip if this skill is already added for this student
                        if (studentSkills.Any(ss => ss.UserID == student.Id && ss.SkillID == skill.SkillID))
                            continue;

                        studentSkills.Add(new StudentSkill
                        {
                            UserID = student.Id,
                            SkillID = skill.SkillID,
                            ProficiencyLevelID = level.LevelID,
                            IsActive = true
                        });
                    }
                }

                context.StudentSkills.AddRange(studentSkills);
                context.SaveChanges();
            }

            // 9. Seed StudentApplications
            if (!context.StudentApplications.Any())
            {
                var students = await userManager.GetUsersInRoleAsync("Student");
                var projects = context.Projects.ToList();
                var random = new Random();

                var applications = new List<StudentApplication>
                {
                    new StudentApplication
                    {
                        UserID = students[0].Id,
                        ProjectID = projects[0].ProjectID,
                        Status = "Pending",
                        CoverLetter = "Tôi rất quan tâm đến dự án của bạn và có kinh nghiệm trong lĩnh vực này.",
                        Salary = 5000000,
                        DateApplied = DateTime.Now.AddDays(-5),
                        LastStatusUpdate = DateTime.Now.AddDays(-5),
                        Notes = "Ghi chú ban đầu",
                        ResumeLink = "https://example.com/resume1.pdf",
                        BusinessConfirmedCompletion = false,
                        StudentConfirmedCompletion = false,
                        IsActive = true
                    },
                    new StudentApplication
                    {
                        UserID = students[1].Id,
                        ProjectID = projects[0].ProjectID,
                        Status = "Accepted",
                        CoverLetter = "Tôi có nhiều kinh nghiệm với các dự án tương tự và mong muốn được hợp tác.",
                        Salary = 5500000,
                        DateApplied = DateTime.Now.AddDays(-4),
                        LastStatusUpdate = DateTime.Now.AddDays(-2),
                        BusinessNotes = "Ứng viên có kinh nghiệm tốt, phù hợp với yêu cầu dự án.",
                        Notes = "Đã được chấp nhận",
                        ResumeLink = "https://example.com/resume2.pdf",
                        BusinessConfirmedCompletion = false,
                        StudentConfirmedCompletion = false,
                        IsActive = true
                    },
                    new StudentApplication
                    {
                        UserID = students[0].Id,
                        ProjectID = projects[1].ProjectID,
                        Status = "Rejected",
                        CoverLetter = "Tôi muốn ứng tuyển vào vị trí này để phát triển kỹ năng của mình.",
                        Salary = 4800000,
                        DateApplied = DateTime.Now.AddDays(-3),
                        LastStatusUpdate = DateTime.Now.AddDays(-1),
                        BusinessNotes = "Ứng viên chưa đáp ứng đủ yêu cầu kỹ năng cho dự án.",
                        Notes = "Đã bị từ chối",
                        ResumeLink = "https://example.com/resume3.pdf",
                        BusinessConfirmedCompletion = false,
                        StudentConfirmedCompletion = false,
                        IsActive = true
                    }
                };

                context.StudentApplications.AddRange(applications);
                context.SaveChanges();
            }

            // Helper method for generating cover letters
            string GetRandomCoverLetter()
            {
                string[] coverLetters = new[]
                {
                    "Tôi rất phù hợp với dự án này vì tôi đã có kinh nghiệm làm việc với các công nghệ tương tự trong các dự án trước đây.",
                    "Tôi là sinh viên năm cuối ngành CNTT, đã hoàn thành nhiều dự án tương tự và rất mong được góp phần vào thành công của dự án này.",
                    "Với kinh nghiệm 2 năm làm việc freelance trong lĩnh vực này, tôi tự tin có thể đáp ứng tốt các yêu cầu của dự án.",
                    "Tôi đã tham gia nhiều dự án tương tự và đạt được kết quả tốt. Tôi cam kết sẽ hoàn thành công việc đúng thời hạn và đảm bảo chất lượng.",
                    "Với niềm đam mê và kiến thức chuyên môn, tôi tự tin sẽ mang lại giá trị gia tăng cho dự án của quý công ty.",
                    "Tôi đã nghiên cứu kỹ về dự án và thấy rằng kỹ năng của mình rất phù hợp. Tôi mong muốn được đóng góp vào sự thành công của dự án này."
                };

                return coverLetters[new Random().Next(coverLetters.Length)];
            }

            // Helper method for generating portfolio links
            string GetRandomPortfolioLink()
            {
                string[] portfolioLinks = new[]
                {
                    "https://github.com/student-portfolio",
                    "https://behance.net/student-design",
                    "https://dribbble.com/student-ui",
                    "https://student-portfolio.vercel.app",
                    "https://linkedin.com/in/student-profile",
                    "https://codepen.io/student-demos"
                };

                return portfolioLinks[new Random().Next(portfolioLinks.Length)];
            }

            // Helper method for generating resume links
            string GetRandomResumeLink()
            {
                string[] resumeLinks = new[]
                {
                    "https://drive.google.com/file/d/abc123/view",
                    "https://dropbox.com/s/cv-student.pdf",
                    "https://onedrive.live.com/cv-student.pdf",
                    "https://docs.google.com/document/d/123abc/edit",
                    "https://resume.io/r/student-cv",
                    "https://cv.student-portfolio.com/resume.pdf"
                };

                return resumeLinks[new Random().Next(resumeLinks.Length)];
            }

            // 10. Seed Messages
            if (!context.Messages.Any())
            {
                var student = await userManager.FindByEmailAsync("student@example.com");
                var business = await userManager.FindByEmailAsync("business@example.com");
                var project = context.Projects.First();

                // Tạo Conversation trước
                var conversation = new Conversation
                {
                    ProjectID = project.ProjectID,
                    ParticipantAID = student.Id,
                    ParticipantBID = business.Id,
                    CreatedAt = DateTime.UtcNow
                };
                context.Conversations.Add(conversation);
                await context.SaveChangesAsync(); // Đảm bảo ConversationID được tạo

                // Tạo các message có liên kết ConversationID
                context.Messages.AddRange(
                    new Message
                    {
                        SenderID = student.Id,
                        ReceiverID = business.Id,
                        ProjectID = project.ProjectID,
                        ConversationID = conversation.ConversationID, // Gán đúng ConversationID
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
                        ConversationID = conversation.ConversationID, // Gán đúng ConversationID
                        Content = "Bạn có thể cho biết thêm kinh nghiệm .NET?",
                        SentAt = DateTime.Now.AddMinutes(5),
                        IsRead = false,
                        IsActive = true
                    }
                );

                await context.SaveChangesAsync();
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
                        IsActive = true,

                        OrderCode = Guid.NewGuid().ToString()
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
                        IsActive = true,

                        OrderCode = Guid.NewGuid().ToString()
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
                var students = await userManager.GetUsersInRoleAsync("Student");
                var businesses = await userManager.GetUsersInRoleAsync("Business");
                var projects = context.Projects.ToList();
                var random = new Random();

                // Get report types
                var spamType = context.ReportTypes.First(t => t.TypeName == "Spam");
                var fraudType = context.ReportTypes.First(t => t.TypeName == "Lừa đảo");
                var inappropriateType = context.ReportTypes.First(t => t.TypeName == "Nội dung không phù hợp");
                var otherType = context.ReportTypes.First(t => t.TypeName == "Khác");
                var reportTypes = new[] { spamType, fraudType, inappropriateType, otherType };

                // Get report statuses
                var pendingStatus = context.ReportStatuses.First(s => s.StatusName == "Đang xử lý");
                var processedStatus = context.ReportStatuses.First(s => s.StatusName == "Đã xử lý");
                var cancelledStatus = context.ReportStatuses.First(s => s.StatusName == "Đã hủy");
                var reportStatuses = new[] { pendingStatus, processedStatus, cancelledStatus };

                var reports = new List<Report>();

                // Generate 5-10 reports from students about businesses
                int numStudentReports = random.Next(5, 11);
                for (int i = 0; i < numStudentReports; i++)
                {
                    var student = students[random.Next(students.Count)];
                    var business = businesses[random.Next(businesses.Count)];
                    var project = projects[random.Next(projects.Count)];
                    var reportType = reportTypes[random.Next(reportTypes.Length)];
                    var reportStatus = reportStatuses[random.Next(reportStatuses.Length)];

                    reports.Add(new Report
                    {
                        ReporterID = student.Id,
                        ReportedUserID = business.Id,
                        ProjectID = project.ProjectID,
                        TypeID = reportType.TypeID,
                        Description = GetReportDescription(reportType.TypeName),
                        StatusID = reportStatus.StatusID,
                        ReportDate = DateTime.Now.AddDays(-random.Next(1, 30)),
                        IsActive = true
                    });
                }

                // Generate 3-7 reports from businesses about students
                int numBusinessReports = random.Next(3, 8);
                for (int i = 0; i < numBusinessReports; i++)
                {
                    var business = businesses[random.Next(businesses.Count)];
                    var student = students[random.Next(students.Count)];
                    var project = projects[random.Next(projects.Count)];
                    var reportType = reportTypes[random.Next(reportTypes.Length)];
                    var reportStatus = reportStatuses[random.Next(reportStatuses.Length)];

                    reports.Add(new Report
                    {
                        ReporterID = business.Id,
                        ReportedUserID = student.Id,
                        ProjectID = project.ProjectID,
                        TypeID = reportType.TypeID,
                        Description = GetReportDescription(reportType.TypeName),
                        StatusID = reportStatus.StatusID,
                        ReportDate = DateTime.Now.AddDays(-random.Next(1, 30)),
                        IsActive = true
                    });
                }

                context.Reports.AddRange(reports);
                context.SaveChanges();
            }

            // Helper method to generate report descriptions
            string GetReportDescription(string reportType)
            {
                switch (reportType)
                {
                    case "Spam":
                        string[] spamReasons = {
                            "Gửi quá nhiều tin nhắn rác liên tục.",
                            "Đăng quảng cáo không liên quan đến công việc.",
                            "Liên tục gửi cùng một tin nhắn."
                        };
                        return spamReasons[new Random().Next(spamReasons.Length)];

                    case "Lừa đảo":
                        string[] fraudReasons = {
                            "Yêu cầu thanh toán trước khi bắt đầu công việc.",
                            "Cung cấp thông tin giả mạo về kinh nghiệm và kỹ năng.",
                            "Sử dụng ảnh đại diện và thông tin của người khác."
                        };
                        return fraudReasons[new Random().Next(fraudReasons.Length)];

                    case "Nội dung không phù hợp":
                        string[] inappropriateReasons = {
                            "Sử dụng ngôn ngữ thô tục, thiếu tôn trọng.",
                            "Đăng nội dung vi phạm bản quyền.",
                            "Chia sẻ thông tin cá nhân của người khác."
                        };
                        return inappropriateReasons[new Random().Next(inappropriateReasons.Length)];

                    default:
                        string[] otherReasons = {
                            "Không trả lời tin nhắn trong thời gian dài.",
                            "Yêu cầu thông tin cá nhân không cần thiết.",
                            "Thay đổi yêu cầu công việc liên tục không báo trước."
                        };
                        return otherReasons[new Random().Next(otherReasons.Length)];
                }
            }

            // 14. Seed Notifications
            if (!context.Notifications.Any())
            {
                var students = await userManager.GetUsersInRoleAsync("Student");
                var businesses = await userManager.GetUsersInRoleAsync("Business");
                var projects = context.Projects.ToList();
                var random = new Random();

                // Get notification types
                var systemType = context.NotificationTypes.First(t => t.TypeName == "Hệ thống");
                var projectType = context.NotificationTypes.First(t => t.TypeName == "Dự án");
                var messageType = context.NotificationTypes.First(t => t.TypeName == "Tin nhắn");
                var transactionType = context.NotificationTypes.First(t => t.TypeName == "Giao dịch");

                var notifications = new List<Notification>();
                var userNotifications = new List<UserNotification>();

                // Generate system notifications for all users
                foreach (var student in students)
                {
                    var notification = new Notification
                    {
                        Title = "Chào mừng bạn đến với FreelanceStudent",
                        Content = "Cảm ơn bạn đã tham gia cộng đồng FreelanceStudent. Hãy cập nhật hồ sơ để bắt đầu nhận dự án.",
                        TypeID = systemType.TypeID,
                        NotificationDate = student.CreatedAt.AddMinutes(5),
                        IsActive = true
                    };

                    notifications.Add(notification);

                    // After adding to the list, we'll need to have an ID for relationships
                    context.Notifications.Add(notification);
                    context.SaveChanges();

                    userNotifications.Add(new UserNotification
                    {
                        UserID = student.Id,
                        NotificationID = notification.NotificationID,
                        IsRead = true
                    });
                }

                foreach (var business in businesses)
                {
                    var notification = new Notification
                    {
                        Title = "Chào mừng đến với FreelanceStudent",
                        Content = "Cảm ơn doanh nghiệp đã tham gia cộng đồng FreelanceStudent. Hãy đăng dự án để kết nối với sinh viên tài năng.",
                        TypeID = systemType.TypeID,
                        NotificationDate = business.CreatedAt.AddMinutes(5),
                        IsActive = true
                    };

                    notifications.Add(notification);

                    // After adding to the list, we'll need to have an ID for relationships
                    context.Notifications.Add(notification);
                    context.SaveChanges();

                    userNotifications.Add(new UserNotification
                    {
                        UserID = business.Id,
                        NotificationID = notification.NotificationID,
                        IsRead = true
                    });
                }

                // Generate project notifications
                foreach (var project in projects.Take(5))
                {
                    // For businesses
                    var businessNotification = new Notification
                    {
                        Title = "Có ứng viên mới cho dự án",
                        Content = "Một sinh viên vừa ứng tuyển vào dự án '" + project.Title + "' của bạn.",
                        TypeID = projectType.TypeID,
                        RelatedID = project.ProjectID,
                        NotificationDate = DateTime.Now.AddDays(-random.Next(1, 10)),
                        IsActive = true
                    };

                    notifications.Add(businessNotification);

                    // After adding to the list, we'll need to have an ID for relationships
                    context.Notifications.Add(businessNotification);
                    context.SaveChanges();

                    userNotifications.Add(new UserNotification
                    {
                        UserID = project.BusinessID,
                        NotificationID = businessNotification.NotificationID,
                        IsRead = random.Next(0, 2) == 1
                    });

                    // For students
                    var student = students[random.Next(students.Count)];
                    var studentNotification = new Notification
                    {
                        Title = "Cập nhật trạng thái ứng tuyển",
                        Content = "Đơn ứng tuyển của bạn cho dự án '" + project.Title + "' đã được chấp nhận.",
                        TypeID = projectType.TypeID,
                        RelatedID = project.ProjectID,
                        NotificationDate = DateTime.Now.AddDays(-random.Next(1, 5)),
                        IsActive = true
                    };

                    notifications.Add(studentNotification);

                    // After adding to the list, we'll need to have an ID for relationships
                    context.Notifications.Add(studentNotification);
                    context.SaveChanges();

                    userNotifications.Add(new UserNotification
                    {
                        UserID = student.Id,
                        NotificationID = studentNotification.NotificationID,
                        IsRead = random.Next(0, 2) == 1
                    });
                }

                // Generate message notifications
                for (int i = 0; i < 10; i++)
                {
                    var student = students[random.Next(students.Count)];
                    var business = businesses[random.Next(businesses.Count)];
                    var project = projects[random.Next(projects.Count)];

                    if (random.Next(0, 2) == 0)
                    {
                        // Student receives message
                        var messageNotification = new Notification
                        {
                            Title = "Tin nhắn mới",
                            Content = "Bạn nhận được tin nhắn mới từ '" + business.FullName + "' về dự án '" + project.Title + "'.",
                            TypeID = messageType.TypeID,
                            SenderID = business.Id,
                            RelatedID = project.ProjectID,
                            NotificationDate = DateTime.Now.AddDays(-random.Next(0, 14)),
                            IsActive = true
                        };

                        notifications.Add(messageNotification);

                        // After adding to the list, we'll need to have an ID for relationships
                        context.Notifications.Add(messageNotification);
                        context.SaveChanges();

                        userNotifications.Add(new UserNotification
                        {
                            UserID = student.Id,
                            NotificationID = messageNotification.NotificationID,
                            IsRead = random.Next(0, 2) == 1
                        });
                    }
                    else
                    {
                        // Business receives message
                        var messageNotification = new Notification
                        {
                            Title = "Tin nhắn mới",
                            Content = "Bạn nhận được tin nhắn mới từ '" + student.FullName + "' về dự án '" + project.Title + "'.",
                            TypeID = messageType.TypeID,
                            SenderID = student.Id,
                            RelatedID = project.ProjectID,
                            NotificationDate = DateTime.Now.AddDays(-random.Next(0, 14)),
                            IsActive = true
                        };

                        notifications.Add(messageNotification);

                        // After adding to the list, we'll need to have an ID for relationships
                        context.Notifications.Add(messageNotification);
                        context.SaveChanges();

                        userNotifications.Add(new UserNotification
                        {
                            UserID = business.Id,
                            NotificationID = messageNotification.NotificationID,
                            IsRead = random.Next(0, 2) == 1
                        });
                    }
                }

                // Generate transaction notifications
                foreach (var business in businesses.Take(3))
                {
                    var transactionNotification = new Notification
                    {
                        Title = "Giao dịch thành công",
                        Content = "Bạn đã nạp " + (random.Next(10, 50) * 1000000).ToString("N0") + " VND vào tài khoản thành công.",
                        TypeID = transactionType.TypeID,
                        NotificationDate = DateTime.Now.AddDays(-random.Next(1, 30)),
                        IsActive = true
                    };

                    notifications.Add(transactionNotification);

                    // After adding to the list, we'll need to have an ID for relationships
                    context.Notifications.Add(transactionNotification);
                    context.SaveChanges();

                    userNotifications.Add(new UserNotification
                    {
                        UserID = business.Id,
                        NotificationID = transactionNotification.NotificationID,
                        IsRead = random.Next(0, 2) == 1
                    });
                }

                foreach (var student in students.Take(2))
                {
                    var transactionNotification = new Notification
                    {
                        Title = "Đã nhận thanh toán",
                        Content = "Bạn đã nhận được " + (random.Next(5, 30) * 1000000).ToString("N0") + " VND từ một dự án đã hoàn thành.",
                        TypeID = transactionType.TypeID,
                        NotificationDate = DateTime.Now.AddDays(-random.Next(1, 15)),
                        IsActive = true
                    };

                    notifications.Add(transactionNotification);

                    // After adding to the list, we'll need to have an ID for relationships
                    context.Notifications.Add(transactionNotification);
                    context.SaveChanges();

                    userNotifications.Add(new UserNotification
                    {
                        UserID = student.Id,
                        NotificationID = transactionNotification.NotificationID,
                        IsRead = random.Next(0, 2) == 1
                    });
                }

                // Add a broadcast notification
                var broadcastNotification = new Notification
                {
                    Title = "Nâng cấp hệ thống",
                    Content = "Hệ thống sẽ bảo trì vào ngày 15/08/2025 từ 0h-2h. Mong quý khách thông cảm.",
                    TypeID = systemType.TypeID,
                    NotificationDate = DateTime.Now.AddDays(-3),
                    IsBroadcast = true,
                    IsActive = true
                };

                notifications.Add(broadcastNotification);
                context.Notifications.Add(broadcastNotification);
                context.SaveChanges();

                // Add UserNotifications with Read status for some users
                foreach (var user in students.Take(3).Concat(businesses.Take(2)))
                {
                    userNotifications.Add(new UserNotification
                    {
                        UserID = user.Id,
                        NotificationID = broadcastNotification.NotificationID,
                        IsRead = true,
                        ReadDate = DateTime.Now.AddDays(-2)
                    });
                }

                context.UserNotifications.AddRange(userNotifications);
                context.SaveChanges();
            }

        }
    }
}