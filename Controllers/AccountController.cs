using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using StudentFreelance.DbContext;
using StudentFreelance.Services.Interfaces;

namespace StudentFreelance.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly StudentFreelance.Services.Interfaces.IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            StudentFreelance.Services.Interfaces.IEmailSender emailSender,
            ApplicationDbContext context,
            INotificationService notificationService
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
            _notificationService = notificationService;
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
                StatusID = 1,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Gán role mặc định (ví dụ: "Student")
                await _userManager.AddToRoleAsync(user, model.Role);

                // Gửi email chào mừng sau khi đăng ký
                var subject = "Chào mừng bạn đến với StudentFreelance!";
                var body = $@"
            <p>Xin chào <strong>{user.FullName}</strong>,</p>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>StudentFreelance</strong>.</p>
            <p>Bạn có thể đăng nhập ngay tại đây: 
                <a href='{Url.Action("Login", "Account", null, Request.Scheme)}'>Đăng nhập</a>.
            </p>
            <p>Chúc bạn một ngày tốt lành!</p>
        ";

                await _emailSender.SendEmailAsync(user.Email, subject, body);

                // Gửi notification hệ thống cho user mới
                await _notificationService.SendNotificationToUserAsync(
                    user.Id,
                    "Chào mừng bạn đến với StudentFreelance!",
                    "Cảm ơn bạn đã đăng ký tài khoản. Hãy cập nhật hồ sơ để bắt đầu nhận dự án.",
                    1 // TypeID: hệ thống (bạn có thể điều chỉnh cho đúng với DB)
                );
                TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
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

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa tạm thời. Vui lòng thử lại sau.");
            }
            else
            {
                ModelState.AddModelError("", "Đăng nhập không hợp lệ.");
            }

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

        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // Xử lý callback từ Google
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"External provider error: {remoteError}");
                return RedirectToAction("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            // Đăng nhập nếu đã có tài khoản
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (signInResult.Succeeded)
            {
                return Redirect(returnUrl);
            }
            else
            {
                // Nếu chưa có, tạo mới user
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Lấy tên nếu có
                    var name = info.Principal.FindFirstValue(ClaimTypes.Name);

                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = name,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsActive = true,
                        StatusID = 1
                    };
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        // Gán role mặc định
                        await _userManager.AddToRoleAsync(user, "Student"); // hoặc "Business" tùy logic của bạn
                        await _userManager.AddLoginAsync(user, info);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Lỗi tạo tài khoản với Google.");
                        return RedirectToAction("Login");
                    }
                }
                else
                {
                    await _userManager.AddLoginAsync(user, info);
                }

                // Kiểm tra và tạo địa chỉ nếu chưa có
                if (user.AddressID == null)
                {
                    var address = new Address
                    {
                        ProvinceCode = "01",
                        ProvinceName = "Hà Nội",
                        DistrictCode = "001",
                        DistrictName = "Quận Ba Đình",
                        WardCode = "00001",
                        WardName = "Phường Phúc Xá",
                        DetailAddress = "Số nhà mặc định",
                        FullAddress = "Số nhà mặc định, Phường Phúc Xá, Quận Ba Đình, Hà Nội",
                        IsActive = true
                    };
                    
                    _context.Addresses.Add(address);
                    await _context.SaveChangesAsync();
                    
                    user.AddressID = address.AddressID;
                    await _userManager.UpdateAsync(user);
                }

                // Đăng nhập
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect(returnUrl);
            }
        }

    }
}


