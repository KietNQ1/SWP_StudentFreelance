using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using System.Linq;

namespace StudentFreelance.Controllers
{
    public class AdvertisementController : Controller
    {
        private readonly IAdvertisementService _advertisementService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<AdvertisementController> _logger;
        private readonly ApplicationDbContext _context;

        public AdvertisementController(
            IAdvertisementService advertisementService,
            UserManager<ApplicationUser> userManager,
            ITransactionService transactionService,
            ILogger<AdvertisementController> logger,
            ApplicationDbContext context)
        {
            _advertisementService = advertisementService;
            _userManager = userManager;
            _transactionService = transactionService;
            _logger = logger;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            // Check if user is business
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.CompanyName))
            {
                _logger.LogWarning("Non-business user {UserId} attempted to access advertisement creation", user?.Id);
                return RedirectToAction("VipSubscription", "User");
            }

            var packageTypes = await _context.AdvertisementPackageTypes
                .Where(pt => pt.IsActive)
                .OrderBy(pt => pt.Price)
                .ToListAsync();

            var model = new AdvertisementViewModel();
            ViewBag.PackageTypes = packageTypes;

            _logger.LogInformation("Business user {UserId} ({CompanyName}) accessed advertisement creation form", user.Id, user.CompanyName);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(AdvertisementViewModel model)
        {
            // When creating a new advertisement, the image file is required
            if (model.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Vui lòng tải lên ảnh quảng cáo");
                _logger.LogError("ImageFile is required for creating a new advertisement");
            }
            else
            {
                _logger.LogInformation("ImageFile provided: {FileName}, {Length} bytes, {ContentType}", 
                    model.ImageFile.FileName, model.ImageFile.Length, model.ImageFile.ContentType);
            }
            
            // Remove fields that should not be validated for create action
            ModelState.Remove("ImagePath");
            ModelState.Remove("ApprovedByName");
            ModelState.Remove("StatusName");
            ModelState.Remove("PackageTypeName");
            ModelState.Remove("BusinessName");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for Advertisement creation.");
                
                // Log all model state errors
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        foreach (var error in state.Errors)
                        {
                            _logger.LogError("Validation error for {Field}: {Error}", key, error.ErrorMessage);
                        }
                    }
                }
                
                // Check for specific issues with ImageFile
                if (model.ImageFile == null)
                {
                    _logger.LogError("ImageFile is null");
                }
                else
                {
                    _logger.LogInformation("ImageFile details: Name={Name}, ContentType={ContentType}, Length={Length}", 
                        model.ImageFile.FileName, 
                        model.ImageFile.ContentType, 
                        model.ImageFile.Length);
                }

                var packageTypes = await _context.AdvertisementPackageTypes
                    .Where(pt => pt.IsActive)
                    .OrderBy(pt => pt.Price)
                    .ToListAsync();
                ViewBag.PackageTypes = packageTypes;
                
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.CompanyName))
            {
                return RedirectToAction("VipSubscription", "User");
            }

            // Get package type to determine price
            var packageType = await _context.AdvertisementPackageTypes.FindAsync(model.PackageTypeID);
            if (packageType == null)
            {
                ModelState.AddModelError("PackageTypeID", "Loại gói quảng cáo không hợp lệ");
                
                var packageTypes = await _context.AdvertisementPackageTypes
                    .Where(pt => pt.IsActive)
                    .OrderBy(pt => pt.Price)
                    .ToListAsync();
                ViewBag.PackageTypes = packageTypes;
                
                return View(model);
            }

            // Check if user has enough money
            if (user.WalletBalance < packageType.Price)
            {
                ModelState.AddModelError("", "Số dư không đủ. Vui lòng nạp thêm tiền.");
                _logger.LogWarning("User {UserId} attempted to create advertisement with insufficient funds.", user.Id);
                
                var packageTypes = await _context.AdvertisementPackageTypes
                    .Where(pt => pt.IsActive)
                    .OrderBy(pt => pt.Price)
                    .ToListAsync();
                ViewBag.PackageTypes = packageTypes;
                
                return View(model);
            }

            // Create advertisement
            int advertisementId = await _advertisementService.CreateAdvertisementAsync(model, user.Id);

            // Không trừ tiền ngay, chỉ tạo quảng cáo
            TempData["Success"] = "Quảng cáo đã được tạo và đang chờ phê duyệt. Tiền sẽ được trừ sau khi quảng cáo được chấp nhận.";

            return RedirectToAction("MyAdvertisements");
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var advertisement = await _advertisementService.GetAdvertisementByIdAsync(id);

            if (advertisement == null || advertisement.BusinessId != user.Id)
            {
                return NotFound();
            }

            // Only allow editing if the advertisement is not approved yet
            if (advertisement.StatusID != 1) // Not Pending
            {
                return RedirectToAction("MyAdvertisements");
            }

            var packageTypes = await _context.AdvertisementPackageTypes
                .Where(pt => pt.IsActive)
                .OrderBy(pt => pt.Price)
                .ToListAsync();
            ViewBag.PackageTypes = packageTypes;

            var model = new AdvertisementViewModel
            {
                Id = advertisement.Id,
                PackageTypeID = advertisement.PackageTypeID,
                PackageTypeName = advertisement.PackageType?.PackageTypeName,
                ImagePath = advertisement.ImagePath,
                StatusID = advertisement.StatusID,
                StatusName = advertisement.Status?.StatusName,
                BusinessName = user.CompanyName,
                CreatedAt = advertisement.CreatedAt
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(AdvertisementViewModel model)
        {
            // Remove fields that should not be validated for edit action
            ModelState.Remove("ImagePath");
            ModelState.Remove("ApprovedByName");
            ModelState.Remove("StatusName");
            ModelState.Remove("PackageTypeName");
            ModelState.Remove("BusinessName");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for Advertisement edit");
                
                // Log all model state errors
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        foreach (var error in state.Errors)
                        {
                            _logger.LogError("Validation error for {Field}: {Error}", key, error.ErrorMessage);
                        }
                    }
                }

                var packageTypes = await _context.AdvertisementPackageTypes
                    .Where(pt => pt.IsActive)
                    .OrderBy(pt => pt.Price)
                    .ToListAsync();
                ViewBag.PackageTypes = packageTypes;
                
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var advertisement = await _advertisementService.GetAdvertisementByIdAsync(model.Id);

            if (advertisement == null || advertisement.BusinessId != user.Id)
            {
                _logger.LogWarning("User {UserId} attempted to edit advertisement {AdvertisementId} that doesn't exist or doesn't belong to them", user.Id, model.Id);
                return NotFound();
            }

            // Only allow editing if the advertisement is not approved yet
            if (advertisement.StatusID != 1) // Not Pending
            {
                _logger.LogWarning("User {UserId} attempted to edit approved advertisement {AdvertisementId}", user.Id, model.Id);
                return RedirectToAction("MyAdvertisements");
            }

            await _advertisementService.UpdateAdvertisementAsync(model);
            _logger.LogInformation("User {UserId} successfully updated advertisement {AdvertisementId}", user.Id, model.Id);

            return RedirectToAction("MyAdvertisements");
        }

        [Authorize]
        public async Task<IActionResult> MyAdvertisements()
        {
            var user = await _userManager.GetUserAsync(User);
            var advertisements = await _advertisementService.GetBusinessAdvertisementsAsync(user.Id);

            var statusList = await _context.AdvertisementStatuses.ToListAsync();
            var packageTypes = await _context.AdvertisementPackageTypes.ToListAsync();

            var models = advertisements.Select(a => new AdvertisementViewModel
            {
                Id = a.Id,
                PackageTypeID = a.PackageTypeID,
                PackageTypeName = packageTypes.FirstOrDefault(pt => pt.PackageTypeID == a.PackageTypeID)?.PackageTypeName,
                ImagePath = a.ImagePath,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                StatusID = a.StatusID,
                StatusName = statusList.FirstOrDefault(s => s.StatusID == a.StatusID)?.StatusName,
                BusinessName = user.CompanyName,
                CreatedAt = a.CreatedAt,
                IsActive = a.IsActive
            }).ToList();

            return View(models);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var advertisement = await _advertisementService.GetAdvertisementByIdAsync(id);

            if (advertisement == null || advertisement.BusinessId != user.Id)
            {
                return NotFound();
            }

            // If this is a POST request, cancel the advertisement
            if (HttpContext.Request.Method == "POST")
            {
                await _advertisementService.DeleteAdvertisementAsync(id);
                TempData["Success"] = "Advertisement has been successfully canceled.";
                return RedirectToAction("MyAdvertisements");
            }

            // Otherwise, show the delete confirmation page
            var statusList = await _context.AdvertisementStatuses.ToListAsync();
            var packageTypes = await _context.AdvertisementPackageTypes.ToListAsync();

            return View(new AdvertisementViewModel
            {
                Id = advertisement.Id,
                PackageTypeID = advertisement.PackageTypeID,
                PackageTypeName = packageTypes.FirstOrDefault(pt => pt.PackageTypeID == advertisement.PackageTypeID)?.PackageTypeName,
                ImagePath = advertisement.ImagePath,
                StatusID = advertisement.StatusID,
                StatusName = statusList.FirstOrDefault(s => s.StatusID == advertisement.StatusID)?.StatusName,
                BusinessName = user.CompanyName,
                CreatedAt = advertisement.CreatedAt
            });
        }

        // Remove or comment out the DeleteConfirmed method since we've combined it into Delete
        /*
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var advertisement = await _advertisementService.GetAdvertisementByIdAsync(id);

            if (advertisement == null || advertisement.BusinessId != user.Id)
            {
                return NotFound();
            }

            await _advertisementService.DeleteAdvertisementAsync(id);

            return RedirectToAction("MyAdvertisements");
        }
        */

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> ManageAdvertisements()
        {
            var advertisements = await _advertisementService.GetAllAdvertisementsAsync();
            var statusList = await _context.AdvertisementStatuses.ToListAsync();
            var packageTypes = await _context.AdvertisementPackageTypes.ToListAsync();

            var models = advertisements.Select(a => new AdvertisementViewModel
            {
                Id = a.Id,
                PackageTypeID = a.PackageTypeID,
                PackageTypeName = packageTypes.FirstOrDefault(pt => pt.PackageTypeID == a.PackageTypeID)?.PackageTypeName,
                ImagePath = a.ImagePath,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                StatusID = a.StatusID,
                StatusName = statusList.FirstOrDefault(s => s.StatusID == a.StatusID)?.StatusName,
                ApprovedAt = a.ApprovedAt,
                ApprovedByName = a.ApprovedBy?.FullName,
                BusinessName = a.Business?.CompanyName,
                CreatedAt = a.CreatedAt,
                IsActive = a.IsActive
            }).ToList();

            var viewModel = new AdvertisementListViewModel
            {
                Advertisements = models,
                ShowApprovalOptions = true
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Lấy thông tin quảng cáo
            var advertisement = await _advertisementService.GetAdvertisementByIdAsync(id);
            if (advertisement == null)
            {
                TempData["Error"] = "Không tìm thấy quảng cáo.";
                return RedirectToAction("ManageAdvertisements");
            }
            
            // Lấy thông tin business
            var business = await _context.Users.FindAsync(advertisement.BusinessId);
            if (business == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin doanh nghiệp.";
                return RedirectToAction("ManageAdvertisements");
            }
            
            // Lấy thông tin gói quảng cáo
            var packageType = await _context.AdvertisementPackageTypes.FindAsync(advertisement.PackageTypeID);
            if (packageType == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin gói quảng cáo.";
                return RedirectToAction("ManageAdvertisements");
            }
            
            // Kiểm tra số dư của business
            if (business.WalletBalance < packageType.Price)
            {
                TempData["Error"] = $"Doanh nghiệp không đủ tiền để thanh toán quảng cáo. Cần {packageType.Price:N0} VND.";
                return RedirectToAction("ManageAdvertisements");
            }
            
            // Phê duyệt quảng cáo
            await _advertisementService.ApproveAdvertisementAsync(id, user.Id);
            
            // Thực hiện giao dịch thanh toán
            await _transactionService.CreateAdvertisementTransactionAsync(business.Id, id, packageType.Price);
            
            TempData["Success"] = "Quảng cáo đã được phê duyệt và thanh toán thành công.";
            return RedirectToAction("ManageAdvertisements");
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            await _advertisementService.RejectAdvertisementAsync(id);

            return RedirectToAction("ManageAdvertisements");
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            await _advertisementService.ActivateAdvertisementAsync(id);
            return RedirectToAction("ManageAdvertisements");
        }

        [Authorize]
        public async Task<IActionResult> Renew(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var advertisement = await _advertisementService.GetAdvertisementByIdAsync(id);

            if (advertisement == null || advertisement.BusinessId != user.Id)
            {
                return NotFound();
            }

            // Get package type to determine price
            var packageType = await _context.AdvertisementPackageTypes.FindAsync(advertisement.PackageTypeID);
            if (packageType == null)
            {
                TempData["Error"] = "Advertisement package information not found.";
                return RedirectToAction("MyAdvertisements");
            }

            // Check if user has enough money
            if (user.WalletBalance < packageType.Price)
            {
                TempData["Error"] = "Insufficient balance. Please add funds to your wallet.";
                return RedirectToAction("MyAdvertisements");
            }

            // Đánh dấu quảng cáo cần gia hạn và đợi phê duyệt
            await _advertisementService.RequestRenewalAsync(id);

            TempData["Success"] = "Advertisement renewal request submitted successfully. It will be processed after admin approval.";
            return RedirectToAction("MyAdvertisements");
        }
    }
} 