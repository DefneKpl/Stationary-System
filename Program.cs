using Microsoft.EntityFrameworkCore;
using SmartStationerySystem.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.AppUsers.Any())
    {
        context.AppUsers.AddRange(
            new SmartStationerySystem.Models.AppUser
            {
                FullName = "Öğrenci Kullanıcı",
                Email = "ogrenci@mail.com",
                Password = "123456",
                Role = "Student"
            },
            new SmartStationerySystem.Models.AppUser
            {
                FullName = "Kırtasiyeci Kullanıcı",
                Email = "kirtasiyeci@mail.com",
                Password = "123456",
                Role = "Stationer"
            }
        );

        context.SaveChanges();
    }
}

app.Run();