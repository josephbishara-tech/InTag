using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Inventory;

namespace InTagLogicLayer.Inventory
{
    public class InventoryExportService
    {
        private readonly IUnitOfWork _uow;
        public InventoryExportService(IUnitOfWork uow) { _uow = uow; }

        public async Task<byte[]> ExportStockReportAsync()
        {
            var stock = await _uow.StockItems.Query()
                .Include(s => s.Product).Include(s => s.Warehouse).Include(s => s.StorageBin)
                .Where(s => s.QuantityOnHand != 0)
                .OrderBy(s => s.Warehouse.Code).ThenBy(s => s.Product.ProductCode)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Stock Report");

            var headers = new[] { "Product Code", "Product Name", "Warehouse", "Bin", "On Hand",
                "Reserved", "Available", "Unit Cost", "Total Value", "Valuation",
                "Reorder Pt", "Min Level", "Max Level", "Lot #", "Serial #", "Expiry" };
            SetHeaders(ws, headers);

            var row = 2;
            foreach (var s in stock)
            {
                ws.Cell(row, 1).Value = s.Product.ProductCode;
                ws.Cell(row, 2).Value = s.Product.Name;
                ws.Cell(row, 3).Value = s.Warehouse.Code;
                ws.Cell(row, 4).Value = s.StorageBin?.BinCode ?? "";
                ws.Cell(row, 5).Value = s.QuantityOnHand;
                ws.Cell(row, 6).Value = s.QuantityReserved;
                ws.Cell(row, 7).Value = s.QuantityOnHand - s.QuantityReserved;
                ws.Cell(row, 8).Value = s.UnitCost;
                ws.Cell(row, 9).Value = s.QuantityOnHand * s.UnitCost;
                ws.Cell(row, 10).Value = s.ValuationMethod.ToString();
                ws.Cell(row, 11).Value = s.ReorderPoint;
                ws.Cell(row, 12).Value = s.MinimumLevel;
                ws.Cell(row, 13).Value = s.MaximumLevel;
                ws.Cell(row, 14).Value = s.LotNumber ?? "";
                ws.Cell(row, 15).Value = s.SerialNumber ?? "";
                ws.Cell(row, 16).Value = s.ExpiryDate?.ToString("yyyy-MM-dd") ?? "";

                if (s.QuantityOnHand <= 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);
                else if (s.ReorderPoint > 0 && s.QuantityOnHand <= s.ReorderPoint)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 243, 205);
                else if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 245, 240);
                row++;
            }

            ws.Range(2, 5, row - 1, 9).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(2, 8, row - 1, 8).Style.NumberFormat.Format = "#,##0.0000";
            ws.Range(2, 11, row - 1, 13).Style.NumberFormat.Format = "#,##0.00";

            ws.Cell(row + 1, 8).Value = "TOTALS:";
            ws.Cell(row + 1, 8).Style.Font.Bold = true;
            ws.Cell(row + 1, 9).FormulaA1 = $"=SUM(I2:I{row - 1})";
            ws.Cell(row + 1, 9).Style.Font.Bold = true;
            ws.Cell(row + 1, 9).Style.NumberFormat.Format = "#,##0.00";

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportTransactionReportAsync(TransactionFilterVm filter)
        {
            var query = _uow.InventoryTransactions.Query()
                .Include(t => t.Product).Include(t => t.Warehouse)
                .AsQueryable();

            if (filter.Type.HasValue) query = query.Where(t => t.Type == filter.Type.Value);
            if (filter.ProductId.HasValue) query = query.Where(t => t.ProductId == filter.ProductId.Value);
            if (filter.WarehouseId.HasValue) query = query.Where(t => t.WarehouseId == filter.WarehouseId.Value);
            if (filter.DateFrom.HasValue) query = query.Where(t => t.TransactionDate >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue) query = query.Where(t => t.TransactionDate <= filter.DateTo.Value);

            var txns = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Transactions");

            var headers = new[] { "TXN #", "Date", "Type", "Product Code", "Product Name",
                "Warehouse", "Qty", "Unit Cost", "Total Cost", "Lot #", "Serial #", "Reference", "Reason" };
            SetHeaders(ws, headers);

            var row = 2;
            foreach (var t in txns)
            {
                ws.Cell(row, 1).Value = t.TransactionNumber;
                ws.Cell(row, 2).Value = t.TransactionDate.ToString("yyyy-MM-dd HH:mm");
                ws.Cell(row, 3).Value = t.Type.ToString();
                ws.Cell(row, 4).Value = t.Product.ProductCode;
                ws.Cell(row, 5).Value = t.Product.Name;
                ws.Cell(row, 6).Value = t.Warehouse.Code;
                ws.Cell(row, 7).Value = t.Quantity;
                ws.Cell(row, 8).Value = t.UnitCost;
                ws.Cell(row, 9).Value = t.Quantity * t.UnitCost;
                ws.Cell(row, 10).Value = t.LotNumber ?? "";
                ws.Cell(row, 11).Value = t.SerialNumber ?? "";
                ws.Cell(row, 12).Value = t.ReferenceNumber ?? "";
                ws.Cell(row, 13).Value = t.Reason ?? "";

                if (row % 2 == 0) ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 245, 240);
                row++;
            }

