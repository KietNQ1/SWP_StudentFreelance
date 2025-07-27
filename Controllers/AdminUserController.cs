using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.Models.Enums;
using System.Threading.Tasks;
using System.Linq;
using StudentFreelance.DbContext;

namespace StudentFreelance.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ITransactionService _transactionService;
        private readonly ApplicationDbContext _context;

        public AdminUserController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole<int>> roleManager,
            ITransactionService transactionService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _transactionService = transactionService;
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, string selectedRole, string status, int pageNumber = 1, int pageSize = 10)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u =>
                    u.FullName.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) || u.PhoneNumber.Contains(searchTerm));
            }

            if (status == "Active")
                users = users.Where(u => u.IsActive);
            else if (status == "Inactive")
                users = users.Where(u => !u.IsActive);

            if (!string.IsNullOrEmpty(selectedRole))
            {
                var userIds = (await _userManager.GetUsersInRoleAsync(selectedRole)).Select(u => u.Id).ToList();
                users = users.Where(u => userIds.Contains(u.Id));
            }

            var totalUsers = await users.CountAsync(); 

            var pagedUsers = await users
                .OrderBy(u => u.FullName) 
                .Skip((pageNumber - 1) * pageSize) 
                .Take(pageSize)
                .ToListAsync();

           
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.AllRoles = allRoles;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            return View(pagedUsers);


          
        }

        // Sửa người dùng
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.AllRoles = allRoles;
            ViewBag.CurrentRole = userRoles.FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, ApplicationUser updatedUser, string selectedRole)
        {
            if (id != updatedUser.Id.ToString()) return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FullName = updatedUser.FullName;
            user.Email = updatedUser.Email;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.IsActive = updatedUser.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(updatedUser);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, selectedRole);

            return RedirectToAction(nameof(Index));
        }
    
       
        [HttpPost]
        public async Task<IActionResult> Deactivate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        // ✅ Khôi phục
        [HttpPost]
        public async Task<IActionResult> Restore(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Payments(string searchUser, int? statusId)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            
            // Filter by status if provided
            if (statusId.HasValue)
            {
                transactions = transactions.Where(t => t.StatusID == statusId.Value);
            }
            
            // Filter by user if provided
            if (!string.IsNullOrEmpty(searchUser))
            {
                transactions = transactions.Where(t => 
                    t.User.FullName.Contains(searchUser) || 
                    t.User.Email.Contains(searchUser));
            }
            
            // Get all transaction statuses for dropdown
            var statuses = await _transactionService.GetAllTransactionStatusesAsync();
            ViewBag.Statuses = statuses;
            
            return View(transactions.OrderByDescending(t => t.TransactionDate).ToList());
        }
        
        [HttpPost]
        public async Task<IActionResult> CancelTransaction(int transactionId)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(transactionId);
            
            if (transaction == null)
                return NotFound();
                
            if (transaction.StatusID != 1) // Only pending transactions can be canceled
                return BadRequest("Only pending transactions can be canceled");
                
            // Update status to Cancelled (assuming StatusID 3 is for Cancelled)
            await _transactionService.UpdateTransactionStatusAsync(transactionId, 3);
            
            return RedirectToAction(nameof(Payments));
        }
        
        [HttpPost]
        public async Task<IActionResult> ProcessTransaction(int transactionId)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(transactionId);
            
            if (transaction == null)
                return NotFound();
                
            if (transaction.StatusID != 1) // Only pending transactions can be processed
                return BadRequest("Only pending transactions can be processed");
            
            // For deposit transactions, we need to update the user's wallet balance
            if (transaction.TypeID == 1) // Deposit
            {
                await _transactionService.ConfirmDepositFromPayOS(long.Parse(transaction.OrderCode));
            }
            else
            {
                // For other transaction types, just update the status to Completed (StatusID 2)
                await _transactionService.UpdateTransactionStatusAsync(transactionId, 2);
            }
            
            return RedirectToAction(nameof(Payments));
        }

        // GET: /AdminUser/TransactionDetail/5
        [HttpGet]
        public async Task<IActionResult> TransactionDetail(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            
            if (transaction == null)
            {
                return NotFound();
            }
            
            // Get associated transaction history record if available
            var transactionHistory = await _context.UserAccountHistories
                .FirstOrDefaultAsync(h => h.ActionType == "TRANSACTION" && 
                                         h.Description.Contains(id.ToString()) && 
                                         h.UserID == transaction.UserID);
            
            if (transactionHistory != null)
            {
                ViewBag.TransactionHistory = transactionHistory;
            }
            
            return View(transaction);
        }
    }
}
