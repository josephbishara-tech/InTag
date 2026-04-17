using System.Text.Json.Serialization;

namespace InTagMobile.Models;

// ── Auth ──

public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Wrapper for the API envelope: { "data": { ... } }
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public class LoginResponse
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("accessTokenExpiration")]
    public DateTimeOffset? AccessTokenExpiration { get; set; }

    [JsonPropertyName("user")]
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("tenantId")]
    public Guid TenantId { get; set; }

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();
}

// ── Tracking ──

public class TrackingRequestDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("requestNumber")]
    public string RequestNumber { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("locationName")]
    public string LocationName { get; set; } = string.Empty;

    [JsonPropertyName("statusDisplay")]
    public string StatusDisplay { get; set; } = string.Empty;

    [JsonPropertyName("totalAssets")]
    public int TotalAssets { get; set; }

    [JsonPropertyName("scannedCount")]
    public int ScannedCount { get; set; }

    [JsonPropertyName("progressPercent")]
    public decimal ProgressPercent { get; set; }
}

public class TrackingRequestDetailDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("requestNumber")]
    public string RequestNumber { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("locationName")]
    public string LocationName { get; set; } = string.Empty;

    [JsonPropertyName("statusDisplay")]
    public string StatusDisplay { get; set; } = string.Empty;

    [JsonPropertyName("totalAssets")]
    public int TotalAssets { get; set; }

    [JsonPropertyName("foundCount")]
    public int FoundCount { get; set; }

    [JsonPropertyName("missingCount")]
    public int MissingCount { get; set; }

    [JsonPropertyName("progressPercent")]
    public decimal ProgressPercent { get; set; }

    [JsonPropertyName("lines")]
    public List<TrackingLineDto> Lines { get; set; } = new();
}

public class TrackingLineDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("assetId")]
    public int AssetId { get; set; }

    [JsonPropertyName("assetCode")]
    public string AssetCode { get; set; } = string.Empty;

    [JsonPropertyName("assetName")]
    public string AssetName { get; set; } = string.Empty;

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }

    [JsonPropertyName("serialNumber")]
    public string? SerialNumber { get; set; }

    [JsonPropertyName("statusDisplay")]
    public string StatusDisplay { get; set; } = string.Empty;

    public bool IsScanned => StatusDisplay != "Pending";
}

public class ScanSubmitDto
{
    [JsonPropertyName("trackingLineId")]
    public int TrackingLineId { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("scannedCode")]
    public string? ScannedCode { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("conditionAtScan")]
    public int ConditionAtScan { get; set; } = 4;

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }
}
