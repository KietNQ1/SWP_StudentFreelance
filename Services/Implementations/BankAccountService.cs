using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BankAccountService : IBankAccountService
{
    private readonly ApplicationDbContext _context;

    public BankAccountService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccount> GetBankAccountByUserIdAsync(int userId)
    {
        return await _context.BankAccounts.FirstOrDefaultAsync(b => b.UserID == userId);
    }

    public async Task SaveOrUpdateBankAccountAsync(int userId, BankAccount account)
    {
        var existing = await GetBankAccountByUserIdAsync(userId);
        if (existing != null)
        {
            existing.BankName = account.BankName;
            existing.AccountNumber = account.AccountNumber;
            existing.AccountHolderName = account.AccountHolderName;
        }
        else
        {
            account.UserID = userId;
            await _context.BankAccounts.AddAsync(account);
        }

        await _context.SaveChangesAsync();
    }
}
