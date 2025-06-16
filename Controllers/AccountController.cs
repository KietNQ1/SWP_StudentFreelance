using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using StudentFreelance.Models;
using StudentFreelance.Models.ViewModels;
using StudentFreelance.ViewModels;

namespace StudentFreelance.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly StudentFreelance.Services.Interfaces.IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, StudentFreelance.Services.Interfaces.IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

       
        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = true,
                StatusID = 1 // Mặc định trạng thái "Hoạt động"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Gán role mặc định (ví dụ: "Student")
                await _userManager.AddToRoleAsync(user, model.Role);
                TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
                // ❌ Không đăng nhập ngay
                // ✅ Chuyển sang trang đăng nhập
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        //Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Đăng nhập không hợp lệ.");
            return View(model);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
        //Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        //Forgot pass
        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null )
            {
                // Không tiết lộ thông tin tồn tại vì lý do bảo mật
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

            // Gửi email
            var subject = "Reset your password";
            var body = $"Vui lòng bấm vào link sau để đặt lại mật khẩu của bạn:<br/><a href='{resetLink}'>{resetLink}</a>";
            await _emailSender.SendEmailAsync(model.Email, subject, body);

            return RedirectToAction("ForgotPasswordConfirmation");
        }


        // GET: /Account/ForgotPasswordConfirmation
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return BadRequest("Link không hợp lệ.");

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["SuccessMessage"] = "Mật khẩu đã được cập nhật.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Mật khẩu đã được cập nhật thành công.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }



    }
}


