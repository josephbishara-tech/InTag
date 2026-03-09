using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Document;

namespace InTagLogicLayer.Document
{
    public class DocumentExportService
    {
        private readonly IUnitOfWork _uow;

        public DocumentExportService(IUnitOfWork uow) { _uow = uow; }

        public async Task<byte[]> ExportDocumentRegisterAsync(DocumentFilterVm filter)
        {
            var query = _uow.Documents.Query()
                .Include(d => d.Department)
                .Include(d => d.Revisions)
                .AsQueryable();

            if (filter.Status.HasValue) query = query.Where(d => d.Status == filter.Status.Value);
            if (filter.Type.HasValue) query = query.Where(d => d.Type == filter.Type.Value);
            if (filter.Category.HasValue) query = query.Where(d => d.Category == filter.Category.Value);

            var docs = await query.OrderBy(d => d.DocNumber).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Document Register");

            var headers = new[] { "Doc Number", "Title", "Type", "Category", "Status",
                "Version", "Department", "Review Cycle", "Effective Date", "Next Review",
                "ISO Reference", "Confidentiality", "Revisions", "Checked Out", "Tags" };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(50, 93, 136);
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var d in docs)
            {
                ws.Cell(row, 1).Value = d.DocNumber;
                ws.Cell(row, 2).Value = d.Title;
                ws.Cell(row, 3).Value = d.Type.ToString();
                ws.Cell(row, 4).Value = d.Category.ToString();
                ws.Cell(row, 5).Value = d.Status.ToString();
                ws.Cell(row, 6).Value = d.CurrentVersion;
                ws.Cell(row, 7).Value = d.Department?.Name ?? "";
                ws.Cell(row, 8).Value = d.ReviewCycle.ToString();
                ws.Cell(row, 9).Value = d.EffectiveDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 10).Value = d.NextReviewDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 11).Value = d.IsoReference ?? "";
                ws.Cell(row, 12).Value = d.ConfidentialityLevel ?? "";
                ws.Cell(row, 13).Value = d.Revisions?.Count ?? 0;
                ws.Cell(row, 14).Value = d.IsCheckedOut ? "Yes" : "No";
                ws.Cell(row, 15).Value = d.Tags ?? "";

                // Highlight overdue reviews
                if (d.NextReviewDate.HasValue && d.NextReviewDate < DateTimeOffset.UtcNow
                    && d.Status == DocumentStatus.Published)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);
                else if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 245, 240);

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportComplianceReportAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var docs = await _uow.Documents.Query()
                .Include(d => d.Department)
                .Include(d => d.Revisions)
                .Where(d => d.Status == DocumentStatus.Published)
                .OrderBy(d => d.DocNumber)
                .ToListAsync();

            var distributions = await _uow.DistributionRecordsRepo.GetAllAsync();

            using var wb = new XLWorkbook();

            // Sheet 1: Review Status
            var wsReview = wb.Worksheets.Add("Review Compliance");
            var reviewHeaders = new[] { "Doc Number", "Title", "Type", "Department",
                "Effective Date", "Review Cycle", "Next Review", "Status", "Days Until/Overdue" };

            for (var i = 0; i < reviewHeaders.Length; i++)
            {
                wsReview.Cell(1, i + 1).Value = reviewHeaders[i];
                wsReview.Cell(1, i + 1).Style.Font.Bold = true;
                wsReview.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(50, 93, 136);
                wsReview.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var rRow = 2;
            foreach (var d in docs.Where(d => d.NextReviewDate.HasValue))
            {
                var daysUntil = (int)(d.NextReviewDate!.Value - now).TotalDays;
                wsReview.Cell(rRow, 1).Value = d.DocNumber;
                wsReview.Cell(rRow, 2).Value = d.Title;
                wsReview.Cell(rRow, 3).Value = d.Type.ToString();
                wsReview.Cell(rRow, 4).Value = d.Department?.Name ?? "";
                wsReview.Cell(rRow, 5).Value = d.EffectiveDate?.ToString("yyyy-MM-dd") ?? "";
                wsReview.Cell(rRow, 6).Value = d.ReviewCycle.ToString();
                wsReview.Cell(rRow, 7).Value = d.NextReviewDate!.Value.ToString("yyyy-MM-dd");
                wsReview.Cell(rRow, 8).Value = daysUntil < 0 ? "OVERDUE" : daysUntil <= 30 ? "Due Soon" : "Compliant";
                wsReview.Cell(rRow, 9).Value = daysUntil;

                if (daysUntil < 0)
                    wsReview.Row(rRow).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);
                else if (daysUntil <= 30)
                    wsReview.Row(rRow).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 243, 205);

                rRow++;
            }
            wsReview.Columns().AdjustToContents();
            wsReview.SheetView.FreezeRows(1);

            // Sheet 2: Distribution Acknowledgment
            var wsDist = wb.Worksheets.Add("Distribution Tracking");
            var distHeaders = new[] { "Doc Number", "Title", "Recipient", "Method",
                "Sent Date", "Acknowledged", "Acknowledged Date", "Days Pending" };

            for (var i = 0; i < distHeaders.Length; i++)
            {
                wsDist.Cell(1, i + 1).Value = distHeaders[i];
                wsDist.Cell(1, i + 1).Style.Font.Bold = true;
                wsDist.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(50, 93, 136);
                wsDist.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var dRow = 2;
            foreach (var dist in distributions.OrderBy(d => d.DocumentId))
            {
                var doc = docs.FirstOrDefault(d => d.Id == dist.DocumentId);
                var daysPending = dist.AcknowledgedDate.HasValue ? 0 : (int)(now - dist.SentDate).TotalDays;

                wsDist.Cell(dRow, 1).Value = doc?.DocNumber ?? "";
                wsDist.Cell(dRow, 2).Value = doc?.Title ?? "";
                wsDist.Cell(dRow, 3).Value = dist.RecipientName ?? dist.RecipientIdentifier;
                wsDist.Cell(dRow, 4).Value = dist.Method.ToString();
                wsDist.Cell(dRow, 5).Value = dist.SentDate.ToString("yyyy-MM-dd");
                wsDist.Cell(dRow, 6).Value = dist.AcknowledgedDate.HasValue ? "Yes" : "No";
                wsDist.Cell(dRow, 7).Value = dist.AcknowledgedDate?.ToString("yyyy-MM-dd") ?? "";
                wsDist.Cell(dRow, 8).Value = daysPending;

                if (!dist.AcknowledgedDate.HasValue && daysPending > 7)
                    wsDist.Row(dRow).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 215, 218);

                dRow++;
            }
            wsDist.Columns().AdjustToContents();
            wsDist.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
