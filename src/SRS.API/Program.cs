using System.Text;
using CloudinaryDotNet;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using SRS.API.Extensions;
using SRS.API.Filters;
using SRS.API.Middleware;
using SRS.Application.Features.ManualBilling.CreateManualBill;
using SRS.Application.Features.ManualBilling.GetManualBillByNumber;
using SRS.Application.Features.ManualBilling.GetManualBillInvoice;
using SRS.Application.Features.ManualBilling.SendManualBillInvoice;
using SRS.Application.Interfaces;
using SRS.Application.Services;
using SRS.Application.Validators;
using SRS.Domain.Interfaces;
using SRS.Infrastructure.Repositories;
using SRS.Infrastructure.Configuration;
using SRS.Infrastructure.FileStorage;
using SRS.Infrastructure.Persistence;
using SRS.Infrastructure.Security;
using SRS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Prefer CLOUDINARY_* env vars when set (no secrets in repo; use user-secrets or env)
var cloudinaryOverrides = new Dictionary<string, string?>();
if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")))
    cloudinaryOverrides["Cloudinary:CloudName"] = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")))
    cloudinaryOverrides["Cloudinary:ApiKey"] = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")))
    cloudinaryOverrides["Cloudinary:ApiSecret"] = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CLOUDINARY_FOLDER")))
    cloudinaryOverrides["Cloudinary:Folder"] = Environment.GetEnvironmentVariable("CLOUDINARY_FOLDER");
if (cloudinaryOverrides.Count > 0)
    builder.Configuration.AddInMemoryCollection(cloudinaryOverrides!);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationProblemDetailsFilter>();
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<VehicleUpdateDtoValidator>();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services
    .AddOptions<CloudinarySettings>()
    .Bind(builder.Configuration.GetSection(CloudinarySettings.SectionName));

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CloudinarySettings>>().Value;
    var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")?.Trim() ?? settings.CloudName;
    var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")?.Trim() ?? settings.ApiKey;
    var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")?.Trim() ?? settings.ApiSecret;
    if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
    {
        throw new InvalidOperationException(
            "Cloudinary configuration is missing. Set CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, CLOUDINARY_API_SECRET (env/user-secrets) or Cloudinary__CloudName, Cloudinary__ApiKey, Cloudinary__ApiSecret.");
    }
    var account = new Account(cloudName, apiKey, apiSecret);
    return new Cloudinary(account) { Api = { Secure = true } };
});

builder.Services.AddScoped<ICloudStorageService, CloudinaryStorageService>();

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
builder.Services.AddScoped<IManualBillRepository, ManualBillRepository>();
builder.Services.AddScoped<ICreateManualBillHandler, CreateManualBillHandler>();
builder.Services.AddScoped<IGetManualBillByNumberHandler, GetManualBillByNumberHandler>();
builder.Services.AddScoped<IGetManualBillInvoiceHandler, GetManualBillInvoiceHandler>();
builder.Services.AddScoped<IManualBillInvoicePdfService, ManualBillInvoicePdfService>();
builder.Services.AddScoped<ISendManualBillInvoiceHandler, SendManualBillInvoiceHandler>();

// wkhtmltopdf CLI: mandatory for Sales + Manual Billing in non-Testing environments. In Testing, use stub so integration tests run without wkhtmltopdf.
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddScoped<IWkhtmltopdfCliGenerator, StubWkhtmltopdfCliGenerator>();
    builder.Services.AddScoped<IDeliveryNoteWkHtmlPdfGenerator, DeliveryNoteWkHtmlPdfGenerator>();
    builder.Services.AddScoped<IManualBillHtmlPdfGenerator, DeliveryNoteWkHtmlPdfGenerator>();
}
else
{
    const string exe = "wkhtmltopdf";
    try
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exe,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        await process.WaitForExitAsync(CancellationToken.None);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "wkhtmltopdf CLI check failed (non-zero exit). Invoice engine unavailable. Install wkhtmltopdf (e.g. apt-get install wkhtmltopdf). See docs/DOCKER_WKHTMLTOPDF.md.");
        }
    }
    catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 2)
    {
        throw new InvalidOperationException(
            "wkhtmltopdf not found. Invoice engine unavailable. Install wkhtmltopdf (e.g. apt-get install wkhtmltopdf). See docs/DOCKER_WKHTMLTOPDF.md.", ex);
    }
    catch (System.IO.FileNotFoundException ex)
    {
        throw new InvalidOperationException(
            "wkhtmltopdf not found. Invoice engine unavailable. Install wkhtmltopdf (e.g. apt-get install wkhtmltopdf). See docs/DOCKER_WKHTMLTOPDF.md.", ex);
    }
    catch (InvalidOperationException)
    {
        throw;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(
            "wkhtmltopdf not available. Invoice engine unavailable. Install wkhtmltopdf (e.g. apt-get install wkhtmltopdf). See docs/DOCKER_WKHTMLTOPDF.md.", ex);
    }

    builder.Services.AddScoped<IWkhtmltopdfCliGenerator, WkhtmltopdfCliGenerator>();
    builder.Services.AddScoped<IDeliveryNoteWkHtmlPdfGenerator, DeliveryNoteWkHtmlPdfGenerator>();
    builder.Services.AddScoped<IManualBillHtmlPdfGenerator, DeliveryNoteWkHtmlPdfGenerator>();
}

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
app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

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
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (context.Database.IsRelational())
            context.Database.Migrate();
        DbInitializer.InitializeAsync(services).Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}
app.Run();
