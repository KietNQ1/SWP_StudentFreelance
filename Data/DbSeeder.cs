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
                    new TransactionType { TypeName = "Hoàn tiền", IsActive = true },
                    new TransactionType { TypeName = "Nâng cấp VIP", IsActive = true },
                    new TransactionType { TypeName = "Thanh toán quảng cáo", IsActive = true },
                    new TransactionType { TypeName = "Thanh toán cho sinh viên", IsActive = true },
                    new TransactionType { TypeName = "Phí dự án", IsActive = true }
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

            if (!context.AdvertisementStatuses.Any())
            {
                context.AdvertisementStatuses.AddRange(
                    new AdvertisementStatus { StatusName = "Chờ duyệt", IsActive = true },
                    new AdvertisementStatus { StatusName = "Đã duyệt", IsActive = true },
                    new AdvertisementStatus { StatusName = "Từ chối", IsActive = true },
                    new AdvertisementStatus { StatusName = "Hết hạn", IsActive = true }
                );
            }

            if (!context.AdvertisementPackageTypes.Any())
            {
                context.AdvertisementPackageTypes.AddRange(
                    new AdvertisementPackageType { 
                        PackageTypeName = "Basic", 
                        Price = 100000, 
                        DurationDays = 7, 
                        IsActive = true 
                    },
                    new AdvertisementPackageType { 
                        PackageTypeName = "Featured", 
                        Price = 200000, 
                        DurationDays = 7, 
                        IsActive = true 
                    }
                );
            }

            context.SaveChanges();
        }

        public static async Task SeedSampleDataAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            // Ensure all existing projects have valid flag values
            foreach (var project in context.Projects.Where(p => p.IsFlagged && p.FlagReason == null))
            {
                project.FlagReason = "Default flag reason";
            }
            await context.SaveChangesAsync();
            
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
                // Keep old example accounts as backup
                ("admin@example.com", "Admin@123", "Admin", "Admin"),
                ("moderator@example.com","Moderator@123","Moderator", "Moderator"),
                ("business@example.com","Business@123", "Business", "Công ty TNHH ABC"),
                ("student@example.com", "Student@123", "Student", "Nguyễn Văn A"),
                ("techsolutions@example.com", "Tech@123", "Business", "Tech Solutions"),
                ("marketingpro@example.com", "Marketing@123", "Business", "Marketing Pro"),
                ("designstudio@example.com", "Design@123", "Business", "Design Studio"),
                ("creativeagency@example.com", "Creative@123", "Business", "Creative Agency"),
                ("itconsulting@example.com", "ITConsult@123", "Business", "IT Consulting Services"),
                ("student1@example.com", "Student1@123", "Student", "Trần Thị B"),
                ("student2@example.com", "Student2@123", "Student", "Lê Minh C"),
                ("student3@example.com", "Student3@123", "Student", "Phạm Hoàng D"),
                ("student4@example.com", "Student4@123", "Student", "Vũ Anh E"),
                ("student5@example.com", "Student5@123", "Student", "Đỗ Thu F"),
                ("student6@example.com", "Student6@123", "Student", "Hoàng Lan G"),
                ("student7@example.com", "Student7@123", "Student", "Ngô Thanh H"),

                // New Admin and Moderator accounts
                ("admin@stjobs.com", "Admin@2024", "Admin", "STJobs Administrator"),
                ("moderator@stjobs.com", "Moderator@2024", "Moderator", "STJobs Moderator"),

                // New Business accounts
                ("techvision@stjobs.com", "TechVision@2024", "Business", "Công ty TNHH Giải pháp Công nghệ TechVision"),
                ("digimark@stjobs.com", "DigiMark@2024", "Business", "Công ty TNHH Giải pháp Marketing DigiMark"),
                ("uixstudio@stjobs.com", "UIXStudio@2024", "Business", "Công ty TNHH Thiết kế UIX Studio"),
                ("fintech@stjobs.com", "FinTech@2024", "Business", "Công ty Cổ phần Công nghệ Tài chính SmartPay"),
                ("edtech@stjobs.com", "EdTech@2024", "Business", "Công ty TNHH Công nghệ Giáo dục EduTech"),

                // New Student accounts
                ("nguyenhoanganh@stjobs.com", "HoangAnh@2024", "Student", "Nguyễn Hoàng Anh"),
                ("tranthiminh@stjobs.com", "ThiMinh@2024", "Student", "Trần Thị Minh"),
                ("leductrung@stjobs.com", "DucTrung@2024", "Student", "Lê Đức Trung"),
                ("phamthuhang@stjobs.com", "ThuHang@2024", "Student", "Phạm Thu Hằng"),
                ("vuminhtam@stjobs.com", "MinhTam@2024", "Student", "Vũ Minh Tâm")
            };

            foreach (var (email, password, role, fullName) in usersToSeed)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    // Create address for user
                    var address = new Address();
                    
                    // Set specific addresses for new accounts
                    if (email == "techvision@stjobs.com")
                    {
                        address = new Address
                        {
                            ProvinceCode = "79",
                            ProvinceName = "TP Hồ Chí Minh",
                            DistrictCode = "763",
                            DistrictName = "Quận 12",
                            WardCode = "26803",
                            WardName = "Phường Tân Chánh Hiệp",
                            DetailAddress = "Tòa nhà TMA, Công viên phần mềm Quang Trung",
                            FullAddress = "Tòa nhà TMA, Công viên phần mềm Quang Trung, Phường Tân Chánh Hiệp, Quận 12, TP.HCM",
                            IsActive = true
                        };
                    }
                    else if (email == "digimark@stjobs.com")
                    {
                        address = new Address
                        {
                            ProvinceCode = "79",
                            ProvinceName = "TP Hồ Chí Minh",
                            DistrictCode = "760",
                            DistrictName = "Quận 1",
                            WardCode = "26734",
                            WardName = "Phường Bến Nghé",
                            DetailAddress = "Tòa nhà Bitexco, 2 Hải Triều",
                            FullAddress = "Tòa nhà Bitexco, 2 Hải Triều, Phường Bến Nghé, Quận 1, TP.HCM",
                            IsActive = true
                        };
                    }
                    else if (email == "uixstudio@stjobs.com")
                    {
                        address = new Address
                        {
                            ProvinceCode = "79",
                            ProvinceName = "TP Hồ Chí Minh",
                            DistrictCode = "778",
                            DistrictName = "Quận 7",
                            WardCode = "27307",
                            WardName = "Phường Tân Phú",
                            DetailAddress = "Tòa nhà Mapletree, 1060 Nguyễn Văn Linh",
                            FullAddress = "Tòa nhà Mapletree, 1060 Nguyễn Văn Linh, Phường Tân Phú, Quận 7, TP.HCM",
                            IsActive = true
                        };
                    }
                    else if (email == "fintech@stjobs.com")
                    {
                        address = new Address
                        {
                            ProvinceCode = "79",
                            ProvinceName = "TP Hồ Chí Minh",
                            DistrictCode = "769",
                            DistrictName = "Quận 2",
                            WardCode = "27154",
                            WardName = "Phường Thảo Điền",
                            DetailAddress = "Tòa nhà Sala, 33 Đường Mai Chí Thọ",
                            FullAddress = "Tòa nhà Sala, 33 Đường Mai Chí Thọ, Phường Thảo Điền, Quận 2, TP.HCM",
                            IsActive = true
                        };
                    }
                    else if (email == "edtech@stjobs.com")
                    {
                        address = new Address
                        {
                            ProvinceCode = "1",
                            ProvinceName = "Hà Nội",
                            DistrictCode = "006",
                            DistrictName = "Quận Cầu Giấy",
                            WardCode = "00190",
                            WardName = "Phường Dịch Vọng Hậu",
                            DetailAddress = "Tòa nhà Hà Nội ICT, 17 Duy Tân",
                            FullAddress = "Tòa nhà Hà Nội ICT, 17 Duy Tân, Phường Dịch Vọng Hậu, Quận Cầu Giấy, Hà Nội",
                            IsActive = true
                        };
                    }
                    else if (email.Contains("@stjobs.com") && role == "Student")
                    {
                        address = new Address
                        {
                            ProvinceCode = "79",
                            ProvinceName = "TP Hồ Chí Minh",
                            DistrictCode = "769",
                            DistrictName = "TP Thủ Đức",
                            WardCode = "26908",
                            WardName = "Phường Linh Trung",
                            DetailAddress = email switch
                            {
                                "nguyenhoanganh@stjobs.com" => "42/3 Đường số 7",
                                "tranthiminh@stjobs.com" => "15 Đường số 12",
                                "leductrung@stjobs.com" => "78 Đường số 5",
                                "phamthuhang@stjobs.com" => "103 Đường số 9",
                                "vuminhtam@stjobs.com" => "25 Đường số 3",
                                _ => "Số nhà mặc định"
                            },
                            FullAddress = $"{address.DetailAddress}, Phường Linh Trung, TP Thủ Đức, TP.HCM",
                            IsActive = true
                        };
                    }
                    else 
                    {
                        address = new Address
                        {
                            ProvinceCode = "1",
                            ProvinceName = "Hà Nội",
                            DistrictCode = "001",
                            DistrictName = "Quận Ba Đình",
                            WardCode = "00001",
                            WardName = "Phường Phúc Xá",
                            DetailAddress = "Số nhà mặc định",
                            FullAddress = "Số nhà mặc định, Phường Phúc Xá, Quận Ba Đình, Hà Nội",
                            IsActive = true
                        };
                    };

                    context.Addresses.Add(address); 
                    context.SaveChanges();

                    string avatarPath = "/image/default-avatar.png";
                    string profilePicturePath = "/image/default-avatar.png";
                    
                    // Assign specific avatar paths for businesses
                    if (email == "business@example.com")
                    {
                        avatarPath = "/uploads/avatars/0d882281-2af4-4dc3-bb7e-a611f24078e3_ABC-Logo-1962-present.png";
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "techsolutions@example.com")
                    {
                        avatarPath = "/uploads/avatars/989681f7-2447-4339-8e2e-b45cf8f94ebe_techsolutionsgroupltd_cover.jpeg";
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "marketingpro@example.com")
                    {
                        avatarPath = "/uploads/avatars/98adebc1-99d5-46df-a6de-37f2d002465e_marketing-logo.jpg";
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "itconsulting@example.com")
                    {
                        avatarPath = "/uploads/avatars/e8990825-3b3e-4dba-955a-4e13cc51ba26_itcompany-logo-by-pixahive.png";
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "designstudio@example.com")
                    {
                        avatarPath = "/uploads/avatars/b4192da7-1087-4709-a7c4-0b93d9f2c4d1_abstract-logo-for-studio-design-with-creative-modern-concept-vector.jpg";
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "creativeagency@example.com")
                    {
                        avatarPath = "/uploads/avatars/2c8597a4-a89c-4865-8cb9-922166e8a5b9_360_F_1242285305_2aOiHPOURRD4SmXOFU3fV0OoUnNYEaTX.jpg";
                        profilePicturePath = avatarPath;
                    }
                    // Add avatars for new businesses
                    else if (email == "techvision@stjobs.com")
                    {
                        avatarPath = "/image/Icon/laptop-1.png";
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "digimark@stjobs.com")
                    {
                        avatarPath = "/image/Icon/get-money.png";
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "uixstudio@stjobs.com")
                    {
                        avatarPath = "/image/Icon/032-writing.png";
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "fintech@stjobs.com")
                    {
                        avatarPath = "/image/Icon/coins-1.png";
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "edtech@stjobs.com")
                    {
                        avatarPath = "/image/Icon/003-book.png";
                        profilePicturePath = avatarPath;
                    }
                    // Add avatars for students using provided character images
                    else if (email == "nguyenhoanganh@stjobs.com")
                    {
                        avatarPath = "/image/ic_001.png"; // X (Blue)
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "tranthiminh@stjobs.com")
                    {
                        avatarPath = "/image/ic_002.png"; // Zero (Red)
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "leductrung@stjobs.com")
                    {
                        avatarPath = "/image/ic_005.png"; // Roll (Orange)
                        profilePicturePath = avatarPath;
                    }
                    else if (email == "phamthuhang@stjobs.com" || email == "vuminhtam@stjobs.com")
                    {
                        avatarPath = "/image/ic_007.png"; // Ciel (Yellow)
                        profilePicturePath = avatarPath;
                    }
                    // For other students, use random existing avatars
                    else if (role == "Student")
                    {
                        string[] studentAvatars = new[]
                        {
                            "/image/ic_001.png",
                            "/image/ic_002.png",
                            "/image/ic_005.png",
                            "/image/ic_007.png",
                            "/image/huan.jpg",
                            "/image/huy.jpg",
                            "/image/kiet.jpg",
                            "/image/tan.jpg",
                            "/image/vy.jpg"
                        };
                        avatarPath = studentAvatars[new Random().Next(studentAvatars.Length)];
                        profilePicturePath = avatarPath;
                    }

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
                        ProfilePicturePath = profilePicturePath,
                        Avatar = avatarPath,
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
                    new Category { 
                        CategoryName = "Công nghệ thông tin", 
                        CategoryType = "Field", 
                        Description = "Lĩnh vực CNTT", 
                        ImagePath = "/image/Icon/laptop-1.png",
                        IsActive = true 
                    },
                    new Category { 
                        CategoryName = "Marketing", 
                        CategoryType = "Field", 
                        Description = "Lĩnh vực marketing", 
                        ImagePath = "/image/Icon/get-money.png",
                        IsActive = true 
                    },
                    new Category { 
                        CategoryName = "Thiết kế", 
                        CategoryType = "Field", 
                        Description = "Lĩnh vực thiết kế và sáng tạo", 
                        ImagePath = "/image/Icon/032-writing.png",
                        IsActive = true 
                    },
                    new Category { 
                        CategoryName = "Viết lách", 
                        CategoryType = "Field", 
                        Description = "Lĩnh vực viết và biên tập", 
                        ImagePath = "/image/Icon/notebook-1.png",
                        IsActive = true 
                    },
                    new Category { 
                        CategoryName = "Dịch thuật", 
                        CategoryType = "Field", 
                        Description = "Lĩnh vực dịch thuật", 
                        ImagePath = "/image/Icon/030-reading-1.png",
                        IsActive = true 
                    },
                    new Category { 
                        CategoryName = "Kinh doanh", 
                        CategoryType = "Field", 
                        Description = "Lĩnh vực kinh doanh và quản lý", 
                        ImagePath = "/image/Icon/coins-1.png",
                        IsActive = true 
                    },
                    new Category {
                        CategoryName = "Pháp lý",
                        CategoryType = "Field",
                        Description = "Lĩnh vực pháp lý",
                        ImagePath = "/image/Icon/justice.png",
                        IsActive = true
                    },
                    new Category {
                        CategoryName = "Giáo dục",
                        CategoryType = "Field",
                        Description = "Lĩnh vực giáo dục",
                        ImagePath = "/image/Icon/003-book.png",
                        IsActive = true
                    }
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
                // Get business users for new projects
                var techvision = await userManager.FindByEmailAsync("techvision@stjobs.com") 
                    ?? throw new Exception("TechVision user not found");
                var digimark = await userManager.FindByEmailAsync("digimark@stjobs.com")
                    ?? throw new Exception("DigiMark user not found");
                var uixstudio = await userManager.FindByEmailAsync("uixstudio@stjobs.com")
                    ?? throw new Exception("UIXStudio user not found");
                var fintech = await userManager.FindByEmailAsync("fintech@stjobs.com")
                    ?? throw new Exception("FinTech user not found");
                var edtech = await userManager.FindByEmailAsync("edtech@stjobs.com")
                    ?? throw new Exception("EdTech user not found");

                // Get student users for project assignments
                var hoangAnh = await userManager.FindByEmailAsync("nguyenhoanganh@stjobs.com")
                    ?? throw new Exception("Hoang Anh user not found");
                var thiMinh = await userManager.FindByEmailAsync("tranthiminh@stjobs.com")
                    ?? throw new Exception("Thi Minh user not found");
                var ducTrung = await userManager.FindByEmailAsync("leductrung@stjobs.com")
                    ?? throw new Exception("Duc Trung user not found");

                // Get existing business users
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
                
                // Create a few addresses for projects
                var projectAddresses = new List<Address>();
                
                // Sample location data for projects
                var sampleLocations = new[]
                {
                    (Code: "1", Name: "Hà Nội", Districts: new[]
                    {
                        (Code: "001", Name: "Quận Ba Đình", Wards: new[] 
                        {
                            (Code: "00001", Name: "Phường Phúc Xá"),
                            (Code: "00004", Name: "Phường Trúc Bạch")
                        }),
                        (Code: "002", Name: "Quận Hoàn Kiếm", Wards: new[] 
                        {
                            (Code: "00037", Name: "Phường Hàng Bạc"),
                            (Code: "00040", Name: "Phường Hàng Bồ")
                        })
                    }),
                    (Code: "79", Name: "TP Hồ Chí Minh", Districts: new[]
                    {
                        (Code: "760", Name: "Quận 1", Wards: new[] 
                        {
                            (Code: "26734", Name: "Phường Bến Nghé"),
                            (Code: "26737", Name: "Phường Bến Thành")
                        }),
                        (Code: "761", Name: "Quận 3", Wards: new[] 
                        {
                            (Code: "26767", Name: "Phường 1"),
                            (Code: "26770", Name: "Phường 2")
                        })
                    }),
                    (Code: "48", Name: "Đà Nẵng", Districts: new[]
                    {
                        (Code: "490", Name: "Quận Hải Châu", Wards: new[] 
                        {
                            (Code: "20194", Name: "Phường Hải Châu 1"),
                            (Code: "20195", Name: "Phường Hải Châu 2")
                        }),
                        (Code: "491", Name: "Quận Thanh Khê", Wards: new[] 
                        {
                            (Code: "20227", Name: "Phường Tam Thuận"),
                            (Code: "20230", Name: "Phường Thanh Khê Tây")
                        })
                    })
                };
                
                for (int i = 0; i < 15; i++)
                {
                    var random = new Random();
                    
                    // Select random province
                    var provinceIndex = random.Next(sampleLocations.Length);
                    var province = sampleLocations[provinceIndex];
                    
                    // Select random district
                    var districtIndex = random.Next(province.Districts.Length);
                    var district = province.Districts[districtIndex];
                    
                    // Select random ward
                    var wardIndex = random.Next(district.Wards.Length);
                    var ward = district.Wards[wardIndex];
                    
                    var address = new Address
                    {
                        ProvinceCode = province.Code,
                        ProvinceName = province.Name,
                        DistrictCode = district.Code,
                        DistrictName = district.Name,
                        WardCode = ward.Code,
                        WardName = ward.Name,
                        DetailAddress = $"Địa chỉ dự án {i+1}",
                        FullAddress = $"Địa chỉ dự án {i+1}, {ward.Name}, {district.Name}, {province.Name}",
                        IsActive = true
                    };
                    
                    context.Addresses.Add(address);
                    context.SaveChanges();
                    projectAddresses.Add(address);
                }
                
                var projects = new List<Project>
                {
                    // Web Development Projects
                    new Project
                    {
                        Title = "Phát triển website bán hàng",
                        Description = "Chúng tôi đang tìm kiếm lập trình viên có kinh nghiệm về .NET và React để phát triển một website thương mại điện tử hoàn chỉnh. Dự án này là một phần trong chiến lược chuyển đổi số của công ty, nhằm mở rộng kênh bán hàng trực tuyến.\n\n" +
                            "Yêu cầu kỹ thuật:\n" +
                            "- Phát triển backend sử dụng .NET Core với kiến trúc microservices\n" +
                            "- Xây dựng frontend sử dụng React với Redux để quản lý state\n" +
                            "- Tích hợp thanh toán với các cổng như VNPay, Momo, ZaloPay\n" +
                            "- Xây dựng hệ thống quản lý kho hàng real-time\n" +
                            "- Tối ưu hiệu năng và khả năng mở rộng của hệ thống\n\n" +
                            "Tính năng chính cần phát triển:\n" +
                            "1. Hệ thống quản lý sản phẩm đa cấp với hỗ trợ SEO\n" +
                            "2. Giỏ hàng thông minh với tính năng lưu và khôi phục\n" +
                            "3. Thanh toán trực tuyến đa phương thức\n" +
                            "4. Quản lý đơn hàng với tracking real-time\n" +
                            "5. Hệ thống khuyến mãi và mã giảm giá linh hoạt\n" +
                            "6. Tích hợp chatbot hỗ trợ khách hàng\n" +
                            "7. Dashboard phân tích dữ liệu bán hàng\n\n" +
                            "Yêu cầu về bảo mật:\n" +
                            "- Tuân thủ các tiêu chuẩn bảo mật trong thanh toán\n" +
                            "- Mã hóa dữ liệu người dùng\n" +
                            "- Phòng chống tấn công CSRF, XSS\n" +
                            "- Logging và monitoring hệ thống\n\n" +
                            "Kinh nghiệm cần thiết:\n" +
                            "- Tối thiểu 3 năm kinh nghiệm với .NET\n" +
                            "- 2 năm kinh nghiệm với React\n" +
                            "- Đã từng tham gia ít nhất 1 dự án thương mại điện tử\n" +
                            "- Hiểu biết về cloud infrastructure và CI/CD",
                        Budget = 50000000,
                        ProjectWallet = 50000000,
                        StartDate = new DateTime(2025, 6, 22),
                        EndDate = new DateTime(2025, 9, 13),
                        BusinessID = businesses[0].Id,
                        CategoryID = webCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = fullTimeType.TypeID,
                        AddressID = projectAddresses[0].AddressID,
                        IsActive = true
                    },
                    new Project
                    {
                        Title = "Xây dựng blog cá nhân chuyên nghiệp",
                        Description = "Chúng tôi đang tìm kiếm một freelancer có khả năng thiết kế và phát triển một blog cá nhân chuyên nghiệp. Dự án này là cho một nhà văn và nhà báo tự do, với mục tiêu xây dựng một nền tảng trực tuyến để chia sẻ nội dung và xây dựng thương hiệu cá nhân.\n\n" +
                            "Mục tiêu của dự án:\n" +
                            "1. Tạo một platform chuyên nghiệp để trình bày portfolio\n" +
                            "2. Tối ưu hóa khả năng tiếp cận độc giả\n" +
                            "3. Xây dựng cộng đồng người đọc trung thành\n\n" +
                            "Yêu cầu kỹ thuật:\n" +
                            "- Sử dụng WordPress làm nền tảng chính\n" +
                            "- Theme tùy chỉnh hoàn toàn với Gutenberg support\n" +
                            "- Tối ưu hóa tốc độ tải trang\n" +
                            "- Responsive design cho mọi thiết bị\n\n" +
                            "Tính năng cần có:\n" +
                            "- Trang chủ động với featured posts\n" +
                            "- Hệ thống phân loại và tag thông minh\n" +
                            "- Tích hợp newsletter và form đăng ký\n" +
                            "- Bình luận và tương tác xã hội\n" +
                            "- Gallery hình ảnh chuyên nghiệp\n" +
                            "- Trang portfolio với layout sáng tạo\n\n" +
                            "Yêu cầu SEO:\n" +
                            "- Cấu trúc URL thân thiện\n" +
                            "- Tối ưu hóa meta tags và schema markup\n" +
                            "- Tích hợp Google Analytics và Search Console\n" +
                            "- Sitemap tự động\n" +
                            "- Tối ưu hóa hình ảnh và nội dung\n\n" +
                            "Kinh nghiệm yêu cầu:\n" +
                            "- Thành thạo WordPress development\n" +
                            "- Hiểu biết sâu về SEO và content marketing\n" +
                            "- Portfolio các dự án blog tương tự",
                        Budget = 5000000,
                        ProjectWallet = 5000000,
                        StartDate = new DateTime(2025, 7, 1),
                        EndDate = new DateTime(2025, 7, 30),
                        BusinessID = businesses[1].Id,
                        CategoryID = webCategory.CategoryID,
                        StatusID = inProgressStatus.StatusID,
                        TypeID = projectBasedType.TypeID,
                        AddressID = projectAddresses[1].AddressID,
                        IsActive = true
                    },
                    new Project
                    {
                        Title = "Nâng cấp hệ thống quản lý nội bộ",
                        Description = "Dự án này nhằm nâng cấp và mở rộng hệ thống quản lý nội bộ hiện tại của công ty, tập trung vào việc cải thiện hiệu suất làm việc và khả năng phân tích dữ liệu. Hệ thống hiện tại đã được sử dụng trong 2 năm và cần được hiện đại hóa để đáp ứng nhu cầu ngày càng tăng của công ty.\n\n" +
                            "Hiện trạng hệ thống:\n" +
                            "- Xây dựng trên Laravel 8.0\n" +
                            "- Frontend sử dụng Vue.js 2.0\n" +
                            "- Database: MySQL\n" +
                            "- Đang phục vụ khoảng 200 người dùng\n\n" +
                            "Yêu cầu nâng cấp:\n" +
                            "1. Cập nhật framework:\n" +
                            "   - Nâng cấp lên Laravel 10.x\n" +
                            "   - Chuyển đổi sang Vue.js 3 với Composition API\n" +
                            "   - Tối ưu hóa database schema\n\n" +
                            "2. Tính năng mới cần phát triển:\n" +
                            "   - Hệ thống báo cáo động với biểu đồ tương tác\n" +
                            "   - Dashboard tùy chỉnh cho từng phòng ban\n" +
                            "   - Export dữ liệu đa định dạng (PDF, Excel, CSV)\n" +
                            "   - Tích hợp notification system realtime\n" +
                            "   - Module quản lý tài sản và resources\n\n" +
                            "3. Cải thiện hiệu năng:\n" +
                            "   - Tối ưu hóa queries và indexing\n" +
                            "   - Implement caching strategy\n" +
                            "   - Lazy loading cho components\n" +
                            "   - API rate limiting và optimization\n\n" +
                            "4. Yêu cầu bảo mật:\n" +
                            "   - Implement 2FA\n" +
                            "   - Role-based access control chi tiết\n" +
                            "   - Audit logging cho mọi thao tác\n" +
                            "   - Mã hóa dữ liệu nhạy cảm\n\n" +
                            "Yêu cầu ứng viên:\n" +
                            "- Thành thạo Laravel và Vue.js\n" +
                            "- Kinh nghiệm với hệ thống enterprise\n" +
                            "- Hiểu biết về optimization và scaling\n" +
                            "- Kỹ năng refactoring code tốt",
                        Budget = 35000000,
                        ProjectWallet = 35000000,
                        StartDate = new DateTime(2025, 5, 15),
                        EndDate = new DateTime(2025, 8, 15),
                        BusinessID = businesses[2].Id,
                        CategoryID = webCategory.CategoryID,
                        StatusID = completedStatus.StatusID,
                        TypeID = fullTimeType.TypeID,
                        AddressID = projectAddresses[2].AddressID,
                        IsActive = true
                    },
                    
                    // Mobile Development Projects
                    new Project
                    {
                        Title = "Phát triển ứng dụng di động đặt đồ ăn",
                        Description = "Dự án phát triển ứng dụng di động đặt đồ ăn toàn diện cho cả nền tảng iOS và Android, nhằm tạo ra một hệ sinh thái kết nối nhà hàng và người dùng. Ứng dụng sẽ mang đến trải nghiệm đặt đồ ăn mượt mà và thuận tiện cho người dùng, đồng thời cung cấp công cụ quản lý hiệu quả cho nhà hàng.\n\n" +
                            "Tính năng cho người dùng:\n" +
                            "1. Tìm kiếm và khám phá:\n" +
                            "   - Tìm kiếm thông minh với bộ lọc đa dạng\n" +
                            "   - Gợi ý nhà hàng dựa trên vị trí và sở thích\n" +
                            "   - Xem menu với hình ảnh chất lượng cao\n" +
                            "   - Đánh giá và bình luận về món ăn\n\n" +
                            "2. Đặt hàng và thanh toán:\n" +
                            "   - Giỏ hàng thông minh với gợi ý món ăn kèm\n" +
                            "   - Nhiều phương thức thanh toán (VNPay, Momo, COD)\n" +
                            "   - Tích hợp mã giảm giá và loyalty points\n" +
                            "   - Lịch sử đơn hàng và đặt lại nhanh\n\n" +
                            "3. Theo dõi đơn hàng:\n" +
                            "   - Tracking realtime vị trí người giao hàng\n" +
                            "   - Thông báo push khi có cập nhật\n" +
                            "   - Chat với người giao hàng\n" +
                            "   - Đánh giá sau khi nhận hàng\n\n" +
                            "Tính năng cho nhà hàng:\n" +
                            "1. Quản lý đơn hàng:\n" +
                            "   - Dashboard theo dõi đơn hàng realtime\n" +
                            "   - Quản lý menu và tồn kho\n" +
                            "   - Báo cáo doanh thu và analytics\n" +
                            "   - Quản lý đánh giá và phản hồi\n\n" +
                            "Yêu cầu kỹ thuật:\n" +
                            "- Native development cho iOS (Swift) và Android (Kotlin)\n" +
                            "- Backend với microservices architecture\n" +
                            "- Real-time tracking với WebSocket\n" +
                            "- Optimize performance và battery usage\n" +
                            "- Offline support với local caching\n\n" +
                            "Yêu cầu ứng viên:\n" +
                            "- Kinh nghiệm phát triển app native\n" +
                            "- Hiểu biết về UX/UI mobile\n" +
                            "- Kinh nghiệm với realtime features\n" +
                            "- Khả năng tối ưu hiệu năng ứng dụng",
                        Budget = 70000000,
                        ProjectWallet = 70000000,
                        StartDate = new DateTime(2025, 7, 10),
                        EndDate = new DateTime(2025, 10, 30),
                        BusinessID = businesses[3].Id,
                        CategoryID = mobileCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = fullTimeType.TypeID,
                        AddressID = projectAddresses[3].AddressID,
                        IsActive = true
                    },
                    new Project
                    {
                        Title = "Ứng dụng theo dõi sức khỏe",
                        Description = "Phát triển ứng dụng di động toàn diện về theo dõi và quản lý sức khỏe cá nhân, tích hợp với các thiết bị đeo thông minh và cung cấp những thông tin chi tiết về sức khỏe người dùng. Ứng dụng tập trung vào tính dễ sử dụng và khả năng cá nhân hóa cao.\n\n" +
                            "Tính năng chính:\n" +
                            "1. Theo dõi hoạt động thể chất:\n" +
                            "   - Đếm bước chân và quãng đường đi bộ\n" +
                            "   - Theo dõi các bài tập cardio\n" +
                            "   - Đo cường độ hoạt động\n" +
                            "   - Tính toán calories tiêu thụ\n\n" +
                            "2. Theo dõi chỉ số sức khỏe:\n" +
                            "   - Nhịp tim và biến thiên nhịp tim\n" +
                            "   - Chất lượng giấc ngủ\n" +
                            "   - Mức độ stress\n" +
                            "   - Đo SpO2 (nếu thiết bị hỗ trợ)\n\n" +
                            "3. Quản lý dinh dưỡng:\n" +
                            "   - Nhật ký thực phẩm với cơ sở dữ liệu phong phú\n" +
                            "   - Tính toán calories và chất dinh dưỡng\n" +
                            "   - Lập kế hoạch bữa ăn\n" +
                            "   - Gợi ý thực đơn cân bằng\n\n" +
                            "4. Tính năng thông minh:\n" +
                            "   - AI phân tích xu hướng sức khỏe\n" +
                            "   - Cảnh báo sớm các vấn đề sức khỏe\n" +
                            "   - Gợi ý tập luyện cá nhân hóa\n" +
                            "   - Báo cáo sức khỏe định kỳ\n\n" +
                            "Yêu cầu kỹ thuật:\n" +
                            "- Phát triển bằng Flutter cho cross-platform\n" +
                            "- Tích hợp HealthKit (iOS) và Google Fit (Android)\n" +
                            "- Backend với ML cho phân tích dữ liệu\n" +
                            "- Bảo mật dữ liệu theo HIPAA\n\n" +
                            "Yêu cầu ứng viên:\n" +
                            "- Kinh nghiệm với Flutter và native health APIs\n" +
                            "- Hiểu biết về y tế và sức khỏe\n" +
                            "- Khả năng xử lý dữ liệu lớn\n" +
                            "- Kinh nghiệm với wearable devices",
                        Budget = 45000000,
                        ProjectWallet = 45000000,
                        StartDate = new DateTime(2025, 6, 15),
                        EndDate = new DateTime(2025, 9, 15),
                        BusinessID = businesses[4].Id,
                        CategoryID = mobileCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = partTimeType.TypeID,
                        AddressID = projectAddresses[4].AddressID,
                        IsActive = true
                    },
                    
                    // Data Science Projects
                    new Project
                    {
                        Title = "Phân tích dữ liệu khách hàng",
                        Description = "Dự án phân tích dữ liệu khách hàng chuyên sâu nhằm tối ưu hóa chiến lược kinh doanh và cải thiện trải nghiệm khách hàng. Dự án sẽ sử dụng các kỹ thuật phân tích dữ liệu tiên tiến và machine learning để tạo ra insights có giá trị cho doanh nghiệp.\n\n" +
                            "Phạm vi dự án:\n" +
                            "1. Phân tích hành vi khách hàng:\n" +
                            "   - Pattern mua sắm và sử dụng sản phẩm\n" +
                            "   - Phân tích customer journey\n" +
                            "   - Segmentation khách hàng\n" +
                            "   - Lifetime value prediction\n\n" +
                            "2. Xây dựng mô hình dự đoán:\n" +
                            "   - Dự đoán khả năng churn\n" +
                            "   - Recommendation system\n" +
                            "   - Dự báo nhu cầu mua hàng\n" +
                            "   - Price optimization\n\n" +
                            "3. Phân tích Marketing:\n" +
                            "   - Attribution modeling\n" +
                            "   - Campaign effectiveness\n" +
                            "   - ROI analysis\n" +
                            "   - Customer acquisition cost\n\n" +
                            "4. Báo cáo và Visualization:\n" +
                            "   - Interactive dashboards\n" +
                            "   - Automated reporting system\n" +
                            "   - Real-time monitoring\n" +
                            "   - Custom KPI tracking\n\n" +
                            "Yêu cầu kỹ thuật:\n" +
                            "- Python ecosystem (pandas, sklearn, TensorFlow)\n" +
                            "- SQL và data warehousing\n" +
                            "- Visualization tools (Tableau, PowerBI)\n" +
                            "- Cloud computing (AWS/GCP)\n\n" +
                            "Deliverables:\n" +
                            "1. Data preprocessing pipeline\n" +
                            "2. ML models với documentation\n" +
                            "3. Interactive dashboards\n" +
                            "4. API cho real-time prediction\n" +
                            "5. Báo cáo phân tích chi tiết\n\n" +
                            "Yêu cầu ứng viên:\n" +
                            "- Master/PhD trong Data Science\n" +
                            "- 3+ năm kinh nghiệm phân tích dữ liệu\n" +
                            "- Thành thạo ML và statistical modeling\n" +
                            "- Kỹ năng truyền đạt phân tích tốt",
                        Budget = 40000000,
                        ProjectWallet = 40000000,
                        StartDate = new DateTime(2025, 7, 5),
                        EndDate = new DateTime(2025, 8, 30),
                        BusinessID = businesses[0].Id,
                        CategoryID = dataCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = projectBasedType.TypeID,
                        AddressID = projectAddresses[5].AddressID,
                        IsActive = true
                    },
                    
                    // UI/UX Design Projects
                    new Project
                    {
                        Title = "Thiết kế lại giao diện website công ty",
                        Description = "Dự án redesign toàn diện website công ty, chuyển đổi từ giao diện cũ sang một thiết kế hiện đại, tối giản và thân thiện với người dùng. Dự án nhằm cải thiện trải nghiệm người dùng, tăng tỷ lệ chuyển đổi và thể hiện đúng giá trị thương hiệu của công ty.\n\n" +
                            "Phạm vi công việc:\n\n" +
                            "1. Research & Planning:\n" +
                            "   - Phân tích website hiện tại\n" +
                            "   - Nghiên cứu đối thủ cạnh tranh\n" +
                            "   - Khảo sát người dùng mục tiêu\n" +
                            "   - Xác định user personas\n" +
                            "   - Lập kế hoạch cải thiện UX\n\n" +
                            "2. Information Architecture:\n" +
                            "   - Tổ chức lại cấu trúc website\n" +
                            "   - Thiết kế user flow mới\n" +
                            "   - Tối ưu hóa navigation\n" +
                            "   - Cải thiện content hierarchy\n\n" +
                            "3. UI Design:\n" +
                            "   - Design system hoàn chỉnh\n" +
                            "   - Typography và color scheme\n" +
                            "   - Component library\n" +
                            "   - Responsive design cho mọi thiết bị\n" +
                            "   - Dark mode (tùy chọn)\n\n" +
                            "4. Tương tác và Animation:\n" +
                            "   - Micro-interactions\n" +
                            "   - Page transitions\n" +
                            "   - Loading states\n" +
                            "   - Scroll animations\n\n" +
                            "5. Prototype và Testing:\n" +
                            "   - Interactive prototypes\n" +
                            "   - Usability testing\n" +
                            "   - A/B testing proposals\n" +
                            "   - Performance optimization\n\n" +
                            "Deliverables:\n" +
                            "1. Brand style guide cập nhật\n" +
                            "2. UI kit đầy đủ\n" +
                            "3. Prototype cho mọi màn hình\n" +
                            "4. Documentation chi tiết\n" +
                            "5. Design handoff cho developers\n\n" +
                            "Yêu cầu ứng viên:\n" +
                            "- Portfolio thiết kế ấn tượng\n" +
                            "- Thành thạo Figma hoặc Adobe XD\n" +
                            "- Kinh nghiệm với design systems\n" +
                            "- Hiểu biết về web development\n" +
                            "- Kỹ năng collaboration tốt",
                        Budget = 25000000,
                        ProjectWallet = 25000000,
                        StartDate = new DateTime(2025, 6, 10),
                        EndDate = new DateTime(2025, 7, 25),
                        BusinessID = businesses[1].Id,
                        CategoryID = uiuxCategory.CategoryID,
                        StatusID = inProgressStatus.StatusID,
                        TypeID = partTimeType.TypeID,
                        AddressID = projectAddresses[6].AddressID,
                        IsActive = true
                    },
                    
                    // Social Media Projects
                    new Project
                    {
                        Title = "Quản lý fanpage và chạy quảng cáo",
                        Description = "Tìm kiếm chuyên gia social media marketing để quản lý và phát triển toàn diện fanpage Facebook của công ty, bao gồm xây dựng chiến lược nội dung, tương tác cộng đồng và triển khai chiến dịch quảng cáo hiệu quả.\n\n" +
                            "1. Quản lý Nội dung & Cộng đồng:\n" +
                            "   - Lập kế hoạch nội dung hàng tháng\n" +
                            "   - Sáng tạo content đa dạng (text, hình ảnh, video)\n" +
                            "   - Thiết kế content calendar\n" +
                            "   - Tương tác và phản hồi comment\n" +
                            "   - Xây dựng cộng đồng người theo dõi\n" +
                            "   - Tổ chức mini-game và event\n\n" +
                            "2. Quảng cáo Facebook:\n" +
                            "   - Phân tích và xây dựng target audience\n" +
                            "   - Thiết kế creatives cho quảng cáo\n" +
                            "   - Tối ưu hóa campaign structure\n" +
                            "   - A/B testing các yếu tố quảng cáo\n" +
                            "   - Quản lý ngân sách quảng cáo\n" +
                            "   - Retargeting và remarketing\n\n" +
                            "3. Analytics & Báo cáo:\n" +
                            "   - Theo dõi các chỉ số KPI chính\n" +
                            "   - Phân tích insight và đề xuất cải thiện\n" +
                            "   - Báo cáo hiệu quả định kỳ\n" +
                            "   - ROI analysis cho mỗi campaign\n\n" +
                            "4. Chiến lược Phát triển:\n" +
                            "   - Nghiên cứu thị trường và đối thủ\n" +
                            "   - Xác định cơ hội tăng trưởng\n" +
                            "   - Đề xuất chiến lược mới\n" +
                            "   - Tối ưu hóa conversion rate\n\n" +
                            "Yêu cầu Ứng viên:\n" +
                            "- 2+ năm kinh nghiệm social media\n" +
                            "- Chứng chỉ Facebook Blueprint\n" +
                            "- Portfolio các campaign thành công\n" +
                            "- Kỹ năng copywriting tốt\n" +
                            "- Có con mắt thẩm mỹ\n\n" +
                            "KPIs:\n" +
                            "- Tăng trưởng follower: 20%/tháng\n" +
                            "- Engagement rate: >5%\n" +
                            "- Response time: <2 giờ\n" +
                            "- ROAS cho quảng cáo: >200%",
                        Budget = 10000000,
                        ProjectWallet = 10000000,
                        StartDate = new DateTime(2025, 7, 1),
                        EndDate = new DateTime(2025, 12, 31),
                        BusinessID = businesses[2].Id,
                        CategoryID = socialMediaCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = partTimeType.TypeID,
                        AddressID = projectAddresses[7].AddressID,
                        IsActive = true
                    },
                    
                    // Content Writing Projects
                    new Project
                    {
                        Title = "Viết nội dung blog về công nghệ",
                        Description = "Tìm kiếm content writer chuyên nghiệp để sáng tạo series bài viết chuyên sâu về công nghệ cho blog công ty. Dự án nhằm xây dựng nguồn nội dung chất lượng cao, cung cấp thông tin giá trị cho độc giả và tăng cường SEO cho website.\n\n" +
                            "Phạm vi công việc:\n\n" +
                            "1. Nội dung yêu cầu:\n" +
                            "   - 20 bài viết chuyên sâu\n" +
                            "   - Độ dài: 1000-1500 từ/bài\n" +
                            "   - Định dạng tối ưu cho SEO\n" +
                            "   - Hình ảnh và infographic minh họa\n\n" +
                            "2. Chủ đề chính:\n" +
                            "   a) Trí tuệ nhân tạo:\n" +
                            "      - Machine Learning applications\n" +
                            "      - AI trong doanh nghiệp\n" +
                            "      - Neural Networks và Deep Learning\n" +
                            "      - Computer Vision và NLP\n\n" +
                            "   b) Blockchain & Crypto:\n" +
                            "      - Smart Contracts\n" +
                            "      - DeFi và Web3\n" +
                            "      - NFTs và metaverse\n" +
                            "      - Blockchain trong enterprise\n\n" +
                            "   c) Công nghệ 5G:\n" +
                            "      - Infrastructure và deployment\n" +
                            "      - Use cases trong IoT\n" +
                            "      - Impact lên mobile computing\n" +
                            "      - Security considerations\n\n" +
                            "3. Yêu cầu chất lượng:\n" +
                            "   - Research kỹ lưỡng và fact-checking\n" +
                            "   - Trích dẫn nguồn uy tín\n" +
                            "   - Tone of voice chuyên nghiệp\n" +
                            "   - Cấu trúc bài viết rõ ràng\n" +
                            "   - Keywords optimization\n\n" +
                            "4. Quy trình làm việc:\n" +
                            "   - Đề xuất outline cho mỗi bài\n" +
                            "   - Draft và revision cycles\n" +
                            "   - Chỉnh sửa theo feedback\n" +
                            "   - Tối ưu hóa meta descriptions\n\n" +
                            "Yêu cầu ứng viên:\n" +
                            "- Background về công nghệ\n" +
                            "- Portfolio bài viết tech\n" +
                            "- Kỹ năng SEO content writing\n" +
                            "- Khả năng giải thích technical concepts\n" +
                            "- Tiếng Anh tốt để đọc tài liệu",
                        Budget = 15000000,
                        ProjectWallet = 15000000,
                        StartDate = new DateTime(2025, 7, 15),
                        EndDate = new DateTime(2025, 9, 15),
                        BusinessID = businesses[3].Id,
                        CategoryID = contentWritingCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = projectBasedType.TypeID,
                        AddressID = projectAddresses[8].AddressID,
                        IsActive = true
                    },
                    
                    // Translation Projects
                    new Project
                    {
                        Title = "Dịch tài liệu kỹ thuật phần mềm",
                        Description = "Tìm kiếm dịch giả chuyên nghiệp để dịch bộ tài liệu kỹ thuật về phần mềm từ tiếng Anh sang tiếng Việt. Dự án yêu cầu sự kết hợp giữa kỹ năng dịch thuật và hiểu biết về công nghệ để đảm bảo tính chính xác và nhất quán của bản dịch.\n\n" +
                            "Thông tin tài liệu:\n" +
                            "1. Khối lượng và nội dung:\n" +
                            "   - Tổng cộng: 50 trang\n" +
                            "   - Loại tài liệu: Technical documentation\n" +
                            "   - Chủ đề: Enterprise software\n" +
                            "   - Độ phức tạp: Cao\n\n" +
                            "2. Các phần chính cần dịch:\n" +
                            "   - System architecture (10 trang)\n" +
                            "   - API documentation (15 trang)\n" +
                            "   - Deployment guide (15 trang)\n" +
                            "   - User manual (10 trang)\n\n" +
                            "3. Yêu cầu chất lượng:\n" +
                            "   - Tính chính xác kỹ thuật cao\n" +
                            "   - Nhất quán về thuật ngữ\n" +
                            "   - Đảm bảo format và styling\n" +
                            "   - Giữ nguyên cấu trúc văn bản\n\n" +
                            "4. Quy trình làm việc:\n" +
                            "   - Review và làm rõ source content\n" +
                            "   - Tạo glossary thuật ngữ\n" +
                            "   - Dịch thử 2 trang mẫu\n" +
                            "   - Phân đoạn công việc theo tuần\n" +
                            "   - QA và review định kỳ\n\n" +
                            "5. Công cụ và tài nguyên:\n" +
                            "   - CAT tools (SDL Trados/MemoQ)\n" +
                            "   - Term base có sẵn\n" +
                            "   - Style guide\n" +
                            "   - Reference materials\n\n" +
                            "Yêu cầu ứng viên:\n" +
                            "- Kinh nghiệm dịch tài liệu kỹ thuật\n" +
                            "- Background IT/công nghệ\n" +
                            "- Thành thạo CAT tools\n" +
                            "- Portfolio dịch thuật tương tự\n" +
                            "- Kỹ năng quản lý thời gian tốt",
                        Budget = 8000000,
                        ProjectWallet = 8000000,
                        StartDate = new DateTime(2025, 8, 1),
                        EndDate = new DateTime(2025, 8, 31),
                        BusinessID = businesses[4].Id,
                        CategoryID = translationCategory.CategoryID,
                        StatusID = recruitingStatus.StatusID,
                        TypeID = projectBasedType.TypeID,
                        AddressID = projectAddresses[9].AddressID,
                        IsActive = true
                    },

                    // New Projects for New Businesses and Students
                    
// TechVision Projects (Completed with Hoang Anh)
new Project
{
    Title = "Xây dựng Website Thương mại Điện tử",
    Description = "Dự án phát triển một website thương mại điện tử toàn diện sử dụng .NET Core và React, nhằm tạo ra một nền tảng bán hàng trực tuyến hiện đại, hiệu quả và dễ sử dụng. Website sẽ bao gồm các tính năng quản lý sản phẩm đa dạng, giỏ hàng thông minh, thanh toán trực tuyến an toàn và hệ thống quản lý đơn hàng real-time.\n\n" +
                  "Mục tiêu dự án:\n" +
                  "- Xây dựng backend ổn định với kiến trúc microservices sử dụng .NET Core\n" +
                  "- Phát triển frontend tương tác cao với React và Redux\n" +
                  "- Tích hợp các cổng thanh toán phổ biến như VNPay, Momo, ZaloPay\n" +
                  "- Tối ưu hóa hiệu năng và khả năng mở rộng của hệ thống\n\n" +
                  "Yêu cầu kỹ thuật:\n" +
                  "- Kinh nghiệm phát triển ứng dụng thương mại điện tử\n" +
                  "- Hiểu biết về bảo mật và mã hóa dữ liệu\n" +
                  "- Kỹ năng làm việc với API và hệ thống thanh toán\n\n" +
                  "Tính năng chính:\n" +
                  "1. Quản lý sản phẩm với phân loại đa cấp và hỗ trợ SEO\n" +
                  "2. Giỏ hàng với tính năng lưu trữ và khôi phục\n" +
                  "3. Thanh toán đa phương thức và bảo mật cao\n" +
                  "4. Quản lý đơn hàng với tracking thời gian thực\n" +
                  "5. Hệ thống khuyến mãi và mã giảm giá linh hoạt\n" +
                  "6. Dashboard phân tích dữ liệu bán hàng\n\n" +
                  "Kinh nghiệm yêu cầu:\n" +
                  "- Ít nhất 3 năm kinh nghiệm với .NET Core\n" +
                  "- 2 năm kinh nghiệm với React\n" +
                  "- Đã tham gia các dự án thương mại điện tử quy mô\n",
    Budget = 25000000,
    ProjectWallet = 25000000,
    StartDate = new DateTime(2025, 5, 1),
    EndDate = new DateTime(2025, 6, 30),
    BusinessID = techvision.Id,
    CategoryID = webCategory.CategoryID,
    StatusID = completedStatus.StatusID,
    TypeID = projectBasedType.TypeID,
    AddressID = projectAddresses[10].AddressID,
    IsActive = true
},

// DigiMark Projects (In Progress with Thi Minh)
new Project
{
    Title = "Xây dựng Hệ thống Phân tích Dữ liệu",
    Description = "Dự án phát triển hệ thống phân tích dữ liệu chuyên sâu sử dụng Python và các thuật toán Machine Learning nhằm tối ưu hóa chiến lược kinh doanh và nâng cao trải nghiệm khách hàng. Hệ thống sẽ cung cấp các báo cáo phân tích, dự đoán xu hướng và hỗ trợ ra quyết định dựa trên dữ liệu lớn.\n\n" +
                  "Phạm vi dự án:\n" +
                  "- Thu thập và xử lý dữ liệu từ nhiều nguồn khác nhau\n" +
                  "- Xây dựng pipeline tiền xử lý dữ liệu tự động\n" +
                  "- Phát triển các mô hình dự đoán và phân loại\n" +
                  "- Tích hợp dashboard tương tác để hiển thị kết quả phân tích\n\n" +
                  "Yêu cầu kỹ thuật:\n" +
                  "- Thành thạo Python và các thư viện như pandas, scikit-learn, TensorFlow\n" +
                  "- Kinh nghiệm với data warehousing và SQL\n" +
                  "- Kỹ năng trực quan hóa dữ liệu với Tableau hoặc PowerBI\n\n" +
                  "Tính năng chính:\n" +
                  "1. Phân tích hành vi khách hàng và phân khúc thị trường\n" +
                  "2. Mô hình dự đoán churn và recommendation system\n" +
                  "3. Báo cáo ROI và hiệu quả chiến dịch marketing\n" +
                  "4. Hệ thống cảnh báo sớm và đề xuất chiến lược\n\n" +
                  "Kinh nghiệm yêu cầu:\n" +
                  "- 3 năm kinh nghiệm trong lĩnh vực phân tích dữ liệu\n" +
                  "- Hiểu biết sâu về machine learning và statistical modeling\n" +
                  "- Kỹ năng truyền đạt kết quả phân tích hiệu quả\n",
    Budget = 40000000,
    ProjectWallet = 40000000,
    StartDate = new DateTime(2025, 6, 1),
    EndDate = new DateTime(2025, 9, 30),
    BusinessID = digimark.Id,
    CategoryID = dataCategory.CategoryID,
    StatusID = inProgressStatus.StatusID,
    TypeID = projectBasedType.TypeID,
    AddressID = projectAddresses[11].AddressID,
    IsActive = true
},

// UIX Studio Projects (In Progress with Duc Trung)
new Project
{
    Title = "Thiết kế UI/UX cho Ứng dụng Giáo dục",
    Description = "Dự án thiết kế giao diện người dùng cho nền tảng học trực tuyến với mục tiêu tạo ra trải nghiệm học tập thân thiện, trực quan và hiệu quả. Thiết kế sẽ tập trung vào việc tối ưu hóa user flow, responsive design và khả năng tương tác cao.\n\n" +
                  "Phạm vi công việc:\n" +
                  "- Nghiên cứu người dùng và phân tích yêu cầu\n" +
                  "- Thiết kế wireframes và prototypes tương tác\n" +
                  "- Xây dựng design system và component library\n" +
                  "- Tối ưu hóa trải nghiệm trên các thiết bị di động và desktop\n" +
                  "- Hỗ trợ testing và cải tiến dựa trên phản hồi người dùng\n\n" +
                  "Yêu cầu kỹ thuật:\n" +
                  "- Thành thạo các công cụ thiết kế như Figma, Adobe XD\n" +
                  "- Kinh nghiệm với design systems và UI kits\n" +
                  "- Hiểu biết về phát triển web và mobile\n\n" +
                  "Kinh nghiệm yêu cầu:\n" +
                  "- Portfolio thiết kế UI/UX ấn tượng\n" +
                  "- Kỹ năng collaboration và giao tiếp tốt\n" +
                  "- Khả năng làm việc độc lập và theo nhóm\n",
    Budget = 28000000,
    ProjectWallet = 28000000,
    StartDate = new DateTime(2025, 7, 1),
    EndDate = new DateTime(2025, 8, 31),
    BusinessID = uixstudio.Id,
    CategoryID = uiuxCategory.CategoryID,
    StatusID = inProgressStatus.StatusID,
    TypeID = projectBasedType.TypeID,
    AddressID = projectAddresses[12].AddressID,
    IsActive = true
},

// Fintech Projects (Recruiting)
new Project
{
    Title = "Phát triển Ứng dụng Quản lý Tài chính",
    Description = "Dự án phát triển ứng dụng quản lý tài chính cá nhân và doanh nghiệp với mục tiêu cung cấp công cụ quản lý chi tiêu, đầu tư và báo cáo tài chính hiệu quả. Ứng dụng sẽ hỗ trợ đa nền tảng và tích hợp các tính năng bảo mật cao.\n\n" +
                  "Tính năng chính:\n" +
                  "- Quản lý thu chi cá nhân và doanh nghiệp\n" +
                  "- Theo dõi đầu tư và danh mục tài sản\n" +
                  "- Báo cáo tài chính chi tiết và trực quan\n" +
                  "- Tích hợp các phương thức thanh toán và ngân hàng\n" +
                  "- Cảnh báo và nhắc nhở thông minh\n\n" +
                  "Yêu cầu kỹ thuật:\n" +
                  "- Phát triển đa nền tảng với Flutter hoặc native\n" +
                  "- Backend ổn định và bảo mật cao\n" +
                  "- Tích hợp API ngân hàng và thanh toán\n\n" +
                  "Kinh nghiệm yêu cầu:\n" +
                  "- Kinh nghiệm phát triển ứng dụng fintech\n" +
                  "- Hiểu biết về bảo mật và quy định tài chính\n" +
                  "- Kỹ năng tối ưu hiệu năng và UX\n",
    Budget = 35000000,
    ProjectWallet = 35000000,
    StartDate = new DateTime(2025, 8, 1),
    EndDate = new DateTime(2025, 10, 31),
    BusinessID = fintech.Id,
    CategoryID = webCategory.CategoryID,
    StatusID = recruitingStatus.StatusID,
    TypeID = projectBasedType.TypeID,
    AddressID = projectAddresses[13].AddressID,
    IsActive = true
},

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

                // Get the new students
                var hoangAnh = await userManager.FindByEmailAsync("nguyenhoanganh@stjobs.com") 
                    ?? throw new Exception("Student Hoang Anh not found");
                var thiMinh = await userManager.FindByEmailAsync("tranthiminh@stjobs.com")
                    ?? throw new Exception("Student Thi Minh not found");
                var ducTrung = await userManager.FindByEmailAsync("leductrung@stjobs.com")
                    ?? throw new Exception("Student Duc Trung not found");

                var applications = new List<StudentApplication>
                {
                    // Hoang Anh's completed applications
                    new StudentApplication
                    {
                        UserID = hoangAnh.Id,
                        ProjectID = projects[0].ProjectID, // Website Thương mại Điện tử
                        Status = "Completed",
                        CoverLetter = "Tôi có kinh nghiệm phát triển website với .NET Core và React, và đã từng làm nhiều dự án thương mại điện tử.",
                        Salary = 25000000,
                        DateApplied = DateTime.Now.AddDays(-180),
                        LastStatusUpdate = DateTime.Now.AddDays(-120),
                        Notes = "Dự án đã hoàn thành thành công",
                        ResumeLink = "https://stjobs.com/resume/hoanganh.pdf",
                        BusinessConfirmedCompletion = true,
                        StudentConfirmedCompletion = true,
                        IsActive = true
                    },
                    new StudentApplication
                    {
                        UserID = hoangAnh.Id,
                        ProjectID = projects[1].ProjectID, // Blog cá nhân
                        Status = "Completed",
                        CoverLetter = "Tôi đã có nhiều kinh nghiệm trong việc phát triển và tối ưu hóa WordPress.",
                        Salary = 5000000,
                        DateApplied = DateTime.Now.AddDays(-120),
                        LastStatusUpdate = DateTime.Now.AddDays(-90),
                        Notes = "Hoàn thành dự án đúng hạn, khách hàng hài lòng",
                        ResumeLink = "https://stjobs.com/resume/hoanganh.pdf",
                        BusinessConfirmedCompletion = true,
                        StudentConfirmedCompletion = true,
                        IsActive = true
                    },
                    new StudentApplication
                    {
                        UserID = hoangAnh.Id,
                        ProjectID = projects[2].ProjectID, // Nâng cấp hệ thống quản lý nội bộ
                        Status = "Completed",
                        CoverLetter = "Tôi thành thạo Laravel và VueJS, đã từng làm nhiều dự án tương tự.",
                        Salary = 35000000,
                        DateApplied = DateTime.Now.AddDays(-90),
                        LastStatusUpdate = DateTime.Now.AddDays(-60),
                        Notes = "Dự án hoàn thành xuất sắc, vượt kỳ vọng khách hàng",
                        ResumeLink = "https://stjobs.com/resume/hoanganh.pdf",
                        BusinessConfirmedCompletion = true,
                        StudentConfirmedCompletion = true,
                        IsActive = true
                    },
                    // Thi Minh's applications
                    new StudentApplication
                    {
                        UserID = thiMinh.Id,
                        ProjectID = projects[1].ProjectID, // Hệ thống Phân tích Dữ liệu
                        Status = "In Progress",
                        CoverLetter = "Tôi có kinh nghiệm với Python và Machine Learning, đã từng làm nhiều dự án phân tích dữ liệu.",
                        Salary = 40000000,
                        DateApplied = DateTime.Now.AddDays(-30),
                        LastStatusUpdate = DateTime.Now.AddDays(-25),
                        BusinessNotes = "Ứng viên có kiến thức sâu về ML và xử lý dữ liệu",
                        Notes = "Dự án đang được thực hiện tốt",
                        ResumeLink = "https://stjobs.com/resume/thiminh.pdf",
                        BusinessConfirmedCompletion = false,
                        StudentConfirmedCompletion = false,
                        IsActive = true
                    },
                    
                    // Duc Trung's applications
                    new StudentApplication
                    {
                        UserID = ducTrung.Id,
                        ProjectID = projects[2].ProjectID, // UI/UX cho Ứng dụng Giáo dục
                        Status = "In Progress",
                        CoverLetter = "Tôi có kinh nghiệm thiết kế UI/UX cho các ứng dụng giáo dục và thành thạo các công cụ thiết kế.",
                        Salary = 28000000,
                        DateApplied = DateTime.Now.AddDays(-20),
                        LastStatusUpdate = DateTime.Now.AddDays(-15),
                        BusinessNotes = "Ứng viên có portfolio ấn tượng và kinh nghiệm phù hợp",
                        Notes = "Dự án đang được thực hiện đúng tiến độ",
                        ResumeLink = "https://stjobs.com/resume/ductrung.pdf",
                        BusinessConfirmedCompletion = false,
                        StudentConfirmedCompletion = false,
                        IsActive = true
                    },
                    // Application for new recruiting project
                    new StudentApplication
                    {
                        UserID = thiMinh.Id,
                        ProjectID = projects[3].ProjectID, // Phát triển Ứng dụng Quản lý Tài chính
                        Status = "Pending",
                        CoverLetter = "Tôi đã có kinh nghiệm phát triển các ứng dụng fintech và rất quan tâm đến dự án này.",
                        Salary = 35000000,
                        DateApplied = DateTime.Now.AddDays(-2),
                        LastStatusUpdate = DateTime.Now.AddDays(-2),
                        BusinessNotes = "",
                        Notes = "Đang chờ phản hồi",
                        ResumeLink = "https://stjobs.com/resume/thiminh_latest.pdf",
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

            // Add email verification deadline for new users (BR-34)
            foreach (var user in context.Users.ToList())
            {
                if (user.EmailVerificationDeadline == null)
                {
                    user.EmailVerificationDeadline = user.CreatedAt.AddDays(1);
                    await userManager.UpdateAsync(user);
                }
            }

            // Seed some verified users
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            var moderatorUser = await userManager.FindByEmailAsync("moderator@example.com");
            var businessUser = await userManager.FindByEmailAsync("business@example.com");

            if (adminUser != null && !adminUser.IsVerified)
            {
                adminUser.IsVerified = true;
                adminUser.VerifiedAt = DateTime.Now.AddDays(-30);
                adminUser.VerifiedByID = adminUser.Id; // Self-verified
                await userManager.UpdateAsync(adminUser);
                
                // Add account action history
                context.UserAccountActions.Add(new UserAccountAction
                {
                    UserID = adminUser.Id,
                    ActionByID = adminUser.Id,
                    ActionType = "Verify",
                    Description = "Self-verification (admin)",
                    ActionDate = DateTime.Now.AddDays(-30),
                    IPAddress = "127.0.0.1",
                    UserAgent = "DbSeeder"
                });
            }

            if (moderatorUser != null && !moderatorUser.IsVerified)
            {
                moderatorUser.IsVerified = true;
                moderatorUser.VerifiedAt = DateTime.Now.AddDays(-25);
                moderatorUser.VerifiedByID = adminUser.Id;
                await userManager.UpdateAsync(moderatorUser);
                
                // Add account action history
                context.UserAccountActions.Add(new UserAccountAction
                {
                    UserID = moderatorUser.Id,
                    ActionByID = adminUser.Id,
                    ActionType = "Verify",
                    Description = "Verified by admin",
                    ActionDate = DateTime.Now.AddDays(-25),
                    IPAddress = "127.0.0.1",
                    UserAgent = "DbSeeder"
                });
            }

            if (businessUser != null && !businessUser.IsVerified)
            {
                businessUser.IsVerified = true;
                businessUser.VerifiedAt = DateTime.Now.AddDays(-20);
                businessUser.VerifiedByID = moderatorUser.Id;
                await userManager.UpdateAsync(businessUser);
                
                // Add account action history
                context.UserAccountActions.Add(new UserAccountAction
                {
                    UserID = businessUser.Id,
                    ActionByID = moderatorUser.Id,
                    ActionType = "Verify",
                    Description = "Verified by moderator",
                    ActionDate = DateTime.Now.AddDays(-20),
                    IPAddress = "127.0.0.1",
                    UserAgent = "DbSeeder"
                });
            }

            // Flag a sample user
            var suspiciousUser = await userManager.FindByEmailAsync("student5@example.com");
            if (suspiciousUser != null && !suspiciousUser.IsFlagged)
            {
                suspiciousUser.IsFlagged = true;
                suspiciousUser.FlagReason = "Suspicious activity detected";
                suspiciousUser.FlaggedAt = DateTime.Now.AddDays(-5);
                suspiciousUser.FlaggedByID = moderatorUser.Id;
                await userManager.UpdateAsync(suspiciousUser);
                
                // Add account action history
                context.UserAccountActions.Add(new UserAccountAction
                {
                    UserID = suspiciousUser.Id,
                    ActionByID = moderatorUser.Id,
                    ActionType = "Flag",
                    Description = "Suspicious activity detected",
                    ActionDate = DateTime.Now.AddDays(-5),
                    IPAddress = "127.0.0.1",
                    UserAgent = "DbSeeder"
                });
            }

            // Flag a sample project
            var projectToFlag = context.Projects.FirstOrDefault(p => p.Title.Contains("Viết nội dung blog"));
            if (projectToFlag != null)
            {
                projectToFlag.IsFlagged = true;
                projectToFlag.FlagReason = "Potential scam - unrealistic payment terms";
                projectToFlag.FlaggedAt = DateTime.Now.AddDays(-3);
                projectToFlag.FlaggedByID = moderatorUser.Id;
                
                // Add project flag history
                context.ProjectFlagActions.Add(new ProjectFlagAction
                {
                    ProjectID = projectToFlag.ProjectID,
                    ActionByID = moderatorUser.Id,
                    ActionType = "Flag",
                    Reason = "Potential scam - unrealistic payment terms",
                    ActionDate = DateTime.Now.AddDays(-3),
                    IPAddress = "127.0.0.1",
                    UserAgent = "DbSeeder",
                    IsActive = true
                });
                
                await context.SaveChangesAsync();
            }
            else
            {
                // If the specific project doesn't exist, try to flag any project
                var anyProject = context.Projects.FirstOrDefault();
                if (anyProject != null)
                {
                    anyProject.IsFlagged = true;
                    anyProject.FlagReason = "Sample flag for testing";
                    anyProject.FlaggedAt = DateTime.Now.AddDays(-3);
                    anyProject.FlaggedByID = moderatorUser.Id;
                    
                    // Add project flag history
                    context.ProjectFlagActions.Add(new ProjectFlagAction
                    {
                        ProjectID = anyProject.ProjectID,
                        ActionByID = moderatorUser.Id,
                        ActionType = "Flag",
                        Reason = "Sample flag for testing",
                        ActionDate = DateTime.Now.AddDays(-3),
                        IPAddress = "127.0.0.1",
                        UserAgent = "DbSeeder",
                        IsActive = true
                    });
                    
                    await context.SaveChangesAsync();
                }
            }
            
            // Fix any projects that might have IsFlagged=true but null FlagReason
            var flaggedProjectsWithNullReason = context.Projects.Where(p => p.IsFlagged && p.FlagReason == null).ToList();
            if (flaggedProjectsWithNullReason.Any())
            {
                foreach (var project in flaggedProjectsWithNullReason)
                {
                    project.FlagReason = "Default flag reason added by DbSeeder";
                    if (project.FlaggedAt == null)
                    {
                        project.FlaggedAt = DateTime.Now;
                    }
                    if (project.FlaggedByID == null && moderatorUser != null)
                    {
                        project.FlaggedByID = moderatorUser.Id;
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}