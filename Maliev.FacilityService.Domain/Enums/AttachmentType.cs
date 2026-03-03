namespace Maliev.FacilityService.Domain.Enums;

/// <summary>
/// Represents the type of attachment for equipment.
/// </summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum AttachmentType
{
    Tool,
    Fixture,
    Collet,
    Chuck,
    Clamp,
    Probe,
    Magazine,
    Other
}
