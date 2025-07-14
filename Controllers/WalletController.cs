using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System.Threading.Tasks;
using StudentFreelance.DbContext;
namespace StudentFreelance.Controllers;
using System.Globalization;


using System.Globalization;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.Helpers;

[Authorize]

public class WalletController : Controller
{
    private readonly IConfiguration _configuration;
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
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _transactionService = transactionService;
        _userManager = userManager;
        _bankAccountService = bankAccountService;
        _payOSService = payOSService;
        _context = context;
        _configuration = configuration;
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
        //Nếu chọn bankTransfer → Gọi PayOS
        if (model.PaymentMethod == "bankTransfer")
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

        if (model.PaymentMethod == "eWallet")
        {
            // Gọi sang action khởi tạo VNPAY
            return RedirectToAction("CreateVnPayPayment", new
            {
                amount = model.Amount,
                description = model.Description
            });
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

    public async Task<IActionResult> CreateVnPayPayment(decimal amount, string? description)
    {
        var config = _configuration.GetSection("VnPay");
        var vnPay = new VnPayLibrary();

        // Tạo mã đơn hàng duy nhất
        var orderId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var amountInVND = ((int)(amount * 100)).ToString("F0", CultureInfo.InvariantCulture);// VNPAY yêu cầu nhân 100

        vnPay.AddRequestData("vnp_Version", "2.1.0");
        vnPay.AddRequestData("vnp_Command", "pay");
        vnPay.AddRequestData("vnp_TmnCode", config["TmnCode"]);
        vnPay.AddRequestData("vnp_Amount", amountInVND);
        vnPay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnPay.AddRequestData("vnp_CurrCode", "VND");

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1")
        {
            ipAddress = "127.0.0.1";
        }
        vnPay.AddRequestData("vnp_IpAddr", ipAddress);
        //vnPay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString());
        vnPay.AddRequestData("vnp_Locale", "vn");
        vnPay.AddRequestData("vnp_OrderInfo", description ?? "Nạp tiền qua VNPAY");
        vnPay.AddRequestData("vnp_OrderType", "other");
        vnPay.AddRequestData("vnp_ReturnUrl", config["ReturnUrl"]);
        vnPay.AddRequestData("vnp_TxnRef", orderId);
        

        // ✅ Truy vấn từ DB
        var type = await _context.TransactionTypes.FirstOrDefaultAsync(t => t.TypeName == "Nạp tiền");
        var status = await _context.TransactionStatuses.FirstOrDefaultAsync(s => s.StatusName == "Đang xử lý");

        if (type == null || status == null)
        {
            return Content("Thiếu TransactionType hoặc TransactionStatus trong database. Hãy kiểm tra bảng dữ liệu.");
        }

        // ✅ Lưu transaction
        var userId = _userManager.GetUserId(User);
        _context.Transactions.Add(new Transaction
        {
            UserID = int.Parse(userId),
            Amount = amount,
            Description = description ?? "Nạp tiền qua VNPAY",
            TypeID = type.TypeID,
            StatusID = status.StatusID,
            TransactionDate = DateTime.Now,
            OrderCode = orderId,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        vnPay.AddRequestData("vnp_SecureHashType", "HMACSHA512");
        // ✅ Tạo URL từ PaymentUrl + HashSecret từ config
        var paymentUrl = vnPay.CreateRequestUrl(config["PaymentUrl"], config["HashSecret"]);

        return Redirect(paymentUrl);
    }


    [HttpGet]
    public async Task<IActionResult> VnPayCallback()
    {
        var config = _configuration.GetSection("VnPay");
        var vnPay = new VnPayLibrary();

        var responseParams = Request.Query;
        foreach (var key in responseParams.Keys)
        {
            if (key.StartsWith("vnp_"))
                vnPay.AddResponseData(key, responseParams[key]);
        }

        var isValid = vnPay.ValidateSignature(config["HashSecret"]);
        if (!isValid)
        {
            return Content("Sai chữ ký hash – giao dịch bị nghi ngờ giả mạo.");
        }

        var orderCode = vnPay.GetResponseData("vnp_TxnRef");
        var responseCode = vnPay.GetResponseData("vnp_ResponseCode");

        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.OrderCode == orderCode);
        if (transaction == null)
        {
            return Content("Không tìm thấy giao dịch.");
        }

        // Lấy status từ DB
        var statusName = responseCode == "00" ? "Thành công" : "Đã hủy";
        var status = await _context.TransactionStatuses.FirstOrDefaultAsync(s => s.StatusName == statusName);

        if (status == null)
        {
            return Content("Không tìm thấy trạng thái phù hợp trong database.");
        }

        transaction.StatusID = status.StatusID;
        await _context.SaveChangesAsync();

        if (statusName == "Thành công")
        {
            // cộng tiền vào WalletBalance
            var user = await _userManager.FindByIdAsync(transaction.UserID.ToString());
            user.WalletBalance += transaction.Amount;
            await _userManager.UpdateAsync(user);
        }

        // ✅ Tạo ViewModel để truyền vào View
        var viewModel = new PaymentResultViewModel
        {
            IsSuccess = statusName == "Thành công",
            OrderCode = orderCode,
            Amount = transaction.Amount,
            TransactionDate = transaction.TransactionDate,
            Description = transaction.Description ?? ""
        };

        return View("PaymentResult", viewModel);
    }

    //[HttpGet]
    //public async Task<IActionResult> VnPayCallback()
    //{
    //    var config = _configuration.GetSection("VnPay");
    //    var vnPay = new VnPayLibrary();

    //    // ✅ B1: Lấy tất cả các tham số vnp_ từ VNPay gửi về
    //    var responseParams = Request.Query;
    //    foreach (var key in responseParams.Keys)
    //    {
    //        if (key.StartsWith("vnp_"))
    //            vnPay.AddResponseData(key, responseParams[key]);
    //    }

    //    // ✅ B2: Kiểm tra chữ ký hash có hợp lệ không
    //    var isValid = vnPay.ValidateSignature(config["HashSecret"]);
    //    if (!isValid)
    //    {
    //        return Content("❌ Sai chữ ký hash – giao dịch bị nghi ngờ giả mạo.");
    //    }

    //    // ✅ B3: Trích xuất các thông tin từ VNPay callback
    //    var orderCode = vnPay.GetResponseData("vnp_TxnRef");
    //    var responseCode = vnPay.GetResponseData("vnp_ResponseCode");

    //    // ✅ B4: Tìm transaction theo mã đơn hàng
    //    var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.OrderCode == orderCode);
    //    if (transaction == null)
    //    {
    //        return Content("❌ Không tìm thấy giao dịch trong hệ thống.");
    //    }

    //    // ✅ B5: Xác định trạng thái dựa theo mã responseCode
    //    string statusName = responseCode switch
    //    {
    //        "00" => "Thành công",
    //        "24" => "Đã hủy",         // Người dùng hủy
    //        _ => "Thất bại"          // Các mã lỗi khác
    //    };

    //    var status = await _context.TransactionStatuses.FirstOrDefaultAsync(s => s.StatusName == statusName);
    //    if (status == null)
    //    {
    //        return Content($"❌ Không tìm thấy trạng thái '{statusName}' trong database.");
    //    }

    //    // ✅ B6: Cập nhật trạng thái cho giao dịch
    //    transaction.StatusID = status.StatusID;
    //    await _context.SaveChangesAsync();

    //    // ✅ B7: Nếu thành công → cộng tiền vào ví
    //    if (statusName == "Thành công")
    //    {
    //        var user = await _userManager.FindByIdAsync(transaction.UserID.ToString());
    //        user.WalletBalance += transaction.Amount;
    //        await _userManager.UpdateAsync(user);
    //    }

    //    // ✅ B8: Trả kết quả ra view (dựa trên status)
    //    ViewBag.Message = statusName switch
    //    {
    //        "Thành công" => "✅ Giao dịch thành công!",
    //        "Đã hủy" => "⚠️ Giao dịch đã bị hủy.",
    //        _ => "❌ Giao dịch thất bại. Vui lòng thử lại sau."
    //    };

    //    return View("PaymentResult"); // Tạo view PaymentResult.cshtml để hiển thị kết quả
    //}



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


