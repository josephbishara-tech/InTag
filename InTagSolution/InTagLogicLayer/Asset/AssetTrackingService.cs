using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Asset;

namespace InTagLogicLayer.Asset
{
    public class AssetTrackingService : IAssetTrackingService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AssetTrackingService> _logger;

        public AssetTrackingService(IUnitOfWork uow, ILogger<AssetTrackingService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<IReadOnlyList<TrackingRequestListVm>> GetRequestsAsync()
        {
            var items = await _uow.TrackingRequests.Query()
                .Include(r => r.Location)
                .Include(r => r.Lines)
                .OrderByDescending(r => r.CreatedDate).ToListAsync();

            return items.Select(r => new TrackingRequestListVm
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                LocationName = r.Location.Name,
                Status = r.Status,
                TotalAssets = r.TotalAssets,
                ScannedCount = r.Lines.Count(l => l.Status != TrackingLineStatus.Pending),
                CreatedDate = r.CreatedDate
            }).ToList();
        }

        public async Task<TrackingRequestDetailVm> GetRequestByIdAsync(int id)
        {
            var req = await _uow.TrackingRequests.Query()
                .Include(r => r.Location)
                .Include(r => r.Lines).ThenInclude(l => l.Asset)
                .Include(r => r.Lines).ThenInclude(l => l.FoundAtLocation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null) throw new KeyNotFoundException("Tracking request not found.");
            return MapDetail(req);
        }

        public async Task<TrackingRequestDetailVm> CreateRequestAsync(TrackingRequestCreateVm model)
        {
            var location = await _uow.Locations.GetByIdAsync(model.LocationId);
            if (location == null) throw new KeyNotFoundException("Location not found.");

            // Generate request number
            var prefix = $"TRK-{DateTimeOffset.UtcNow:yyyyMM}-";
            var existing = await _uow.TrackingRequests.FindAsync(r => r.RequestNumber.StartsWith(prefix));
            var maxSeq = 0;
            foreach (var r in existing)
            {
                var numPart = r.RequestNumber.Replace(prefix, "");
                if (int.TryParse(numPart, out var seq) && seq > maxSeq) maxSeq = seq;
            }
            var number = $"{prefix}{(maxSeq + 1):D5}";

            // Get all assets at this location
            var assetsAtLocation = await _uow.Assets.FindAsync(a => a.LocationId == model.LocationId);

            var request = new TrackingRequest
            {
                RequestNumber = number,
                Title = model.Title,
                Description = model.Description,
                Status = TrackingRequestStatus.Draft,
                LocationId = model.LocationId,
                AssignedToUserId = model.AssignedToUserId,
                TotalAssets = assetsAtLocation.Count
            };

            await _uow.TrackingRequests.AddAsync(request);
            await _uow.SaveChangesAsync();

            // Create tracking lines for each asset at the location
            foreach (var asset in assetsAtLocation)
            {
                var line = new TrackingLine
                {
                    TrackingRequestId = request.Id,
                    AssetId = asset.Id,
                    Status = TrackingLineStatus.Pending
                };
                await _uow.TrackingLines.AddAsync(line);
            }
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Tracking request {Number} created for location {Location} with {Count} assets",
                number, location.Name, assetsAtLocation.Count);

