using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IBankAccountService
{
    Task<BankAccount> GetBankAccountByUserIdAsync(int userId);
    Task SaveOrUpdateBankAccountAsync(int userId, BankAccount account);
    Task<bool> VerifyBankAccountAsync(string accountNumber, string bankName, string accountHolderName);

}
