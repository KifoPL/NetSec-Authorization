using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetSec.MVC.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<Seed>();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 0;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;

            options.ClaimsIdentity.RoleClaimType = JwtClaimTypes.Role;
            options.ClaimsIdentity.EmailClaimType = JwtClaimTypes.Email;
            options.ClaimsIdentity.UserNameClaimType = JwtClaimTypes.Name;
            options.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject;
        })
       .AddRoles<IdentityRole>()
       .AddRoleManager<RoleManager<IdentityRole>>()
       .AddUserManager<UserManager<IdentityUser>>()
       .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

var seed = app.Services.CreateScope().ServiceProvider.GetRequiredService<Seed>();
var res = await seed.SeedData();

if (!res.Succeeded)
{
    Console.WriteLine($"{string.Join("\n", res.Errors.Select(x => $"{x.Code} - {x.Description}"))}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) { app.UseMigrationsEndPoint(); }
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default",
    "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();