using System.Text.Json.Serialization;

namespace AttendanceManagement.Application.Vehicles;

public sealed record CreateVehicleRequest(
    [property: JsonPropertyName("plate")] string? Plate,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("capacity")] int? Capacity);

public sealed record UpdateVehicleRequest(
    [property: JsonPropertyName("plate")] string? Plate,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("capacity")] int? Capacity,
    [property: JsonPropertyName("active")] bool Active);

public sealed record VehicleResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("plate")] string Plate,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("capacity")] int? Capacity,
    [property: JsonPropertyName("active")] bool Active);
