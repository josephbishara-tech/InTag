using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;

namespace InTagLogicLayer.Manufacturing
{
    public class ManufacturingExportService
    {
        private readonly IUnitOfWork _uow;
        public ManufacturingExportService(IUnitOfWork uow) { _uow = uow; }

        public async Task<byte[]> ExportProductionReportAsync()
        {
            var orders = await _uow.ProductionOrders.Query()
                .Include(o => o.Product)
                .Include(o => o.BOM)
                .Include(o => o.ProductionLogs)
                .OrderBy(o => o.OrderNumber).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Production Orders");

            var headers = new[] { "Order #", "Product", "Status", "Priority", "Planned Qty",
                "Completed", "Scrap", "Completion %", "Planned Start", "Planned End",
                "Actual Start", "Actual End", "BOM", "Log Entries" };
            SetHeaders(ws, headers);

            var row = 2;
            foreach (var o in orders)
            {
                var pct = o.PlannedQuantity > 0 ? Math.Round(o.CompletedQuantity / o.PlannedQuantity * 100, 1) : 0;
                ws.Cell(row, 1).Value = o.OrderNumber;
                ws.Cell(row, 2).Value = o.Product.Name;
                ws.Cell(row, 3).Value = o.Status.ToString();
                ws.Cell(row, 4).Value = o.Priority.ToString();
                ws.Cell(row, 5).Value = o.PlannedQuantity;
                ws.Cell(row, 6).Value = o.CompletedQuantity;
                ws.Cell(row, 7).Value = o.ScrapQuantity;
                ws.Cell(row, 8).Value = pct;
                ws.Cell(row, 9).Value = o.PlannedStartDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 10).Value = o.PlannedEndDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 11).Value = o.ActualStartDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 12).Value = o.ActualEndDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 13).Value = o.BOM?.BOMCode ?? "";
                ws.Cell(row, 14).Value = o.ProductionLogs.Count;

                if (o.Status == ProductionOrderStatus.InProgress && o.PlannedEndDate.HasValue && o.PlannedEndDate < DateTimeOffset.UtcNow)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);
                else if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 245, 240);
                row++;
            }

            ws.Range(2, 5, row - 1, 8).Style.NumberFormat.Format = "#,##0.0";
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportQualityReportAsync()
        {
            var checks = await _uow.QualityChecks.Query()
                .Include(q => q.ProductionOrder).ThenInclude(o => o!.Product)
                .Include(q => q.LotBatch)
                .OrderByDescending(q => q.CheckDate).ToListAsync();

            var lots = await _uow.LotBatches.Query()
                .Include(l => l.Product)
                .OrderBy(l => l.LotNumber).ToListAsync();

            using var wb = new XLWorkbook();

            // Sheet 1: Quality Checks
            var wsQC = wb.Worksheets.Add("Quality Checks");
            var qcHeaders = new[] { "Date", "Order #", "Product", "Lot #", "Check Name",
                "Specification", "Actual", "Result", "Findings", "Corrective Action" };
            SetHeaders(wsQC, qcHeaders);

            var qRow = 2;
            foreach (var q in checks)
            {
                wsQC.Cell(qRow, 1).Value = q.CheckDate.ToString("yyyy-MM-dd HH:mm");
                wsQC.Cell(qRow, 2).Value = q.ProductionOrder?.OrderNumber ?? "";
                wsQC.Cell(qRow, 3).Value = q.ProductionOrder?.Product?.Name ?? "";
                wsQC.Cell(qRow, 4).Value = q.LotBatch?.LotNumber ?? "";
                wsQC.Cell(qRow, 5).Value = q.CheckName;
                wsQC.Cell(qRow, 6).Value = q.Specification ?? "";
                wsQC.Cell(qRow, 7).Value = q.ActualValue ?? "";
                wsQC.Cell(qRow, 8).Value = q.Result.ToString();
                wsQC.Cell(qRow, 9).Value = q.Findings ?? "";
                wsQC.Cell(qRow, 10).Value = q.CorrectiveAction ?? "";

                if (q.Result == QualityCheckResult.Fail)
                    wsQC.Row(qRow).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);
                qRow++;
            }
            wsQC.Columns().AdjustToContents();
            wsQC.SheetView.FreezeRows(1);

            // Sheet 2: Lot Traceability
            var wsLot = wb.Worksheets.Add("Lot Traceability");
            var lotHeaders = new[] { "Lot #", "Product", "Quantity", "Status",
                "Manufacture Date", "Expiry", "Storage", "Order ID" };
            SetHeaders(wsLot, lotHeaders);

            var lRow = 2;
            foreach (var l in lots)
            {
                wsLot.Cell(lRow, 1).Value = l.LotNumber;
                wsLot.Cell(lRow, 2).Value = l.Product.Name;
                wsLot.Cell(lRow, 3).Value = l.Quantity;
                wsLot.Cell(lRow, 4).Value = l.Status.ToString();
                wsLot.Cell(lRow, 5).Value = l.ManufactureDate.ToString("yyyy-MM-dd");
                wsLot.Cell(lRow, 6).Value = l.ExpiryDate?.ToString("yyyy-MM-dd") ?? "";
                wsLot.Cell(lRow, 7).Value = l.StorageLocation ?? "";
                wsLot.Cell(lRow, 8).Value = l.ProductionOrderId?.ToString() ?? "";

                if (l.Status == LotBatchStatus.Quarantine)
                    wsLot.Row(lRow).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 243, 205);
                else if (l.Status == LotBatchStatus.Rejected)
                    wsLot.Row(lRow).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);
                lRow++;
            }
            wsLot.Columns().AdjustToContents();
            wsLot.SheetView.FreezeRows(1);

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
