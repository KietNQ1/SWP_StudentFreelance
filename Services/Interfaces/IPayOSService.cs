using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IPayOSService
{
    Task<string> CreatePaymentLink(decimal amount, string description, string orderCode);
    Task<bool> TransferToBankAsync(string bankCode, string accountNumber, decimal amount, string description);
}
