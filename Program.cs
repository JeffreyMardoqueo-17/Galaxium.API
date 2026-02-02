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
using GalaxiumERP.API.Repository.repos;
using Galaxium.API.Services.service;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Services.service;
using Galaxium.Api.Repository.repos;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.API.Repository.repos;
using Galaxium.Api.Services.Service;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<GalaxiumDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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

//caragar las fotos 
builder.Services.AddScoped<IProductPhotoRepository, ProductPhotoRepository>();
builder.Services.AddScoped<IProductPhotoService, ProductPhotoService>();

builder.Services.AddControllers();

// Configurar CORS **antes** de Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Leer configuración JWT y validar que exista la clave
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

/// servicio de claudinary
builder.Services.AddSingleton(sp =>
{
    var cloudinaryUrl = builder.Configuration["URL:Claudinary"];
    if (string.IsNullOrWhiteSpace(cloudinaryUrl))
        throw new Exception("Cloudinary URL is not configured.");
    return new Cloudinary(cloudinaryUrl);
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

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
