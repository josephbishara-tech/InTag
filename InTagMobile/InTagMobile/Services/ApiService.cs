using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using InTagMobile.Models;

namespace InTagMobile.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _token;

    public ApiService()
    {
        var handler = new HttpClientHandler();
#if DEBUG
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://intag.identecheg.com")
        };
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private void SetAuth()
    {
        _token ??= SecureStorage.GetAsync("auth_token").Result;
        if (!string.IsNullOrEmpty(_token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/v1/auth/login",
                new LoginRequest { Email = email, Password = password });
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            if (result?.Token != null)
            {
                _token = result.Token;
                await SecureStorage.SetAsync("auth_token", _token);
                SetAuth();
            }
            return result;
        }
        catch { return null; }
    }

    public async Task<List<TrackingRequestDto>> GetRequestsAsync()
    {
        SetAuth();
        try
        {
            var response = await _http.GetAsync("/api/v1/tracking/requests");
            var body = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[API] GET requests: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[API] Body: {body}");

            if (!response.IsSuccessStatusCode) return new();

            return JsonSerializer.Deserialize<List<TrackingRequestDto>>(body, _jsonOptions) ?? new();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API] Error: {ex.Message}");
            return new();
        }
    }

    public async Task<TrackingRequestDetailDto?> GetRequestAsync(int id)
    {
        SetAuth();
        try
        {
            var response = await _http.GetAsync($"/api/v1/tracking/requests/{id}");
            var body = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[API] GET request/{id}: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[API] Body: {body}");

            if (!response.IsSuccessStatusCode) return null;

            return JsonSerializer.Deserialize<TrackingRequestDetailDto>(body, _jsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API] Error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> StartRequestAsync(int id)
    {
        SetAuth();
        try
        {
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var r = await _http.PostAsync($"/api/v1/tracking/requests/{id}/start", content);
            System.Diagnostics.Debug.WriteLine($"[API] POST start/{id}: {r.StatusCode}");
            return r.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> SubmitScanAsync(ScanSubmitDto scan)
    {
        SetAuth();
        try
        {
            var r = await _http.PostAsJsonAsync("/api/v1/tracking/scan", scan);
            System.Diagnostics.Debug.WriteLine($"[API] POST scan: {r.StatusCode}");
            return r.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> CompleteRequestAsync(int id)
    {
        SetAuth();
        try
        {
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var r = await _http.PostAsync($"/api/v1/tracking/requests/{id}/complete", content);
            return r.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
