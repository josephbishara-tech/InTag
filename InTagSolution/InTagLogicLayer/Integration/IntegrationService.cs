using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Integration;
using ClosedXML.Excel;

namespace InTagLogicLayer.Integration
{
    public class IntegrationService : IIntegrationService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<IntegrationService> _logger;

        public IntegrationService(IUnitOfWork uow, ILogger<IntegrationService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ExecutiveDashboardVm> GetExecutiveDashboardAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);

            // ── Assets ──
            var assets = await _uow.Assets.GetAllAsync();
            var totalAssetValue = assets.Sum(a => a.CurrentBookValue);
            var latestInspections = await _uow.Inspections.Query()
     .GroupBy(i => i.AssetId)
     .Select(g => new { AssetId = g.Key, LatestScore = g.OrderByDescending(i => i.InspectionDate).First().ConditionScore })
     .ToListAsync();
            var assetsAttention = latestInspections.Count(i => (int)i.LatestScore <= 2);

            // ── Documents ──
            var docs = await _uow.Documents.GetAllAsync();
            var docsPublished = docs.Count(d => d.Status == DocumentStatus.Published);
            var docsOverdue = docs.Count(d => d.NextReviewDate.HasValue && d.NextReviewDate < now);
            var docCompliance = docs.Count > 0
                ? Math.Round((docs.Count - docsOverdue) * 100m / docs.Count, 1) : 100;

            // ── Manufacturing ──
            var prodOrders = await _uow.ProductionOrders.GetAllAsync();
            var inProgress = prodOrders.Count(o => o.Status == ProductionOrderStatus.InProgress);
            var completedPO = prodOrders.Count(o => o.Status == ProductionOrderStatus.Completed);
            var qcChecks = await _uow.QualityChecks.GetAllAsync();
            var passRate = qcChecks.Count > 0
                ? Math.Round(qcChecks.Count(q => q.Result == QualityCheckResult.Pass) * 100m / qcChecks.Count, 1) : 100;

            // ── Maintenance ──
            var workOrders = await _uow.WorkOrders.GetAllAsync();
            var openWOs = workOrders.Count(w => w.Status == WorkOrderStatus.Open || w.Status == WorkOrderStatus.InProgress);
            var overdueWOs = workOrders.Count(w => w.DueDate.HasValue && w.DueDate < now
                && w.Status != WorkOrderStatus.Completed && w.Status != WorkOrderStatus.Closed && w.Status != WorkOrderStatus.Cancelled);
            var pmSchedules = await _uow.PMSchedules.Query().Where(p => p.IsEnabled).ToListAsync();
            var pmOverdue = pmSchedules.Count(p => p.NextDueDate.HasValue && p.NextDueDate < now);
            var pmCompliance = pmSchedules.Count > 0
                ? Math.Round((pmSchedules.Count - pmOverdue) * 100m / pmSchedules.Count, 1) : 100;
            var slaWOs = workOrders.Where(w => w.SLATargetHours.HasValue && w.StartedDate.HasValue && w.CompletedDate.HasValue).ToList();
            var slaMet = slaWOs.Count(w => (decimal)(w.CompletedDate!.Value - w.StartedDate!.Value).TotalHours <= w.SLATargetHours!.Value);
            var slaCompliance = slaWOs.Count > 0 ? Math.Round(slaMet * 100m / slaWOs.Count, 1) : 100;

            // ── Inventory ──
            var stock = await _uow.StockItems.Query().ToListAsync();
            var invValue = stock.Where(s => s.QuantityOnHand > 0).Sum(s => s.QuantityOnHand * s.UnitCost);
            var outOfStock = stock.Count(s => s.QuantityOnHand <= 0);
            var expiring = stock.Count(s => s.ExpiryDate.HasValue && s.ExpiryDate <= now.AddDays(30) && s.QuantityOnHand > 0);

