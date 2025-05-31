using MudBlazor.Services;
using nbaplayers.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<NBAPlayerService>();
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // First, connect to the default 'postgres' database to handle database creation
        var masterConnectionString = connectionString?.Replace("Database=nbaplayers", "Database=postgres");
        
        using (var connection = new NpgsqlConnection(masterConnectionString))
        {
            connection.Open();
            
            // Terminate existing connections to the database
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT pg_terminate_backend(pg_stat_activity.pid)
                    FROM pg_stat_activity
                    WHERE pg_stat_activity.datname = 'nbaplayers'
                    AND pid <> pg_backend_pid();";
                command.ExecuteNonQuery();
            }
            
            // Drop the database if it exists and create a new one
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    DROP DATABASE IF EXISTS nbaplayers;
                    CREATE DATABASE nbaplayers;";
                command.ExecuteNonQuery();
            }
        }

        // Now use EF Core to create tables and apply migrations
        context.Database.Migrate();
        
        Console.WriteLine("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        Console.WriteLine($"Database initialization error: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
