using InTagMobile.Models;

namespace InTagMobile.Services;

public interface IApiService
{
    Task<LoginResponse?> LoginAsync(string email, string password);
    Task<List<TrackingRequestDto>> GetRequestsAsync();
    Task<TrackingRequestDetailDto?> GetRequestAsync(int id);
    Task<bool> StartRequestAsync(int id);
    Task<bool> SubmitScanAsync(ScanSubmitDto scan);
    Task<bool> CompleteRequestAsync(int id);
}
