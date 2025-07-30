using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PoliMarket.Components.Authorization;
using PoliMarket.Components.Authentication;
using PoliMarket.Components.Products;
using PoliMarket.Components.Customers;
using PoliMarket.Components.Infrastructure;
using PoliMarket.Components.Infrastructure.Data;
using PoliMarket.Components.Sales;
using PoliMarket.Components.Inventory;
using PoliMarket.Components.Delivery;
using PoliMarket.Contracts;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to handle circular references
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        // Use camelCase naming policy to match frontend expectations
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure Entity Framework with database from environment
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=polimarket.db";

builder.Services.AddDbContext<PoliMarketDbContext>(options =>
    options.UseSqlite(connectionString));

// Add HTTP context accessor for audit service
builder.Services.AddHttpContextAccessor();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PoliMarket CBSE API",
        Version = "v1.0.0",
        Description = @"
        ## Component-Based Software Engineering API for PoliMarket System

        This API implements a **Component-Based Software Engineering (CBSE)** architecture with the following functional requirements:

        ### 🔐 **RF1: Authorization Component** (`/api/Autorizacion`)
        - Seller authorization and validation
        - HR employee management
        - Permission control

        ### 💰 **RF2: Sales Component** (`/api/Ventas`)
        - Sales transaction processing
        - Total calculation with taxes and discounts
        - Sales history and management

        ### 📦 **RF3: Inventory Component** (`/api/Inventario`)
        - Stock availability checking
        - Inventory movement tracking
        - Stock alerts and reservations

        ### 🔗 **Integration Component** (`/api/Integracion`)
        - Business transaction orchestration
        - System health monitoring
        - Component synchronization

        ### 🏗️ **Architecture Principles**
        - **Separation of Concerns**: Each component has single responsibility
        - **Interface-Based Design**: All components implement contracts
        - **Dependency Injection**: Loose coupling through DI container
        - **Reusability**: Components designed for reuse across projects
        - **Composability**: Components can be combined for complex operations
        ",
        Contact = new OpenApiContact
        {
            Name = "PoliMarket Development Team",
            Email = "dev@polimarket.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Enable XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, true);
    }

    // Group endpoints by tags
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((name, api) => true);
});

// Configure CORS from environment variables
var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")?.Split(',')
                    ?? new[] { "http://localhost:4200", "http://localhost:3001" };

var allowCredentials = bool.Parse(Environment.GetEnvironmentVariable("CORS_ALLOW_CREDENTIALS") ?? "true");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClients", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();

        if (allowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// Register Component Services (CBSE Architecture)
RegisterComponentServices(builder.Services);

var app = builder.Build();

// Add startup logging
app.Logger.LogInformation("Starting PoliMarket CBSE API...");

// Initialize database
await InitializeDatabaseAsync(app);

app.Logger.LogInformation("Database initialization completed");

// Configure the HTTP request pipeline
// Enable Swagger in all environments for this demo
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PoliMarket CBSE API v1");
    c.RoutePrefix = "swagger"; // Serve Swagger UI at /swagger/index.html
    c.DocumentTitle = "PoliMarket CBSE API Documentation";
    c.DefaultModelsExpandDepth(-1); // Hide models section by default
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
    c.EnableValidator();
});

// Add redirect from root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

// Configure middleware pipeline in correct order
app.UseCors("AllowClients");

// app.UseHttpsRedirection(); // Disabled for development
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Add API info endpoint
app.MapGet("/api", () => new
{
    Service = "PoliMarket CBSE API",
    Version = "v1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow,
    Documentation = "/swagger/index.html",
    HealthCheck = "/api/Integracion/health"
});

app.Logger.LogInformation("PoliMarket CBSE API is ready and listening...");

app.Run();

// Helper methods
static void RegisterComponentServices(IServiceCollection services)
{
    // Authentication Component
    services.AddScoped<IAuthenticationComponent, AuthenticationComponent>();

    // Authorization Component
    services.AddScoped<IAutorizacionComponent, AutorizacionComponent>();
    services.AddScoped<IAutorizacionRepository, AutorizacionDbRepository>();

    // Products Component
    services.AddScoped<PoliMarket.Components.Products.IProductosComponent, ProductosComponent>();

    // Customers Component
    services.AddScoped<ICustomersComponent, CustomersComponent>();

    // Sales Component
    services.AddScoped<IVentasComponent, VentasComponent>();
    services.AddScoped<IVentasRepository, VentasDbRepository>();

    // Inventory Component
    services.AddScoped<IInventarioComponent, InventarioComponent>();

    // Delivery Component
    services.AddScoped<IEntregasComponent, EntregasComponent>();

    // Infrastructure Components
    services.AddScoped<IIntegracionComponent, IntegracionComponent>();

    // Infrastructure Services
    services.AddScoped<PoliMarket.Components.Infrastructure.Services.IAuditService, PoliMarket.Components.Infrastructure.Services.AuditService>();

    // Database Seeder
    services.AddScoped<DatabaseSeeder>();

    // TODO: Add other components as they are implemented
    // services.AddScoped<INotificacionesComponent, NotificacionesComponent>();
}

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PoliMarketDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

    try
    {
        await context.Database.EnsureCreatedAsync();
        app.Logger.LogInformation("Database initialized successfully");

        // Seed database with test data
        await seeder.SeedAsync();
        app.Logger.LogInformation("Database seeded successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error initializing database");
    }
}