            // ── Workflows ──
            var wfInstances = await _uow.WorkflowInstances.GetAllAsync();
            var pendingApprovals = wfInstances.Count(i => i.Status == WorkflowInstanceStatus.InProgress);
            var completedWF = wfInstances.Count(i => i.Status == WorkflowInstanceStatus.Completed);
            var wfActions = await _uow.WorkflowActions.GetAllAsync();
            var escalated = wfActions.Count(a => a.Result == StepActionResult.Escalated);

            // ── Critical Alerts ──
            var alerts = new List<CriticalAlertVm>();
            if (overdueWOs > 0) alerts.Add(new CriticalAlertVm
            {
                Module = "Maintenance",
                Icon = "bi-wrench-adjustable",
                Color = "text-danger",
                Title = $"{overdueWOs} Overdue Work Order(s)",
                Description = "Work orders past due date require attention.",
                ActionUrl = "/WorkOrder",
                Date = now
            });
            if (outOfStock > 0) alerts.Add(new CriticalAlertVm
            {
                Module = "Inventory",
                Icon = "bi-boxes",
                Color = "text-danger",
                Title = $"{outOfStock} Out-of-Stock Item(s)",
                Description = "Stock items at zero quantity.",
                ActionUrl = "/Inventory/Reorder",
                Date = now
            });
            if (docsOverdue > 0) alerts.Add(new CriticalAlertVm
            {
                Module = "Documents",
                Icon = "bi-file-earmark-text",
                Color = "text-warning",
                Title = $"{docsOverdue} Document(s) Overdue Review",
                Description = "Documents past scheduled review date.",
                ActionUrl = "/Document",
                Date = now
            });
            if (assetsAttention > 0) alerts.Add(new CriticalAlertVm
            {
                Module = "Assets",
                Icon = "bi-shield-exclamation",
                Color = "text-warning",
                Title = $"{assetsAttention} Asset(s) in Poor Condition",
                Description = "Assets with condition score ≤ 2.",
                ActionUrl = "/AssetDashboard",
                Date = now
            });
            if (pendingApprovals > 3) alerts.Add(new CriticalAlertVm
            {
                Module = "Workflow",
                Icon = "bi-check-square",
                Color = "text-info",
                Title = $"{pendingApprovals} Pending Approval(s)",
                Description = "Workflow items awaiting action.",
                ActionUrl = "/Workflow/PendingApprovals",
                Date = now
            });

            // ── Module Summaries ──
            var summaries = new List<ModuleSummaryVm>
            {
                new() { Module = "Assets", Icon = "bi-building-gear", Color = "primary",
                    PrimaryMetric = assets.Count.ToString(), PrimaryLabel = "Total Assets",
                    SecondaryMetric = totalAssetValue.ToString("C0"), SecondaryLabel = "Book Value",
                    Url = "/AssetDashboard" },
                new() { Module = "Documents", Icon = "bi-file-earmark-text", Color = "info",
                    PrimaryMetric = docs.Count.ToString(), PrimaryLabel = "Total Documents",
                    SecondaryMetric = $"{docCompliance}%", SecondaryLabel = "Compliance",
                    Url = "/DocumentDashboard" },
                new() { Module = "Manufacturing", Icon = "bi-gear", Color = "success",
                    PrimaryMetric = inProgress.ToString(), PrimaryLabel = "In Progress",
                    SecondaryMetric = $"{passRate}%", SecondaryLabel = "Quality",
                    Url = "/ManufacturingDashboard" },
                new() { Module = "Maintenance", Icon = "bi-wrench-adjustable", Color = "warning",
                    PrimaryMetric = openWOs.ToString(), PrimaryLabel = "Open WOs",
                    SecondaryMetric = $"{pmCompliance}%", SecondaryLabel = "PM Compliance",
                    Url = "/MaintenanceDashboard" },
                new() { Module = "Inventory", Icon = "bi-boxes", Color = "danger",
                    PrimaryMetric = invValue.ToString("C0"), PrimaryLabel = "Stock Value",
                    SecondaryMetric = outOfStock.ToString(), SecondaryLabel = "Out of Stock",
                    Url = "/InventoryDashboard" },
                new() { Module = "Workflows", Icon = "bi-arrow-repeat", Color = "secondary",
                    PrimaryMetric = pendingApprovals.ToString(), PrimaryLabel = "Pending",
                    SecondaryMetric = completedWF.ToString(), SecondaryLabel = "Completed",
                    Url = "/Workflow" }
            };

