using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System.Threading.Tasks;

namespace StudentFreelance.Controllers

{
    [Authorize]

    public class WalletController : Controller
    {
        private readonly IBankAccountService _bankAccountService;

        public WalletController(
            ITransactionService transactionService,
            UserManager<ApplicationUser> userManager,
            IBankAccountService bankAccountService,
            IPayOSService payOSService)
        {
            _transactionService = transactionService;
            _userManager = userManager;
            _bankAccountService = bankAccountService;
            _payOSService = payOSService;
        }
        private readonly IPayOSService _payOSService;
        private readonly ITransactionService _transactionService;
        private readonly UserManager<ApplicationUser> _userManager;


        // GET: /Wallet
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var transactions = await _transactionService.GetTransactionsByUserIdAsync(user.Id.ToString());
            
            var viewModel = new WalletViewModel
            {
                WalletBalance = user.WalletBalance,
                Transactions = transactions
            };

            return View(viewModel);
        }

        //MangeBankAccount
        [HttpGet]
        public async Task<IActionResult> ManageBankAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var bankAccount = await _bankAccountService.GetBankAccountByUserIdAsync(user.Id);

            var viewModel = bankAccount != null
                ? new BankAccountViewModel
                {
                    BankName = bankAccount.BankName,
                    AccountNumber = bankAccount.AccountNumber,
                    AccountHolderName = bankAccount.AccountHolderName
                }
                : new BankAccountViewModel();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageBankAccount(BankAccountViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var account = new BankAccount
            {
                BankName = model.BankName,
                AccountNumber = model.AccountNumber,
                AccountHolderName = model.AccountHolderName
            };

            await _bankAccountService.SaveOrUpdateBankAccountAsync(user.Id, account);

            TempData["SuccessMessage"] = "Bank account info saved successfully.";
            return RedirectToAction(nameof(Index));
        }


        // GET: /Wallet/Deposit
        public IActionResult Deposit()
        {
            return View();
        }

        // POST: /Wallet/Deposit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(DepositViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // 1. Tạo giao dịch Pending
            //Nếu chọn E - Wallet → Gọi PayOS
            if (model.PaymentMethod == "eWallet")
            {
                var transaction = await _transactionService.CreatePendingDeposit(
                    user.Id,
                    model.Amount,
                    model.Description ?? $"Deposit of {model.Amount:C}"
                );

                if (transaction == null)
                {
                    ModelState.AddModelError("", "Failed to create transaction.");
                    return View(model);
                }

                var paymentUrl = await _payOSService.CreatePaymentLink(
                    amount: model.Amount,
                    orderCode: transaction.OrderCode,
                    description: model.Description ?? $"Nạp {model.Amount} vào ví"
                );

                return Redirect(paymentUrl);
            }

            if (model.PaymentMethod == "creditCard")
            {
                var success = await _transactionService.ProcessDepositAsync(
                user.Id,
                model.Amount,
                model.Description ?? $"Deposit of {model.Amount:C}"
            );

                if (success)
                {
                    TempData["SuccessMessage"] = $"Successfully deposited {model.Amount:C} to your wallet.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to process deposit. Please try again.");
                    return View(model);
                }
            }
            // Nếu là CreditCard hoặc BankTransfer → xử lý khác (ví dụ nội bộ hoặc hiện thông báo chưa hỗ trợ)
            ModelState.AddModelError("", "Hiện tại chỉ hỗ trợ E-Wallet thông qua PayOS.");
            return View(model); ;
        
            //var success = await _transactionService.ProcessDepositAsync(
            //    user.Id, 
            //    model.Amount, 
            //    model.Description ?? $"Deposit of {model.Amount:C}"
            //);

            //if (success)
            //{
            //    TempData["SuccessMessage"] = $"Successfully deposited {model.Amount:C} to your wallet.";
            //    return RedirectToAction(nameof(Index));
            //}
            //else
            //{
            //    ModelState.AddModelError("", "Failed to process deposit. Please try again.");
            //    return View(model);
            //}
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDeposit(DepositViewModel model)
        {
            if (!ModelState.IsValid) return View("Deposit", model);

            var user = await _userManager.GetUserAsync(User);
            var orderCode = Guid.NewGuid().ToString();

            var checkoutUrl = await _payOSService.CreatePaymentLink(
                model.Amount,
                model.Description ?? $"Deposit {model.Amount:C}",
                orderCode
            );

            // Optionally: lưu transaction với trạng thái "Pending" trước
            await _transactionService.CreatePendingDeposit(user.Id, model.Amount, orderCode);

            return Redirect(checkoutUrl);
        }


        // GET: /Wallet/Withdraw
        public IActionResult Withdraw()
        {
            return View();
        }

        // POST: /Wallet/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(WithdrawViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (user.WalletBalance < model.Amount)
            {
                ModelState.AddModelError("", "Insufficient funds in your wallet.");
                return View(model);
            }

            var success = await _transactionService.ProcessWithdrawalAsync(
                user.Id, 
                model.Amount, 
                model.Description ?? $"Withdrawal of {model.Amount:C}"
            );

            if (success)
            {
                TempData["SuccessMessage"] = $"Successfully withdrew {model.Amount:C} from your wallet.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Failed to process withdrawal. Please try again.");
                return View(model);
            }
        }

        // GET: /Wallet/TransactionHistory
        public async Task<IActionResult> TransactionHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var transactions = await _transactionService.GetTransactionsByUserIdAsync(user.Id.ToString());
            return View(transactions);
        }
    }
} 