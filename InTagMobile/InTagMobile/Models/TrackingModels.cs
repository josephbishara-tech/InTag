using System.Text.Json.Serialization;

namespace InTagMobile.Models;

public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;
}

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
