using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;

namespace InTagLogicLayer.Maintenance
{
    public class MaintenanceExportService
    {
        private readonly IUnitOfWork _uow;
        public MaintenanceExportService(IUnitOfWork uow) { _uow = uow; }

        public async Task<byte[]> ExportWorkOrderReportAsync()
        {
            var orders = await _uow.WorkOrders.Query()
                .Include(w => w.Asset)
                .Include(w => w.LaborEntries)
                .Include(w => w.Parts)
                .OrderBy(w => w.WorkOrderNumber).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Work Orders");

            var headers = new[] { "WO #", "Title", "Asset Code", "Asset Name", "Type", "Priority",
                "Status", "Due Date", "Started", "Completed", "SLA (hrs)", "Actual (hrs)",
                "SLA Met", "Labor Cost", "Parts Cost", "External", "Total Cost", "Resolution" };
            SetHeaders(ws, headers);

            var row = 2;
            foreach (var w in orders)
            {
                decimal? actual = null;
                if (w.StartedDate.HasValue)
                {
                    var end = w.CompletedDate ?? DateTimeOffset.UtcNow;
                    actual = Math.Round((decimal)(end - w.StartedDate.Value).TotalHours, 1);
                }
                var slaMet = w.SLATargetHours.HasValue && actual.HasValue
                    ? (actual <= w.SLATargetHours ? "Yes" : "No") : "";

                ws.Cell(row, 1).Value = w.WorkOrderNumber;
                ws.Cell(row, 2).Value = w.Title;
                ws.Cell(row, 3).Value = w.Asset.AssetCode;
                ws.Cell(row, 4).Value = w.Asset.Name;
                ws.Cell(row, 5).Value = w.Type.ToString();
                ws.Cell(row, 6).Value = w.Priority.ToString();
                ws.Cell(row, 7).Value = w.Status.ToString();
                ws.Cell(row, 8).Value = w.DueDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 9).Value = w.StartedDate?.ToString("yyyy-MM-dd HH:mm") ?? "";
                ws.Cell(row, 10).Value = w.CompletedDate?.ToString("yyyy-MM-dd HH:mm") ?? "";
                ws.Cell(row, 11).Value = w.SLATargetHours?.ToString("N1") ?? "";
                ws.Cell(row, 12).Value = actual?.ToString("N1") ?? "";
                ws.Cell(row, 13).Value = slaMet;
                ws.Cell(row, 14).Value = w.LaborCost;
                ws.Cell(row, 15).Value = w.PartsCost;
                ws.Cell(row, 16).Value = w.ExternalCost;
                ws.Cell(row, 17).Value = w.LaborCost + w.PartsCost + w.ExternalCost;
                ws.Cell(row, 18).Value = w.Resolution ?? "";

                if (slaMet == "No")
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);
                else if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 245, 240);
                row++;
            }

            ws.Range(2, 14, row - 1, 17).Style.NumberFormat.Format = "#,##0.00";

            // Totals
            ws.Cell(row + 1, 13).Value = "TOTALS:";
            ws.Cell(row + 1, 13).Style.Font.Bold = true;
            ws.Cell(row + 1, 14).FormulaA1 = $"=SUM(N2:N{row - 1})";
            ws.Cell(row + 1, 15).FormulaA1 = $"=SUM(O2:O{row - 1})";
            ws.Cell(row + 1, 16).FormulaA1 = $"=SUM(P2:P{row - 1})";
            ws.Cell(row + 1, 17).FormulaA1 = $"=SUM(Q2:Q{row - 1})";
            ws.Range(row + 1, 14, row + 1, 17).Style.Font.Bold = true;
            ws.Range(row + 1, 14, row + 1, 17).Style.NumberFormat.Format = "#,##0.00";

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportPMComplianceReportAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var pms = await _uow.PMSchedules.Query()
                .Include(p => p.Asset)
                .Include(p => p.GeneratedWorkOrders)
                .Where(p => p.IsEnabled)
                .OrderBy(p => p.NextDueDate).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("PM Compliance");

            var headers = new[] { "Schedule Name", "Asset Code", "Asset Name", "Trigger", "Frequency",
                "Last Executed", "Next Due", "Status", "Days Until/Overdue", "WOs Generated" };
            SetHeaders(ws, headers);

            var row = 2;
            foreach (var p in pms)
            {
                var daysUntil = p.NextDueDate.HasValue ? (int)(p.NextDueDate.Value - now).TotalDays : 0;
                var status = !p.NextDueDate.HasValue ? "N/A" : daysUntil < 0 ? "OVERDUE" : daysUntil <= 7 ? "Due Soon" : "Compliant";

                ws.Cell(row, 1).Value = p.Name;
                ws.Cell(row, 2).Value = p.Asset.AssetCode;
                ws.Cell(row, 3).Value = p.Asset.Name;
                ws.Cell(row, 4).Value = p.TriggerType.ToString();
                ws.Cell(row, 5).Value = p.Frequency.ToString();
                ws.Cell(row, 6).Value = p.LastExecutedDate?.ToString("yyyy-MM-dd") ?? "Never";
                ws.Cell(row, 7).Value = p.NextDueDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 8).Value = status;
                ws.Cell(row, 9).Value = daysUntil;
                ws.Cell(row, 10).Value = p.GeneratedWorkOrders.Count;

                if (status == "OVERDUE")
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);
                else if (status == "Due Soon")
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 243, 205);
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportMaintenanceCostReportAsync()
        {
            var orders = await _uow.WorkOrders.Query()
                .Include(w => w.Asset)
                .Include(w => w.LaborEntries)
                .Include(w => w.Parts)
                .Where(w => w.Status == WorkOrderStatus.Completed || w.Status == WorkOrderStatus.Closed)
                .OrderBy(w => w.Asset.AssetCode).ToListAsync();

            using var wb = new XLWorkbook();

            // Sheet 1: By Asset
            var ws1 = wb.Worksheets.Add("Cost By Asset");
            var h1 = new[] { "Asset Code", "Asset Name", "Work Orders", "Labor", "Parts", "External", "Total" };
            SetHeaders(ws1, h1);

            var byAsset = orders.GroupBy(w => w.AssetId).Select(g =>
            {
                var first = g.First();
                return new
                {
                    Code = first.Asset.AssetCode,
                    Name = first.Asset.Name,
                    Count = g.Count(),
                    Labor = g.Sum(w => w.LaborCost),
                    Parts = g.Sum(w => w.PartsCost),
                    External = g.Sum(w => w.ExternalCost),
                    Total = g.Sum(w => w.LaborCost + w.PartsCost + w.ExternalCost)
                };
            }).OrderByDescending(a => a.Total);

            var r1 = 2;
            foreach (var a in byAsset)
            {
                ws1.Cell(r1, 1).Value = a.Code; ws1.Cell(r1, 2).Value = a.Name;
                ws1.Cell(r1, 3).Value = a.Count; ws1.Cell(r1, 4).Value = a.Labor;
                ws1.Cell(r1, 5).Value = a.Parts; ws1.Cell(r1, 6).Value = a.External;
                ws1.Cell(r1, 7).Value = a.Total;
                r1++;
            }
            ws1.Range(2, 4, r1 - 1, 7).Style.NumberFormat.Format = "#,##0.00";
            ws1.Columns().AdjustToContents();
            ws1.SheetView.FreezeRows(1);

            // Sheet 2: Detail
            var ws2 = wb.Worksheets.Add("Cost Detail");
            var h2 = new[] { "WO #", "Asset", "Type", "Completed", "Labor", "Parts", "External", "Total" };
            SetHeaders(ws2, h2);

            var r2 = 2;
            foreach (var w in orders.OrderByDescending(w => w.CompletedDate))
            {
                ws2.Cell(r2, 1).Value = w.WorkOrderNumber;
                ws2.Cell(r2, 2).Value = w.Asset.AssetCode;
                ws2.Cell(r2, 3).Value = w.Type.ToString();
                ws2.Cell(r2, 4).Value = w.CompletedDate?.ToString("yyyy-MM-dd") ?? "";
                ws2.Cell(r2, 5).Value = w.LaborCost;
                ws2.Cell(r2, 6).Value = w.PartsCost;
                ws2.Cell(r2, 7).Value = w.ExternalCost;
                ws2.Cell(r2, 8).Value = w.LaborCost + w.PartsCost + w.ExternalCost;
                r2++;
            }
            ws2.Range(2, 5, r2 - 1, 8).Style.NumberFormat.Format = "#,##0.00";
            ws2.Columns().AdjustToContents();
            ws2.SheetView.FreezeRows(1);

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
