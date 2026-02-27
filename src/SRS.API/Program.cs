using System.Text;
using CloudinaryDotNet;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using SRS.API.Extensions;
using SRS.Application.Interfaces;
using SRS.Application.Services;
using SRS.Application.Validators;
using SRS.Domain.Interfaces;
using SRS.Infrastructure.Configuration;
using SRS.Infrastructure.FileStorage;
using SRS.Infrastructure.Persistence;
using SRS.Infrastructure.Security;
using SRS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<VehicleUpdateDtoValidator>();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services
    .AddOptions<CloudinarySettings>()
    .Bind(builder.Configuration.GetSection(CloudinarySettings.SectionName));

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CloudinarySettings>>().Value;
    if (string.IsNullOrWhiteSpace(settings.CloudName) || string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.ApiSecret))
    {
        throw new InvalidOperationException(
            "Cloudinary configuration is missing. Set Cloudinary__CloudName, Cloudinary__ApiKey, and Cloudinary__ApiSecret in environment or configuration.");
    }
    var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
    return new Cloudinary(account) { Api = { Secure = true } };
});

builder.Services.AddScoped<ICloudStorageService, CloudinaryStorageService>();
builder.Services.AddHttpClient<IPdfGenerator, DeliveryNotePdfGenerator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IDeliveryNoteSettingsService, DeliveryNoteSettingsService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IPurchaseExpenseService, PurchaseExpenseService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IFinanceCompanyService, FinanceCompanyService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<ICustomerPhotoStorageService, LocalFileStorageService>();
}
else
{
    builder.Services.AddScoped<ICustomerPhotoStorageService, CloudinaryCustomerPhotoStorageService>();
}

builder.Services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
builder.Services.AddHttpClient<IWhatsAppService, MetaWhatsAppService>();
builder.Services.AddScoped<IInvoicePdfService, InvoicePdfService>();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };
    });
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("At least one origin must be configured under 'Cors:AllowedOrigins'.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();


var app = builder.Build();
var uploadPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
Directory.CreateDirectory(uploadPath);
await DbInitializer.InitializeAsync(app.Services);
app.UseCors("AllowFrontend");

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }


app.UseSwagger();
app.UseSwaggerUI();



app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// In Program.cs after app.Build()
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (context.Database.IsRelational())
        {
             context.Database.Migrate();
        }
        DbInitializer.InitializeAsync(services).Wait();
        
        
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

QuestPDF.Settings.License = LicenseType.Community;
var fontPath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Services", "Pdf", "Fonts", "NotoSansTamil-Regular.ttf");
if (File.Exists(fontPath))
{
    FontManager.RegisterFont(File.OpenRead(fontPath));
}
app.Run();
