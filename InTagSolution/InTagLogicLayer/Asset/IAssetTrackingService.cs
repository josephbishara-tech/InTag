using InTagViewModelLayer.Asset;

namespace InTagLogicLayer.Asset
{
    public interface IAssetTrackingService
    {
        Task<IReadOnlyList<TrackingRequestListVm>> GetRequestsAsync();
        Task<TrackingRequestDetailVm> GetRequestByIdAsync(int id);
        Task<TrackingRequestDetailVm> CreateRequestAsync(TrackingRequestCreateVm model);
        Task<TrackingRequestDetailVm> OpenRequestAsync(int id);
        Task<TrackingRequestDetailVm> CompleteRequestAsync(int id);
        Task CancelRequestAsync(int id);
        Task<TrackingLineVm> SubmitScanAsync(TrackingScanSubmitVm model);
        Task MarkLineMissingAsync(int lineId);
    }
}
