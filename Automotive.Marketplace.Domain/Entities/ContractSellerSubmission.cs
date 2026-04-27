namespace Automotive.Marketplace.Domain.Entities;

public class ContractSellerSubmission : BaseEntity
{
    public Guid ContractCardId { get; set; }

    // Vehicle
    public string? SdkCode { get; set; }
    public string Make { get; set; } = string.Empty;
    public string CommercialName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string? Vin { get; set; }
    public string? RegistrationCertificate { get; set; }
    public bool TechnicalInspectionValid { get; set; }
    public bool WasDamaged { get; set; }
    public bool? DamageKnown { get; set; }
    public bool DefectBrakes { get; set; }
    public bool DefectSafety { get; set; }
    public bool DefectSteering { get; set; }
    public bool DefectExhaust { get; set; }
    public bool DefectLighting { get; set; }
    public string? DefectDetails { get; set; }
    public decimal? Price { get; set; }

    // Seller personal
    public string PersonalIdCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Country { get; set; } = "Lietuva";
    public DateTime SubmittedAt { get; set; }

    public virtual ContractCard ContractCard { get; set; } = null!;
}
