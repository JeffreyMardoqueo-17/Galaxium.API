using System.Text;
using CloudinaryDotNet;
using Galaxium.API.Data;
using Galaxium.API.Middlewares;
using Galaxium.API.Repository.Interfaces;
using Galaxium.API.Repository.Repos;
using Galaxium.API.Services.Interfaces;
using Galaxium.API.Services.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Galaxium.API.Entities;
using Galaxium.Api.Entities;
using GalaxiumERP.API.Repository.repos;
using Galaxium.Api.Services.service;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Services.service;
using Galaxium.Api.Repository.repos;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.API.Repository.repos;
using Galaxium.Api.Services.Service;
using FluentValidation;
using Galaxium.Api.Validators;
using Galaxium.Api.Services;
using Galaxium.Api.Services.Rules;
using Galaxium.Api.Services.Implementations;
using Galaxium.API.Repositories.Implementations;
using Galaxium.Api.Repository.Repositories;
using Galaxium.Api.Services.Services;
using Galaxium.Api.Mappings;
using Microsoft.Extensions.Hosting;
using Galaxium.Api.Services.service.StockMovements;
using Galaxium.Api.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Galaxium.Api.Services.AI;
using Galaxium.Api.Services.AI.Core;
using Galaxium.Api.Services.AI.Interfaces;
using Galaxium.Api.Shared.MultiTenant;
using Galaxium.Api.Features.Tenants.Repositories;
using Galaxium.Api.Features.Tenants.Services;
using Galaxium.API.Services.service;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var runningInDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (!builder.Environment.IsDevelopment())
{
    var keysPath = "/app/dataprotection-keys";
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

var defaultConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

var isDesignTime = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "DesignTime"
    || Environment.GetEnvironmentVariable("EF_DESIGN_TIME") == "1";
if (string.IsNullOrWhiteSpace(defaultConnection) && !isDesignTime)
{
    throw new InvalidOperationException(
        "Falta la variable de entorno 'ConnectionStrings__DefaultConnection'. " +
        "Configúrala en Docker Compose o en tu entorno antes de iniciar la API.");
}

builder.Services.AddDbContext<GalaxiumDbContext>(options =>
    options.UseNpgsql(defaultConnection, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }));

// ==========================
// MultiTenant
// ==========================
builder.Services.AddMultiTenant();

builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Tenant CRUD
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ITenantService, TenantService>();

// Auth & Users
builder.Services.AddScoped<IRoleRespository, RolRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Products
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<ICategoryCodeGenerator, CategoryCodeGenerator>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductFilterRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISkuGenerator, SkuGenerator>();

// Customers
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

// Inventory
builder.Services.AddScoped<IStockEntryRepository, StockEntryRepository>();
builder.Services.AddScoped<IStockEntryService, StockEntryService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IStockMovementHandler, PurchaseStockMovementHandler>();
builder.Services.AddScoped<IStockMovementHandler, SaleStockMovementHandler>();
builder.Services.AddScoped<IStockMovementHandler, AdjustmentStockMovementHandler>();
builder.Services.AddScoped<IStockMovementHandler, ReturnStockMovementHandler>();
builder.Services.AddScoped<IStockMovementHandlerFactory, StockMovementHandlerFactory>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IStockAlertService, StockAlertService>();

// Validators
builder.Services.AddScoped<IValidator<Customer>, CustomerValidator>();

// Product Photos
builder.Services.AddScoped<IProductPhotoRepository, ProductPhotoRepository>();
builder.Services.AddScoped<IProductPhotoService, ProductPhotoService>();

// Sales
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISalePdfService, SalePdfService>();
builder.Services.AddScoped<ISaleReportingService, SaleReportingService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISaleDetailService, SaleDetailService>();
builder.Services.AddScoped<ISaleDetailRepository, SaleDetailRepository>();

// Dashboard
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Business Rules
builder.Services.AddScoped<StockEntryRules>();
builder.Services.AddScoped<SaleRules>();
builder.Services.AddScoped<SaleDetailsRules>();

// AI Copilot
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.Configuration = redisConnection;
    options.InstanceName = "GalaxiumAI:";
});
builder.Services.AddHttpClient<Galaxium.Api.Services.AI.Core.GeminiProvider>();
builder.Services.AddSingleton<IToolRegistry, Galaxium.Api.Services.AI.Core.ToolRegistry>();
builder.Services.AddScoped<IAIProvider, Galaxium.Api.Services.AI.Core.GeminiProvider>();
builder.Services.AddScoped<IConversationContextStore, Galaxium.Api.Services.AI.Context.RedisConversationContextStore>();
builder.Services.AddScoped<IToolExecutor, Galaxium.Api.Services.AI.Core.ToolExecutor>();
builder.Services.AddScoped<IIntentParser, Galaxium.Api.Services.AI.Core.PromptBuilder>();
builder.Services.AddScoped<IResponseFormatter, Galaxium.Api.Services.AI.Core.ResponseFormatter>();
builder.Services.AddScoped<IAICopilotService, AICopilotService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// CORS
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Frontend:Origins")
        .Get<string[]>()
        ?? new[] { "http://localhost:3000", "http://localhost:3001", "http://192.168.1.206:3000", "http://192.168.1.206:3001" };

    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT
var configuredJwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var jwtOptions = configuredJwtOptions;

if (string.IsNullOrWhiteSpace(configuredJwtOptions.Key))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtOptions = new JwtOptions
        {
            Key = "dev-only-jwt-key-change-before-production-1234567890",
            Issuer = string.IsNullOrWhiteSpace(configuredJwtOptions.Issuer) ? "galaxium.dev" : configuredJwtOptions.Issuer,
            Audience = string.IsNullOrWhiteSpace(configuredJwtOptions.Audience) ? "galaxium.dev" : configuredJwtOptions.Audience,
            AccessTokenMinutes = configuredJwtOptions.AccessTokenMinutes == 0 ? 30 : configuredJwtOptions.AccessTokenMinutes,
            RefreshTokenDays = configuredJwtOptions.RefreshTokenDays == 0 ? 30 : configuredJwtOptions.RefreshTokenDays,
        };
    }
    else
    {
        throw new Exception("JWT Key is not configured.");
    }
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrWhiteSpace(context.Token) &&
                context.Request.Cookies.TryGetValue("access_token", out var cookieToken))
            {
                context.Token = cookieToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(AuthorizationPolicies.Configure);

// Cloudinary
builder.Services.AddSingleton(sp =>
{
    var cloudinaryUrl = builder.Configuration["URL:Claudinary"];
    if (!string.IsNullOrWhiteSpace(cloudinaryUrl))
        return new Cloudinary(cloudinaryUrl);
    if (builder.Environment.IsDevelopment())
        return new Cloudinary(new Account("demo", "demo", "demo"));
    throw new Exception("Cloudinary URL is not configured.");
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Galaxium ERP API",
        Version = "v1",
        Description = "MultiTenant ERP/POS Platform - Core API"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================
// Build & Middleware Pipeline
// ==========================
var app = builder.Build();

// ==========================
// Auto-Migration on startup
// ==========================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GalaxiumDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors("AllowFrontend");
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Galaxium ERP API v1");
    options.RoutePrefix = string.Empty;
});

if (!app.Environment.IsDevelopment() && !runningInDocker)
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// MultiTenant middleware — MUST be after auth, before controllers
app.UseMultiTenant();

app.MapControllers();

await app.RunAsync();
