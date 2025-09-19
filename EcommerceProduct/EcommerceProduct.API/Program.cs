using EcommerceProduct.API.DbContexts;
using EcommerceProduct.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (environment == Environments.Development)
{
    builder.Host.UseSerilog(
        (context, loggerConfiguration) => loggerConfiguration
            .MinimumLevel.Debug()
            .WriteTo.Console());
}

// Add services to the container.

builder.Services.AddControllers()
    .AddNewtonsoftJson() // Add support for JSON format using Newtonsoft.Json (required for JSON Patch)
    .AddXmlDataContractSerializerFormatters(); // Add support for XML format.// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure Database
builder.Services.AddDbContext<ProductContext>(dbContextOptions =>
{
    dbContextOptions.UseSqlite(builder.Configuration.GetConnectionString("ProductDBConnectionString"));
});

// Register repository and services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Configure AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"] ??
                    throw new InvalidOperationException("Authentication:SecretForKey is required")))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeAdmin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("role_name", "Admin");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();