using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectApprovalSystem.Data;
using ProjectApprovalSystem.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseMigrationsEndPoint(); } else { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
using (var scope = app.Services.CreateScope()) { var services = scope.ServiceProvider; try { await DbInitializer.InitializeAsync(services); } catch (Exception ex) { var logger = services.GetRequiredService<ILogger<Program>>(); logger.LogError(ex, "Seeding failed."); } }
app.Run();
