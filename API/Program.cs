using API.Attributes;
using Business;
using Business.Contracts;
using DataAccess;
using DataAccess.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// DbContext
builder.Services.AddDbContext<JujuTestContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Development")));

// Servicios existentes
builder.Services.AddScoped<BaseService<DataAccess.Data.Customer>, BaseService<DataAccess.Data.Customer>>();
builder.Services.AddScoped<BaseModel<DataAccess.Data.Customer>, BaseModel<DataAccess.Data.Customer>>();
builder.Services.AddScoped<BaseService<DataAccess.Data.Post>, BaseService<DataAccess.Data.Post>>();
builder.Services.AddScoped<BaseModel<DataAccess.Data.Post>, BaseModel<DataAccess.Data.Post>>();

// Repositorios
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Attributes
builder.Services.AddScoped<JwtAuthorizationAttribute>();
builder.Services.AddScoped<ValidarModeloAttribute>();
builder.Services.AddScoped<LogAttribute>();
builder.Services.AddScoped<ExceptionAttribute>();

// JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:Key"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

try
{
    Log.Information("Starting web application");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowSpecificOrigins");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}