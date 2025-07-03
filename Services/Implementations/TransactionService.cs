using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.Type)
                .Include(t => t.Status)
                .ToListAsync();
        }

        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.Type)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.TransactionID == id);
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            transaction.TransactionDate = DateTime.Now;
            
            // Update user's wallet balance
            var user = await _context.Users.FindAsync(transaction.UserID);
            if (user != null)
            {
                // For deposits, add to balance
                if (transaction.TypeID == 1) // Assuming TypeID 1 is Deposit
                {
                    user.WalletBalance += transaction.Amount;
                }
                // For withdrawals, subtract from balance if sufficient funds
                else if (transaction.TypeID == 2) // Assuming TypeID 2 is Withdrawal
                {
                    if (user.WalletBalance >= transaction.Amount)
                    {
                        user.WalletBalance -= transaction.Amount;
                    }
                    else
                    {
                        transaction.StatusID = 2; // Assuming StatusID 2 is Failed
                        transaction.Description += " - Insufficient funds";
                    }
                }
                // For project payments, subtract from balance if sufficient funds
                else if (transaction.TypeID == 3) // Assuming TypeID 3 is Payment
                {
                    if (user.WalletBalance >= transaction.Amount)
                    {
                        user.WalletBalance -= transaction.Amount;
                    }
                    else
                    {
                        transaction.StatusID = 2; // Assuming StatusID 2 is Failed
                        transaction.Description += " - Insufficient funds";
                    }
                }
                
                _context.Users.Update(user);
            }

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            
            return transaction;
        }

        public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return false;

            transaction.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(string userId)
        {
            int userIdInt;
            if (!int.TryParse(userId, out userIdInt))
                return new List<Transaction>();

            return await _context.Transactions
                .Include(t => t.Type)
                .Include(t => t.Status)
                .Include(t => t.Project)
                .Where(t => t.UserID == userIdInt)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByProjectIdAsync(int projectId)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Type)
                .Include(t => t.Status)
                .Where(t => t.ProjectID == projectId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByStatusAsync(int statusId)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.Type)
                .Include(t => t.Status)
                .Where(t => t.StatusID == statusId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(int typeId)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.Type)
                .Include(t => t.Status)
                .Where(t => t.TypeID == typeId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateTransactionStatusAsync(int transactionId, int statusId)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            if (transaction == null)
                return false;

            transaction.StatusID = statusId;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> ProcessDepositAsync(int userId, decimal amount, string description)
        {
            var transaction = new Transaction
            {
                UserID = userId,
                Amount = amount,
                TypeID = 1, // Deposit
                StatusID = 1, // Completed
                Description = description,
                TransactionDate = DateTime.Now,
                IsActive = true
            };
            
            await CreateTransactionAsync(transaction);
            return transaction.StatusID == 1; // Return true if status is Completed
        }
        
        public async Task<bool> ProcessWithdrawalAsync(int userId, decimal amount, string description)
        {
            var transaction = new Transaction
            {
                UserID = userId,
                Amount = amount,
                TypeID = 2, // Withdrawal
                StatusID = 1, // Initially set as Completed
                Description = description,
                TransactionDate = DateTime.Now,
                IsActive = true
            };
            
            await CreateTransactionAsync(transaction);
            return transaction.StatusID == 1; // Return true if status is Completed
        }
    }
} 