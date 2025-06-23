using BookinhMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Add DbContext with SQL Server
builder.Services.AddDbContext<BookingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed default admin and CSKH accounts
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingContext>();
    var passwordHasher = new PasswordHasher<NguoiDung>();

    // Seed admin account if not exists

    // Seed CSKH account if not exists
    if (!db.CsKhs.Any(u => u.Username == "cskh1"))
    {
        db.CsKhs.Add(new CsKh
        {
            Username = "cskh1",
            Password = "cskh123456",
            FullName = "Chăm sóc khách hàng",
            Email = "cskh1@fourrock.com",
            Phone = "0123456789",
            CreatedAt = DateTime.Now
        });
        db.SaveChanges();
    }
}

// Error handling and environment config
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled exception: {ex}");
            throw;
        }
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Ensure UseSession() is called before UseRouting()
app.UseSession();
app.UseRouting();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();
app.MapRazorPages();

app.Run();