using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Commands.RegisterEquipment;

/// <summary>
/// Command to register a new piece of equipment in the facility management system.
/// </summary>
/// <param name="Name">Display name of the equipment.</param>
/// <param name="Category">Category of the equipment.</param>
/// <param name="Brand">Brand or manufacturer.</param>
/// <param name="ModelName">Model name.</param>
/// <param name="ManufacturerSerialNumber">Manufacturer's serial number.</param>
/// <param name="SubCategory">Sub-category classification.</param>
/// <param name="PurchasePriceTHB">Purchase price in Thai Baht.</param>
/// <param name="PurchaseDate">Date of purchase.</param>
/// <param name="WarrantyExpiryDate">Warranty expiration date.</param>
/// <param name="NextServiceDueDate">Next scheduled service date.</param>
/// <param name="Spec">Category-specific spec properties as key-value pairs.</param>
public record RegisterEquipmentCommand(
    string Name,
    EquipmentCategory Category,
    string? Brand,
    string? ModelName,
    string? ManufacturerSerialNumber,
    string? SubCategory,
    decimal? PurchasePriceTHB,
    DateOnly? PurchaseDate,
    DateOnly? WarrantyExpiryDate,
    DateOnly? NextServiceDueDate,
    Dictionary<string, object?>? Spec);
