using AcmePortal.Web.Data;
using AcmePortal.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var connectionString = builder.Configuration.GetConnectionString("Portal")
    ?? "Data Source=acme-portal.db";
builder.Services.AddSingleton(new Database(connectionString));
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

app.Services.GetRequiredService<Database>().Initialize();

// Legacy: detailed error pages are shown in every environment.
app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
