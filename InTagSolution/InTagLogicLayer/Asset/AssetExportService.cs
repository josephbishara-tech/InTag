using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Asset;

namespace InTagLogicLayer.Asset
{
    public class AssetExportService
    {
        private readonly IUnitOfWork _uow;

        public AssetExportService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<byte[]> ExportAssetRegisterAsync(AssetFilterVm filter)
        {
            var query = _uow.Assets.Query()
                .Include(a => a.AssetType)
                .Include(a => a.Location)
                .Include(a => a.Department)
                .Include(a => a.Vendor)
                .AsQueryable();

            if (filter.Status.HasValue) query = query.Where(a => a.Status == filter.Status.Value);
            if (filter.Category.HasValue) query = query.Where(a => a.Category == filter.Category.Value);
            if (filter.LocationId.HasValue) query = query.Where(a => a.LocationId == filter.LocationId.Value);

            var assets = await query.OrderBy(a => a.AssetCode).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Asset Register");

            // Header style
            var headerRow = 1;
            var headers = new[] { "Asset Code", "Name", "Category", "Type", "Status",
                "Location", "Department", "Vendor", "Serial Number", "Manufacturer",
                "Purchase Cost", "Salvage Value", "Book Value", "Acc. Depreciation",
                "Dep. Method", "Useful Life (Mo)", "Acquisition Date", "Warranty End" };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(headerRow, i + 1).Value = headers[i];
                ws.Cell(headerRow, i + 1).Style.Font.Bold = true;
                ws.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(50, 93, 136);
                ws.Cell(headerRow, i + 1).Style.Font.FontColor = XLColor.White;
                ws.Cell(headerRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Data rows
            var row = 2;
            foreach (var a in assets)
            {
                ws.Cell(row, 1).Value = a.AssetCode;
                ws.Cell(row, 2).Value = a.Name;
                ws.Cell(row, 3).Value = a.Category.ToString();
                ws.Cell(row, 4).Value = a.AssetType?.Name ?? "";
                ws.Cell(row, 5).Value = a.Status.ToString();
                ws.Cell(row, 6).Value = a.Location?.Name ?? "";
                ws.Cell(row, 7).Value = a.Department?.Name ?? "";
                ws.Cell(row, 8).Value = a.Vendor?.Name ?? "";
                ws.Cell(row, 9).Value = a.SerialNumber ?? "";
                ws.Cell(row, 10).Value = a.Manufacturer ?? "";
                ws.Cell(row, 11).Value = a.PurchaseCost;
                ws.Cell(row, 12).Value = a.SalvageValue ?? 0;
                ws.Cell(row, 13).Value = a.CurrentBookValue;
                ws.Cell(row, 14).Value = a.AccumulatedDepreciation;
                ws.Cell(row, 15).Value = a.DepreciationMethod.ToString();
                ws.Cell(row, 16).Value = a.UsefulLifeMonths;
                ws.Cell(row, 17).Value = a.AcquisitionDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 18).Value = a.WarrantyEndDate?.ToString("yyyy-MM-dd") ?? "";

                // Alternate row shading
                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 245, 240);

                row++;
            }

            // Number formatting
            ws.Range(2, 11, row - 1, 14).Style.NumberFormat.Format = "#,##0.00";

