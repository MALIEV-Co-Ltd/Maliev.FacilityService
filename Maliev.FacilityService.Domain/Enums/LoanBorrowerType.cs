namespace Maliev.FacilityService.Domain.Enums;

/// <summary>
/// Represents the type of borrower for equipment loans.
/// </summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum LoanBorrowerType
{
    Employee,
    Customer
}
