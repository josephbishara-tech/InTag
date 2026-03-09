using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Maintenance;
using InTagEntitiesLayer.Interfaces;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Maintenance;

namespace InTagLogicLayer.Maintenance
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITenantService _tenantService;
        private readonly ILogger<MaintenanceService> _logger;
        private readonly IWorkflowHook _workflowHook;

        public MaintenanceService(
            IUnitOfWork uow,
            ITenantService tenantService,
            ILogger<MaintenanceService> logger,
            IWorkflowHook workflowHook)
        {
            _uow = uow;
            _tenantService = tenantService;
            _logger = logger;
            _workflowHook = workflowHook;
        }

        // ═══════════════════════════════════════
        //  WORK ORDERS
        // ═══════════════════════════════════════

        public async Task<WorkOrderDetailVm> GetWorkOrderByIdAsync(int id)
        {
            var wo = await _uow.WorkOrders.GetWithDetailsAsync(id);
            if (wo == null) throw new KeyNotFoundException("Work order not found.");
            return MapToDetailVm(wo);
        }

        public async Task<WorkOrderListResultVm> GetWorkOrdersAsync(WorkOrderFilterVm filter)
        {
            var query = _uow.WorkOrders.Query()
                .Include(w => w.Asset).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(w => w.WorkOrderNumber.Contains(term)
                                         || w.Title.Contains(term)
                                         || w.Asset.AssetCode.Contains(term));
            }
            if (filter.Status.HasValue) query = query.Where(w => w.Status == filter.Status.Value);
            if (filter.Type.HasValue) query = query.Where(w => w.Type == filter.Type.Value);
            if (filter.Priority.HasValue) query = query.Where(w => w.Priority == filter.Priority.Value);
            if (filter.AssetId.HasValue) query = query.Where(w => w.AssetId == filter.AssetId.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "title" => filter.SortDescending ? query.OrderByDescending(w => w.Title) : query.OrderBy(w => w.Title),
                "status" => filter.SortDescending ? query.OrderByDescending(w => w.Status) : query.OrderBy(w => w.Status),
                "priority" => filter.SortDescending ? query.OrderByDescending(w => w.Priority) : query.OrderBy(w => w.Priority),
                "due" => filter.SortDescending ? query.OrderByDescending(w => w.DueDate) : query.OrderBy(w => w.DueDate),
                "asset" => filter.SortDescending ? query.OrderByDescending(w => w.Asset.AssetCode) : query.OrderBy(w => w.Asset.AssetCode),
                _ => filter.SortDescending ? query.OrderByDescending(w => w.WorkOrderNumber) : query.OrderBy(w => w.WorkOrderNumber)
            };

            var total = await query.CountAsync();
            var items = await query.Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize).ToListAsync();

            return new WorkOrderListResultVm
            {
                Items = items.Select(w => new WorkOrderListItemVm
                {
                    Id = w.Id,
                    WorkOrderNumber = w.WorkOrderNumber,
                    Title = w.Title,
                    AssetCode = w.Asset.AssetCode,
                    AssetName = w.Asset.Name,
                    Type = w.Type,
                    Priority = w.Priority,
                    Status = w.Status,
                    DueDate = w.DueDate
                }).ToList(),
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<WorkOrderDetailVm> CreateWorkOrderAsync(WorkOrderCreateVm model)
        {
            var asset = await _uow.Assets.GetByIdAsync(model.AssetId);
            if (asset == null) throw new KeyNotFoundException("Asset not found.");

            var number = await WorkOrderNumberGenerator.GenerateAsync(_uow);

            var wo = new WorkOrder
            {
                WorkOrderNumber = number,
                Title = model.Title,
                Description = model.Description,
                Type = model.Type,
                Priority = model.Priority,
                Status = WorkOrderStatus.Open,
                AssetId = model.AssetId,
                AssignedToUserId = model.AssignedToUserId,
                DueDate = model.DueDate,
                SLATargetHours = model.SLATargetHours,
                FailureType = model.FailureType,
                FailureCause = model.FailureCause,
                Notes = model.Notes
            };

            await _uow.WorkOrders.AddAsync(wo);
            await _uow.SaveChangesAsync();

            // If corrective/emergency, create failure log
            if (model.Type == WorkOrderType.Corrective || model.Type == WorkOrderType.Emergency)
            {
                var failure = new FailureLog
                {
                    AssetId = model.AssetId,
                    WorkOrderId = wo.Id,
                    FailureType = model.FailureType ?? FailureType.Unknown,
                    FailureDate = DateTimeOffset.UtcNow,
                    Description = model.FailureCause
                };
                await _uow.FailureLogs.AddAsync(failure);
                await _uow.SaveChangesAsync();
            }

            _logger.LogInformation("Work order created: {Number} for asset {Asset}", number, asset.AssetCode);
            if (model.Priority == WorkOrderPriority.Critical || model.Type == WorkOrderType.Emergency)
                await _workflowHook.OnCriticalWorkOrderCreatedAsync(wo.Id, number, asset.AssetCode);
            return await GetWorkOrderByIdAsync(wo.Id);
        }

        public async Task<WorkOrderDetailVm> ChangeStatusAsync(int id, WorkOrderStatus newStatus)
        {
            var wo = await _uow.WorkOrders.GetByIdAsync(id);
            if (wo == null) throw new KeyNotFoundException("Work order not found.");

            ValidateStatusTransition(wo.Status, newStatus);

            var oldStatus = wo.Status;

            if (newStatus == WorkOrderStatus.InProgress && !wo.StartedDate.HasValue)
                wo.StartedDate = DateTimeOffset.UtcNow;

            wo.Status = newStatus;
            _uow.WorkOrders.Update(wo);
            await _uow.SaveChangesAsync();

            await _workflowHook.OnWorkOrderStatusChangedAsync(id, wo.WorkOrderNumber, oldStatus.ToString(), newStatus.ToString());

            _logger.LogInformation("Work order {Number} status → {Status}", wo.WorkOrderNumber, newStatus);
            return await GetWorkOrderByIdAsync(id);
        }

        public async Task<WorkOrderDetailVm> CompleteWorkOrderAsync(WorkOrderCompleteVm model)
        {
            var wo = await _uow.WorkOrders.GetWithDetailsAsync(model.WorkOrderId);
            if (wo == null) throw new KeyNotFoundException("Work order not found.");

            if (wo.Status != WorkOrderStatus.InProgress && wo.Status != WorkOrderStatus.Open)
                throw new InvalidOperationException("Work order must be in progress or open to complete.");

            wo.Status = WorkOrderStatus.Completed;
            wo.CompletedDate = DateTimeOffset.UtcNow;
            wo.Resolution = model.Resolution;
            wo.ExternalCost = model.ExternalCost;

            // Recalculate costs
            wo.LaborCost = wo.LaborEntries.Sum(l => l.HoursWorked * l.HourlyRate);
            wo.PartsCost = wo.Parts.Sum(p => p.Quantity * p.UnitCost);

            _uow.WorkOrders.Update(wo);

            // Update failure log with repair end
            if (wo.Type == WorkOrderType.Corrective || wo.Type == WorkOrderType.Emergency)
            {
                var failure = await _uow.FailureLogs.Query()
                    .FirstOrDefaultAsync(f => f.WorkOrderId == wo.Id);
                if (failure != null)
                {
                    failure.RepairEndDate = DateTimeOffset.UtcNow;
                    failure.RepairStartDate ??= wo.StartedDate;
                    failure.DowntimeHours = failure.RepairStartDate.HasValue
                        ? (decimal)(DateTimeOffset.UtcNow - failure.RepairStartDate.Value).TotalHours
                        : null;
                    failure.CorrectiveAction = model.Resolution;
                    _uow.FailureLogs.Update(failure);
                }
            }

            await _uow.SaveChangesAsync();

            // SLA check
            if (wo.SLATargetHours.HasValue && wo.StartedDate.HasValue)
            {
                var actualHours = (decimal)(wo.CompletedDate!.Value - wo.StartedDate.Value).TotalHours;
                if (actualHours > wo.SLATargetHours.Value)
                    await _workflowHook.OnWorkOrderSLABreachedAsync(wo.Id, wo.WorkOrderNumber, actualHours, wo.SLATargetHours.Value);
            }

            _logger.LogInformation("Work order {Number} completed", wo.WorkOrderNumber);
            return await GetWorkOrderByIdAsync(model.WorkOrderId);
        }

        public async Task<WorkOrderDetailVm> AddLaborAsync(LaborEntryCreateVm model)
        {
            var wo = await _uow.WorkOrders.GetByIdAsync(model.WorkOrderId);
            if (wo == null) throw new KeyNotFoundException("Work order not found.");

            var labor = new WorkOrderLabor
            {
                WorkOrderId = model.WorkOrderId,
                TechnicianUserId = _tenantService.GetCurrentUserId(),
                TechnicianName = model.TechnicianName,
                StartTime = DateTimeOffset.UtcNow.AddHours(-(double)model.HoursWorked),
                EndTime = DateTimeOffset.UtcNow,
                HoursWorked = model.HoursWorked,
                HourlyRate = model.HourlyRate,
                WorkPerformed = model.WorkPerformed
            };

            await _uow.WorkOrderLabors.AddAsync(labor);

            wo.LaborCost += labor.HoursWorked * labor.HourlyRate;
            _uow.WorkOrders.Update(wo);
            await _uow.SaveChangesAsync();

            return await GetWorkOrderByIdAsync(model.WorkOrderId);
        }

        public async Task<WorkOrderDetailVm> AddPartAsync(PartEntryCreateVm model)
        {
            var wo = await _uow.WorkOrders.GetByIdAsync(model.WorkOrderId);
            if (wo == null) throw new KeyNotFoundException("Work order not found.");

            var part = new WorkOrderPart
            {
                WorkOrderId = model.WorkOrderId,
                PartNumber = model.PartNumber,
                PartName = model.PartName,
                Quantity = model.Quantity,
                UnitCost = model.UnitCost
            };

            await _uow.WorkOrderParts.AddAsync(part);

            wo.PartsCost += part.Quantity * part.UnitCost;
            _uow.WorkOrders.Update(wo);
            await _uow.SaveChangesAsync();

            return await GetWorkOrderByIdAsync(model.WorkOrderId);
        }

        public async Task RemoveLaborAsync(int laborId)
        {
            var labor = await _uow.WorkOrderLabors.GetByIdAsync(laborId);
            if (labor == null) throw new KeyNotFoundException("Labor entry not found.");
            _uow.WorkOrderLabors.SoftDelete(labor);
            await _uow.SaveChangesAsync();
        }

        public async Task RemovePartAsync(int partId)
        {
            var part = await _uow.WorkOrderParts.GetByIdAsync(partId);
            if (part == null) throw new KeyNotFoundException("Part entry not found.");
            _uow.WorkOrderParts.SoftDelete(part);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  PM SCHEDULES
        // ═══════════════════════════════════════

        public async Task<PMScheduleDetailVm> GetPMScheduleByIdAsync(int id)
        {
            var pm = await _uow.PMSchedules.Query()
                .Include(p => p.Asset)
                .Include(p => p.GeneratedWorkOrders)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pm == null) throw new KeyNotFoundException("PM schedule not found.");
            return MapPMDetail(pm);
        }

        public async Task<IReadOnlyList<PMScheduleListItemVm>> GetPMSchedulesAsync()
        {
            var pms = await _uow.PMSchedules.Query()
                .Include(p => p.Asset)
                .OrderBy(p => p.NextDueDate)
                .ToListAsync();

            return pms.Select(p => new PMScheduleListItemVm
            {
                Id = p.Id,
                Name = p.Name,
                AssetCode = p.Asset.AssetCode,
                AssetName = p.Asset.Name,
                TriggerDisplay = p.TriggerType.ToString(),
                FrequencyDisplay = p.Frequency.ToString(),
                NextDueDate = p.NextDueDate,
                IsEnabled = p.IsEnabled
            }).ToList();
        }

        public async Task<PMScheduleDetailVm> CreatePMScheduleAsync(PMScheduleCreateVm model)
        {
            var asset = await _uow.Assets.GetByIdAsync(model.AssetId);
            if (asset == null) throw new KeyNotFoundException("Asset not found.");

            var pm = new PMSchedule
            {
                Name = model.Name,
                Description = model.Description,
                AssetId = model.AssetId,
                TriggerType = model.TriggerType,
                Frequency = model.Frequency,
                NextDueDate = DateTimeOffset.UtcNow.AddDays((int)model.Frequency),
                MeterType = model.MeterType,
                MeterIntervalValue = model.MeterIntervalValue,
                TriggerConditionThreshold = model.TriggerConditionThreshold,
                DefaultPriority = model.DefaultPriority,
                EstimatedLaborHours = model.EstimatedLaborHours,
                SLATargetHours = model.SLATargetHours,
                TaskDescription = model.TaskDescription,
                IsEnabled = true
            };

            if (model.TriggerType == PMTriggerType.MeterBased && model.MeterIntervalValue.HasValue)
            {
                pm.LastMeterReading = 0;
                pm.NextMeterThreshold = model.MeterIntervalValue;
            }

            await _uow.PMSchedules.AddAsync(pm);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("PM schedule created: {Name} for asset {Asset}", pm.Name, asset.AssetCode);
            return await GetPMScheduleByIdAsync(pm.Id);
        }

        public async Task<PMScheduleDetailVm> TogglePMScheduleAsync(int id)
        {
            var pm = await _uow.PMSchedules.GetByIdAsync(id);
            if (pm == null) throw new KeyNotFoundException("PM schedule not found.");
            pm.IsEnabled = !pm.IsEnabled;
            _uow.PMSchedules.Update(pm);
            await _uow.SaveChangesAsync();
            return await GetPMScheduleByIdAsync(id);
        }

        public async Task<PMGenerationResultVm> GeneratePMWorkOrdersAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var dueSchedules = await _uow.PMSchedules.Query()
                .Include(p => p.Asset)
                .Where(p => p.IsEnabled
                            && p.TriggerType == PMTriggerType.Calendar
                            && p.NextDueDate.HasValue
                            && p.NextDueDate <= now)
                .ToListAsync();

            var details = new List<PMGenerationItemVm>();

            foreach (var pm in dueSchedules)
            {
                try
                {
                    var number = await WorkOrderNumberGenerator.GenerateAsync(_uow);

                    var wo = new WorkOrder
                    {
                        WorkOrderNumber = number,
                        Title = $"PM: {pm.Name}",
                        Description = pm.TaskDescription,
                        Type = WorkOrderType.Preventive,
                        Priority = pm.DefaultPriority,
                        Status = WorkOrderStatus.Open,
                        AssetId = pm.AssetId,
                        PMScheduleId = pm.Id,
                        AssignedToUserId = pm.DefaultAssigneeUserId,
                        DueDate = now.AddDays((int)pm.Frequency),
                        SLATargetHours = pm.SLATargetHours
                    };

                    await _uow.WorkOrders.AddAsync(wo);

                    // Advance schedule
                    pm.LastExecutedDate = now;
                    pm.NextDueDate = now.AddDays((int)pm.Frequency);
                    _uow.PMSchedules.Update(pm);

                    await _uow.SaveChangesAsync();

                    details.Add(new PMGenerationItemVm
                    {
                        ScheduleName = pm.Name,
                        AssetCode = pm.Asset.AssetCode,
                        Status = "Generated",
                        WorkOrderNumber = number
                    });

                    _logger.LogInformation("PM work order {Number} generated for schedule {PM}", number, pm.Name);
                }
                catch (Exception ex)
                {
                    details.Add(new PMGenerationItemVm
                    {
                        ScheduleName = pm.Name,
                        AssetCode = pm.Asset.AssetCode,
                        Status = "Failed",
                        Message = ex.Message
                    });
                }
            }

            return new PMGenerationResultVm
            {
                WorkOrdersGenerated = details.Count(d => d.Status == "Generated"),
                SchedulesProcessed = dueSchedules.Count,
                SchedulesSkipped = details.Count(d => d.Status == "Failed"),
                Details = details
            };
        }

        // ═══════════════════════════════════════
        //  RELIABILITY (MTBF/MTTR)
        // ═══════════════════════════════════════

        public async Task<MTBFMTTRResultVm> CalculateMTBFMTTRAsync(int assetId)
        {
            var engine = new ReliabilityEngine(_uow);
            return await engine.CalculateAsync(assetId);
        }

        public async Task<IReadOnlyList<MTBFMTTRResultVm>> GetReliabilityOverviewAsync()
        {
            var engine = new ReliabilityEngine(_uow);
            return await engine.CalculateAllAsync();
        }

        // ═══════════════════════════════════════
        //  STATUS VALIDATION
        // ═══════════════════════════════════════

        private static void ValidateStatusTransition(WorkOrderStatus current, WorkOrderStatus target)
        {
            var allowed = current switch
            {
                WorkOrderStatus.Draft => new[] { WorkOrderStatus.Open, WorkOrderStatus.Cancelled },
                WorkOrderStatus.Open => new[] { WorkOrderStatus.Assigned, WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled },
                WorkOrderStatus.Assigned => new[] { WorkOrderStatus.InProgress, WorkOrderStatus.OnHold, WorkOrderStatus.Cancelled },
                WorkOrderStatus.InProgress => new[] { WorkOrderStatus.Completed, WorkOrderStatus.OnHold, WorkOrderStatus.Cancelled },
                WorkOrderStatus.OnHold => new[] { WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled },
                WorkOrderStatus.Completed => new[] { WorkOrderStatus.Closed },
                _ => Array.Empty<WorkOrderStatus>()
            };

            if (!allowed.Contains(target))
                throw new InvalidOperationException($"Cannot transition from {current} to {target}.");
        }

        // ═══════════════════════════════════════
        //  MAPPING
        // ═══════════════════════════════════════

        private static WorkOrderDetailVm MapToDetailVm(WorkOrder wo)
        {
            decimal? actualHours = null;
            if (wo.StartedDate.HasValue)
            {
                var end = wo.CompletedDate ?? DateTimeOffset.UtcNow;
                actualHours = Math.Round((decimal)(end - wo.StartedDate.Value).TotalHours, 1);
            }

            return new WorkOrderDetailVm
            {
                Id = wo.Id,
                WorkOrderNumber = wo.WorkOrderNumber,
                Title = wo.Title,
                Description = wo.Description,
                Type = wo.Type,
                Priority = wo.Priority,
                Status = wo.Status,
                AssetId = wo.AssetId,
                AssetCode = wo.Asset.AssetCode,
                AssetName = wo.Asset.Name,
                PMScheduleName = wo.PMSchedule?.Name,
                FailureType = wo.FailureType,
                FailureCause = wo.FailureCause,
                DueDate = wo.DueDate,
                StartedDate = wo.StartedDate,
                CompletedDate = wo.CompletedDate,
                SLATargetHours = wo.SLATargetHours,
                ActualHours = actualHours,
                LaborCost = wo.LaborCost,
                PartsCost = wo.PartsCost,
                ExternalCost = wo.ExternalCost,
                Resolution = wo.Resolution,
                Notes = wo.Notes,
                CreatedDate = wo.CreatedDate,
                LaborEntries = wo.LaborEntries.Select(l => new LaborEntryVm
                {
                    Id = l.Id,
                    TechnicianName = l.TechnicianName,
                    StartTime = l.StartTime,
                    EndTime = l.EndTime,
                    HoursWorked = l.HoursWorked,
                    HourlyRate = l.HourlyRate,
                    Cost = l.HoursWorked * l.HourlyRate,
                    WorkPerformed = l.WorkPerformed
                }).ToList(),
                Parts = wo.Parts.Select(p => new PartEntryVm
                {
                    Id = p.Id,
                    PartNumber = p.PartNumber,
                    PartName = p.PartName,
                    Quantity = p.Quantity,
                    UnitCost = p.UnitCost,
                    TotalCost = p.Quantity * p.UnitCost
                }).ToList()
            };
        }

        private static PMScheduleDetailVm MapPMDetail(PMSchedule pm) => new()
        {
            Id = pm.Id,
            Name = pm.Name,
            Description = pm.Description,
            AssetId = pm.AssetId,
            AssetCode = pm.Asset.AssetCode,
            AssetName = pm.Asset.Name,
            TriggerType = pm.TriggerType,
            Frequency = pm.Frequency,
            LastExecutedDate = pm.LastExecutedDate,
            NextDueDate = pm.NextDueDate,
            MeterType = pm.MeterType,
            MeterIntervalValue = pm.MeterIntervalValue,
            LastMeterReading = pm.LastMeterReading,
            NextMeterThreshold = pm.NextMeterThreshold,
            TriggerConditionThreshold = pm.TriggerConditionThreshold,
            DefaultPriority = pm.DefaultPriority,
            EstimatedLaborHours = pm.EstimatedLaborHours,
            SLATargetHours = pm.SLATargetHours,
            IsEnabled = pm.IsEnabled,
            TaskDescription = pm.TaskDescription,
            GeneratedWorkOrderCount = pm.GeneratedWorkOrders?.Count ?? 0
        };

        // ═══════════════════════════════════════
        //  METER READINGS
        // ═══════════════════════════════════════

        public async Task<MeterReadingResultVm> RecordMeterReadingAsync(MeterReadingVm model)
        {
            var pm = await _uow.PMSchedules.Query()
                .Include(p => p.Asset)
                .FirstOrDefaultAsync(p => p.Id == model.PMScheduleId);

            if (pm == null) throw new KeyNotFoundException("PM schedule not found.");
            if (pm.TriggerType != PMTriggerType.MeterBased)
                throw new InvalidOperationException("This schedule is not meter-based.");

            var previousReading = pm.LastMeterReading ?? 0;
            pm.LastMeterReading = model.CurrentReading;

            var result = new MeterReadingResultVm
            {
                PMScheduleId = pm.Id,
                ScheduleName = pm.Name,
                AssetCode = pm.Asset.AssetCode,
                MeterType = pm.MeterType ?? "Units",
                PreviousReading = previousReading,
                CurrentReading = model.CurrentReading,
                NextThreshold = pm.NextMeterThreshold ?? 0,
                ThresholdReached = false
            };

            // Check if threshold reached
            if (pm.NextMeterThreshold.HasValue && model.CurrentReading >= pm.NextMeterThreshold.Value)
            {
                result.ThresholdReached = true;

                // Generate work order
                var number = await WorkOrderNumberGenerator.GenerateAsync(_uow);
                var wo = new WorkOrder
                {
                    WorkOrderNumber = number,
                    Title = $"PM (Meter): {pm.Name} — {pm.MeterType} reading {model.CurrentReading}",
                    Description = pm.TaskDescription,
                    Type = WorkOrderType.Preventive,
                    Priority = pm.DefaultPriority,
                    Status = WorkOrderStatus.Open,
                    AssetId = pm.AssetId,
                    PMScheduleId = pm.Id,
                    AssignedToUserId = pm.DefaultAssigneeUserId,
                    DueDate = DateTimeOffset.UtcNow.AddDays(7),
                    SLATargetHours = pm.SLATargetHours
                };

                await _uow.WorkOrders.AddAsync(wo);

                // Advance threshold
                pm.LastExecutedDate = DateTimeOffset.UtcNow;
                pm.NextMeterThreshold = model.CurrentReading + (pm.MeterIntervalValue ?? 0);

                result.GeneratedWorkOrderNumber = number;

                _logger.LogInformation("Meter threshold reached for {PM}: reading {Reading}, WO {Number} generated",
                    pm.Name, model.CurrentReading, number);
            }

            _uow.PMSchedules.Update(pm);
            await _uow.SaveChangesAsync();

            return result;
        }

        public async Task<PMGenerationResultVm> GenerateConditionBasedPMsAsync()
        {
            var conditionPMs = await _uow.PMSchedules.Query()
                .Include(p => p.Asset)
                .Where(p => p.IsEnabled && p.TriggerType == PMTriggerType.ConditionBased
                            && p.TriggerConditionThreshold.HasValue)
                .ToListAsync();

            var details = new List<PMGenerationItemVm>();

            foreach (var pm in conditionPMs)
            {
                // Get latest inspection score for this asset
                var latestInspection = await _uow.Inspections.Query()
                    .Where(i => i.AssetId == pm.AssetId)
                    .OrderByDescending(i => i.InspectionDate)
                    .FirstOrDefaultAsync();

                if (latestInspection == null)
                {
                    details.Add(new PMGenerationItemVm
                    {
                        ScheduleName = pm.Name,
                        AssetCode = pm.Asset.AssetCode,
                        Status = "Skipped",
                        Message = "No inspection data"
                    });
                    continue;
                }

                if (latestInspection.ConditionScore <= pm.TriggerConditionThreshold!.Value)
                {
                    // Check if a PM WO already exists and is open for this schedule
                    var existingOpen = await _uow.WorkOrders.ExistsAsync(
                        w => w.PMScheduleId == pm.Id
                             && w.Status != WorkOrderStatus.Completed
                             && w.Status != WorkOrderStatus.Closed
                             && w.Status != WorkOrderStatus.Cancelled);

                    if (existingOpen)
                    {
                        details.Add(new PMGenerationItemVm
                        {
                            ScheduleName = pm.Name,
                            AssetCode = pm.Asset.AssetCode,
                            Status = "Skipped",
                            Message = "Open WO already exists"
                        });
                        continue;
                    }

                    var number = await WorkOrderNumberGenerator.GenerateAsync(_uow);
                    var wo = new WorkOrder
                    {
                        WorkOrderNumber = number,
                        Title = $"PM (Condition): {pm.Name} — Score {latestInspection.ConditionScore}",
                        Description = pm.TaskDescription,
                        Type = WorkOrderType.Predictive,
                        Priority = pm.DefaultPriority,
                        Status = WorkOrderStatus.Open,
                        AssetId = pm.AssetId,
                        PMScheduleId = pm.Id,
                        AssignedToUserId = pm.DefaultAssigneeUserId,
                        DueDate = DateTimeOffset.UtcNow.AddDays(3),
                        SLATargetHours = pm.SLATargetHours
                    };

                    await _uow.WorkOrders.AddAsync(wo);
                    pm.LastExecutedDate = DateTimeOffset.UtcNow;
                    _uow.PMSchedules.Update(pm);
                    await _uow.SaveChangesAsync();

                    details.Add(new PMGenerationItemVm
                    {
                        ScheduleName = pm.Name,
                        AssetCode = pm.Asset.AssetCode,
                        Status = "Generated",
                        WorkOrderNumber = number
                    });
                }
                else
                {
                    details.Add(new PMGenerationItemVm
                    {
                        ScheduleName = pm.Name,
                        AssetCode = pm.Asset.AssetCode,
                        Status = "Skipped",
                        Message = $"Condition {latestInspection.ConditionScore} above threshold"
                    });
                }
            }

            return new PMGenerationResultVm
            {
                WorkOrdersGenerated = details.Count(d => d.Status == "Generated"),
                SchedulesProcessed = conditionPMs.Count,
                SchedulesSkipped = details.Count(d => d.Status != "Generated"),
                Details = details
            };
        }

        // ═══════════════════════════════════════
        //  FAILURE ANALYSIS
        // ═══════════════════════════════════════

        public async Task<FailureAnalysisVm> GetFailureAnalysisAsync(DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            var periodStart = from ?? DateTimeOffset.UtcNow.AddYears(-1);
            var periodEnd = to ?? DateTimeOffset.UtcNow;

            var failures = await _uow.FailureLogs.Query()
                .Include(f => f.Asset)
                .Where(f => f.FailureDate >= periodStart && f.FailureDate <= periodEnd)
                .ToListAsync();

            var totalDowntime = failures.Sum(f => f.DowntimeHours ?? 0);
            var avgRepair = failures.Count > 0 ? Math.Round(totalDowntime / failures.Count, 1) : 0;

            // By type
            var byType = failures.GroupBy(f => f.FailureType)
                .Select(g => new FailureByTypeVm
                {
                    Type = g.Key.ToString(),
                    Count = g.Count(),
                    TotalDowntime = g.Sum(f => f.DowntimeHours ?? 0),
                    Percentage = failures.Count > 0 ? Math.Round(g.Count() * 100m / failures.Count, 1) : 0
                }).OrderByDescending(t => t.Count).ToList();

            // By asset (top 10 worst)
            var byAsset = failures.GroupBy(f => f.AssetId)
                .Select(g =>
                {
                    var first = g.First();
                    var downtime = g.Sum(f => f.DowntimeHours ?? 0);
                    return new FailureByAssetVm
                    {
                        AssetId = g.Key,
                        AssetCode = first.Asset.AssetCode,
                        AssetName = first.Asset.Name,
                        FailureCount = g.Count(),
                        TotalDowntime = downtime,
                        AvgRepairTime = g.Count() > 0 ? Math.Round(downtime / g.Count(), 1) : 0
                    };
                }).OrderByDescending(a => a.FailureCount).Take(10).ToList();

            // Monthly trend
            var monthlyTrend = failures
                .GroupBy(f => $"{f.FailureDate.Year}-{f.FailureDate.Month:D2}")
                .OrderBy(g => g.Key)
                .Select(g => new FailureTrendVm
                {
                    Period = g.Key,
                    Count = g.Count(),
                    DowntimeHours = g.Sum(f => f.DowntimeHours ?? 0)
                }).ToList();

            // Top repeat failures (same asset + same failure type)
            var topRepeats = failures
                .GroupBy(f => new { f.AssetId, f.FailureType })
                .Where(g => g.Count() > 1)
                .Select(g =>
                {
                    var first = g.First();
                    return new TopFailureVm
                    {
                        AssetId = g.Key.AssetId,
                        AssetCode = first.Asset.AssetCode,
                        FailureType = g.Key.FailureType,
                        OccurrenceCount = g.Count(),
                        CommonCause = g.Select(f => f.RootCause).FirstOrDefault(c => c != null)
                    };
                }).OrderByDescending(r => r.OccurrenceCount).Take(10).ToList();

            return new FailureAnalysisVm
            {
                TotalFailures = failures.Count,
                TotalDowntimeHours = totalDowntime,
                AvgRepairTimeHours = avgRepair,
                ByType = byType,
                ByAsset = byAsset,
                MonthlyTrend = monthlyTrend,
                TopRepeatFailures = topRepeats
            };
        }

        // ═══════════════════════════════════════
        //  SLA & BACKLOG
        // ═══════════════════════════════════════

        public async Task<SLAReportVm> GetSLAReportAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var completedWithSLA = await _uow.WorkOrders.Query()
                .Include(w => w.Asset)
                .Where(w => w.SLATargetHours.HasValue
                            && (w.Status == WorkOrderStatus.Completed || w.Status == WorkOrderStatus.Closed)
                            && w.StartedDate.HasValue && w.CompletedDate.HasValue)
                .ToListAsync();

            var slaItems = completedWithSLA.Select(w =>
            {
                var actual = (decimal)(w.CompletedDate!.Value - w.StartedDate!.Value).TotalHours;
                return new { WO = w, ActualHours = actual, Breached = actual > w.SLATargetHours!.Value };
            }).ToList();

            var metCount = slaItems.Count(s => !s.Breached);
            var breachedCount = slaItems.Count(s => s.Breached);
            var totalSLA = slaItems.Count;
            var compliance = totalSLA > 0 ? Math.Round(metCount * 100m / totalSLA, 1) : 100;

            var breachedOrders = slaItems.Where(s => s.Breached)
                .OrderByDescending(s => s.ActualHours - s.WO.SLATargetHours!.Value)
                .Select(s => new SLAWorkOrderVm
                {
                    Id = s.WO.Id,
                    WorkOrderNumber = s.WO.WorkOrderNumber,
                    Title = s.WO.Title,
                    AssetCode = s.WO.Asset.AssetCode,
                    Priority = s.WO.Priority,
                    SLATargetHours = s.WO.SLATargetHours!.Value,
                    ActualHours = Math.Round(s.ActualHours, 1),
                    OverageHours = Math.Round(s.ActualHours - s.WO.SLATargetHours!.Value, 1)
                }).ToList();

            // By priority
            var byPriority = slaItems.GroupBy(s => s.WO.Priority)
                .Select(g =>
                {
                    var met = g.Count(s => !s.Breached);
                    var total = g.Count();
                    return new SLAByPriorityVm
                    {
                        Priority = g.Key.ToString(),
                        Total = total,
                        Met = met,
                        Breached = total - met,
                        CompliancePercent = total > 0 ? Math.Round(met * 100m / total, 1) : 100,
                        AvgResolutionHours = g.Any() ? Math.Round(g.Average(s => s.ActualHours), 1) : 0
                    };
                }).ToList();

            // Avg response (time from created to started)
            var allStarted = await _uow.WorkOrders.Query()
                .Where(w => w.StartedDate.HasValue)
                .ToListAsync();
            var avgResponse = allStarted.Any()
                ? Math.Round((decimal)allStarted.Average(w => (w.StartedDate!.Value - w.CreatedDate).TotalHours), 1) : 0;
            var avgResolution = slaItems.Any() ? Math.Round(slaItems.Average(s => s.ActualHours), 1) : 0;

            // Backlog (open WOs sorted by age)
            var backlog = await _uow.WorkOrders.Query()
                .Include(w => w.Asset)
                .Where(w => w.Status != WorkOrderStatus.Completed
                            && w.Status != WorkOrderStatus.Closed
                            && w.Status != WorkOrderStatus.Cancelled)
                .OrderByDescending(w => w.CreatedDate)
                .Take(20)
                .Select(w => new BacklogItemVm
                {
                    Id = w.Id,
                    WorkOrderNumber = w.WorkOrderNumber,
                    Title = w.Title,
                    AssetCode = w.Asset.AssetCode,
                    Priority = w.Priority,
                    CreatedDate = w.CreatedDate,
                    AgeDays = (int)(now - w.CreatedDate).TotalDays
                }).ToListAsync();

            return new SLAReportVm
            {
                TotalWithSLA = totalSLA,
                SLAMetCount = metCount,
                SLABreachedCount = breachedCount,
                SLACompliancePercent = compliance,
                AvgResponseHours = avgResponse,
                AvgResolutionHours = avgResolution,
                BreachedOrders = breachedOrders,
                ByPriority = byPriority,
                Backlog = backlog.OrderByDescending(b => b.AgeDays).ToList()
            };
        }

        public async Task<IReadOnlyList<MaintenanceCostByAssetVm>> GetMaintenanceCostByAssetAsync()
        {
            var orders = await _uow.WorkOrders.Query()
                .Include(w => w.Asset)
                .Where(w => w.Status == WorkOrderStatus.Completed || w.Status == WorkOrderStatus.Closed)
                .ToListAsync();

            return orders.GroupBy(w => w.AssetId)
                .Select(g =>
                {
                    var first = g.First();
                    return new MaintenanceCostByAssetVm
                    {
                        AssetId = g.Key,
                        AssetCode = first.Asset.AssetCode,
                        AssetName = first.Asset.Name,
                        WorkOrderCount = g.Count(),
                        LaborCost = g.Sum(w => w.LaborCost),
                        PartsCost = g.Sum(w => w.PartsCost),
                        ExternalCost = g.Sum(w => w.ExternalCost),
                        TotalCost = g.Sum(w => w.LaborCost + w.PartsCost + w.ExternalCost)
                    };
                }).OrderByDescending(c => c.TotalCost).ToList();
        }

        public async Task<MaintenanceDashboardVm> GetDashboardAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

            var allWOs = await _uow.WorkOrders.Query()
                .Include(w => w.Asset).ToListAsync();
            var allPMs = await _uow.PMSchedules.Query()
                .Include(p => p.Asset).ToListAsync();

            var activeStatuses = new[] { WorkOrderStatus.Open, WorkOrderStatus.Assigned, WorkOrderStatus.InProgress, WorkOrderStatus.OnHold };

            var completedThisMonth = allWOs.Count(w =>
                w.Status == WorkOrderStatus.Completed && w.CompletedDate >= monthStart);

            var costThisMonth = allWOs
                .Where(w => w.CompletedDate >= monthStart && (w.Status == WorkOrderStatus.Completed || w.Status == WorkOrderStatus.Closed))
                .Sum(w => w.LaborCost + w.PartsCost + w.ExternalCost);

            // Overdue WOs
            var overdueWOs = allWOs
                .Where(w => w.DueDate.HasValue && w.DueDate < now && activeStatuses.Contains(w.Status))
                .Select(w => new OverdueWOAlertVm
                {
                    Id = w.Id,
                    WorkOrderNumber = w.WorkOrderNumber,
                    Title = w.Title,
                    AssetCode = w.Asset.AssetCode,
                    Priority = w.Priority,
                    DaysOverdue = (int)(now - w.DueDate!.Value).TotalDays
                }).OrderByDescending(o => o.DaysOverdue).ToList();

            // PM due/overdue
            var enabledPMs = allPMs.Where(p => p.IsEnabled && p.NextDueDate.HasValue).ToList();
            var pmsDue = enabledPMs.Where(p => p.NextDueDate <= now.AddDays(7)).ToList();
            var pmsOverdue = enabledPMs.Where(p => p.NextDueDate < now).ToList();

            var pmAlerts = enabledPMs
                .Where(p => p.NextDueDate <= now.AddDays(14))
                .Select(p => new PMDueAlertVm
                {
                    ScheduleId = p.Id,
                    Name = p.Name,
                    AssetCode = p.Asset.AssetCode,
                    DueDate = p.NextDueDate!.Value,
                    IsOverdue = p.NextDueDate < now,
                    DaysOverdueOrRemaining = p.NextDueDate < now
                        ? (int)(now - p.NextDueDate.Value).TotalDays
                        : (int)(p.NextDueDate.Value - now).TotalDays
                }).OrderBy(a => a.DueDate).ToList();

            // PM compliance = (total enabled - overdue) / total enabled
            var pmCompliance = enabledPMs.Count > 0
                ? Math.Round((enabledPMs.Count - pmsOverdue.Count) * 100m / enabledPMs.Count, 1)
                : 100;

            // Critical/emergency WOs still open
            var criticalAlerts = allWOs
                .Where(w => (w.Priority == WorkOrderPriority.Critical || w.Type == WorkOrderType.Emergency)
                            && activeStatuses.Contains(w.Status))
                .Select(w => new CriticalWOAlertVm
                {
                    Id = w.Id,
                    WorkOrderNumber = w.WorkOrderNumber,
                    Title = w.Title,
                    AssetCode = w.Asset.AssetCode,
                    AgeDays = (int)(now - w.CreatedDate).TotalDays
                }).OrderByDescending(c => c.AgeDays).ToList();

            // By status
            var byStatus = allWOs.GroupBy(w => w.Status)
                .Select(g => new WOStatusBreakdownVm
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = allWOs.Count > 0 ? Math.Round(g.Count() * 100m / allWOs.Count, 1) : 0
                }).OrderByDescending(s => s.Count).ToList();

            // By type
            var byType = allWOs.GroupBy(w => w.Type)
                .Select(g => new WOTypeBreakdownVm
                {
                    Type = g.Key.ToString(),
                    Count = g.Count(),
                    TotalCost = g.Sum(w => w.LaborCost + w.PartsCost + w.ExternalCost)
                }).OrderByDescending(t => t.Count).ToList();

            // Top cost assets (top 5)
            var topCost = allWOs
                .Where(w => w.Status == WorkOrderStatus.Completed || w.Status == WorkOrderStatus.Closed)
                .GroupBy(w => w.AssetId)
                .Select(g =>
                {
                    var first = g.First();
                    return new TopCostAssetVm
                    {
                        AssetCode = first.Asset.AssetCode,
                        AssetName = first.Asset.Name,
                        WOCount = g.Count(),
                        TotalCost = g.Sum(w => w.LaborCost + w.PartsCost + w.ExternalCost)
                    };
                }).OrderByDescending(c => c.TotalCost).Take(5).ToList();

            // SLA
            var slaWOs = allWOs.Where(w => w.SLATargetHours.HasValue
                && w.StartedDate.HasValue && w.CompletedDate.HasValue).ToList();
            var slaMet = slaWOs.Count(w => (decimal)(w.CompletedDate!.Value - w.StartedDate!.Value).TotalHours <= w.SLATargetHours!.Value);
            var slaCompliance = slaWOs.Count > 0 ? Math.Round(slaMet * 100m / slaWOs.Count, 1) : 100;

            // MTBF/MTTR averages
            var reliabilityEngine = new ReliabilityEngine(_uow);
            var reliabilityData = await reliabilityEngine.CalculateAllAsync();
            var avgMTBF = reliabilityData.Any() ? Math.Round(reliabilityData.Average(r => r.MTBF), 0) : 0;
            var avgMTTR = reliabilityData.Any() ? Math.Round(reliabilityData.Average(r => r.MTTR), 1) : 0;

            // Recent activity
            var recentCompleted = allWOs.Where(w => w.CompletedDate.HasValue)
                .OrderByDescending(w => w.CompletedDate).Take(5)
                .Select(w => new RecentWOActivityVm
                {
                    Icon = "bi-check-circle-fill",
                    Description = $"{w.WorkOrderNumber} completed — {w.Asset.AssetCode}",
                    Date = w.CompletedDate!.Value,
                    WorkOrderId = w.Id
                });
            var recentCreated = allWOs.OrderByDescending(w => w.CreatedDate).Take(5)
                .Select(w => new RecentWOActivityVm
                {
                    Icon = "bi-plus-circle",
                    Description = $"{w.WorkOrderNumber} created — {w.Title}",
                    Date = w.CreatedDate,
                    WorkOrderId = w.Id
                });
            var recentActivity = recentCompleted.Concat(recentCreated)
                .OrderByDescending(a => a.Date).Take(10).ToList();

            return new MaintenanceDashboardVm
            {
                OpenWorkOrders = allWOs.Count(w => w.Status == WorkOrderStatus.Open),
                InProgressWorkOrders = allWOs.Count(w => w.Status == WorkOrderStatus.InProgress),
                CompletedThisMonth = completedThisMonth,
                OverdueWorkOrders = overdueWOs.Count,
                OnHoldWorkOrders = allWOs.Count(w => w.Status == WorkOrderStatus.OnHold),
                TotalMaintenanceCostThisMonth = costThisMonth,
                TotalPMSchedules = enabledPMs.Count,
                PMsDueCount = pmsDue.Count,
                PMsOverdueCount = pmsOverdue.Count,
                PMCompliancePercent = pmCompliance,
                AvgMTBF = avgMTBF,
                AvgMTTR = avgMTTR,
                SLACompliancePercent = slaCompliance,
                SLABreachedCount = slaWOs.Count - slaMet,
                OverdueAlerts = overdueWOs,
                PMDueAlerts = pmAlerts,
                CriticalAlerts = criticalAlerts,
                ByStatus = byStatus,
                ByType = byType,
                TopCostAssets = topCost,
                RecentActivity = recentActivity
            };
        }
        public async Task<byte[]> ExportWorkOrderReportAsync()
        {
            var export = new MaintenanceExportService(_uow);
            return await export.ExportWorkOrderReportAsync();
        }

        public async Task<byte[]> ExportPMComplianceReportAsync()
        {
            var export = new MaintenanceExportService(_uow);
            return await export.ExportPMComplianceReportAsync();
        }

        public async Task<byte[]> ExportMaintenanceCostReportAsync()
        {
            var export = new MaintenanceExportService(_uow);
            return await export.ExportMaintenanceCostReportAsync();
        }
    }
}
