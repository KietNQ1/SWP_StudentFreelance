using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using StudentFreelance.Models;   // Namespace chứa ApplicationUser

namespace StudentFreelance.Middleware
{
    
    public class AccountStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public AccountStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
                                      UserManager<ApplicationUser> userManager,
                                      SignInManager<ApplicationUser> signInManager)
        {

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user is not null && !user.IsActive)
                {
                    
                    await signInManager.SignOutAsync();

                    
                    context.Response.Redirect("/Account/Login?locked=true");
                    return;                            
                }
            }

            
            await _next(context);
        }
    }
}
