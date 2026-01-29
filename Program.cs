using System.Text;
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
using GalaxiumERP.API.Repository.repos;
using Galaxium.API.Services.service;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Services.service;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<GalaxiumDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configurar opciones JWT (asegúrate que en appsettings.json tengas la sección "Jwt")
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Registrar servicios, repositorios y servicios de autenticación
builder.Services.AddScoped<IRoleRespository, RolRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddScoped<IUserRepository, UserRepository>(); // Para gestión de usuarios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>(); // Para creación y login
builder.Services.AddScoped<IUserAuthService, UserAuthService>();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>(); // Para guardar tokens refresh


builder.Services.AddControllers();

// Leer configuración JWT
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();

if (jwtOptions == null || string.IsNullOrWhiteSpace(jwtOptions.Key))
    throw new Exception("JWT Key is not configured.");

// Configurar autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
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
});

// Configurar Swagger + seguridad JWT en Swagger UI
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Galaxium ERP API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
