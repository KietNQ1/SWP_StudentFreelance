using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;

namespace StudentFreelance.Controllers

{
    [Authorize]

    public class WalletController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IPayOSService _payOSService;
        private readonly ITransactionService _transactionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public WalletController(
            ITransactionService transactionService,
            UserManager<ApplicationUser> userManager,
            IBankAccountService bankAccountService,
            IPayOSService payOSService,
            ApplicationDbContext context)
        {
            _transactionService = transactionService;
            _userManager = userManager;
            _bankAccountService = bankAccountService;
            _payOSService = payOSService;
            _context = context;
        }


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
                // orderCode phải là số nguyên duy nhất, dùng timestamp kiểu long
                long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var transaction = await _transactionService.CreatePendingDeposit(
                    user.Id,
                    model.Amount,
                    model.Description ?? $"Deposit of {model.Amount:C}",
                    orderCode
                );

                if (transaction == null)
                {
                    ModelState.AddModelError("", "Failed to create transaction.");
                    return View(model);
                }

                //var paymentUrl = await _payOSService.CreatePaymentLink(
                //    amount: model.Amount,
                //    orderCode: orderCode, // truyền orderCode là long
                //    description: model.Description ?? $"Nạp {model.Amount} vào ví"
                //);

                // Tạo returnUrl và cancelUrl động từ Url.Action
                var returnUrl = Url.Action("DepositSuccess", "Wallet", new { orderCode }, Request.Scheme);
                var cancelUrl = Url.Action("DepositCancel", "Wallet", new { orderCode }, Request.Scheme);

                var paymentUrl = await _payOSService.CreatePaymentLink(
                    amount: model.Amount,
                    orderCode: orderCode,
                    description: model.Description ?? $"Nạp {model.Amount} vào ví",
                    returnUrl: returnUrl,
                    cancelUrl: cancelUrl
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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CreateDeposit(DepositViewModel model)
        //{
        //    if (!ModelState.IsValid) return View("Deposit", model);

        //    var user = await _userManager.GetUserAsync(User);
        //    // orderCode phải là số nguyên duy nhất, dùng timestamp kiểu long
        //    long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        //    var checkoutUrl = await _payOSService.CreatePaymentLink(
        //        model.Amount,
        //        model.Description ?? $"Deposit {model.Amount:C}",
        //        orderCode
        //    );

        //    // Optionally: lưu transaction với trạng thái "Pending" trước
        //    await _transactionService.CreatePendingDeposit(user.Id, model.Amount, model.Description, orderCode);

        //    return Redirect(checkoutUrl);
        //}


        //mới thêm 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDeposit(DepositViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Deposit", model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // ✅ Tạo returnUrl và cancelUrl từ Url.Action
            var returnUrl = Url.Action("DepositSuccess", "Wallet", new { orderCode }, Request.Scheme);
            var cancelUrl = Url.Action("DepositCancel", "Wallet", new { orderCode }, Request.Scheme);

            // ✅ Gọi PayOS có returnUrl/cancelUrl
            var checkoutUrl = await _payOSService.CreatePaymentLink(
                model.Amount,
                orderCode,
                model.Description ?? $"Deposit {model.Amount:C}",
                returnUrl,
                cancelUrl
            );

            // ✅ Lưu giao dịch với trạng thái Pending
            await _transactionService.CreatePendingDeposit(user.Id, model.Amount, model.Description, orderCode);

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
        
        // GET: /Wallet/TransactionDetail/5
        public async Task<IActionResult> TransactionDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            
            // Check if transaction exists and belongs to the current user
            if (transaction == null || transaction.UserID != user.Id)
            {
                return NotFound();
            }
            
            // Get associated transaction history record if available
            var transactionHistory = await _context.UserAccountHistories
                .FirstOrDefaultAsync(h => h.ActionType == "TRANSACTION" && 
                                         h.Description.Contains(id.ToString()) && 
                                         h.UserID == user.Id);
            
            if (transactionHistory != null)
            {
                ViewBag.TransactionHistory = transactionHistory;
            }
            
            return View(transaction);
        }

        // GET: /Wallet/DepositSuccess
        //public IActionResult DepositSuccess(string orderCode)
        //{
        //    // TODO: Có thể kiểm tra trạng thái giao dịch với PayOS hoặc cập nhật trạng thái giao dịch trong DB nếu cần
        //    ViewBag.OrderCode = orderCode;
        //    ViewBag.Message = "Nạp tiền thành công!";
        //    return View();
        //}

        // GET: /Wallet/DepositSuccess
        public async Task<IActionResult> DepositSuccess(string orderCode)
        {
            if (!long.TryParse(orderCode, out var orderCodeLong))
            {
                ViewBag.Message = "Mã giao dịch không hợp lệ.";
                return View();
            }

            // ✅ Xác nhận và cộng tiền vào ví nếu chưa xử lý
            var confirmed = await _transactionService.ConfirmDepositFromPayOS(orderCodeLong);

            ViewBag.OrderCode = orderCode;
            ViewBag.Message = confirmed
                ? "Nạp tiền thành công!"
                : "Nạp tiền thất bại hoặc giao dịch đã được xử lý.";

            return View();
        }


        // GET: /Wallet/DepositCancel
        public IActionResult DepositCancel(string orderCode)
        {
            ViewBag.OrderCode = orderCode;
            ViewBag.Message = "Bạn đã hủy giao dịch nạp tiền.";
            return View();
        }
    }
} 

