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
        private readonly ITransactionService _transactionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public WalletController(
            ITransactionService transactionService, 
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _transactionService = transactionService;
            _userManager = userManager;
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
    }
} 