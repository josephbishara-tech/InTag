using Microsoft.EntityFrameworkCore;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Maintenance;

namespace InTagLogicLayer.Maintenance
{
    public class ReliabilityEngine
    {
        private readonly IUnitOfWork _uow;

        public ReliabilityEngine(IUnitOfWork uow) { _uow = uow; }

        /// <summary>
        /// Calculates MTBF, MTTR, and Availability for a single asset.
        /// </summary>
        public async Task<MTBFMTTRResultVm> CalculateAsync(int assetId, DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            var asset = await _uow.Assets.GetByIdAsync(assetId);
            if (asset == null) throw new KeyNotFoundException("Asset not found.");

            var periodStart = from ?? asset.AcquisitionDate ?? asset.CreatedDate;
            var periodEnd = to ?? DateTimeOffset.UtcNow;
            var totalPeriodHours = (decimal)(periodEnd - periodStart).TotalHours;

            var failures = await _uow.FailureLogs.Query()
                .Where(f => f.AssetId == assetId
                            && f.FailureDate >= periodStart
                            && f.FailureDate <= periodEnd)
                .OrderBy(f => f.FailureDate)
                .ToListAsync();

            var failureCount = failures.Count;
            var totalDowntimeHours = failures.Sum(f => f.DowntimeHours ?? 0);
            var totalOperatingHours = totalPeriodHours - totalDowntimeHours;

            // MTBF = Total Operating Time / Number of Failures
            var mtbf = failureCount > 0 ? Math.Round(totalOperatingHours / failureCount, 1) : totalPeriodHours;

            // MTTR = Total Repair Time / Number of Failures
            var mttr = failureCount > 0 ? Math.Round(totalDowntimeHours / failureCount, 1) : 0;

            // Availability = MTBF / (MTBF + MTTR)
            var availability = (mtbf + mttr) > 0 ? Math.Round(mtbf / (mtbf + mttr) * 100, 1) : 100;

            return new MTBFMTTRResultVm
            {
                AssetId = assetId,
                AssetCode = asset.AssetCode,
                AssetName = asset.Name,
                MTBF = mtbf,
                MTTR = mttr,
                Availability = availability,
                FailureCount = failureCount,
                TotalDowntimeHours = totalDowntimeHours,
                TotalOperatingHours = Math.Max(0, totalOperatingHours)
            };
        }

        /// <summary>
        /// Calculates MTBF/MTTR for all assets with failure history.
        /// </summary>
        public async Task<IReadOnlyList<MTBFMTTRResultVm>> CalculateAllAsync(DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            var assetIds = await _uow.FailureLogs.Query()
                .Select(f => f.AssetId)
                .Distinct()
                .ToListAsync();

            var results = new List<MTBFMTTRResultVm>();
            foreach (var assetId in assetIds)
            {
                try
                {
                    results.Add(await CalculateAsync(assetId, from, to));
                }
                catch { /* skip assets with issues */ }
            }

            return results.OrderBy(r => r.Availability).ToList();
        }
    }
}