            // ── Monthly Cost Trend (last 6 months) ──
            var monthlyCost = new List<CostSummaryVm>();
            for (var m = 5; m >= 0; m--)
            {
                var mStart = monthStart.AddMonths(-m);
                var mEnd = mStart.AddMonths(1);
                var mntCost = workOrders
                    .Where(w => w.CompletedDate >= mStart && w.CompletedDate < mEnd)
                    .Sum(w => w.LaborCost + w.PartsCost + w.ExternalCost);
                monthlyCost.Add(new CostSummaryVm
                {
                    Period = mStart.ToString("MMM yy"),
                    MaintenanceCost = mntCost,
                    DepreciationCost = 0, // simplified — would query depreciation table
                    InventoryValue = invValue
                });
            }

            return new ExecutiveDashboardVm
            {
                TotalAssets = assets.Count,
                TotalAssetValue = totalAssetValue,
                AssetsRequiringAttention = assetsAttention,
                TotalDocuments = docs.Count,
                DocumentsPublished = docsPublished,
                DocumentsOverdueReview = docsOverdue,
                DocumentCompliancePercent = docCompliance,
                ProductionOrdersInProgress = inProgress,
                ProductionOrdersCompleted = completedPO,
                QualityPassRate = passRate,
                OpenWorkOrders = openWOs,
                OverdueWorkOrders = overdueWOs,
                PMCompliancePercent = pmCompliance,
                SLACompliancePercent = slaCompliance,
                InventoryValue = invValue,
                OutOfStockItems = outOfStock,
                ExpiringItems = expiring,
                PendingApprovals = pendingApprovals,
                CompletedWorkflows = completedWF,
                EscalatedWorkflows = escalated,
                ModuleSummaries = summaries,
                CriticalAlerts = alerts,
                MonthlyCostTrend = monthlyCost
            };
        }

        public async Task<ComplianceReportVm> GetComplianceReportAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var issues = new List<ComplianceIssueVm>();
            var modules = new List<ComplianceModuleVm>();

            // Document compliance
            var docs = await _uow.Documents.GetAllAsync();
            var docOverdue = docs.Count(d => d.NextReviewDate.HasValue && d.NextReviewDate < now);
            var docTotal = docs.Count;
            var docPassed = docTotal - docOverdue;
            modules.Add(new ComplianceModuleVm
            {
                Module = "Documents",
                TotalChecks = docTotal,
                PassedChecks = docPassed,
                FailedChecks = docOverdue,
                Score = docTotal > 0 ? Math.Round(docPassed * 100m / docTotal, 1) : 100
            });
            if (docOverdue > 0) issues.Add(new ComplianceIssueVm
            {
                Module = "Documents",
                Severity = "High",
                Description = $"{docOverdue} document(s) past review date",
                ActionUrl = "/Document"
            });

            // PM compliance
            var pms = await _uow.PMSchedules.Query().Where(p => p.IsEnabled).ToListAsync();
            var pmOverdue = pms.Count(p => p.NextDueDate.HasValue && p.NextDueDate < now);
            modules.Add(new ComplianceModuleVm
            {
                Module = "Maintenance",
                TotalChecks = pms.Count,
                PassedChecks = pms.Count - pmOverdue,
                FailedChecks = pmOverdue,
                Score = pms.Count > 0 ? Math.Round((pms.Count - pmOverdue) * 100m / pms.Count, 1) : 100
            });
            if (pmOverdue > 0) issues.Add(new ComplianceIssueVm
            {
                Module = "Maintenance",
                Severity = "High",
                Description = $"{pmOverdue} PM schedule(s) overdue",
                ActionUrl = "/PMSchedule"
            });

            // Inspection compliance
            var assets = await _uow.Assets.GetAllAsync();
            var latestInspections = await _uow.Inspections.Query()
      .GroupBy(i => i.AssetId)
      .Select(g => new { AssetId = g.Key, LatestScore = g.OrderByDescending(i => i.InspectionDate).First().ConditionScore })
      .ToListAsync();
            var poorCondition = latestInspections.Count(i => (int)i.LatestScore <= 2);
            var inspTotal = latestInspections.Count;
            modules.Add(new ComplianceModuleVm
            {
                Module = "Assets",
                TotalChecks = inspTotal,
                PassedChecks = inspTotal - poorCondition,
                FailedChecks = poorCondition,
                Score = inspTotal > 0 ? Math.Round((inspTotal - poorCondition) * 100m / inspTotal, 1) : 100
            });
            if (poorCondition > 0) issues.Add(new ComplianceIssueVm
            {
                Module = "Assets",
                Severity = "Medium",
                Description = $"{poorCondition} asset(s) in poor condition (score ≤ 2)",
                ActionUrl = "/AssetDashboard"
            });

            // Quality compliance
            var qcChecks = await _uow.QualityChecks.GetAllAsync();
            var qcFailed = qcChecks.Count(q => q.Result == QualityCheckResult.Fail);
            modules.Add(new ComplianceModuleVm
            {
                Module = "Quality",
                TotalChecks = qcChecks.Count,
                PassedChecks = qcChecks.Count - qcFailed,
                FailedChecks = qcFailed,
                Score = qcChecks.Count > 0 ? Math.Round((qcChecks.Count - qcFailed) * 100m / qcChecks.Count, 1) : 100
            });
            if (qcFailed > 0) issues.Add(new ComplianceIssueVm
            {
                Module = "Quality",
                Severity = "High",
                Description = $"{qcFailed} quality check(s) failed",
                ActionUrl = "/Production"
            });

            // Inventory expiry
            var expired = await _uow.StockItems.Query()
                .CountAsync(s => s.ExpiryDate.HasValue && s.ExpiryDate < now && s.QuantityOnHand > 0);
            if (expired > 0) issues.Add(new ComplianceIssueVm
            {
                Module = "Inventory",
                Severity = "Critical",
                Description = $"{expired} expired stock item(s) still in inventory",
                ActionUrl = "/Inventory/ExpiryTracking"
            });

            var overall = modules.Count > 0 ? Math.Round(modules.Average(m => m.Score), 1) : 100;

            return new ComplianceReportVm
            {
                OverallComplianceScore = overall,
                Modules = modules.OrderBy(m => m.Score).ToList(),
                Issues = issues.OrderBy(i => i.Severity == "Critical" ? 0 : i.Severity == "High" ? 1 : 2).ToList()
            };
        }

        public async Task<AuditTrailResultVm> GetAuditTrailAsync(AuditTrailFilterVm filter)
        {
            var query = _uow.WorkflowActions.Query()
                .Include(a => a.WorkflowInstance).ThenInclude(i => i.WorkflowDefinition)
                .Include(a => a.WorkflowStep)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.EntityType))
                query = query.Where(a => a.WorkflowInstance.EntityType == filter.EntityType);
            if (filter.DateFrom.HasValue)
                query = query.Where(a => a.ActionDate >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                query = query.Where(a => a.ActionDate <= filter.DateTo.Value);

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(a => a.ActionDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize).ToListAsync();

            return new AuditTrailResultVm
            {
                Items = items.Select(a => new AuditEntryVm
                {
                    Module = a.WorkflowInstance.WorkflowDefinition.Module ?? a.WorkflowInstance.WorkflowDefinition.Category.ToString(),
                    EntityType = a.WorkflowInstance.EntityType,
                    Action = a.Result.ToString(),
                    EntityReference = a.WorkflowInstance.EntityReference,
                    UserName = a.ActionByUserName,
                    Date = a.ActionDate,
                    Details = a.Comments
                }).ToList(),
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<byte[]> ExportExecutiveReportAsync()
        {
            var data = await GetExecutiveDashboardAsync();
            using var wb = new XLWorkbook();

            // Sheet 1: KPI Summary
            var ws1 = wb.Worksheets.Add("Executive KPIs");
            SetHeaders(ws1, new[] { "Module", "Metric", "Value" });
            var r = 2;
            void AddRow(string mod, string metric, string val) { ws1.Cell(r, 1).Value = mod; ws1.Cell(r, 2).Value = metric; ws1.Cell(r, 3).Value = val; r++; }

            AddRow("Assets", "Total Assets", data.TotalAssets.ToString());
            AddRow("Assets", "Book Value", data.TotalAssetValue.ToString("C0"));
            AddRow("Assets", "Requiring Attention", data.AssetsRequiringAttention.ToString());
            AddRow("Documents", "Total Documents", data.TotalDocuments.ToString());
            AddRow("Documents", "Compliance %", $"{data.DocumentCompliancePercent}%");
            AddRow("Documents", "Overdue Reviews", data.DocumentsOverdueReview.ToString());
            AddRow("Manufacturing", "Orders In Progress", data.ProductionOrdersInProgress.ToString());
            AddRow("Manufacturing", "Quality Pass Rate", $"{data.QualityPassRate}%");
            AddRow("Maintenance", "Open Work Orders", data.OpenWorkOrders.ToString());
            AddRow("Maintenance", "Overdue Work Orders", data.OverdueWorkOrders.ToString());
            AddRow("Maintenance", "PM Compliance", $"{data.PMCompliancePercent}%");
            AddRow("Maintenance", "SLA Compliance", $"{data.SLACompliancePercent}%");
            AddRow("Inventory", "Stock Value", data.InventoryValue.ToString("C0"));
            AddRow("Inventory", "Out of Stock", data.OutOfStockItems.ToString());
            AddRow("Inventory", "Expiring Items", data.ExpiringItems.ToString());
            AddRow("Workflow", "Pending Approvals", data.PendingApprovals.ToString());
            AddRow("Workflow", "Completed Workflows", data.CompletedWorkflows.ToString());

            ws1.Columns().AdjustToContents();
            ws1.SheetView.FreezeRows(1);

            // Sheet 2: Alerts
            var ws2 = wb.Worksheets.Add("Critical Alerts");
            SetHeaders(ws2, new[] { "Module", "Alert", "Description" });
            var r2 = 2;
            foreach (var a in data.CriticalAlerts)
            { ws2.Cell(r2, 1).Value = a.Module; ws2.Cell(r2, 2).Value = a.Title; ws2.Cell(r2, 3).Value = a.Description; r2++; }
            ws2.Columns().AdjustToContents();

            // Sheet 3: Cost Trend
            var ws3 = wb.Worksheets.Add("Cost Trend");
            SetHeaders(ws3, new[] { "Period", "Maintenance Cost", "Inventory Value" });
            var r3 = 2;
            foreach (var c in data.MonthlyCostTrend)
            { ws3.Cell(r3, 1).Value = c.Period; ws3.Cell(r3, 2).Value = c.MaintenanceCost; ws3.Cell(r3, 3).Value = c.InventoryValue; r3++; }
            ws3.Range(2, 2, r3 - 1, 3).Style.NumberFormat.Format = "#,##0.00";
            ws3.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static void SetHeaders(IXLWorksheet ws, string[] headers)
        {
            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(50, 93, 136);
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }
        }
    }
}
