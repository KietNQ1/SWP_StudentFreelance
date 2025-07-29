using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Data;
using StudentFreelance.Models.Email;
using StudentFreelance.Models.PayOS;
using StudentFreelance.Services.Implementations;
using StudentFreelance.Middleware;
using DinkToPdf;
using DinkToPdf.Contracts;

using System.IO;
using StudentFreelance.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Load native library libwkhtmltox.dll
var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "DinkToPdfLib", "libwkhtmltox.dll"));

// Đăng ký dịch vụ DinkToPdf
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddSignalR();              // Đăng ký SignalR
// 1. Configure Entity Framework Core (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Identity with custom ApplicationUser and IdentityRole<int>
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
  
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Cấu hình khóa tài khoản
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); 
    options.Lockout.MaxFailedAccessAttempts = 5; 
    options.Lockout.AllowedForNewUsers = true;  
});


// 3. Configure Authentication Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});
// 3b. Add Google Authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
        // N?u mu?n l?y th�ng tin profile c th? dng scope:
        // options.Scope.Add("profile");
        // options.Scope.Add("email");
    });

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<StudentFreelance.Services.Interfaces.IEmailSender, StudentFreelance.Services.Implementations.GmailEmailSender>();

// Register application services
builder.Services.AddHttpClient<StudentFreelance.Services.Interfaces.ILocationApiService, StudentFreelance.Services.Implementations.LocationApiService>();
builder.Services.AddScoped<StudentFreelance.Services.Interfaces.IProjectService, StudentFreelance.Services.Implementations.ProjectService>();
builder.Services.AddScoped<StudentFreelance.Services.Interfaces.IReportService, StudentFreelance.Services.Implementations.ReportService>();
builder.Services.AddScoped<StudentFreelance.Services.Interfaces.IApplicationService, StudentFreelance.Services.Implementations.ApplicationService>();
builder.Services.AddScoped<StudentFreelance.Services.Interfaces.INotificationService, StudentFreelance.Services.Implementations.NotificationService>();
builder.Services.AddScoped<StudentFreelance.Services.Interfaces.IProjectSubmissionService, StudentFreelance.Services.Implementations.ProjectSubmissionService>();
builder.Services.AddScoped<StudentFreelance.Services.Interfaces.ITransactionService, StudentFreelance.Services.Implementations.TransactionService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<StudentFreelance.Services.Interfaces.IAdvertisementService, StudentFreelance.Services.Implementations.AdvertisementService>();
//builder.Services.AddScoped<IPayOSService, PayOSService>();
builder.Services.AddHttpClient<IPayOSService, PayOSService>();
builder.Services.Configure<PayOSConfig>(
builder.Configuration.GetSection("PayOS"));

// Đăng ký cấu hình PayOS
builder.Services.Configure<PayOSConfig>(builder.Configuration.GetSection("PayOS"));
// 4. Add MVC support
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

var app = builder.Build();

// 5. Run database migration and seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    //db.Database.Migrate();

    DbSeeder.SeedEnums(db); // enum tables: statuses, types, etc.
    await DbSeeder.SeedSampleDataAsync(db, userManager, roleManager); // Identity + related data
}

// 6. Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Ensure upload directories exist
var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads");
var advertisementsPath = Path.Combine(uploadsPath, "advertisements");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
if (!Directory.Exists(advertisementsPath))
{
    Directory.CreateDirectory(advertisementsPath);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();// Identity: must come before UseAuthorization
app.UseMiddleware<AccountStatusMiddleware>(); 
app.UseAuthorization();

// 7. Configure default routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// Route cho SignalR hub
app.MapHub<StudentFreelance.Hubs.ChatHub>("/chathub");
app.Run();