            ws.Range(2, 7, row - 1, 9).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportValuationReportAsync()
        {
            var stock = await _uow.StockItems.Query()
                .Include(s => s.Product).Include(s => s.Warehouse)
                .Where(s => s.QuantityOnHand > 0)
                .OrderBy(s => s.Product.ProductCode).ToListAsync();

            using var wb = new XLWorkbook();

            // Sheet 1: By Product
            var ws1 = wb.Worksheets.Add("By Product");
            var h1 = new[] { "Product Code", "Product Name", "Total Qty", "Avg Unit Cost", "Total Value", "Valuation Method" };
            SetHeaders(ws1, h1);

            var byProduct = stock.GroupBy(s => s.ProductId).Select(g =>
            {
                var first = g.First();
                var totalQty = g.Sum(s => s.QuantityOnHand);
                var totalVal = g.Sum(s => s.QuantityOnHand * s.UnitCost);
                return new
                {
                    Code = first.Product.ProductCode,
                    Name = first.Product.Name,
                    Qty = totalQty,
                    AvgCost = totalQty > 0 ? totalVal / totalQty : 0,
                    Total = totalVal,
                    Method = first.ValuationMethod.ToString()
                };
            }).OrderByDescending(p => p.Total);

            var r1 = 2;
            foreach (var p in byProduct)
            {
                ws1.Cell(r1, 1).Value = p.Code; ws1.Cell(r1, 2).Value = p.Name;
                ws1.Cell(r1, 3).Value = p.Qty; ws1.Cell(r1, 4).Value = p.AvgCost;
                ws1.Cell(r1, 5).Value = p.Total; ws1.Cell(r1, 6).Value = p.Method;
                r1++;
            }
            ws1.Range(2, 3, r1 - 1, 5).Style.NumberFormat.Format = "#,##0.00";
            ws1.Columns().AdjustToContents();
            ws1.SheetView.FreezeRows(1);

            // Sheet 2: By Warehouse
            var ws2 = wb.Worksheets.Add("By Warehouse");
            var h2 = new[] { "Warehouse", "SKUs", "Total Qty", "Total Value" };
            SetHeaders(ws2, h2);

            var byWH = stock.GroupBy(s => s.WarehouseId).Select(g =>
            {
                var first = g.First();
                return new
                {
                    Code = first.Warehouse.Code,
                    Name = first.Warehouse.Name,
                    SKUs = g.Count(),
                    Qty = g.Sum(s => s.QuantityOnHand),
                    Value = g.Sum(s => s.QuantityOnHand * s.UnitCost)
                };
            }).OrderByDescending(w => w.Value);

            var r2 = 2;
            foreach (var w in byWH)
            {
                ws2.Cell(r2, 1).Value = $"{w.Code} — {w.Name}"; ws2.Cell(r2, 2).Value = w.SKUs;
                ws2.Cell(r2, 3).Value = w.Qty; ws2.Cell(r2, 4).Value = w.Value;
                r2++;
            }
            ws2.Range(2, 3, r2 - 1, 4).Style.NumberFormat.Format = "#,##0.00";
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
