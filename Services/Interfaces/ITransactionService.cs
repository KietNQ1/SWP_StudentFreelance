using StudentFreelance.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<Transaction> GetTransactionByIdAsync(int id);
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<Transaction> UpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(int id);
        Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(string userId);
        Task<IEnumerable<Transaction>> GetTransactionsByProjectIdAsync(int projectId);
        Task<IEnumerable<Transaction>> GetTransactionsByStatusAsync(int statusId);
        Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(int typeId);
        Task<Transaction> CreatePendingDeposit(int userId, decimal amount,string decription, long orderCode);
        Task<bool> UpdateTransactionStatusAsync(int transactionId, int statusId);
        Task<bool> ProcessDepositAsync(int userId, decimal amount, string description);
        Task<bool> ProcessWithdrawalAsync(int userId, decimal amount, string description);

        //m?i thêm
        Task<bool> ConfirmDepositFromPayOS(long orderCode);

    }
} 