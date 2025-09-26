using EcommerceProduct.API.DbContexts;
using EcommerceProduct.API.Repository.Implementation;
using EcommerceProduct.API.Repository.Interface;
using EcommerceProduct.API.Services.Implementation;
using EcommerceProduct.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EcommerceProduct API",
        Version = "v1",
        Description = "A comprehensive ASP.NET Core Web API for e-commerce product management",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "EcommerceProduct API Support",
            Email = "support@ecommerceproduct.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure Database
builder.Services.AddDbContext<ProductContext>(dbContextOptions =>
{
    dbContextOptions.UseSqlite(builder.Configuration.GetConnectionString("ProductDBConnectionString"));
});

// Register repository and services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<IProductCategoriesRepository, ProductCategoriesRepository>();
builder.Services.AddScoped<IProductCategoriesService, ProductCategoriesService>();

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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EcommerceProduct API v1");
        options.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
        options.DocumentTitle = "EcommerceProduct API Documentation";
        options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
        options.DisplayRequestDuration();
        options.EnableFilter();
        options.ShowExtensions();
        options.EnableDeepLinking();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();