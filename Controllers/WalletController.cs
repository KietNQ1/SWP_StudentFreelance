using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;
using StudentFreelance.Models.PayOS;
using StudentFreelance.Models.Constants;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StudentFreelance.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PayOSConfig _payOSConfig;

        public WalletController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IOptions<PayOSConfig> payOSOptions)
        {
            _db = db;
            _userManager = userManager;
            _payOSConfig = payOSOptions.Value;
        }

        public IActionResult Index() => RedirectToAction("Deposit");

        public async Task<IActionResult> Deposit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !_db.BankAccounts.Any(b => b.UserID == user.Id && b.IsActive))
            {
                return RedirectToAction("Add", "BankAccount", new { message = "Vui lòng thêm tài khoản ngân hàng trước khi nạp tiền." });
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeposit(decimal amount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var hasBankAccount = await _db.BankAccounts.AnyAsync(b => b.UserID == user.Id && b.IsActive);
            if (!hasBankAccount)
                return BadRequest("Bạn cần thêm tài khoản ngân hàng trước khi nạp tiền.");

            var transaction = new Transaction
            {
                UserID = user.Id,
                Amount = amount,
                TypeID = TransactionTypeConstants.Deposit,
                StatusID = TransactionStatusConstants.Pending,
                TransactionDate = DateTime.Now,
                Description = $"Nạp {amount:N0} VNĐ vào ví qua PayOS"
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            var payRequest = new
            {
                orderCode = transaction.TransactionID,
                amount = (int)amount,
                description = transaction.Description,
                returnUrl = _payOSConfig.ReturnUrl,
                cancelUrl = _payOSConfig.CancelUrl
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _payOSConfig.ApiKey);
            var content = new StringContent(JsonSerializer.Serialize(payRequest), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api-sandbox.payos.vn/v1/payment-requests", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return Content("Lỗi tạo link thanh toán: " + json);

            var result = JsonDocument.Parse(json);
            var checkoutUrl = result.RootElement.GetProperty("checkoutUrl").GetString();

            return Redirect(checkoutUrl);
        }

        public async Task<IActionResult> DepositSuccess(int orderCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var transaction = await _db.Transactions
                .FirstOrDefaultAsync(t => t.TransactionID == orderCode && t.UserID == user.Id);

            if (transaction == null)
                return Content("Không tìm thấy giao dịch.");

            if (transaction.StatusID != TransactionStatusConstants.Pending)
                return Content("Giao dịch đã xử lý trước đó.");

            transaction.StatusID = TransactionStatusConstants.Success;
            transaction.TransactionDate = DateTime.Now;

            user.WalletBalance += transaction.Amount;

            await _db.SaveChangesAsync();

            return Content($"✅ Nạp tiền thành công! Số dư ví hiện tại: {user.WalletBalance:N0} VNĐ.");
        }

        public IActionResult DepositCancel() => Content("Bạn đã huỷ giao dịch.");

        public async Task<IActionResult> Withdraw()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !_db.BankAccounts.Any(b => b.UserID == user.Id && b.IsActive))
            {
                return RedirectToAction("Add", "BankAccount", new { message = "Vui lòng thêm tài khoản ngân hàng trước khi rút tiền." });
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestWithdraw(decimal amount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (amount <= 0)
                return BadRequest("Số tiền không hợp lệ.");

            if (user.WalletBalance < amount)
                return BadRequest("❌ Số dư không đủ để rút.");

            var hasBankAccount = await _db.BankAccounts.AnyAsync(b => b.UserID == user.Id && b.IsActive);
            if (!hasBankAccount)
                return BadRequest("Bạn cần thêm tài khoản ngân hàng trước khi rút tiền.");

            var transaction = new Transaction
            {
                UserID = user.Id,
                Amount = amount,
                TypeID = TransactionTypeConstants.Withdraw,
                StatusID = TransactionStatusConstants.Pending,
                TransactionDate = DateTime.Now,
                Description = $"Yêu cầu rút {amount:N0} VNĐ từ ví"
            };

            user.WalletBalance -= amount;

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            return Content($"✅ Đã ghi nhận yêu cầu rút {amount:N0} VNĐ. Admin sẽ duyệt sau.");
        }

        public async Task<IActionResult> History(int? typeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var query = _db.Transactions.Where(t => t.UserID == user.Id);

            if (typeId.HasValue)
            {
                query = query.Where(t => t.TypeID == typeId.Value);
            }

            var result = await query
                .Include(t => t.Type)
                .Include(t => t.Status)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            ViewBag.CurrentTypeId = typeId;
            ViewBag.TransactionTypes = await _db.TransactionTypes.Where(t => t.IsActive).ToListAsync();

            return View(result);
        }
    }
}
