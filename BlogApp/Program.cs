using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Configure password requirements
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login"; // Set the login path for unauthenticated users";
    options.AccessDeniedPath = "/Auth/AccessDenied"; // Set the access denied path for unauthorized users   
    //automatically logout the user after 7 days of inactivity
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    //after 7 days of inactivity, the user will be logged out and will have to log in again
    options.SlidingExpiration = true;
});

var app = builder.Build();

//using block = service scope, to create a scope for the services to be used in the block
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin7@gmail.com";
    string adminPassword = "admin7";

    var existingAdmin = await roleManager.FindByNameAsync("Admin");

    if(existingAdmin == null)
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    var existingUser = await userManager.FindByEmailAsync(adminEmail);
    if(existingUser == null)
    {
        var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };
        await userManager.CreateAsync(adminUser, adminPassword);
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// Serve files from wwwroot (and static web assets). Without this the linked CSS/JS
// files (for example the bootswatch theme under wwwroot/lib/bootswatch) may not be
// served and the theme will not be visible in the browser.
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Post}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
