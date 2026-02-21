using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Services;

public class VehicleService(
    AppDbContext context,
    ICloudinaryService cloudinaryService,
    ILogger<VehicleService> logger) : IVehicleService
{
    private const int MaxPhotosPerRequest = 5;
    private const long MaxFileSize = 2 * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    public async Task<List<VehicleResponseDto>> GetAllAsync()
    {
        var vehicles = await context.Vehicles
            .AsNoTracking()
            .Include(v => v.Photos)
            .Where(v => !v.IsDeleted)
            .ToListAsync();

        return vehicles.Select(MapVehicle).ToList();
    }

    public async Task<VehicleResponseDto?> GetByIdAsync(int id)
    {
        var vehicle = await context.Vehicles
            .AsNoTracking()
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        return vehicle is null ? null : MapVehicle(vehicle);
    }

    public async Task<List<VehicleResponseDto>> GetAvailableAsync()
    {
        var vehicles = await context.Vehicles
            .AsNoTracking()
            .Include(v => v.Photos)
            .Where(v => !v.IsDeleted && v.Status == VehicleStatus.Available)
            .ToListAsync();

        return vehicles.Select(MapVehicle).ToList();
    }

    public async Task<VehicleResponseDto> UpdateVehicleAsync(int id, VehicleUpdateDto dto)
    {
        var vehicle = await context.Vehicles
            .Include(v => v.Purchase)
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle is null || vehicle.IsDeleted)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        if (vehicle.Purchase is null)
        {
            throw new InvalidOperationException("Purchase record not found for the vehicle.");
        }

        if (vehicle.Status == VehicleStatus.Sold)
        {
            throw new InvalidOperationException("Sold vehicle cannot be updated.");
        }

        if (!string.IsNullOrWhiteSpace(dto.RegistrationNumber))
        {
            var registrationNumber = dto.RegistrationNumber.Trim();

            var duplicateExists = await context.Vehicles
                .AnyAsync(v => v.Id != id &&
                               !v.IsDeleted &&
                               v.RegistrationNumber == registrationNumber);

            if (duplicateExists)
            {
                throw new InvalidOperationException(
                    $"A vehicle with registration number '{registrationNumber}' already exists.");
            }

            vehicle.RegistrationNumber = registrationNumber;
        }

        var totalAcquisitionCost = vehicle.Purchase.BuyingCost + vehicle.Purchase.Expense;
        if (dto.SellingPrice < totalAcquisitionCost)
        {
            throw new ArgumentException(
                $"SellingPrice cannot be less than total acquisition cost ({totalAcquisitionCost:0.00}).");
        }

        vehicle.SellingPrice = dto.SellingPrice;
        vehicle.Colour = string.IsNullOrWhiteSpace(dto.Colour) ? null : dto.Colour.Trim();
        vehicle.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return MapVehicle(vehicle);
    }

    public async Task SoftDeleteVehicleAsync(int id)
    {
        var vehicle = await context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle is null)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        if (vehicle.IsDeleted)
        {
            throw new InvalidOperationException("Vehicle is already deleted.");
        }

        if (vehicle.Status == VehicleStatus.Sold)
        {
            throw new InvalidOperationException("Sold vehicle cannot be deleted.");
        }

        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<VehicleResponseDto> UpdateVehicleStatusAsync(int id, VehicleStatusUpdateDto dto)
    {
        var vehicle = await context.Vehicles
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle is null || vehicle.IsDeleted)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        if (vehicle.Status == VehicleStatus.Sold && dto.Status == VehicleStatus.Available)
        {
            throw new InvalidOperationException("Cannot change status from Sold back to Available.");
        }

        var previousStatus = vehicle.Status;
        if (previousStatus != dto.Status)
        {
            vehicle.Status = dto.Status;
            vehicle.UpdatedAt = DateTime.UtcNow;

            logger.LogInformation(
                "Vehicle status changed for VehicleId {VehicleId} from {PreviousStatus} to {NewStatus}.",
                vehicle.Id,
                previousStatus,
                dto.Status);

            await context.SaveChangesAsync();
        }

        return MapVehicle(vehicle);
    }

    public async Task<VehiclePhotoUploadResponseDto> UploadPhotosAsync(
        int vehicleId,
        IReadOnlyCollection<IFormFile> files)
    {
        if (files.Count == 0)
        {
            throw new ArgumentException("At least one image is required.");
        }

        if (files.Count > MaxPhotosPerRequest)
        {
            throw new ArgumentException($"A maximum of {MaxPhotosPerRequest} images can be uploaded per request.");
        }

        var vehicle = await context.Vehicles
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == vehicleId && !v.IsDeleted);

        if (vehicle is null)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        foreach (var file in files)
        {
            await ValidatePhotoFileAsync(file);
        }

        var nextDisplayOrder = vehicle.Photos.Count == 0
            ? 0
            : vehicle.Photos.Max(p => p.DisplayOrder) + 1;

        var hasPrimaryPhoto = vehicle.Photos.Any(p => p.IsPrimary);
        var newPhotos = new List<VehiclePhoto>();

        foreach (var file in files)
        {
            var photoUrl = await cloudinaryService.UploadImageAsync(file);
            var shouldBePrimary = !hasPrimaryPhoto && newPhotos.Count == 0;

            var photo = new VehiclePhoto
            {
                VehicleId = vehicleId,
                PhotoUrl = photoUrl,
                IsPrimary = shouldBePrimary,
                DisplayOrder = nextDisplayOrder++,
                CreatedAt = DateTime.UtcNow
            };

            newPhotos.Add(photo);
        }

        context.VehiclePhotos.AddRange(newPhotos);
        await context.SaveChangesAsync();

        return new VehiclePhotoUploadResponseDto
        {
            VehicleId = vehicleId,
            Photos = newPhotos
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Id)
                .Select(p => new UploadedVehiclePhotoDto
                {
                    Id = p.Id,
                    Url = p.PhotoUrl,
                    IsPrimary = p.IsPrimary
                })
                .ToList()
        };
    }

    public async Task<List<VehiclePhotoDto>> SetPrimaryPhotoAsync(int vehicleId, int photoId)
    {
        var vehicle = await context.Vehicles
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == vehicleId && !v.IsDeleted);

        if (vehicle is null)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        var selectedPhoto = vehicle.Photos.FirstOrDefault(p => p.Id == photoId);
        if (selectedPhoto is null)
        {
            throw new KeyNotFoundException("Photo not found for this vehicle.");
        }

        foreach (var photo in vehicle.Photos)
        {
            photo.IsPrimary = false;
        }

        selectedPhoto.IsPrimary = true;
        await context.SaveChangesAsync();

        return MapVehiclePhotos(vehicle.Photos);
    }

    public async Task<List<VehiclePhotoDto>> DeletePhotoAsync(int photoId)
    {
        var photo = await context.VehiclePhotos
            .FirstOrDefaultAsync(vp => vp.Id == photoId);

        if (photo is null)
        {
            throw new KeyNotFoundException("Photo not found.");
        }

        var vehicleId = photo.VehicleId;

        context.VehiclePhotos.Remove(photo);
        await context.SaveChangesAsync();

        var remainingPhotos = await context.VehiclePhotos
            .Where(vp => vp.VehicleId == vehicleId)
            .OrderBy(vp => vp.DisplayOrder)
            .ThenBy(vp => vp.Id)
            .ToListAsync();

        if (remainingPhotos.Count > 0 && remainingPhotos.All(vp => !vp.IsPrimary))
        {
            remainingPhotos[0].IsPrimary = true;
            await context.SaveChangesAsync();
        }

        return MapVehiclePhotos(remainingPhotos);
    }

    private static VehicleResponseDto MapVehicle(Vehicle v)
    {
        return new VehicleResponseDto
        {
            Id = v.Id,
            Brand = v.Brand,
            Model = v.Model,
            Year = v.Year,
            RegistrationNumber = v.RegistrationNumber,
            ChassisNumber = v.ChassisNumber,
            EngineNumber = v.EngineNumber,
            Colour = v.Colour,
            SellingPrice = v.SellingPrice,
            Status = v.Status,
            CreatedAt = v.CreatedAt,
            Photos = MapVehiclePhotos(v.Photos)
        };
    }

    private static List<VehiclePhotoDto> MapVehiclePhotos(IEnumerable<VehiclePhoto> photos)
    {
        return photos
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Id)
            .Select(p => new VehiclePhotoDto
            {
                Id = p.Id,
                PhotoUrl = p.PhotoUrl,
                IsPrimary = p.IsPrimary,
                DisplayOrder = p.DisplayOrder
            })
            .ToList();
    }

    private static async Task ValidatePhotoFileAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("One or more files are invalid.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException($"File '{file.FileName}' exceeds the 2MB limit.");
        }

        if (!AllowedImageTypes.Contains(file.ContentType))
        {
            throw new ArgumentException($"File '{file.FileName}' must be JPG, PNG, or WEBP.");
        }

        await using var input = file.OpenReadStream();
        var hasValidSignature = await HasValidSignatureAsync(input, file.ContentType);
        if (!hasValidSignature)
        {
            throw new ArgumentException($"File '{file.FileName}' content does not match a supported image format.");
        }
    }

    private static async Task<bool> HasValidSignatureAsync(Stream input, string contentType)
    {
        var header = new byte[12];
        var read = await input.ReadAsync(header);

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => read >= 3 &&
                                           header[0] == 0xFF &&
                                           header[1] == 0xD8 &&
                                           header[2] == 0xFF,
            "image/png" => read >= 8 &&
                           header[0] == 0x89 &&
                           header[1] == 0x50 &&
                           header[2] == 0x4E &&
                           header[3] == 0x47 &&
                           header[4] == 0x0D &&
                           header[5] == 0x0A &&
                           header[6] == 0x1A &&
                           header[7] == 0x0A,
            "image/webp" => read >= 12 &&
                            header[0] == 0x52 &&
                            header[1] == 0x49 &&
                            header[2] == 0x46 &&
                            header[3] == 0x46 &&
                            header[8] == 0x57 &&
                            header[9] == 0x45 &&
                            header[10] == 0x42 &&
                            header[11] == 0x50,
            _ => false
        };
    }
}
