using SRS.Domain.Enums;

namespace SRS.Application.DTOs;

public class SaleInvoiceDto
{
    public int BillNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public TimeSpan? DeliveryTime { get; set; }

    public string CustomerName { get; set; } = null!;
    public string? FatherName { get; set; }
    public string Phone { get; set; } = null!;
    public string? Address { get; set; }
    public string PhotoUrl { get; set; } = null!;
    public string? IdProofNumber { get; set; }

    public string CustomerPhone { get; set; } = null!;
    public string? CustomerAddress { get; set; }
    public string CustomerPhotoUrl { get; set; } = null!;

    public string VehicleBrand { get; set; } = null!;
    public string VehicleModel { get; set; } = null!;
    public string RegistrationNumber { get; set; } = null!;
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? Colour { get; set; }

    public decimal SellingPrice { get; set; }

    public PaymentMode PaymentMode { get; set; }
    public decimal? CashAmount { get; set; }
    public decimal? UpiAmount { get; set; }
    public decimal? FinanceAmount { get; set; }
    public string? FinanceCompany { get; set; }
    public bool RcBookReceived { get; set; }
    public bool OwnershipTransferAccepted { get; set; }
    public bool VehicleAcceptedInAsIsCondition { get; set; }

    public decimal Profit { get; set; }
}
