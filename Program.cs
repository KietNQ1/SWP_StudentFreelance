using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Data;
using StudentFreelance.Models.Email;
using StudentFreelance.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

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
        // N?u mu?n l?y thêm thông tin profile có th? dùng scope:
        // options.Scope.Add("profile");
        // options.Scope.Add("email");
    });

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<StudentFreelance.Services.Interfaces.IEmailSender, StudentFreelance.Services.Implementations.GmailEmailSender>();


// 4. Add MVC support
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. Run database migration and seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    db.Database.Migrate();

    DbSeeder.SeedEnums(db); // enum tables: statuses, types, etc.
    await DbSeeder.SeedSampleDataAsync(db, userManager, roleManager); // Identity + related data
}

// 6. Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Identity: must come before UseAuthorization
app.UseAuthorization();

// 7. Configure default routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
