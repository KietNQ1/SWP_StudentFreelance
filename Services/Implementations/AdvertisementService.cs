using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Implementations
{
    public class AdvertisementService : IAdvertisementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AdvertisementService> _logger;

        public AdvertisementService(
            ApplicationDbContext context, 
            IWebHostEnvironment webHostEnvironment,
            ILogger<AdvertisementService> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<List<Advertisement>> GetActiveAdvertisementsAsync()
        {
            return await _context.Advertisements
                .Where(a => a.StatusID == 2 && a.IsActive && a.StartDate <= DateTime.Now && a.EndDate >= DateTime.Now)
                .Include(a => a.Business)
                .Include(a => a.Status)
                .Include(a => a.PackageType)
                .ToListAsync();
        }

        public async Task<List<Advertisement>> GetAllAdvertisementsAsync()
        {
            return await _context.Advertisements
                .Include(a => a.Business)
                .Include(a => a.ApprovedBy)
                .Include(a => a.Status)
                .Include(a => a.PackageType)
                .ToListAsync();
        }

        public async Task<List<Advertisement>> GetPendingAdvertisementsAsync()
        {
            return await _context.Advertisements
                .Where(a => a.StatusID == 1 && a.IsActive)
                .Include(a => a.Business)
                .Include(a => a.Status)
                .Include(a => a.PackageType)
                .ToListAsync();
        }

        public async Task<List<Advertisement>> GetBusinessAdvertisementsAsync(int businessId)
        {
            return await _context.Advertisements
                .Where(a => a.BusinessId == businessId)
                .Include(a => a.Status)
                .Include(a => a.PackageType)
                .ToListAsync();
        }

        public async Task<Advertisement> GetAdvertisementByIdAsync(int id)
        {
            return await _context.Advertisements
                .Include(a => a.Business)
                .Include(a => a.ApprovedBy)
                .Include(a => a.Status)
                .Include(a => a.PackageType)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<int> CreateAdvertisementAsync(AdvertisementViewModel model, int businessId)
        {
            // Save the image
            string uniqueFileName = null;
            if (model.ImageFile != null)
            {
                try
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "advertisements");
                    
                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        _logger.LogInformation("Creating advertisements upload directory: {Directory}", uploadsFolder);
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    _logger.LogInformation("Saving advertisement image to: {FilePath}", filePath);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                    
                    _logger.LogInformation("Advertisement image saved successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving advertisement image: {ErrorMessage}", ex.Message);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("No image file provided for advertisement");
            }

            // Get package type duration
            var packageType = await _context.AdvertisementPackageTypes.FindAsync(model.PackageTypeID);
            if (packageType == null)
            {
                throw new ArgumentException($"Invalid package type ID: {model.PackageTypeID}");
            }

            // Create end date based on package type duration
            DateTime startDate = DateTime.Now;
            DateTime endDate = startDate.AddDays(packageType.DurationDays);

            // Create the advertisement
            var advertisement = new Advertisement
            {
                BusinessId = businessId,
                ImagePath = uniqueFileName,
                PackageTypeID = model.PackageTypeID,
                StartDate = startDate,
                EndDate = endDate,
                StatusID = 1, // Pending
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Advertisements.Add(advertisement);
            await _context.SaveChangesAsync();

            return advertisement.Id;
        }

        public async Task<bool> UpdateAdvertisementAsync(AdvertisementViewModel model)
        {
            var advertisement = await _context.Advertisements.FindAsync(model.Id);
            
            if (advertisement == null)
            {
                _logger.LogWarning("Attempted to update non-existent advertisement with ID: {Id}", model.Id);
                return false;
            }

            // Handle new image upload if provided
            if (model.ImageFile != null)
            {
                try
                {
                    _logger.LogInformation("Processing new image upload for advertisement ID: {Id}", model.Id);
                    
                    // Delete the old image if it exists
                    if (!string.IsNullOrEmpty(advertisement.ImagePath))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "advertisements", advertisement.ImagePath);
                        if (File.Exists(oldFilePath))
                        {
                            _logger.LogInformation("Deleting old advertisement image: {FilePath}", oldFilePath);
                            File.Delete(oldFilePath);
                        }
                        else
                        {
                            _logger.LogWarning("Old advertisement image not found: {FilePath}", oldFilePath);
                        }
                    }

                    // Save the new image
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "advertisements");
                    
                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        _logger.LogInformation("Creating advertisements upload directory: {Directory}", uploadsFolder);
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    _logger.LogInformation("Saving new advertisement image to: {FilePath}", filePath);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    advertisement.ImagePath = uniqueFileName;
                    _logger.LogInformation("New advertisement image saved successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating advertisement image: {ErrorMessage}", ex.Message);
                    return false;
                }
            }

            // Only update package type if it's a new advertisement (not approved yet)
            if (advertisement.StatusID == 1) // Pending
            {
                _logger.LogInformation("Updating package type from {OldTypeID} to {NewTypeID} for advertisement ID: {Id}", 
                    advertisement.PackageTypeID, model.PackageTypeID, model.Id);
                advertisement.PackageTypeID = model.PackageTypeID;
            }

            advertisement.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Advertisement with ID: {Id} updated successfully", model.Id);
            return true;
        }

        public async Task<bool> ApproveAdvertisementAsync(int id, int approvedById)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            
            if (advertisement == null)
                return false;

            // Get package type duration
            var packageType = await _context.AdvertisementPackageTypes.FindAsync(advertisement.PackageTypeID);
            if (packageType == null)
            {
                throw new ArgumentException($"Invalid package type ID: {advertisement.PackageTypeID}");
            }

            advertisement.StatusID = 2; // Approved
            advertisement.ApprovedById = approvedById;
            advertisement.ApprovedAt = DateTime.Now;
            advertisement.StartDate = DateTime.Now;
            advertisement.EndDate = DateTime.Now.AddDays(packageType.DurationDays);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAdvertisementAsync(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            
            if (advertisement == null)
                return false;

            advertisement.StatusID = 3; // Rejected
            advertisement.IsActive = false;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAdvertisementAsync(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            
            if (advertisement == null)
                return false;

            // Instead of deleting the image file, we'll keep it since we're just changing the status
            // and not actually deleting the advertisement
            
            // Set advertisement as canceled (not active but still in the database)
            advertisement.IsActive = false;
            
            // If the advertisement is pending approval (StatusID = 1),
            // we need to change its status to approved (StatusID = 2) and mark it as inactive
            // to indicate it's canceled
            if (advertisement.StatusID == 1) // Pending
            {
                advertisement.StatusID = 2; // Set to Approved (but inactive)
            }
            // For other statuses, we don't need to change them, just keep them inactive
            
            await _context.SaveChangesAsync();
            return true;
        }

        public bool IsAdvertisementExpired(Advertisement advertisement)
        {
            return advertisement.EndDate < DateTime.Now;
        }

        public async Task CleanupExpiredAdvertisementsAsync()
        {
            var expiredAds = await _context.Advertisements
                .Where(a => a.EndDate < DateTime.Now && a.IsActive)
                .ToListAsync();

            foreach (var ad in expiredAds)
            {
                ad.StatusID = 4; // Expired
                ad.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ActivateAdvertisementAsync(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            
            if (advertisement == null)
                return false;

            advertisement.IsActive = true;
            
            // If it was rejected or expired, set status to approved
            if (advertisement.StatusID == 3 || advertisement.StatusID == 4)
            {
                advertisement.StatusID = 2; // Approved
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RenewAdvertisementAsync(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            
            if (advertisement == null)
                return false;

            // Get package type duration
            var packageType = await _context.AdvertisementPackageTypes.FindAsync(advertisement.PackageTypeID);
            if (packageType == null)
            {
                throw new ArgumentException($"Invalid package type ID: {advertisement.PackageTypeID}");
            }

            // Extend end date by package duration days from current end date
            // If already expired, set new period starting from today
            if (advertisement.EndDate < DateTime.Now)
            {
                advertisement.StartDate = DateTime.Now;
                advertisement.EndDate = DateTime.Now.AddDays(packageType.DurationDays);
            }
            else
            {
                advertisement.EndDate = advertisement.EndDate.AddDays(packageType.DurationDays);
            }

            // If it was inactive, make it active again
            advertisement.IsActive = true;
            
            // Set status to approved
            advertisement.StatusID = 2; // Approved
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> RequestRenewalAsync(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            
            if (advertisement == null)
                return false;
            
            // Đánh dấu quảng cáo cần gia hạn và đợi phê duyệt
            advertisement.StatusID = 1; // Pending approval
            advertisement.IsActive = true;
            advertisement.ApprovedById = null;
            advertisement.ApprovedAt = null;
            
            _logger.LogInformation("Advertisement {Id} marked for renewal and pending approval", id);
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 