using _1000Problems.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "NOT_CONFIGURED";

builder.Services.AddSingleton(new ApplicationRepository(connectionString));

var app = builder.Build();

// Initialize database in background - don't block app startup
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ApplicationRepository>();
        await repo.EnsureTableExistsAsync();
        app.Logger.LogInformation("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to initialize database on startup. App will continue without DB.");
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
