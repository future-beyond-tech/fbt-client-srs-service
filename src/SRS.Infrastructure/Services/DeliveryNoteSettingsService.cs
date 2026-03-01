using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Services;

public class DeliveryNoteSettingsService(AppDbContext context) : IDeliveryNoteSettingsService
{
    private const int SingletonSettingsId = 1;

    public async Task<DeliveryNoteSettingsDto> GetAsync()
    {
        var settings = await GetOrCreateAsync();
        return Map(settings);
    }

    public async Task<DeliveryNoteSettingsDto> UpdateAsync(UpdateDeliveryNoteSettingsDto dto)
    {
        var settings = await GetOrCreateAsync();

        settings.ShopName = dto.ShopName.Trim();
        settings.ShopAddress = dto.ShopAddress.Trim();
        settings.GSTNumber = Normalize(dto.GSTNumber);
        settings.ContactNumber = Normalize(dto.ContactNumber);
        settings.FooterText = Normalize(dto.FooterText);
        settings.TermsAndConditions = Normalize(dto.TermsAndConditions);
        settings.TamilTermsAndConditions = Normalize(dto.TamilTermsAndConditions);
        settings.LogoUrl = Normalize(dto.LogoUrl);
        settings.SignatureLine = Normalize(dto.SignatureLine);
        settings.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Map(settings);
    }

    private async Task<DeliveryNoteSettings> GetOrCreateAsync()
    {
        var existing = await context.DeliveryNoteSettings
            .FirstOrDefaultAsync(x => x.Id == SingletonSettingsId);

        if (existing is not null)
        {
            return existing;
        }

        var settings = CreateDefault();
        context.DeliveryNoteSettings.Add(settings);

        try
        {
            await context.SaveChangesAsync();
            return settings;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraint(ex))
        {
            context.Entry(settings).State = EntityState.Detached;

            return await context.DeliveryNoteSettings
                .FirstAsync(x => x.Id == SingletonSettingsId);
        }
    }

    private static DeliveryNoteSettings CreateDefault()
    {
        return new DeliveryNoteSettings
        {
            Id = SingletonSettingsId,
            ShopName = "SREE RAMALINGAM SONS",
            ShopAddress = "H.O.: 154, Pycrofts Road, Royapettah (Opp. Sub Reg. Office) Chennai - 600 014",
            FooterText = "Thank you for your purchase.",
            TermsAndConditions =
                "I confirm that I have received the vehicle in good condition and accepted all sale terms.",
            TamilTermsAndConditions = null,
            SignatureLine = "Authorized Signature",
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static DeliveryNoteSettingsDto Map(DeliveryNoteSettings settings)
    {
        const string defaultShopName = "SREE RAMALINGAM SONS";
        const string defaultAddress = "H.O.: 154, Pycrofts Road, Royapettah (Opp. Sub Reg. Office) Chennai - 600 014";

        var address = settings.ShopAddress;
        if (string.IsNullOrWhiteSpace(address) ||
            address.Contains("not configured", StringComparison.OrdinalIgnoreCase))
        {
            address = defaultAddress;
        }

        return new DeliveryNoteSettingsDto
        {
            Id = settings.Id,
            ShopName = string.IsNullOrWhiteSpace(settings.ShopName) ? defaultShopName : settings.ShopName,
            ShopAddress = address,
            GSTNumber = settings.GSTNumber,
            ContactNumber = settings.ContactNumber,
            FooterText = settings.FooterText,
            TermsAndConditions = settings.TermsAndConditions,
            TamilTermsAndConditions = settings.TamilTermsAndConditions,
            LogoUrl = settings.LogoUrl,
            SignatureLine = settings.SignatureLine,
            UpdatedAt = settings.UpdatedAt
        };
    }

    private static bool IsUniqueConstraint(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
               || ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