            // Auto-fit and freeze
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            // Summary row
            ws.Cell(row + 1, 10).Value = "TOTALS:";
            ws.Cell(row + 1, 10).Style.Font.Bold = true;
            ws.Cell(row + 1, 11).FormulaA1 = $"=SUM(K2:K{row - 1})";
            ws.Cell(row + 1, 13).FormulaA1 = $"=SUM(M2:M{row - 1})";
            ws.Cell(row + 1, 14).FormulaA1 = $"=SUM(N2:N{row - 1})";
            ws.Range(row + 1, 11, row + 1, 14).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row + 1, 11, row + 1, 14).Style.Font.Bold = true;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportDepreciationScheduleAsync(int fiscalYear)
        {
            var records = await _uow.DepreciationRecords.Query()
                .Where(d => d.FiscalYear == fiscalYear)
                .Include(d => d.Asset)
                .OrderBy(d => d.Asset.AssetCode)
                .ThenBy(d => d.Period)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add($"Depreciation FY{fiscalYear}");

            var headers = new[] { "Asset Code", "Asset Name", "Period", "Method",
                "Opening Value", "Depreciation", "Accumulated", "Closing Value", "Posted" };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(50, 93, 136);
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var d in records)
            {
                ws.Cell(row, 1).Value = d.Asset?.AssetCode ?? "";
                ws.Cell(row, 2).Value = d.Asset?.Name ?? "";
                ws.Cell(row, 3).Value = d.Period;
                ws.Cell(row, 4).Value = d.Method.ToString();
                ws.Cell(row, 5).Value = d.OpeningBookValue;
                ws.Cell(row, 6).Value = d.DepreciationAmount;
                ws.Cell(row, 7).Value = d.AccumulatedDepreciation;
                ws.Cell(row, 8).Value = d.ClosingBookValue;
                ws.Cell(row, 9).Value = d.IsPosted ? "Yes" : "No";

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 245, 240);
                row++;
            }

            ws.Range(2, 5, row - 1, 8).Style.NumberFormat.Format = "#,##0.00";

            // Totals
            ws.Cell(row + 1, 5).Value = "TOTALS:";
            ws.Cell(row + 1, 5).Style.Font.Bold = true;
            ws.Cell(row + 1, 6).FormulaA1 = $"=SUM(F2:F{row - 1})";
            ws.Cell(row + 1, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row + 1, 6).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportTCOReportAsync()
        {
            var assets = await _uow.Assets.Query()
                .Include(a => a.AssetType)
                .Include(a => a.Location)
                .Where(a => a.Status != AssetStatus.Disposed)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("TCO Analysis");

            var headers = new[] { "Asset Code", "Name", "Category", "Purchase Cost",
                "Acc. Depreciation", "Book Value", "Depreciation %",
                "Age (Months)", "Cost Per Month", "Useful Life (Mo)", "Remaining Life (Mo)" };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(50, 93, 136);
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            var now = DateTimeOffset.UtcNow;
            foreach (var a in assets)
            {
                var ageMonths = a.AcquisitionDate.HasValue
                    ? (int)((now - a.AcquisitionDate.Value).TotalDays / 30.44)
                    : 0;
                var depPct = a.PurchaseCost > 0 ? a.AccumulatedDepreciation / a.PurchaseCost * 100 : 0;
                var costPerMonth = ageMonths > 0 ? a.PurchaseCost / ageMonths : 0;
                var remainingLife = Math.Max(0, a.UsefulLifeMonths - ageMonths);

                ws.Cell(row, 1).Value = a.AssetCode;
                ws.Cell(row, 2).Value = a.Name;
                ws.Cell(row, 3).Value = a.Category.ToString();
                ws.Cell(row, 4).Value = a.PurchaseCost;
                ws.Cell(row, 5).Value = a.AccumulatedDepreciation;
                ws.Cell(row, 6).Value = a.CurrentBookValue;
                ws.Cell(row, 7).Value = Math.Round(depPct, 1);
                ws.Cell(row, 8).Value = ageMonths;
                ws.Cell(row, 9).Value = Math.Round(costPerMonth, 2);
                ws.Cell(row, 10).Value = a.UsefulLifeMonths;
                ws.Cell(row, 11).Value = remainingLife;

                // Highlight assets nearing end of life
                if (remainingLife <= 6 && remainingLife > 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 243, 205);
                else if (remainingLife == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);

                if (row % 2 == 0 && remainingLife > 6)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 245, 240);

                row++;
            }

            ws.Range(2, 4, row - 1, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(2, 7, row - 1, 7).Style.NumberFormat.Format = "0.0";
            ws.Range(2, 9, row - 1, 9).Style.NumberFormat.Format = "#,##0.00";

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
