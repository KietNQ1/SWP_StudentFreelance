using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure EF Core to use SQL Server and your connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. Apply any pending migrations at startup and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbSeeder.SeedEnums(db);
    DbSeeder.SeedSampleData(db);
}

// 4. Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
