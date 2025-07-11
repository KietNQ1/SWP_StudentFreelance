using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IPayOSService
{
    //Task<string> CreatePaymentLink(decimal amount, string description, long orderCode);

    //mới thêm 
    Task<string> CreatePaymentLink(decimal amount, long orderCode, string description, string returnUrl, string cancelUrl);

    Task<bool> TransferToBankAsync(string bankCode, string accountNumber, decimal amount, string description);
}
