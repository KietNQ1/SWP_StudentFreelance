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
    public Task<bool> VerifyBankAccountAsync(string accountNumber, string bankName, string accountHolderName)
    {
        // Kiểm tra tên không chứa số hoặc ký tự đặc biệt
        bool isValidName = !string.IsNullOrWhiteSpace(accountHolderName) &&
                           accountHolderName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));

        // Kiểm tra số tài khoản là chuỗi số có độ dài hợp lệ (ví dụ 8–16 ký tự)
        bool isValidAccountNumber = !string.IsNullOrWhiteSpace(accountNumber) &&
                                    accountNumber.All(char.IsDigit) &&
                                    accountNumber.Length >= 8 && accountNumber.Length <= 16;

        // Kiểm tra tên ngân hàng không rỗng và không chứa ký tự lạ
        bool isValidBankName = !string.IsNullOrWhiteSpace(bankName) &&
                               bankName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));

        return Task.FromResult(isValidName && isValidAccountNumber && isValidBankName);
    }


}
