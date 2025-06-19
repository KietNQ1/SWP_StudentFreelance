using StudentFreelance.Models;

namespace StudentFreelance.Interfaces
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
        Task<bool> UpdateTransactionStatusAsync(int transactionId, int statusId);
    }
} 