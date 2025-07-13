using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.ViewModels;
using System.Security.Claims;

namespace StudentFreelance.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILocationApiService _locationApiService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(
            ApplicationDbContext context, 
            IWebHostEnvironment env,
            ILocationApiService locationApiService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _locationApiService = locationApiService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Profile(int page = 1)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound();

                var skills = await _context.StudentSkills
                    .Where(s => s.UserID == userId && s.IsActive)
                    .Include(s => s.Skill)
                    .Include(s => s.ProficiencyLevel)
                    .ToListAsync();

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var roleId = 0;
                
                // Map role names to IDs: 3 for Business, 4 for Student
                if (roles.Contains("Business"))
                    roleId = 3;
                else if (roles.Contains("Student"))
                    roleId = 4;

                var viewModel = new UserProfileViewModel
                {
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    University = user.University,
                    Major = user.Major,
                    CompanyName = user.CompanyName,
                    Industry = user.Industry,
                    ProvinceCode = user.Address?.ProvinceCode,
                    ProvinceName = user.Address?.ProvinceName,
                    DistrictCode = user.Address?.DistrictCode,
                    DistrictName = user.Address?.DistrictName,
                    WardCode = user.Address?.WardCode,
                    WardName = user.Address?.WardName,
                    DetailAddress = user.Address?.DetailAddress,
                    FullAddress = user.Address?.FullAddress,
                    AvatarPath = user.Avatar,
                    Skills = skills.Select(s => new SkillItem
                    {
                        SkillID = s.SkillID,
                        ProficiencyLevelID = s.ProficiencyLevelID,
                        SkillName = s.Skill.SkillName,
                        ProficiencyLevelName = s.ProficiencyLevel.LevelName
                    }).ToList(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Email = user.Email,
                    RoleId = roleId,
                    IsVip = user.VipStatus,
                    VipExpiryDate = user.VipExpiryDate
                };

            

                // ⭐ Thêm phần đánh giá
                var ratingsQuery = _context.Ratings
                    .Include(r => r.Reviewer)
                    .Where(r => r.RevieweeID == userId)
                    .OrderByDescending(r => r.DateRated);

                var totalRatings = await ratingsQuery.CountAsync();
                var pageSize = 5;

                var averageRating = totalRatings > 0
                    ? (double?)(await ratingsQuery.AverageAsync(r => r.Score))
                    : null;


                var ratingList = await ratingsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RatingViewModel
                    {
                        ReviewerName = r.Reviewer.FullName,
                        ReviewerAvatarPath = string.IsNullOrEmpty(r.Reviewer.ProfilePicturePath) ? "/image/default-avatar.png" : r.Reviewer.ProfilePicturePath,
                        Score = r.Score,
                        Comment = r.Comment,
                        DateRated = r.DateRated
                    }).ToListAsync();

                viewModel.AverageRating = averageRating;
                viewModel.TotalReviews = totalRatings;
                viewModel.ReceivedRatings = ratingList;
                viewModel.CurrentPage = page;
                viewModel.TotalPages = (int)Math.Ceiling((double)totalRatings / pageSize);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GET Profile: {ex.Message}");
                return StatusCode(500);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound();

                var skills = await _context.StudentSkills
                    .Where(s => s.UserID == userId && s.IsActive)
                    .ToListAsync();

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var roleId = 0;
                
                // Map role names to IDs: 3 for Business, 4 for Student
                if (roles.Contains("Business"))
                    roleId = 3;
                else if (roles.Contains("Student"))
                    roleId = 4;

                var viewModel = new UserProfileViewModel
                {
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    University = user.University,
                    Major = user.Major,
                    CompanyName = user.CompanyName,
                    Industry = user.Industry,
                    
                    // API location data
                    ProvinceCode = user.Address?.ProvinceCode,
                    DistrictCode = user.Address?.DistrictCode,
                    WardCode = user.Address?.WardCode,
                    
                    DetailAddress = user.Address?.DetailAddress,
                    AvatarPath = user.Avatar,
                    Skills = skills.Select(s => new SkillItem
                    {
                        SkillID = s.SkillID,
                        ProficiencyLevelID = s.ProficiencyLevelID
                    }).ToList(),
                    RoleId = roleId,
                    IsVip = user.VipStatus,
                    VipExpiryDate = user.VipExpiryDate,

                    // Get provinces from API
                    Provinces = (await _locationApiService.GetProvincesAsync())
                        .Select(p => new OptionItem { ID = p.Id, Name = p.Name }).ToList(),

                    // Get districts from API if province is selected
                    Districts = !string.IsNullOrEmpty(user.Address?.ProvinceCode)
                        ? (await _locationApiService.GetDistrictsByProvinceAsync(user.Address.ProvinceCode))
                            .Select(d => new OptionItem { ID = d.Id, Name = d.Name }).ToList()
                        : new(),

                    // Get wards from API if district is selected
                    Wards = !string.IsNullOrEmpty(user.Address?.DistrictCode)
                        ? (await _locationApiService.GetWardsByDistrictAsync(user.Address.DistrictCode))
                            .Select(w => new OptionItem { ID = w.Id, Name = w.Name }).ToList()
                        : new(),

                    AvailableSkills = await _context.Skills.Where(s => s.IsActive)
                        .Select(s => new OptionItem { ID = s.SkillID.ToString(), Name = s.SkillName }).ToListAsync(),

                    AvailableProficiencyLevels = await _context.ProficiencyLevels.Where(p => p.IsActive)
                        .Select(p => new OptionItem { ID = p.LevelID.ToString(), Name = p.LevelName }).ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GET Edit: {ex.Message}");
                return StatusCode(500);
            }
        }

        // Helper method to repopulate view model
        private async Task RepopulateViewModel(UserProfileViewModel model)
        {
            // Repopulate skills
            model.AvailableSkills = await _context.Skills.Where(s => s.IsActive)
                .Select(s => new OptionItem { ID = s.SkillID.ToString(), Name = s.SkillName }).ToListAsync();
            
            model.AvailableProficiencyLevels = await _context.ProficiencyLevels.Where(p => p.IsActive)
                .Select(p => new OptionItem { ID = p.LevelID.ToString(), Name = p.LevelName }).ToListAsync();
            
            // Repopulate location data
            model.Provinces = (await _locationApiService.GetProvincesAsync())
                .Select(p => new OptionItem { ID = p.Id, Name = p.Name }).ToList();
            
            if (!string.IsNullOrEmpty(model.ProvinceCode))
            {
                model.Districts = (await _locationApiService.GetDistrictsByProvinceAsync(model.ProvinceCode))
                    .Select(d => new OptionItem { ID = d.Id, Name = d.Name }).ToList();
            }
            else
            {
                model.Districts = new List<OptionItem>();
            }
            
            if (!string.IsNullOrEmpty(model.DistrictCode))
            {
                model.Wards = (await _locationApiService.GetWardsByDistrictAsync(model.DistrictCode))
                    .Select(w => new OptionItem { ID = w.Id, Name = w.Name }).ToList();
            }
            else
            {
                model.Wards = new List<OptionItem>();
            }
        }

        // Helper method to generate full address string
        private async Task<string> GetFullAddress(string provinceCode, string districtCode, string wardCode, string detailAddress)
        {
            try
            {
                if (string.IsNullOrEmpty(provinceCode) && string.IsNullOrEmpty(districtCode) && string.IsNullOrEmpty(wardCode) && string.IsNullOrWhiteSpace(detailAddress))
                {
                    return string.Empty;
                }
                
                string fullAddress = !string.IsNullOrWhiteSpace(detailAddress) ? detailAddress : "";
                
                if (!string.IsNullOrEmpty(wardCode) && !string.IsNullOrEmpty(districtCode))
                {
                    var wards = await _locationApiService.GetWardsByDistrictAsync(districtCode);
                    var ward = wards.FirstOrDefault(w => w.Id == wardCode);
                    if (ward != null)
                        fullAddress += (fullAddress.Length > 0 ? ", " : "") + ward.Name;
                }
                
                if (!string.IsNullOrEmpty(districtCode) && !string.IsNullOrEmpty(provinceCode))
                {
                    var districts = await _locationApiService.GetDistrictsByProvinceAsync(provinceCode);
                    var district = districts.FirstOrDefault(d => d.Id == districtCode);
                    if (district != null)
                        fullAddress += (fullAddress.Length > 0 ? ", " : "") + district.Name;
                }
                
                if (!string.IsNullOrEmpty(provinceCode))
                {
                    var provinces = await _locationApiService.GetProvincesAsync();
                    var province = provinces.FirstOrDefault(p => p.Id == provinceCode);
                    if (province != null)
                        fullAddress += (fullAddress.Length > 0 ? ", " : "") + province.Name;
                }
                
                return fullAddress;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetFullAddress: {ex.Message}");
                return detailAddress ?? "";
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfileViewModel model)
        {
            try
            {
                // Debug information
                Console.WriteLine("[DEBUG] Edit POST method called");
                Console.WriteLine($"[DEBUG] ProvinceCode: {model.ProvinceCode}");
                Console.WriteLine($"[DEBUG] DistrictCode: {model.DistrictCode}");
                Console.WriteLine($"[DEBUG] WardCode: {model.WardCode}");
                Console.WriteLine($"[DEBUG] DetailAddress: {model.DetailAddress}");
                Console.WriteLine($"[DEBUG] Skills count: {model.Skills?.Count ?? 0}");
                
                if (!ModelState.IsValid)
                {
                    // Repopulate the view model with data
                    await RepopulateViewModel(model);
                    return View(model);
                }
                
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound();

                // Get user roles to determine if user is a business
                var roles = await _userManager.GetRolesAsync(user);
                var isBusiness = roles.Contains("Business");

                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                
                // Update role-specific fields
                if (!isBusiness) // Student fields
                {
                    user.University = model.University;
                    user.Major = model.Major;
                }
                
                if (isBusiness) // Business fields
                {
                    user.CompanyName = model.CompanyName;
                    user.Industry = model.Industry;
                }
                
                user.UpdatedAt = DateTime.Now;

                if (model.AvatarImage != null)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = $"{Guid.NewGuid()}_{model.AvatarImage.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await model.AvatarImage.CopyToAsync(stream);

                    var imagePath = $"/uploads/avatars/{fileName}";
                    user.Avatar = imagePath;
                    user.ProfilePicturePath = imagePath;
                }

                // Get location names from API
                string provinceName = "", districtName = "", wardName = "";
                
                if (!string.IsNullOrEmpty(model.ProvinceCode))
                {
                    var provinces = await _locationApiService.GetProvincesAsync();
                    var province = provinces.FirstOrDefault(p => p.Id == model.ProvinceCode);
                    if (province != null) provinceName = province.Name;
                    
                    if (!string.IsNullOrEmpty(model.DistrictCode))
                    {
                        var districts = await _locationApiService.GetDistrictsByProvinceAsync(model.ProvinceCode);
                        var district = districts.FirstOrDefault(d => d.Id == model.DistrictCode);
                        if (district != null) districtName = district.Name;
                        
                        if (!string.IsNullOrEmpty(model.WardCode))
                        {
                            var wards = await _locationApiService.GetWardsByDistrictAsync(model.DistrictCode);
                            var ward = wards.FirstOrDefault(w => w.Id == model.WardCode);
                            if (ward != null) wardName = ward.Name;
                        }
                    }
                }

                // Xử lý địa chỉ
                Console.WriteLine($"[DEBUG] User address before update: {user.Address?.FullAddress ?? "NULL"}");
                
                if (user.Address == null || user.AddressID == null)
                {
                    Console.WriteLine("[DEBUG] Creating new address");
                    var newAddress = new Address
                    {
                        ProvinceCode = model.ProvinceCode,
                        ProvinceName = provinceName,
                        DistrictCode = model.DistrictCode,
                        DistrictName = districtName,
                        WardCode = model.WardCode,
                        WardName = wardName,
                        DetailAddress = model.DetailAddress,
                        FullAddress = await GetFullAddress(model.ProvinceCode, model.DistrictCode, model.WardCode, model.DetailAddress),
                        IsActive = true
                    };
                    _context.Addresses.Add(newAddress);
                    await _context.SaveChangesAsync();
                    user.AddressID = newAddress.AddressID;
                    Console.WriteLine($"[DEBUG] New address created with ID: {newAddress.AddressID}");
                }
                else
                {
                    Console.WriteLine("[DEBUG] Updating existing address");
                    user.Address.ProvinceCode = model.ProvinceCode;
                    user.Address.ProvinceName = provinceName;
                    user.Address.DistrictCode = model.DistrictCode;
                    user.Address.DistrictName = districtName;
                    user.Address.WardCode = model.WardCode;
                    user.Address.WardName = wardName;
                    user.Address.DetailAddress = model.DetailAddress;
                    user.Address.FullAddress = await GetFullAddress(model.ProvinceCode, model.DistrictCode, model.WardCode, model.DetailAddress);
                    _context.Addresses.Update(user.Address);
                    Console.WriteLine($"[DEBUG] Updated address with ID: {user.Address.AddressID}");
                }

                _context.Users.Update(user);

                // Only update skills for non-business users
                if (!isBusiness)
                {
                    var oldSkills = _context.StudentSkills.Where(s => s.UserID == userId);
                    _context.StudentSkills.RemoveRange(oldSkills);

                    if (model.Skills != null)
                    {
                        foreach (var skill in model.Skills)
                        {
                            _context.StudentSkills.Add(new StudentSkill
                            {
                                UserID = userId,
                                SkillID = skill.SkillID,
                                ProficiencyLevelID = skill.ProficiencyLevelID,
                                IsActive = true
                            });
                        }
                    }
                }

                Console.WriteLine("[DEBUG] About to save changes to database");
                await _context.SaveChangesAsync();
                Console.WriteLine("[DEBUG] Changes saved successfully");

                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] POST Edit: {ex.Message}");
                
                // Add more detailed error logging
                Console.WriteLine($"Exception details: {ex}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception details: {ex.InnerException}");
                }
                
                // Repopulate the view model with data to avoid losing user input
                try
                {
                    await RepopulateViewModel(model);
                    
                    // Set a clear error message without special characters
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin. Vui lòng thử lại.";
                    return View(model);
                }
                catch (Exception repopulateEx)
                {
                    Console.WriteLine($"[ERROR] Failed to repopulate form: {repopulateEx.Message}");
                    TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                    return RedirectToAction("Edit");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDistricts(string provinceId)
        {
            try
            {
                if (string.IsNullOrEmpty(provinceId))
                {
                    return BadRequest("Province ID is required");
                }
                
                // Use the API service to get districts
                var districts = await _locationApiService.GetDistrictsByProvinceAsync(provinceId);
                return Json(districts.Select(d => new { id = d.Id, name = d.Name }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetDistricts: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve districts" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWards(string districtId)
        {
            try
            {
                if (string.IsNullOrEmpty(districtId))
                {
                    return BadRequest("District ID is required");
                }
                
                // Use the API service to get wards
                var wards = await _locationApiService.GetWardsByDistrictAsync(districtId);
                return Json(wards.Select(w => new { id = w.Id, name = w.Name }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetWards: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve wards" });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> VipSubscription()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound();
                
                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var roleId = 0;
                
                // Map role names to IDs: 3 for Business, 4 for Student
                if (roles.Contains("Business"))
                    roleId = 3;
                else if (roles.Contains("Student"))
                    roleId = 4;
                
                var baseMonthlyPrice = 100000m; // 100,000 VND per month
                
                var viewModel = new VipSubscriptionViewModel
                {
                    RoleId = roleId,
                    WalletBalance = user.WalletBalance,
                    IsVip = user.VipStatus,
                    VipExpiryDate = user.VipExpiryDate,
                    Plans = new List<VipPlanOption>
                    {
                        new VipPlanOption
                        {
                            Months = 1,
                            Price = baseMonthlyPrice,
                            DiscountPercentage = 0,
                            OriginalPrice = baseMonthlyPrice,
                            FinalPrice = baseMonthlyPrice,
                            Description = "1 tháng"
                        },
                        new VipPlanOption
                        {
                            Months = 3,
                            Price = baseMonthlyPrice * 3 * 0.68m, // 32% discount
                            DiscountPercentage = 32,
                            OriginalPrice = baseMonthlyPrice * 3,
                            FinalPrice = baseMonthlyPrice * 3 * 0.68m,
                            Description = "3 tháng (Tiết kiệm 32%)"
                        },
                        new VipPlanOption
                        {
                            Months = 12,
                            Price = baseMonthlyPrice * 12 * 0.44m, // 56% discount
                            DiscountPercentage = 56,
                            OriginalPrice = baseMonthlyPrice * 12,
                            FinalPrice = baseMonthlyPrice * 12 * 0.44m,
                            Description = "12 tháng (Tiết kiệm 56%)"
                        }
                    }
                };
                
                if (TempData["Error"] != null)
                {
                    viewModel.ErrorMessage = TempData["Error"].ToString();
                }
                
                if (TempData["Success"] != null)
                {
                    viewModel.SuccessMessage = TempData["Success"].ToString();
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] VipSubscription: {ex.Message}");
                return StatusCode(500);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PurchaseVip(int months)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound();
                
                // Calculate price based on months
                var baseMonthlyPrice = 100000m; // 100,000 VND per month
                decimal finalPrice;
                
                switch (months)
                {
                    case 1:
                        finalPrice = baseMonthlyPrice;
                        break;
                    case 3:
                        finalPrice = baseMonthlyPrice * 3 * 0.68m; // 32% discount
                        break;
                    case 12:
                        finalPrice = baseMonthlyPrice * 12 * 0.44m; // 56% discount
                        break;
                    default:
                        return BadRequest("Invalid subscription plan");
                }
                
                // Check if user has enough balance
                if (user.WalletBalance < finalPrice)
                {
                    TempData["Error"] = "Số dư không đủ để nâng cấp tài khoản VIP. Vui lòng nạp thêm tiền.";
                    return RedirectToAction("VipSubscription");
                }
                
                // Deduct balance
                user.WalletBalance -= finalPrice;
                
                // Update VIP status
                user.VipStatus = true;
                
                // Calculate expiry date
                if (user.VipExpiryDate != null && user.VipExpiryDate > DateTime.Now)
                {
                    // Extend existing subscription
                    user.VipExpiryDate = user.VipExpiryDate.Value.AddMonths(months);
                }
                else
                {
                    // New subscription
                    user.VipExpiryDate = DateTime.Now.AddMonths(months);
                }
                
                // Create transaction record
                var transaction = new Transaction
                {
                    UserID = userId,
                    Amount = finalPrice,
                    TransactionDate = DateTime.Now,
                    Description = $"Nâng cấp tài khoản VIP {months} tháng",
                    TypeID = 5, // Assuming 5 is for VIP subscription
                    StatusID = 2, // Changed from 1 (Đang xử lý) to 2 (Thành công)
                    OrderCode = Guid.NewGuid().ToString() // Generate a unique order code
                };
                
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Đã nâng cấp thành công tài khoản VIP {months} tháng. Hết hạn: {user.VipExpiryDate?.ToString("dd/MM/yyyy")}";
                return RedirectToAction("VipSubscription");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] PurchaseVip: {ex.Message}");
                TempData["Error"] = "Đã xảy ra lỗi khi xử lý giao dịch. Vui lòng thử lại sau.";
                return RedirectToAction("VipSubscription");
            }
        }
    }
}
