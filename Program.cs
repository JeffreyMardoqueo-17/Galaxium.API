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
using Microsoft.Data.SqlClient;
using GalaxiumERP.API.Repository.repos;
using Galaxium.API.Services.service;
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

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con SQL Server y fallback seguro en desarrollo
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    if (builder.Environment.IsDevelopment())
    {
        defaultConnection = "Server=(localdb)\\MSSQLLocalDB;Database=GalaxiumBD;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
        Console.WriteLine("[WARN] No se encontro ConnectionStrings:DefaultConnection. Se usara LocalDB de desarrollo.");
    }
    else
    {
        throw new InvalidOperationException("Missing configuration 'ConnectionStrings:DefaultConnection'. Configure it in appsettings or environment variables.");
    }
}

EnsureLocalDevelopmentDatabase(defaultConnection, builder.Environment);

builder.Services.AddDbContext<GalaxiumDbContext>(options =>
{
    options.UseSqlServer(defaultConnection, sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });
});

static void EnsureLocalDevelopmentDatabase(string connectionString, IHostEnvironment environment)
{
    if (!environment.IsDevelopment())
    {
        return;
    }

    try
    {
        var connectionBuilder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = connectionBuilder.InitialCatalog;
        var dataSource = connectionBuilder.DataSource ?? string.Empty;

        if (string.IsNullOrWhiteSpace(databaseName) || !dataSource.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var masterBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        using var connection = new SqlConnection(masterBuilder.ConnectionString);
        connection.Open();

        using var existsCommand = new SqlCommand("SELECT DB_ID(@databaseName)", connection);
        existsCommand.Parameters.AddWithValue("@databaseName", databaseName);

        var existsResult = existsCommand.ExecuteScalar();
        if (existsResult is null || existsResult == DBNull.Value)
        {
            var escapedDbName = databaseName.Replace("]", "]]", StringComparison.Ordinal);
            using var createCommand = new SqlCommand($"CREATE DATABASE [{escapedDbName}]", connection);
            createCommand.ExecuteNonQuery();
            Console.WriteLine($"[INFO] Development database '{databaseName}' was created on LocalDB.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARN] Could not ensure LocalDB development database: {ex.Message}");
    }
}


// AutoMapper: escanear solo el ensamblado de perfiles propios para evitar reflection sobre ensamblados externos.
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

// Configurar opciones JWT
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Registrar servicios, repositorios y servicios de autenticación
builder.Services.AddScoped<IRoleRespository, RolRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddScoped<IUserRepository, UserRepository>(); 
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>(); 
builder.Services.AddScoped<IUserAuthService, UserAuthService>();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

///
/// //////////////todo de lo que ser ael stok 
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<ICategoryCodeGenerator, CategoryCodeGenerator>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductFilterRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISkuGenerator, SkuGenerator>();
builder.Services.AddScoped<IProductFilterRepository, ProductRepository>();

//servicios de usaurios
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

//para el stokEntry
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

//validaciones
builder.Services.AddScoped<IValidator<Customer>, CustomerValidator>();


//caragar las fotos 
builder.Services.AddScoped<IProductPhotoRepository, ProductPhotoRepository>();
builder.Services.AddScoped<IProductPhotoService, ProductPhotoService>();

//servicios de ventas

builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();

builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISalePdfService, SalePdfService>();
builder.Services.AddScoped<ISaleReportingService, SaleReportingService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<ISaleDetailService, SaleDetailService>();
builder.Services.AddScoped<ISaleDetailRepository, SaleDetailRepository>();

///===================================SERVICIO DE DASHBOARD ============================================
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();


///----------------------------------------LAS REGLAS LAS REGISTRATE AQUI PARA ABAJO SIEMPRE ------------------------------
builder.Services.AddScoped<StockEntryRules>();
builder.Services.AddScoped<SaleRules>();
builder.Services.AddScoped<SaleDetailsRules>();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()); //Esto le dice a .NET que cuando reciba "Purchase", lo mapee a StockReferenceType.Purchase.
    });

// Configurar CORS **antes** de Build()
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Frontend:Origins")
        .Get<string[]>()
        ?? new[]
        {
            "http://localhost:3000",
            "http://localhost:3001",
            "http://192.168.1.206:3000",
            "http://192.168.1.206:3001"
        };

    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Leer configuración JWT y usar fallback de desarrollo cuando falte configuración.
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

// Configurar autenticación JWT
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

        ClockSkew = TimeSpan.Zero
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

/// servicio de claudinary
builder.Services.AddSingleton(sp =>
{
    var cloudinaryUrl = builder.Configuration["URL:Claudinary"];
    if (!string.IsNullOrWhiteSpace(cloudinaryUrl))
    {
        return new Cloudinary(cloudinaryUrl);
    }

    if (builder.Environment.IsDevelopment())
    {
        // En desarrollo permitimos iniciar sin credenciales reales de Cloudinary.
        return new Cloudinary(new Account("demo", "demo", "demo"));
    }

    throw new Exception("Cloudinary URL is not configured.");
});

// Swagger con soporte JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Galaxium ERP API",
        Version = "v1",
        Description = "Core API for Galaxium ERP - Future-ready enterprise platform"
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Middleware CORS
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Galaxium ERP API v1");
        options.RoutePrefix = string.Empty;
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseMiddleware<ExceptionMiddleware>();
// app.UseMiddleware<RateLimitMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
