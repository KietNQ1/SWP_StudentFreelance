using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Interfaces
{
    public interface IAdvertisementService
    {
        Task<List<Advertisement>> GetActiveAdvertisementsAsync();
        Task<List<Advertisement>> GetAllAdvertisementsAsync();
        Task<List<Advertisement>> GetPendingAdvertisementsAsync();
        Task<List<Advertisement>> GetBusinessAdvertisementsAsync(int businessId);
        Task<Advertisement> GetAdvertisementByIdAsync(int id);
        Task<int> CreateAdvertisementAsync(AdvertisementViewModel model, int businessId);
        Task<bool> UpdateAdvertisementAsync(AdvertisementViewModel model);
        Task<bool> ApproveAdvertisementAsync(int id, int approvedById);
        Task<bool> RejectAdvertisementAsync(int id);
        Task<bool> DeleteAdvertisementAsync(int id);
        Task<bool> ActivateAdvertisementAsync(int id);
        Task<bool> RenewAdvertisementAsync(int id);
        Task<bool> RequestRenewalAsync(int id);
        bool IsAdvertisementExpired(Advertisement advertisement);
        Task CleanupExpiredAdvertisementsAsync();
    }
} 