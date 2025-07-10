using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System.Security.Claims;

namespace StudentFreelance.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

                var viewModel = new UserProfileViewModel
                {
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    University = user.University,
                    Major = user.Major,
                    CompanyName = user.CompanyName,
                    Industry = user.Industry,
                    ProvinceID = user.Address?.ProvinceID,
                    DistrictID = user.Address?.DistrictID,
                    WardID = user.Address?.WardID,
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
                    Email = user.Email
                };

                // Tên tỉnh/huyện/xã
                if (user.Address?.ProvinceID != null)
                {
                    var province = await _context.Provinces.FirstOrDefaultAsync(p => p.ProvinceID == user.Address.ProvinceID);
                    viewModel.ProvinceName = province?.Name;
                }

                if (user.Address?.DistrictID != null)
                {
                    var district = await _context.Districts.FirstOrDefaultAsync(d => d.DistrictID == user.Address.DistrictID);
                    viewModel.DistrictName = district?.Name;
                }

                if (user.Address?.WardID != null)
                {
                    var ward = await _context.Wards.FirstOrDefaultAsync(w => w.WardID == user.Address.WardID);
                    viewModel.WardName = ward?.Name;
                }

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

                var viewModel = new UserProfileViewModel
                {
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    University = user.University,
                    Major = user.Major,
                    CompanyName = user.CompanyName,
                    Industry = user.Industry,
                    ProvinceID = user.Address?.ProvinceID,
                    DistrictID = user.Address?.DistrictID,
                    WardID = user.Address?.WardID,
                    DetailAddress = user.Address?.DetailAddress,
                    AvatarPath = user.Avatar,
                    Skills = skills.Select(s => new SkillItem
                    {
                        SkillID = s.SkillID,
                        ProficiencyLevelID = s.ProficiencyLevelID
                    }).ToList(),

                    Provinces = await _context.Provinces
                        .Select(p => new OptionItem { ID = p.ProvinceID, Name = p.Name }).ToListAsync(),

                    Districts = user.Address?.ProvinceID != null
                        ? await _context.Districts.Where(d => d.ProvinceID == user.Address.ProvinceID)
                            .Select(d => new OptionItem { ID = d.DistrictID, Name = d.Name }).ToListAsync()
                        : new(),

                    Wards = user.Address?.DistrictID != null
                        ? await _context.Wards.Where(w => w.DistrictID == user.Address.DistrictID)
                            .Select(w => new OptionItem { ID = w.WardID, Name = w.Name }).ToListAsync()
                        : new(),

                    AvailableSkills = await _context.Skills.Where(s => s.IsActive)
                        .Select(s => new OptionItem { ID = s.SkillID, Name = s.SkillName }).ToListAsync(),

                    AvailableProficiencyLevels = await _context.ProficiencyLevels.Where(p => p.IsActive)
                        .Select(p => new OptionItem { ID = p.LevelID, Name = p.LevelName }).ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GET Edit: {ex.Message}");
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserProfileViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound();

                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.University = model.University;
                user.Major = model.Major;
                user.CompanyName = model.CompanyName;
                user.Industry = model.Industry;
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

                // Xử lý địa chỉ
                if (user.Address == null || user.AddressID == null)
                {
                    var newAddress = new Address
                    {
                        ProvinceID = model.ProvinceID,
                        DistrictID = model.DistrictID,
                        WardID = model.WardID,
                        DetailAddress = model.DetailAddress,
                        FullAddress = GetFullAddress(model.ProvinceID, model.DistrictID, model.WardID, model.DetailAddress),
                        IsActive = true
                    };
                    _context.Addresses.Add(newAddress);
                    await _context.SaveChangesAsync();
                    user.AddressID = newAddress.AddressID;
                }
                else
                {
                    user.Address.ProvinceID = model.ProvinceID;
                    user.Address.DistrictID = model.DistrictID;
                    user.Address.WardID = model.WardID;
                    user.Address.DetailAddress = model.DetailAddress;
                    user.Address.FullAddress = GetFullAddress(model.ProvinceID, model.DistrictID, model.WardID, model.DetailAddress);
                    _context.Addresses.Update(user.Address);
                }

                _context.Users.Update(user);

                var oldSkills = _context.StudentSkills.Where(s => s.UserID == userId);
                _context.StudentSkills.RemoveRange(oldSkills);

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

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] POST Edit: {ex.Message}");
                return StatusCode(500);
            }
        }

        // Helper method to generate full address string
        private string GetFullAddress(int? provinceID, int? districtID, int? wardID, string detailAddress)
        {
            try
            {
                string fullAddress = detailAddress ?? "";
                
                if (wardID.HasValue)
                {
                    var ward = _context.Wards.FirstOrDefault(w => w.WardID == wardID);
                    if (ward != null)
                        fullAddress += $", {ward.Name}";
                }
                
                if (districtID.HasValue)
                {
                    var district = _context.Districts.FirstOrDefault(d => d.DistrictID == districtID);
                    if (district != null)
                        fullAddress += $", {district.Name}";
                }
                
                if (provinceID.HasValue)
                {
                    var province = _context.Provinces.FirstOrDefault(p => p.ProvinceID == provinceID);
                    if (province != null)
                        fullAddress += $", {province.Name}";
                }
                
                return fullAddress;
            }
            catch
            {
                return detailAddress ?? "";
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            try
            {
                var districts = await _context.Districts
                    .Where(d => d.ProvinceID == provinceId)
                    .Select(d => new { id = d.DistrictID, name = d.Name })
                    .ToListAsync();
                return Json(districts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetDistricts: {ex.Message}");
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWards(int districtId)
        {
            try
            {
                var wards = await _context.Wards
                    .Where(w => w.DistrictID == districtId)
                    .Select(w => new { id = w.WardID, name = w.Name })
                    .ToListAsync();
                return Json(wards);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetWards: {ex.Message}");
                return StatusCode(500);
            }
        }
    }
}