            return await GetRequestByIdAsync(request.Id);
        }

        public async Task<TrackingRequestDetailVm> OpenRequestAsync(int id)
        {
            var req = await _uow.TrackingRequests.GetByIdAsync(id);
            if (req == null) throw new KeyNotFoundException("Tracking request not found.");
            if (req.Status != TrackingRequestStatus.Draft)
                throw new InvalidOperationException("Only draft requests can be opened.");

            req.Status = TrackingRequestStatus.Open;
            _uow.TrackingRequests.Update(req);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Tracking request {Number} opened", req.RequestNumber);
            return await GetRequestByIdAsync(id);
        }

        public async Task<TrackingRequestDetailVm> CompleteRequestAsync(int id)
        {
            var req = await _uow.TrackingRequests.Query()
                .Include(r => r.Lines)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null) throw new KeyNotFoundException("Tracking request not found.");

            req.Status = TrackingRequestStatus.Completed;
            req.CompletedDate = DateTimeOffset.UtcNow;
            req.FoundCount = req.Lines.Count(l => l.Status == TrackingLineStatus.Found);
            req.MissingCount = req.Lines.Count(l => l.Status == TrackingLineStatus.Missing);
            req.RelocatedCount = req.Lines.Count(l => l.Status == TrackingLineStatus.Relocated);
            req.DamagedCount = req.Lines.Count(l => l.Status == TrackingLineStatus.Damaged);

            // Mark remaining pending as missing
            foreach (var line in req.Lines.Where(l => l.Status == TrackingLineStatus.Pending))
            {
                line.Status = TrackingLineStatus.Missing;
                _uow.TrackingLines.Update(line);
            }
            req.MissingCount = req.Lines.Count(l => l.Status == TrackingLineStatus.Missing);

            _uow.TrackingRequests.Update(req);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Tracking request {Number} completed: {Found} found, {Missing} missing, {Relocated} relocated",
                req.RequestNumber, req.FoundCount, req.MissingCount, req.RelocatedCount);

            return await GetRequestByIdAsync(id);
        }

        public async Task CancelRequestAsync(int id)
        {
            var req = await _uow.TrackingRequests.GetByIdAsync(id);
            if (req == null) throw new KeyNotFoundException("Tracking request not found.");
            req.Status = TrackingRequestStatus.Cancelled;
            _uow.TrackingRequests.Update(req);
            await _uow.SaveChangesAsync();
        }

        public async Task<TrackingLineVm> SubmitScanAsync(TrackingScanSubmitVm model)
        {
            var line = await _uow.TrackingLines.Query()
                .Include(l => l.Asset)
                .Include(l => l.TrackingRequest)
                .FirstOrDefaultAsync(l => l.Id == model.TrackingLineId);

            if (line == null) throw new KeyNotFoundException("Tracking line not found.");

            var req = line.TrackingRequest;
            if (req.Status != TrackingRequestStatus.Open && req.Status != TrackingRequestStatus.InProgress)
                throw new InvalidOperationException("Request is not open for scanning.");

            // Auto-set to InProgress on first scan
            if (req.Status == TrackingRequestStatus.Open)
            {
                req.Status = TrackingRequestStatus.InProgress;
                req.StartedDate = DateTimeOffset.UtcNow;
                _uow.TrackingRequests.Update(req);
            }

            line.Status = model.Status;
            line.ScannedDate = DateTimeOffset.UtcNow;
            line.ScannedCode = model.ScannedCode;
            line.Latitude = model.Latitude;
            line.Longitude = model.Longitude;
            line.FoundAtLocationId = model.FoundAtLocationId;
            line.ConditionAtScan = model.ConditionAtScan;
            line.Remarks = model.Remarks;

            _uow.TrackingLines.Update(line);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Asset {AssetCode} scanned as {Status} in request {Request}",
                line.Asset.AssetCode, model.Status, req.RequestNumber);

            return new TrackingLineVm
            {
                Id = line.Id,
                AssetId = line.AssetId,
                AssetCode = line.Asset.AssetCode,
                AssetName = line.Asset.Name,
                Status = line.Status,
                ScannedDate = line.ScannedDate,
                ScannedCode = line.ScannedCode,
                ConditionAtScan = line.ConditionAtScan,
                Remarks = line.Remarks,
                Latitude = line.Latitude,
                Longitude = line.Longitude
            };
        }

        public async Task MarkLineMissingAsync(int lineId)
        {
            var line = await _uow.TrackingLines.GetByIdAsync(lineId);
            if (line == null) throw new KeyNotFoundException("Tracking line not found.");
            line.Status = TrackingLineStatus.Missing;
            line.ScannedDate = DateTimeOffset.UtcNow;
            _uow.TrackingLines.Update(line);
            await _uow.SaveChangesAsync();
        }

        private static TrackingRequestDetailVm MapDetail(TrackingRequest req) => new()
        {
            Id = req.Id,
            RequestNumber = req.RequestNumber,
            Title = req.Title,
            Description = req.Description,
            Status = req.Status,
            LocationId = req.LocationId,
            LocationName = req.Location.Name,
            CreatedDate = req.CreatedDate,
            StartedDate = req.StartedDate,
            CompletedDate = req.CompletedDate,
            TotalAssets = req.TotalAssets,
            FoundCount = req.FoundCount,
            MissingCount = req.MissingCount,
            RelocatedCount = req.RelocatedCount,
            DamagedCount = req.DamagedCount,
            Notes = req.Notes,
            Lines = req.Lines.OrderBy(l => l.Asset.AssetCode).Select(l => new TrackingLineVm
            {
                Id = l.Id,
                AssetId = l.AssetId,
                AssetCode = l.Asset.AssetCode,
                AssetName = l.Asset.Name,
                Barcode = l.Asset.Barcode,
                SerialNumber = l.Asset.SerialNumber,
                Status = l.Status,
                ScannedDate = l.ScannedDate,
                ScannedCode = l.ScannedCode,
                FoundAtLocationName = l.FoundAtLocation?.Name,
                ConditionAtScan = l.ConditionAtScan,
                Remarks = l.Remarks,
                Latitude = l.Latitude,
                Longitude = l.Longitude
            }).ToList()
        };

        public async Task<TrackingLineVm> FindLineByScannedCodeAsync(int requestId, string scannedCode)
        {
            var code = scannedCode.Trim();

            var line = await _uow.TrackingLines.Query()
                .Include(l => l.Asset)
                .Include(l => l.TrackingRequest)
                .Include(l => l.FoundAtLocation)
                .Where(l => l.TrackingRequestId == requestId && l.Status == TrackingLineStatus.Pending)
                .FirstOrDefaultAsync(l =>
                    l.Asset.AssetCode == code
                    || l.Asset.Barcode == code
                    || l.Asset.SerialNumber == code
                    // Partial match for barcodes with prefix/suffix
                    || (l.Asset.Barcode != null && l.Asset.Barcode.Contains(code))
                    || (l.Asset.AssetCode != null && code.Contains(l.Asset.AssetCode))
                );

            if (line == null)
                throw new KeyNotFoundException($"No pending asset matches code '{code}' in this request.");

            return new TrackingLineVm
            {
                Id = line.Id,
                AssetId = line.AssetId,
                AssetCode = line.Asset.AssetCode,
                AssetName = line.Asset.Name,
                Barcode = line.Asset.Barcode,
                SerialNumber = line.Asset.SerialNumber,
                Status = line.Status,
                ScannedDate = line.ScannedDate,
                ScannedCode = line.ScannedCode,
                FoundAtLocationName = line.FoundAtLocation?.Name,
                ConditionAtScan = line.ConditionAtScan,
                Remarks = line.Remarks
            };
        }
    }
}
