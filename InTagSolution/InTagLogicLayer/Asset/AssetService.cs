using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Asset;

namespace InTagLogicLayer.Asset
{
    public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITenantService _tenantService;
        private readonly ILogger<AssetService> _logger;
        private readonly AssetExportService _exportService;
        private readonly IWorkflowHook _workflowHook;

        public AssetService(
            IUnitOfWork uow,
            ITenantService tenantService,
            ILogger<AssetService> logger,
            IWorkflowHook workflowHook)
        {
            _uow = uow;
            _tenantService = tenantService;
            _logger = logger;
            _exportService = new AssetExportService(uow);
            _workflowHook = workflowHook;
        }

        // ══════════════════════════════════════
        //  CRUD
        // ══════════════════════════════════════

        public async Task<AssetDetailVm> GetByIdAsync(int id)
        {
            var asset = await _uow.Assets.GetWithDetailsAsync(id);
            if (asset == null)
                throw new KeyNotFoundException($"Asset with ID {id} not found.");
            return MapToDetailVm(asset);
        }

        public async Task<AssetListResultVm> GetAllAsync(AssetFilterVm filter)
        {
            var query = _uow.Assets.Query()
                .Include(a => a.AssetType)
                .Include(a => a.Location)
                .Include(a => a.Department)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(a =>
                    a.AssetCode.Contains(term) ||
                    a.Name.Contains(term) ||
                    (a.SerialNumber != null && a.SerialNumber.Contains(term)));
            }

            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);

            if (filter.Category.HasValue)
                query = query.Where(a => a.Category == filter.Category.Value);

            if (filter.AssetTypeId.HasValue)
                query = query.Where(a => a.AssetTypeId == filter.AssetTypeId.Value);

            if (filter.LocationId.HasValue)
                query = query.Where(a => a.LocationId == filter.LocationId.Value);

            if (filter.DepartmentId.HasValue)
                query = query.Where(a => a.DepartmentId == filter.DepartmentId.Value);

            // Sort
            query = filter.SortBy?.ToLower() switch
            {
                "code" => filter.SortDescending ? query.OrderByDescending(a => a.AssetCode) : query.OrderBy(a => a.AssetCode),
                "status" => filter.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                "cost" => filter.SortDescending ? query.OrderByDescending(a => a.PurchaseCost) : query.OrderBy(a => a.PurchaseCost),
                "bookvalue" => filter.SortDescending ? query.OrderByDescending(a => a.CurrentBookValue) : query.OrderBy(a => a.CurrentBookValue),
                "date" => filter.SortDescending ? query.OrderByDescending(a => a.AcquisitionDate) : query.OrderBy(a => a.AcquisitionDate),
                _ => filter.SortDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new AssetListResultVm
            {
                Items = items.Select(MapToListItemVm).ToList(),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<AssetDetailVm> CreateAsync(AssetCreateVm model)
        {
            // Validation: unique asset code
            if (await _uow.Assets.AssetCodeExistsAsync(model.AssetCode))
                throw new InvalidOperationException($"Asset code '{model.AssetCode}' already exists.");

            // Validation: asset type exists
            var assetType = await _uow.AssetTypes.GetByIdAsync(model.AssetTypeId);
            if (assetType == null)
                throw new InvalidOperationException("Invalid asset type.");

            // Validation: warranty dates
            if (model.WarrantyStartDate.HasValue && model.WarrantyEndDate.HasValue
                && model.WarrantyEndDate < model.WarrantyStartDate)
                throw new InvalidOperationException("Warranty end date cannot be before start date.");

            // Validation: parent asset exists if specified
            if (model.ParentAssetId.HasValue)
            {
                var parent = await _uow.Assets.GetByIdAsync(model.ParentAssetId.Value);
                if (parent == null)
                    throw new InvalidOperationException("Parent asset not found.");
            }

            var salvageValue = model.SalvageValue ?? (model.PurchaseCost * assetType.DefaultSalvageValuePercent / 100);

            var asset = new AssetItem
            {
                AssetCode = model.AssetCode,
                Name = model.Name,
                Description = model.Description,
                Category = model.Category,
                Status = AssetStatus.Draft,
                AssetTypeId = model.AssetTypeId,
                PurchaseCost = model.PurchaseCost,
                SalvageValue = salvageValue,
                DepreciationMethod = model.DepreciationMethod,
                UsefulLifeMonths = model.UsefulLifeMonths,
                CurrentBookValue = model.PurchaseCost,
                AccumulatedDepreciation = 0,
                AcquisitionDate = model.AcquisitionDate,
                PurchaseOrderNumber = model.PurchaseOrderNumber,
                SerialNumber = model.SerialNumber,
                Barcode = model.Barcode,
                Manufacturer = model.Manufacturer,
                ModelNumber = model.ModelNumber,
                WarrantyStartDate = model.WarrantyStartDate,
                WarrantyEndDate = model.WarrantyEndDate,
                LocationId = model.LocationId,
                DepartmentId = model.DepartmentId,
                VendorId = model.VendorId,
                ParentAssetId = model.ParentAssetId,
                Notes = model.Notes
            };

            await _uow.Assets.AddAsync(asset);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Asset created: {AssetCode} (ID: {AssetId})", asset.AssetCode, asset.Id);

            return await GetByIdAsync(asset.Id);
        }

        public async Task<AssetDetailVm> UpdateAsync(int id, AssetUpdateVm model)
        {
            var asset = await _uow.Assets.GetByIdAsync(id);
            if (asset == null)
                throw new KeyNotFoundException($"Asset with ID {id} not found.");

            if (asset.Status == AssetStatus.Disposed)
                throw new InvalidOperationException("Cannot modify a disposed asset.");

            asset.Name = model.Name;
            asset.Description = model.Description;
            asset.Category = model.Category;
            asset.AssetTypeId = model.AssetTypeId;
            asset.SalvageValue = model.SalvageValue;
            asset.LocationId = model.LocationId;
            asset.DepartmentId = model.DepartmentId;
            asset.VendorId = model.VendorId;
            asset.ParentAssetId = model.ParentAssetId;
            asset.SerialNumber = model.SerialNumber;
            asset.Barcode = model.Barcode;
            asset.Manufacturer = model.Manufacturer;
            asset.ModelNumber = model.ModelNumber;
            asset.WarrantyStartDate = model.WarrantyStartDate;
            asset.WarrantyEndDate = model.WarrantyEndDate;
            asset.WarrantyTerms = model.WarrantyTerms;
            asset.Notes = model.Notes;

            _uow.Assets.Update(asset);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Asset updated: {AssetCode} (ID: {AssetId})", asset.AssetCode, asset.Id);

            return await GetByIdAsync(asset.Id);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var asset = await _uow.Assets.GetByIdAsync(id);
            if (asset == null)
                throw new KeyNotFoundException($"Asset with ID {id} not found.");

            var childCount = await _uow.Assets.CountAsync(a => a.ParentAssetId == id);
            if (childCount > 0)
                throw new InvalidOperationException("Cannot delete an asset that has child assets.");

            _uow.Assets.SoftDelete(asset);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Asset soft-deleted: {AssetCode} (ID: {AssetId})", asset.AssetCode, id);
        }

        // ══════════════════════════════════════
        //  LIFECYCLE
        // ══════════════════════════════════════

        public async Task<AssetDetailVm> ChangeStatusAsync(int id, AssetStatus newStatus)
        {
            var asset = await _uow.Assets.GetByIdAsync(id);
            if (asset == null)
                throw new KeyNotFoundException($"Asset with ID {id} not found.");

            // Validate state transitions
            ValidateStatusTransition(asset.Status, newStatus);

            asset.Status = newStatus;

            // Auto-set acquisition date on commissioning
            if (newStatus == AssetStatus.Commissioned && !asset.AcquisitionDate.HasValue)
                asset.AcquisitionDate = DateTimeOffset.UtcNow;

            _uow.Assets.Update(asset);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Asset {AssetCode} status changed to {NewStatus}", asset.AssetCode, newStatus);
            await _workflowHook.OnAssetStatusChangedAsync(id, asset.Status.ToString(), newStatus.ToString());
            return await GetByIdAsync(id);
        }

        public async Task<AssetTransferResultVm> TransferAsync(AssetTransferCreateVm model)
        {
            var asset = await _uow.Assets.GetByIdAsync(model.AssetId);
            if (asset == null)
                throw new KeyNotFoundException("Asset not found.");

            if (asset.Status == AssetStatus.Disposed || asset.Status == AssetStatus.Decommissioned)
                throw new InvalidOperationException("Cannot transfer a disposed or decommissioned asset.");

            if (model.FromLocationId == model.ToLocationId)
                throw new InvalidOperationException("From and To locations must be different.");

            var fromLocation = await _uow.Locations.GetByIdAsync(model.FromLocationId);
            var toLocation = await _uow.Locations.GetByIdAsync(model.ToLocationId);
            if (fromLocation == null || toLocation == null)
                throw new InvalidOperationException("Invalid location.");

            var transfer = new AssetTransfer
            {
                AssetId = model.AssetId,
                FromLocationId = model.FromLocationId,
                ToLocationId = model.ToLocationId,
                TransferDate = DateTimeOffset.UtcNow,
                Status = TransferStatus.Completed,
                Reason = model.Reason,
                Notes = model.Notes
            };

            // Update the asset's location
            asset.LocationId = model.ToLocationId;

            await _uow.AssetTransfers.AddAsync(transfer);
            _uow.Assets.Update(asset);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Asset {AssetCode} transferred from {From} to {To}",
                asset.AssetCode, fromLocation.Name, toLocation.Name);
            await _workflowHook.OnAssetTransferredAsync(model.AssetId, model.FromLocationId, model.ToLocationId);

            return new AssetTransferResultVm
            {
                TransferId = transfer.Id,
                AssetName = asset.Name,
                FromLocationName = fromLocation.Name,
                ToLocationName = toLocation.Name,
                Status = transfer.Status.ToString()
            };
        }

        // ══════════════════════════════════════
        //  DEPRECIATION
        // ══════════════════════════════════════

        public async Task<DepreciationResultVm> RunDepreciationAsync(
            int assetId, int fiscalYear, int fiscalMonth, decimal? unitsProduced = null)
        {
            var asset = await _uow.Assets.GetByIdAsync(assetId);
            if (asset == null)
                throw new KeyNotFoundException("Asset not found.");

            if (asset.Status != AssetStatus.Operational && asset.Status != AssetStatus.Commissioned)
                throw new InvalidOperationException("Depreciation can only be run on operational/commissioned assets.");

            if (!asset.AcquisitionDate.HasValue)
                throw new InvalidOperationException("Asset has no acquisition date.");

            var period = $"{fiscalYear}-{fiscalMonth:D2}";

            // Check if already depreciated for this period
            var existing = await _uow.DepreciationRecords
                .FirstOrDefaultAsync(d => d.AssetId == assetId && d.Period == period);
            if (existing != null)
                throw new InvalidOperationException($"Depreciation already recorded for period {period}.");

            // Calculate elapsed months since acquisition
            var acquisitionDate = asset.AcquisitionDate.Value;
            var periodDate = new DateTimeOffset(fiscalYear, fiscalMonth, 1, 0, 0, 0, TimeSpan.Zero);
            var elapsedMonths = ((periodDate.Year - acquisitionDate.Year) * 12)
                                + (periodDate.Month - acquisitionDate.Month);

            if (elapsedMonths < 0)
                throw new InvalidOperationException("Period is before the asset acquisition date.");

            // Run the depreciation engine
            var result = DepreciationEngine.Calculate(
                method: asset.DepreciationMethod,
                purchaseCost: asset.PurchaseCost,
                salvageValue: asset.SalvageValue ?? 0,
                usefulLifeMonths: asset.UsefulLifeMonths,
                currentBookValue: asset.CurrentBookValue,
                accumulatedDepreciation: asset.AccumulatedDepreciation,
                elapsedMonths: elapsedMonths,
                unitsProducedThisPeriod: unitsProduced,
                totalEstimatedUnits: null // Would come from asset config for UoP
            );

            // Create the depreciation record
            var record = new DepreciationRecord
            {
                AssetId = assetId,
                Period = period,
                FiscalYear = fiscalYear,
                FiscalMonth = fiscalMonth,
                Method = asset.DepreciationMethod,
                OpeningBookValue = asset.CurrentBookValue,
                DepreciationAmount = result.DepreciationAmount,
                AccumulatedDepreciation = result.AccumulatedDepreciation,
                ClosingBookValue = result.ClosingBookValue,
                UnitsProduced = unitsProduced,
                IsPosted = true
            };

            // Update the asset
            asset.CurrentBookValue = result.ClosingBookValue;
            asset.AccumulatedDepreciation = result.AccumulatedDepreciation;

            await _uow.DepreciationRecords.AddAsync(record);
            _uow.Assets.Update(asset);
            await _uow.SaveChangesAsync();

            _logger.LogInformation(
                "Depreciation recorded for {AssetCode}: Period={Period}, Amount={Amount:C}",
                asset.AssetCode, period, result.DepreciationAmount);

            return new DepreciationResultVm
            {
                AssetId = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.Name,
                Period = period,
                OpeningBookValue = record.OpeningBookValue,
                DepreciationAmount = record.DepreciationAmount,
                AccumulatedDepreciation = record.AccumulatedDepreciation,
                ClosingBookValue = record.ClosingBookValue,
                Method = asset.DepreciationMethod.ToString(),
                IsFullyDepreciated = result.ClosingBookValue <= (asset.SalvageValue ?? 0)
            };
        }

        public async Task<int> RunBulkDepreciationAsync(int fiscalYear, int fiscalMonth)
        {
            var period = $"{fiscalYear}-{fiscalMonth:D2}";

            // Get all depreciable assets
            var assets = await _uow.Assets.Query()
                .Where(a => (a.Status == AssetStatus.Operational || a.Status == AssetStatus.Commissioned)
                            && a.AcquisitionDate.HasValue
                            && a.CurrentBookValue > (a.SalvageValue ?? 0))
                .ToListAsync();

            var count = 0;

            foreach (var asset in assets)
            {
                try
                {
                    // Skip if already depreciated this period
                    var exists = await _uow.DepreciationRecords
                        .ExistsAsync(d => d.AssetId == asset.Id && d.Period == period);
                    if (exists) continue;

                    await RunDepreciationAsync(asset.Id, fiscalYear, fiscalMonth);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Bulk depreciation skipped for asset {AssetCode}: {Message}",
                        asset.AssetCode, ex.Message);
                }
            }

            _logger.LogInformation(
                "Bulk depreciation completed: {Count} assets processed for period {Period}",
                count, period);

            return count;
        }

        // ══════════════════════════════════════
        //  INSPECTIONS
        // ══════════════════════════════════════

        public async Task<InspectionResultVm> CreateInspectionAsync(InspectionCreateVm model)
        {
            var asset = await _uow.Assets.GetByIdAsync(model.AssetId);
            if (asset == null)
                throw new KeyNotFoundException("Asset not found.");

            if (asset.Status == AssetStatus.Disposed)
                throw new InvalidOperationException("Cannot inspect a disposed asset.");

            var inspection = new Inspection
            {
                AssetId = model.AssetId,
                InspectionDate = DateTimeOffset.UtcNow,
                ConditionScore = model.ConditionScore,
                Findings = model.Findings,
                Recommendations = model.Recommendations,
                NextDueDate = model.NextDueDate,
                InspectorUserId = _tenantService.GetCurrentUserId(),
                ChecklistName = model.ChecklistName,
                Notes = model.Notes
            };

            await _uow.Inspections.AddAsync(inspection);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Inspection recorded for {AssetCode}: Score={Score}",
                asset.AssetCode, model.ConditionScore);
            await _workflowHook.OnInspectionCompletedAsync(model.AssetId, inspection.Id, model.ConditionScore.ToString());

            return new InspectionResultVm
            {
                InspectionId = inspection.Id,
                AssetName = asset.Name,
                ConditionScore = model.ConditionScore.ToString(),
                InspectionDate = inspection.InspectionDate,
                NextDueDate = model.NextDueDate
            };
        }

        // ══════════════════════════════════════
        //  LOOKUPS
        // ══════════════════════════════════════

        public async Task<IReadOnlyList<AssetTypeListVm>> GetAssetTypesAsync()
        {
            var types = await _uow.AssetTypes.GetAllAsync();
            return types.Select(t => new AssetTypeListVm
            {
                Id = t.Id,
                Name = t.Name,
                Category = t.Category.ToString()
            }).ToList();
        }

        public async Task<IReadOnlyList<LocationListVm>> GetLocationsAsync()
        {
            var locations = await _uow.Locations.GetAllAsync();
            return locations.Select(l => new LocationListVm
            {
                Id = l.Id,
                Name = l.Name,
                Code = l.Code
            }).ToList();
        }

        public async Task<IReadOnlyList<DepartmentListVm>> GetDepartmentsAsync()
        {
            var departments = await _uow.Departments.GetAllAsync();
            return departments.Select(d => new DepartmentListVm
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code
            }).ToList();
        }

        public async Task<IReadOnlyList<VendorListVm>> GetVendorsAsync()
        {
            var vendors = await _uow.Vendors.GetAllAsync();
            return vendors.Select(v => new VendorListVm
            {
                Id = v.Id,
                Name = v.Name
            }).ToList();
        }

        // ══════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════

        private static void ValidateStatusTransition(AssetStatus current, AssetStatus target)
        {
            // Valid transitions:
            // Draft → Acquired → Commissioned → Operational → UnderMaintenance ↔ Operational
            //                                              → Decommissioned → Disposed
            var validTransitions = new Dictionary<AssetStatus, AssetStatus[]>
            {
                [AssetStatus.Draft] = new[] { AssetStatus.Acquired },
                [AssetStatus.Acquired] = new[] { AssetStatus.Commissioned },
                [AssetStatus.Commissioned] = new[] { AssetStatus.Operational },
                [AssetStatus.Operational] = new[] { AssetStatus.UnderMaintenance, AssetStatus.Decommissioned },
                [AssetStatus.UnderMaintenance] = new[] { AssetStatus.Operational, AssetStatus.Decommissioned },
                [AssetStatus.Decommissioned] = new[] { AssetStatus.Disposed }
            };

            if (!validTransitions.ContainsKey(current) || !validTransitions[current].Contains(target))
            {
                throw new InvalidOperationException(
                    $"Cannot transition from '{current}' to '{target}'.");
            }
        }

        private static AssetDetailVm MapToDetailVm(AssetItem asset)
        {
            return new AssetDetailVm
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                Name = asset.Name,
                Description = asset.Description,
                Category = asset.Category,
                Status = asset.Status,
                PurchaseCost = asset.PurchaseCost,
                SalvageValue = asset.SalvageValue,
                DepreciationMethod = asset.DepreciationMethod,
                UsefulLifeMonths = asset.UsefulLifeMonths,
                CurrentBookValue = asset.CurrentBookValue,
                AccumulatedDepreciation = asset.AccumulatedDepreciation,
                AcquisitionDate = asset.AcquisitionDate,
                WarrantyStartDate = asset.WarrantyStartDate,
                WarrantyEndDate = asset.WarrantyEndDate,
                SerialNumber = asset.SerialNumber,
                Barcode = asset.Barcode,
                Manufacturer = asset.Manufacturer,
                ModelNumber = asset.ModelNumber,
                AssetTypeName = asset.AssetType?.Name,
                LocationName = asset.Location?.Name,
                DepartmentName = asset.Department?.Name,
                VendorName = asset.Vendor?.Name,
                ParentAssetName = asset.ParentAsset?.Name,
                ParentAssetId = asset.ParentAssetId,
                ChildAssetCount = asset.ChildAssets?.Count ?? 0,
                Notes = asset.Notes,
                CreatedDate = asset.CreatedDate,
                ModifiedDate = asset.ModifiedDate
            };
        }

        private static AssetListItemVm MapToListItemVm(AssetItem asset)
        {
            return new AssetListItemVm
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                Name = asset.Name,
                Category = asset.Category,
                Status = asset.Status,
                PurchaseCost = asset.PurchaseCost,
                CurrentBookValue = asset.CurrentBookValue,
                AssetTypeName = asset.AssetType?.Name,
                LocationName = asset.Location?.Name,
                DepartmentName = asset.Department?.Name,
                AcquisitionDate = asset.AcquisitionDate
            };
        }

        // ══════════════════════════════════════
        //  DEPRECIATION HISTORY & FORECAST
        // ══════════════════════════════════════
        public async Task<IReadOnlyList<DepreciationResultVm>> GetDepreciationHistoryAsync(int assetId)
        {
            var asset = await _uow.Assets.GetByIdAsync(assetId);
            if (asset == null)
                throw new KeyNotFoundException("Asset not found.");

            var records = await _uow.DepreciationRecords
                .Query()
                .Where(d => d.AssetId == assetId)
                .OrderBy(d => d.Period)
                .ToListAsync();

            return records.Select(d => new DepreciationResultVm
            {
                AssetId = d.AssetId,
                AssetCode = asset.AssetCode,
                AssetName = asset.Name,
                Period = d.Period,
                OpeningBookValue = d.OpeningBookValue,
                DepreciationAmount = d.DepreciationAmount,
                AccumulatedDepreciation = d.AccumulatedDepreciation,
                ClosingBookValue = d.ClosingBookValue,
                Method = d.Method.ToString(),
                IsFullyDepreciated = d.ClosingBookValue <= (asset.SalvageValue ?? 0)
            }).ToList();
        }

        public async Task<IReadOnlyList<DepreciationForecastVm>> ForecastDepreciationAsync(
            int assetId, int monthsAhead)
        {
            var asset = await _uow.Assets.GetByIdAsync(assetId);
            if (asset == null)
                throw new KeyNotFoundException("Asset not found.");

            if (!asset.AcquisitionDate.HasValue)
                throw new InvalidOperationException("Asset has no acquisition date.");

            var forecast = new List<DepreciationForecastVm>();
            var currentBookValue = asset.CurrentBookValue;
            var accumulatedDep = asset.AccumulatedDepreciation;
            var salvageValue = asset.SalvageValue ?? 0;
            var now = DateTimeOffset.UtcNow;

            for (var i = 1; i <= monthsAhead; i++)
            {
                if (currentBookValue <= salvageValue) break;

                var futureDate = now.AddMonths(i);
                var elapsedMonths = ((futureDate.Year - asset.AcquisitionDate.Value.Year) * 12)
                                    + (futureDate.Month - asset.AcquisitionDate.Value.Month);

                var result = DepreciationEngine.Calculate(
                    method: asset.DepreciationMethod,
                    purchaseCost: asset.PurchaseCost,
                    salvageValue: salvageValue,
                    usefulLifeMonths: asset.UsefulLifeMonths,
                    currentBookValue: currentBookValue,
                    accumulatedDepreciation: accumulatedDep,
                    elapsedMonths: elapsedMonths);

                forecast.Add(new DepreciationForecastVm
                {
                    Period = $"{futureDate.Year}-{futureDate.Month:D2}",
                    OpeningBookValue = currentBookValue,
                    DepreciationAmount = result.DepreciationAmount,
                    AccumulatedDepreciation = result.AccumulatedDepreciation,
                    ClosingBookValue = result.ClosingBookValue,
                    IsProjected = true
                });

                currentBookValue = result.ClosingBookValue;
                accumulatedDep = result.AccumulatedDepreciation;
            }

            return forecast;
        }

        public async Task<DepreciationSummaryVm> GetDepreciationSummaryAsync(int? fiscalYear = null)
        {
            var year = fiscalYear ?? DateTimeOffset.UtcNow.Year;

            var allAssets = await _uow.Assets.Query().ToListAsync();

            var depreciableAssets = allAssets
                .Where(a => a.Status == AssetStatus.Operational || a.Status == AssetStatus.Commissioned)
                .ToList();

            var fullyDepreciated = allAssets
                .Where(a => a.CurrentBookValue <= (a.SalvageValue ?? 0) && a.PurchaseCost > 0)
                .ToList();

            var yearRecords = await _uow.DepreciationRecords
                .Query()
                .Where(d => d.FiscalYear == year)
                .ToListAsync();

            // By method
            var byMethod = allAssets
                .GroupBy(a => a.DepreciationMethod)
                .Select(g => new DepreciationByMethodVm
                {
                    Method = g.Key.ToString(),
                    AssetCount = g.Count(),
                    TotalDepreciation = yearRecords
                        .Where(r => g.Select(a => a.Id).Contains(r.AssetId))
                        .Sum(r => r.DepreciationAmount)
                }).ToList();

            // By month this year
            var byMonth = yearRecords
                .GroupBy(d => d.Period)
                .OrderBy(g => g.Key)
                .Select(g => new DepreciationByMonthVm
                {
                    Period = g.Key,
                    TotalDepreciation = g.Sum(d => d.DepreciationAmount),
                    AssetsProcessed = g.Count()
                }).ToList();

            // By category
            var byCategory = allAssets
                .GroupBy(a => a.Category)
                .Select(g => new DepreciationByCategoryVm
                {
                    Category = g.Key.ToString(),
                    AssetCount = g.Count(),
                    TotalCost = g.Sum(a => a.PurchaseCost),
                    TotalBookValue = g.Sum(a => a.CurrentBookValue),
                    TotalDepreciation = g.Sum(a => a.AccumulatedDepreciation)
                }).ToList();

            return new DepreciationSummaryVm
            {
                FiscalYear = year,
                TotalAssets = allAssets.Count,
                DepreciableAssets = depreciableAssets.Count,
                FullyDepreciatedAssets = fullyDepreciated.Count,
                TotalPurchaseCost = allAssets.Sum(a => a.PurchaseCost),
                TotalBookValue = allAssets.Sum(a => a.CurrentBookValue),
                TotalAccumulatedDepreciation = allAssets.Sum(a => a.AccumulatedDepreciation),
                TotalDepreciationThisYear = yearRecords.Sum(d => d.DepreciationAmount),
                ByMethod = byMethod,
                ByMonth = byMonth,
                ByCategory = byCategory
            };
        }

        public async Task<BulkDepreciationResultVm> RunBulkDepreciationDetailedAsync(int fiscalYear, int fiscalMonth)
        {
            var period = $"{fiscalYear}-{fiscalMonth:D2}";
            var details = new List<BulkDepreciationItemVm>();
            var totalDep = 0m;

            var assets = await _uow.Assets.Query()
                .Where(a => (a.Status == AssetStatus.Operational || a.Status == AssetStatus.Commissioned)
                            && a.AcquisitionDate.HasValue)
                .ToListAsync();

            foreach (var asset in assets)
            {
                // Already depreciated?
                var exists = await _uow.DepreciationRecords
                    .ExistsAsync(d => d.AssetId == asset.Id && d.Period == period);
                if (exists)
                {
                    details.Add(new BulkDepreciationItemVm
                    {
                        AssetCode = asset.AssetCode,
                        AssetName = asset.Name,
                        DepreciationAmount = 0,
                        ClosingBookValue = asset.CurrentBookValue,
                        Status = "Skipped",
                        ErrorMessage = "Already processed for this period"
                    });
                    continue;
                }

                // Fully depreciated?
                if (asset.CurrentBookValue <= (asset.SalvageValue ?? 0))
                {
                    details.Add(new BulkDepreciationItemVm
                    {
                        AssetCode = asset.AssetCode,
                        AssetName = asset.Name,
                        DepreciationAmount = 0,
                        ClosingBookValue = asset.CurrentBookValue,
                        Status = "Skipped",
                        ErrorMessage = "Fully depreciated"
                    });
                    continue;
                }

                try
                {
                    var result = await RunDepreciationAsync(asset.Id, fiscalYear, fiscalMonth);
                    totalDep += result.DepreciationAmount;
                    details.Add(new BulkDepreciationItemVm
                    {
                        AssetCode = result.AssetCode,
                        AssetName = result.AssetName,
                        DepreciationAmount = result.DepreciationAmount,
                        ClosingBookValue = result.ClosingBookValue,
                        Status = "Processed"
                    });
                }
                catch (Exception ex)
                {
                    details.Add(new BulkDepreciationItemVm
                    {
                        AssetCode = asset.AssetCode,
                        AssetName = asset.Name,
                        DepreciationAmount = 0,
                        ClosingBookValue = asset.CurrentBookValue,
                        Status = "Failed",
                        ErrorMessage = ex.Message
                    });
                }
            }

            _logger.LogInformation("Bulk depreciation for {Period}: {Processed} processed, {Skipped} skipped, {Failed} failed",
                period,
                details.Count(d => d.Status == "Processed"),
                details.Count(d => d.Status == "Skipped"),
                details.Count(d => d.Status == "Failed"));

            var bulkResult = new BulkDepreciationResultVm
            {
                AssetsProcessed = details.Count(d => d.Status == "Processed"),
                AssetsSkipped = details.Count(d => d.Status == "Skipped"),
                AssetsFailed = details.Count(d => d.Status == "Failed"),
                Period = period,
                TotalDepreciation = totalDep,
                Details = details
            };

            await _workflowHook.OnDepreciationRunCompletedAsync(period, bulkResult.AssetsProcessed, bulkResult.TotalDepreciation);

            return bulkResult;
        }


        // ══════════════════════════════════════
        //  DASHBOARD
        // ══════════════════════════════════════
        public async Task<AssetDashboardVm> GetDashboardAsync()
        {
            var allAssets = await _uow.Assets.Query()
                .Include(a => a.AssetType)
                .Include(a => a.Location)
                .ToListAsync();

            var now = DateTimeOffset.UtcNow;

            // Warranty expiring within 30 days
            var warrantyAlerts = allAssets
                .Where(a => a.WarrantyEndDate.HasValue
                            && a.WarrantyEndDate > now
                            && a.WarrantyEndDate <= now.AddDays(30))
                .Select(a => new WarrantyAlertVm
                {
                    AssetId = a.Id,
                    AssetCode = a.AssetCode,
                    Name = a.Name,
                    WarrantyEndDate = a.WarrantyEndDate!.Value,
                    DaysRemaining = (int)(a.WarrantyEndDate!.Value - now).TotalDays
                })
                .OrderBy(a => a.DaysRemaining)
                .ToList();

            // Overdue inspections
            var inspectionAlerts = await _uow.Inspections.Query()
                .Where(i => i.NextDueDate.HasValue && i.NextDueDate < now)
                .OrderBy(i => i.NextDueDate)
                .Take(10)
                .Select(i => new InspectionAlertVm
                {
                    AssetId = i.AssetId,
                    AssetCode = i.Asset.AssetCode,
                    Name = i.Asset.Name,
                    DueDate = i.NextDueDate!.Value,
                    DaysOverdue = (int)(now - i.NextDueDate!.Value).TotalDays
                })
                .ToListAsync();

            // Assets in poor/critical condition (latest inspection)
            var conditionAlerts = await _uow.Inspections.Query()
                .Where(i => i.ConditionScore <= ConditionRating.Poor)
                .OrderByDescending(i => i.InspectionDate)
                .Take(10)
                .Select(i => new ConditionAlertVm
                {
                    AssetId = i.AssetId,
                    AssetCode = i.Asset.AssetCode,
                    Name = i.Asset.Name,
                    ConditionScore = i.ConditionScore,
                    InspectionDate = i.InspectionDate
                })
                .ToListAsync();

            // By status
            var byStatus = allAssets
                .GroupBy(a => a.Status)
                .Select(g => new StatusBreakdownVm
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = allAssets.Count > 0 ? Math.Round(g.Count() * 100m / allAssets.Count, 1) : 0
                })
                .OrderByDescending(s => s.Count)
                .ToList();

            // By category
            var byCategory = allAssets
                .GroupBy(a => a.Category)
                .Select(g => new CategoryBreakdownVm
                {
                    Category = g.Key.ToString(),
                    Count = g.Count(),
                    TotalValue = g.Sum(a => a.CurrentBookValue)
                })
                .OrderByDescending(c => c.TotalValue)
                .ToList();

            // By location
            var byLocation = allAssets
                .Where(a => a.Location != null)
                .GroupBy(a => a.Location!.Name)
                .Select(g => new LocationBreakdownVm
                {
                    Location = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(a => a.CurrentBookValue)
                })
                .OrderByDescending(l => l.Count)
                .ToList();

            // Recent activity (transfers + inspections)
            var recentTransfers = await _uow.AssetTransfers.Query()
                .OrderByDescending(t => t.TransferDate)
                .Take(5)
                .Select(t => new RecentActivityVm
                {
                    Icon = "bi-arrow-left-right",
                    Description = $"{t.Asset.AssetCode} transferred to {t.ToLocation.Name}",
                    Date = t.TransferDate,
                    AssetId = t.AssetId
                })
                .ToListAsync();

            var recentInspections = await _uow.Inspections.Query()
                .OrderByDescending(i => i.InspectionDate)
                .Take(5)
                .Select(i => new RecentActivityVm
                {
                    Icon = "bi-clipboard-check",
                    Description = $"{i.Asset.AssetCode} inspected — {i.ConditionScore}",
                    Date = i.InspectionDate,
                    AssetId = i.AssetId
                })
                .ToListAsync();

            var recentActivity = recentTransfers.Concat(recentInspections)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToList();

            return new AssetDashboardVm
            {
                TotalAssets = allAssets.Count,
                OperationalAssets = allAssets.Count(a => a.Status == AssetStatus.Operational),
                UnderMaintenanceAssets = allAssets.Count(a => a.Status == AssetStatus.UnderMaintenance),
                DecommissionedAssets = allAssets.Count(a => a.Status == AssetStatus.Decommissioned),
                TotalAssetValue = allAssets.Sum(a => a.PurchaseCost),
                TotalBookValue = allAssets.Sum(a => a.CurrentBookValue),
                WarrantyAlerts = warrantyAlerts,
                InspectionAlerts = inspectionAlerts,
                ConditionAlerts = conditionAlerts,
                ByStatus = byStatus,
                ByCategory = byCategory,
                ByLocation = byLocation,
                RecentActivity = recentActivity
            };
        }
       

        public Task<byte[]> ExportAssetRegisterAsync(AssetFilterVm filter)
            => _exportService.ExportAssetRegisterAsync(filter);

        public Task<byte[]> ExportDepreciationScheduleAsync(int fiscalYear)
            => _exportService.ExportDepreciationScheduleAsync(fiscalYear);

        public Task<byte[]> ExportTCOReportAsync()
            => _exportService.ExportTCOReportAsync();

    }
}
