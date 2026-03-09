using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Document;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Document;

namespace InTagLogicLayer.Document
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITenantService _tenantService;
        private readonly ILogger<DocumentService> _logger;
        private readonly DocumentExportService _exportService;
        private readonly IWorkflowHook _workflowHook;


        public DocumentService(
            IUnitOfWork uow,
            ITenantService tenantService,
            ILogger<DocumentService> logger,
            IWorkflowHook workflowHook)
        {
            _uow = uow;
            _tenantService = tenantService;
            _logger = logger;
            _exportService  = new DocumentExportService(uow); 
            _workflowHook = workflowHook;
        }

        // ══════════════════════════════════════
        //  CRUD
        // ══════════════════════════════════════

        public async Task<DocumentDetailVm> GetByIdAsync(int id)
        {
            var doc = await _uow.Documents.GetWithRevisionsAsync(id);
            if (doc == null)
                throw new KeyNotFoundException($"Document with ID {id} not found.");
            return MapToDetailVm(doc);
        }

        public async Task<DocumentListResultVm> GetAllAsync(DocumentFilterVm filter)
        {
            var query = _uow.Documents.Query()
                .Include(d => d.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(d =>
                    d.DocNumber.Contains(term) ||
                    d.Title.Contains(term) ||
                    (d.Tags != null && d.Tags.Contains(term)));
            }

            if (filter.Status.HasValue)
                query = query.Where(d => d.Status == filter.Status.Value);
            if (filter.Type.HasValue)
                query = query.Where(d => d.Type == filter.Type.Value);
            if (filter.Category.HasValue)
                query = query.Where(d => d.Category == filter.Category.Value);
            if (filter.DepartmentId.HasValue)
                query = query.Where(d => d.DepartmentId == filter.DepartmentId.Value);
            if (filter.OverdueReview == true)
                query = query.Where(d => d.NextReviewDate.HasValue && d.NextReviewDate < DateTimeOffset.UtcNow);

            query = filter.SortBy?.ToLower() switch
            {
                "title" => filter.SortDescending ? query.OrderByDescending(d => d.Title) : query.OrderBy(d => d.Title),
                "status" => filter.SortDescending ? query.OrderByDescending(d => d.Status) : query.OrderBy(d => d.Status),
                "type" => filter.SortDescending ? query.OrderByDescending(d => d.Type) : query.OrderBy(d => d.Type),
                "review" => filter.SortDescending ? query.OrderByDescending(d => d.NextReviewDate) : query.OrderBy(d => d.NextReviewDate),
                "version" => filter.SortDescending ? query.OrderByDescending(d => d.CurrentVersion) : query.OrderBy(d => d.CurrentVersion),
                _ => filter.SortDescending ? query.OrderByDescending(d => d.DocNumber) : query.OrderBy(d => d.DocNumber)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new DocumentListResultVm
            {
                Items = items.Select(MapToListItemVm).ToList(),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<DocumentDetailVm> CreateAsync(DocumentCreateVm model)
        {
            // Generate or validate doc number
            string docNumber;
            if (!string.IsNullOrWhiteSpace(model.DocNumberOverride))
            {
                if (await _uow.Documents.DocNumberExistsAsync(model.DocNumberOverride))
                    throw new InvalidOperationException($"Document number '{model.DocNumberOverride}' already exists.");
                docNumber = model.DocNumberOverride;
            }
            else
            {
                docNumber = await DocumentNumberGenerator.GenerateAsync(model.Type, model.Category, _uow);
            }

            var doc = new InTagEntitiesLayer.Document.Document
            {
                DocNumber = docNumber,
                Title = model.Title,
                Description = model.Description,
                Type = model.Type,
                Category = model.Category,
                Status = DocumentStatus.Draft,
                CurrentVersion = "1.0",
                ReviewCycle = model.ReviewCycle,
                IsoReference = model.IsoReference,
                ConfidentialityLevel = model.ConfidentialityLevel,
                AuthorUserId = _tenantService.GetCurrentUserId(),
                OwnerUserId = _tenantService.GetCurrentUserId(),
                DepartmentId = model.DepartmentId,
                Tags = model.Tags,
                Notes = model.Notes
            };

            await _uow.Documents.AddAsync(doc);
            await _uow.SaveChangesAsync();

            // Create initial revision
            var revision = new DocumentRevision
            {
                DocumentId = doc.Id,
                RevisionNumber = "1.0",
                ChangeDescription = "Initial version",
                ApprovalStatus = ApprovalAction.Pending
            };

            await _uow.DocumentRevisions.AddAsync(revision);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Document created: {DocNumber} — {Title}", doc.DocNumber, doc.Title);
            return await GetByIdAsync(doc.Id);
        }

        public async Task<DocumentDetailVm> UpdateAsync(int id, DocumentUpdateVm model)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null) throw new KeyNotFoundException("Document not found.");

            if (doc.Status == DocumentStatus.Obsolete || doc.Status == DocumentStatus.Archived)
                throw new InvalidOperationException("Cannot modify an obsolete or archived document.");

            if (doc.IsCheckedOut && doc.CheckedOutByUserId != _tenantService.GetCurrentUserId())
                throw new InvalidOperationException("Document is checked out by another user.");

            doc.Title = model.Title;
            doc.Description = model.Description;
            doc.Category = model.Category;
            doc.DepartmentId = model.DepartmentId;
            doc.ReviewCycle = model.ReviewCycle;
            doc.IsoReference = model.IsoReference;
            doc.ConfidentialityLevel = model.ConfidentialityLevel;
            doc.Tags = model.Tags;
            doc.Notes = model.Notes;

            _uow.Documents.Update(doc);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Document updated: {DocNumber}", doc.DocNumber);
            return await GetByIdAsync(id);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null) throw new KeyNotFoundException("Document not found.");

            if (doc.IsCheckedOut)
                throw new InvalidOperationException("Cannot delete a checked-out document.");

            if (doc.Status == DocumentStatus.Published)
                throw new InvalidOperationException("Cannot delete a published document. Obsolete it first.");

            _uow.Documents.SoftDelete(doc);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Document soft-deleted: {DocNumber}", doc.DocNumber);
        }

        // ══════════════════════════════════════
        //  CHECK-IN / CHECK-OUT
        // ══════════════════════════════════════

        public async Task<DocumentDetailVm> CheckOutAsync(int id)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null) throw new KeyNotFoundException("Document not found.");

            if (doc.IsCheckedOut)
                throw new InvalidOperationException(
                    $"Document is already checked out since {doc.CheckedOutDate:g}.");

            if (doc.Status == DocumentStatus.Obsolete || doc.Status == DocumentStatus.Archived)
                throw new InvalidOperationException("Cannot check out an obsolete or archived document.");

            doc.IsCheckedOut = true;
            doc.CheckedOutByUserId = _tenantService.GetCurrentUserId();
            doc.CheckedOutDate = DateTimeOffset.UtcNow;

            _uow.Documents.Update(doc);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Document {DocNumber} checked out by user {UserId}",
                doc.DocNumber, doc.CheckedOutByUserId);

            return await GetByIdAsync(id);
        }

        public async Task<DocumentDetailVm> CheckInAsync(int id, RevisionCreateVm revision)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null) throw new KeyNotFoundException("Document not found.");

            if (!doc.IsCheckedOut)
                throw new InvalidOperationException("Document is not checked out.");

            if (doc.CheckedOutByUserId != _tenantService.GetCurrentUserId())
                throw new InvalidOperationException("Only the user who checked out the document can check it in.");

            // Create new revision
            var newVersion = DocumentNumberGenerator.IncrementVersion(doc.CurrentVersion);

            var rev = new DocumentRevision
            {
                DocumentId = id,
                RevisionNumber = newVersion,
                ChangeDescription = revision.ChangeDescription,
                ApprovalStatus = ApprovalAction.Pending,
                Notes = revision.Notes
            };

            await _uow.DocumentRevisions.AddAsync(rev);

            // Update document
            doc.CurrentVersion = newVersion;
            doc.IsCheckedOut = false;
            doc.CheckedOutByUserId = null;
            doc.CheckedOutDate = null;

            // If it was published, move back to InReview
            if (doc.Status == DocumentStatus.Published)
                doc.Status = DocumentStatus.InReview;

            _uow.Documents.Update(doc);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Document {DocNumber} checked in — new revision {Version}",
                doc.DocNumber, newVersion);

            return await GetByIdAsync(id);
        }

        public async Task<DocumentDetailVm> CancelCheckOutAsync(int id)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null) throw new KeyNotFoundException("Document not found.");

            if (!doc.IsCheckedOut)
                throw new InvalidOperationException("Document is not checked out.");

            // Allow the checkout user or an admin to cancel
            doc.IsCheckedOut = false;
            doc.CheckedOutByUserId = null;
            doc.CheckedOutDate = null;

            _uow.Documents.Update(doc);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Document {DocNumber} check-out cancelled", doc.DocNumber);
            return await GetByIdAsync(id);
        }

        // ══════════════════════════════════════
        //  REVISIONS & APPROVAL
        // ══════════════════════════════════════

        public async Task<DocumentDetailVm> CreateRevisionAsync(RevisionCreateVm model)
        {
            var doc = await _uow.Documents.GetByIdAsync(model.DocumentId);
            if (doc == null) throw new KeyNotFoundException("Document not found.");

            if (doc.IsCheckedOut)
                throw new InvalidOperationException("Document is checked out. Use check-in to create a revision.");

            var newVersion = DocumentNumberGenerator.IncrementVersion(doc.CurrentVersion);

            var rev = new DocumentRevision
            {
                DocumentId = model.DocumentId,
                RevisionNumber = newVersion,
                ChangeDescription = model.ChangeDescription,
                ApprovalStatus = ApprovalAction.Pending,
                Notes = model.Notes
            };

            await _uow.DocumentRevisions.AddAsync(rev);

            doc.CurrentVersion = newVersion;
            if (doc.Status == DocumentStatus.Published)
                doc.Status = DocumentStatus.InReview;

            _uow.Documents.Update(doc);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Revision {Version} created for {DocNumber}", newVersion, doc.DocNumber);
            return await GetByIdAsync(model.DocumentId);
        }

        public async Task<DocumentDetailVm> ApproveRevisionAsync(RevisionApprovalVm model)
        {
            var revision = await _uow.DocumentRevisions.GetByIdAsync(model.RevisionId);
            if (revision == null) throw new KeyNotFoundException("Revision not found.");

            if (revision.ApprovalStatus != ApprovalAction.Pending)
                throw new InvalidOperationException("Revision has already been actioned.");

            revision.ApproverUserId = _tenantService.GetCurrentUserId();
            revision.ApprovalDate = DateTimeOffset.UtcNow;
            revision.ApprovalComments = model.Comments;
            revision.ApprovalStatus = model.IsApproved
                ? ApprovalAction.Approved
                : ApprovalAction.Rejected;

            _uow.DocumentRevisions.Update(revision);

            // If approved, move document to Approved status
            if (model.IsApproved)
            {
                var doc = await _uow.Documents.GetByIdAsync(revision.DocumentId);
                if (doc != null && doc.Status == DocumentStatus.InReview)
                {
                    doc.Status = DocumentStatus.Approved;
                    _uow.Documents.Update(doc);
                }
                await _workflowHook.OnRevisionApprovedAsync(revision.DocumentId, doc.DocNumber, revision.RevisionNumber);
            }
            else
            {
                var rejDoc = await _uow.Documents.GetByIdAsync(revision.DocumentId);
                if (rejDoc != null)
                    await _workflowHook.OnRevisionRejectedAsync(revision.DocumentId, rejDoc.DocNumber, revision.RevisionNumber);
            }

            await _uow.SaveChangesAsync();

            _logger.LogInformation("Revision {RevId} {Action} for document {DocId}",
                model.RevisionId,
                model.IsApproved ? "approved" : "rejected",
                revision.DocumentId);

            return await GetByIdAsync(revision.DocumentId);
        }

        // ══════════════════════════════════════
        //  LIFECYCLE
        // ══════════════════════════════════════

        public async Task<DocumentDetailVm> PublishAsync(int id)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null) throw new KeyNotFoundException("Document not found.");

            if (doc.Status != DocumentStatus.Approved && doc.Status != DocumentStatus.Draft)
                throw new InvalidOperationException(
                    $"Only approved or draft documents can be published. Current status: {doc.Status}.");

            doc.Status = DocumentStatus.Published;
            doc.EffectiveDate = DateTimeOffset.UtcNow;

            // Set next review date based on cycle
            if (doc.ReviewCycle != ReviewCycle.None)
            {
                doc.NextReviewDate = DateTimeOffset.UtcNow.AddMonths((int)doc.ReviewCycle);
            }

            _uow.Documents.Update(doc);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Document {DocNumber} published. Next review: {ReviewDate}",
                doc.DocNumber, doc.NextReviewDate);
            await _workflowHook.OnDocumentPublishedAsync(id, doc.DocNumber);
            return await GetByIdAsync(id);
        }

        public async Task<DocumentDetailVm> ObsoleteAsync(int id)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null) throw new KeyNotFoundException("Document not found.");

            if (doc.IsCheckedOut)
                throw new InvalidOperationException("Cannot obsolete a checked-out document.");

            doc.Status = DocumentStatus.Obsolete;
            doc.ExpiryDate = DateTimeOffset.UtcNow;
            doc.NextReviewDate = null;

            _uow.Documents.Update(doc);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Document {DocNumber} marked obsolete", doc.DocNumber);
            return await GetByIdAsync(id);
        }

        public async Task<DocumentDetailVm> ArchiveAsync(int id)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null) throw new KeyNotFoundException("Document not found.");

            doc.Status = DocumentStatus.Archived;
            doc.NextReviewDate = null;

            _uow.Documents.Update(doc);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Document {DocNumber} archived", doc.DocNumber);
            return await GetByIdAsync(id);
        }

        // ══════════════════════════════════════
        //  DISTRIBUTION
        // ══════════════════════════════════════

        public async Task DistributeAsync(DistributionCreateVm model)
        {
            var doc = await _uow.Documents.GetByIdAsync(model.DocumentId);
            if (doc == null) throw new KeyNotFoundException("Document not found.");

            if (doc.Status != DocumentStatus.Published)
                throw new InvalidOperationException("Only published documents can be distributed.");

            var record = new DistributionRecord
            {
                DocumentId = model.DocumentId,
                Method = model.Method,
                RecipientType = model.RecipientType,
                RecipientIdentifier = model.RecipientIdentifier,
                RecipientName = model.RecipientName,
                SentDate = DateTimeOffset.UtcNow
            };

            await _uow.DistributionRecordsRepo.AddAsync(record);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Document {DocNumber} distributed to {Recipient} via {Method}",
                doc.DocNumber, model.RecipientName ?? model.RecipientIdentifier, model.Method);
            await _workflowHook.OnDocumentDistributedAsync(model.DocumentId, doc.DocNumber, model.RecipientName ?? model.RecipientIdentifier);
        }

        public async Task AcknowledgeDistributionAsync(int distributionId)
        {
            var record = await _uow.DistributionRecordsRepo.GetByIdAsync(distributionId);
            if (record == null) throw new KeyNotFoundException("Distribution record not found.");

            if (record.AcknowledgedDate.HasValue)
                throw new InvalidOperationException("Already acknowledged.");

            record.AcknowledgedDate = DateTimeOffset.UtcNow;
            _uow.DistributionRecordsRepo.Update(record);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Distribution {Id} acknowledged", distributionId);
        }

        // ══════════════════════════════════════
        //  REVIEW
        // ══════════════════════════════════════

        public async Task<IReadOnlyList<DocumentListItemVm>> GetDueForReviewAsync()
        {
            var docs = await _uow.Documents.GetDueForReviewAsync();
            return docs.Select(MapToListItemVm).ToList();
        }

        // ══════════════════════════════════════
        //  MAPPING
        // ══════════════════════════════════════

        private static DocumentDetailVm MapToDetailVm(InTagEntitiesLayer.Document.Document doc)
        {
            return new DocumentDetailVm
            {
                Id = doc.Id,
                DocNumber = doc.DocNumber,
                Title = doc.Title,
                Description = doc.Description,
                Type = doc.Type,
                Category = doc.Category,
                Status = doc.Status,
                CurrentVersion = doc.CurrentVersion,
                EffectiveDate = doc.EffectiveDate,
                ExpiryDate = doc.ExpiryDate,
                ReviewCycle = doc.ReviewCycle,
                NextReviewDate = doc.NextReviewDate,
                IsCheckedOut = doc.IsCheckedOut,
                CheckedOutByUserId = doc.CheckedOutByUserId,
                CheckedOutDate = doc.CheckedOutDate,
                IsoReference = doc.IsoReference,
                ConfidentialityLevel = doc.ConfidentialityLevel,
                Tags = doc.Tags,
                DepartmentName = doc.Department?.Name,
                Notes = doc.Notes,
                RevisionCount = doc.Revisions?.Count ?? 0,
                Revisions = doc.Revisions?.Select(r => new RevisionListItemVm
                {
                    Id = r.Id,
                    RevisionNumber = r.RevisionNumber,
                    ChangeDescription = r.ChangeDescription,
                    ApprovalStatus = r.ApprovalStatus,
                    ApprovalDate = r.ApprovalDate,
                    CreatedDate = r.CreatedDate,
                    FileCount = r.Files?.Count ?? 0
                }).ToList() ?? new List<RevisionListItemVm>(),
                Distributions = doc.DistributionRecords?.Select(d => new DistributionListItemVm
                {
                    Id = d.Id,
                    RecipientName = d.RecipientName ?? d.RecipientIdentifier,
                    Method = d.Method.ToString(),
                    SentDate = d.SentDate,
                    IsAcknowledged = d.IsAcknowledged,
                    AcknowledgedDate = d.AcknowledgedDate
                }).ToList() ?? new List<DistributionListItemVm>(),
                CreatedDate = doc.CreatedDate,
                ModifiedDate = doc.ModifiedDate
            };
        }

        private static DocumentListItemVm MapToListItemVm(InTagEntitiesLayer.Document.Document doc)
        {
            return new DocumentListItemVm
            {
                Id = doc.Id,
                DocNumber = doc.DocNumber,
                Title = doc.Title,
                Type = doc.Type,
                Status = doc.Status,
                CurrentVersion = doc.CurrentVersion,
                DepartmentName = doc.Department?.Name,
                NextReviewDate = doc.NextReviewDate,
                IsCheckedOut = doc.IsCheckedOut
            };
        }

        // =══════════════════════════════════════
        // Dashboard & Reports (not implemented in this snippet)
        // =══════════════════════════════════════
        public async Task<DocumentDashboardVm> GetDashboardAsync()
        {
            var now = DateTimeOffset.UtcNow;

            var allDocs = await _uow.Documents.Query()
                .Include(d => d.Department)
                .ToListAsync();

            var allRevisions = await _uow.DocumentRevisions.Query()
                .Include(r => r.Document)
                .ToListAsync();

            var allDistributions = await _uow.DistributionRecordsRepo.GetAllAsync();

            // Overdue reviews
            var overdueReviews = allDocs
                .Where(d => d.NextReviewDate.HasValue && d.NextReviewDate < now && d.Status == DocumentStatus.Published)
                .Select(d => new ReviewAlertVm
                {
                    DocumentId = d.Id,
                    DocNumber = d.DocNumber,
                    Title = d.Title,
                    ReviewDate = d.NextReviewDate!.Value,
                    DaysOverdueOrRemaining = (int)(now - d.NextReviewDate!.Value).TotalDays
                })
                .OrderByDescending(r => r.DaysOverdueOrRemaining)
                .ToList();

            // Upcoming reviews (next 30 days)
            var upcomingReviews = allDocs
                .Where(d => d.NextReviewDate.HasValue
                            && d.NextReviewDate >= now
                            && d.NextReviewDate <= now.AddDays(30)
                            && d.Status == DocumentStatus.Published)
                .Select(d => new ReviewAlertVm
                {
                    DocumentId = d.Id,
                    DocNumber = d.DocNumber,
                    Title = d.Title,
                    ReviewDate = d.NextReviewDate!.Value,
                    DaysOverdueOrRemaining = (int)(d.NextReviewDate!.Value - now).TotalDays
                })
                .OrderBy(r => r.DaysOverdueOrRemaining)
                .ToList();

            // Pending approvals
            var pendingApprovals = allRevisions
                .Where(r => r.ApprovalStatus == ApprovalAction.Pending)
                .Select(r => new PendingApprovalVm
                {
                    RevisionId = r.Id,
                    DocumentId = r.DocumentId,
                    DocNumber = r.Document.DocNumber,
                    Title = r.Document.Title,
                    RevisionNumber = r.RevisionNumber,
                    SubmittedDate = r.CreatedDate,
                    DaysPending = (int)(now - r.CreatedDate).TotalDays
                })
                .OrderByDescending(p => p.DaysPending)
                .ToList();

            // Checked out
            var checkedOut = allDocs
                .Where(d => d.IsCheckedOut && d.CheckedOutDate.HasValue)
                .Select(d => new CheckedOutDocVm
                {
                    DocumentId = d.Id,
                    DocNumber = d.DocNumber,
                    Title = d.Title,
                    CheckedOutDate = d.CheckedOutDate!.Value,
                    DaysCheckedOut = (int)(now - d.CheckedOutDate!.Value).TotalDays
                })
                .OrderByDescending(c => c.DaysCheckedOut)
                .ToList();

            // Review compliance: published docs with review date set and not overdue
            var publishedWithReview = allDocs.Count(d => d.Status == DocumentStatus.Published && d.NextReviewDate.HasValue);
            var compliantDocs = allDocs.Count(d => d.Status == DocumentStatus.Published
                                                  && d.NextReviewDate.HasValue && d.NextReviewDate >= now);
            var reviewCompliance = publishedWithReview > 0
                ? Math.Round(compliantDocs * 100m / publishedWithReview, 1) : 100;

            // By status
            var byStatus = allDocs
                .GroupBy(d => d.Status)
                .Select(g => new DocStatusBreakdownVm
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = allDocs.Count > 0 ? Math.Round(g.Count() * 100m / allDocs.Count, 1) : 0
                })
                .OrderByDescending(s => s.Count)
                .ToList();

            // By type
            var byType = allDocs
                .GroupBy(d => d.Type)
                .Select(g => new DocTypeBreakdownVm
                {
                    Type = g.Key.ToString(),
                    Count = g.Count(),
                    PublishedCount = g.Count(d => d.Status == DocumentStatus.Published)
                })
                .OrderByDescending(t => t.Count)
                .ToList();

            // By category
            var byCategory = allDocs
                .GroupBy(d => d.Category)
                .Select(g => new DocCategoryBreakdownVm
                {
                    Category = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Count)
                .ToList();

            // Recent activity
            var recentRevisions = allRevisions
                .OrderByDescending(r => r.CreatedDate)
                .Take(5)
                .Select(r => new RecentDocActivityVm
                {
                    Icon = r.ApprovalStatus == ApprovalAction.Approved ? "bi-check-circle-fill" : "bi-file-earmark-plus",
                    Description = $"{r.Document.DocNumber} revision {r.RevisionNumber} — {r.ApprovalStatus}",
                    Date = r.CreatedDate,
                    DocumentId = r.DocumentId
                });

            var recentDistributions = allDistributions
                .OrderByDescending(d => d.SentDate)
                .Take(5)
                .Select(d =>
                {
                    var doc = allDocs.FirstOrDefault(doc => doc.Id == d.DocumentId);
                    return new RecentDocActivityVm
                    {
                        Icon = "bi-send",
                        Description = $"{doc?.DocNumber ?? "?"} distributed to {d.RecipientName ?? d.RecipientIdentifier}",
                        Date = d.SentDate,
                        DocumentId = d.DocumentId
                    };
                });

            var recentActivity = recentRevisions.Concat(recentDistributions)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToList();

            return new DocumentDashboardVm
            {
                TotalDocuments = allDocs.Count,
                PublishedDocuments = allDocs.Count(d => d.Status == DocumentStatus.Published),
                DraftDocuments = allDocs.Count(d => d.Status == DocumentStatus.Draft),
                InReviewDocuments = allDocs.Count(d => d.Status == DocumentStatus.InReview),
                ObsoleteDocuments = allDocs.Count(d => d.Status == DocumentStatus.Obsolete),
                CheckedOutDocuments = checkedOut.Count,
                OverdueReviewCount = overdueReviews.Count,
                UpcomingReviewCount = upcomingReviews.Count,
                PendingApprovalCount = pendingApprovals.Count,
                UnacknowledgedDistributionCount = allDistributions.Count(d => !d.AcknowledgedDate.HasValue),
                ReviewCompliancePercent = reviewCompliance,
                OverdueReviews = overdueReviews,
                UpcomingReviews = upcomingReviews,
                PendingApprovals = pendingApprovals,
                CheckedOutDocs = checkedOut,
                ByStatus = byStatus,
                ByType = byType,
                ByCategory = byCategory,
                RecentActivity = recentActivity
            };
        }


        public Task<byte[]> ExportDocumentRegisterAsync(DocumentFilterVm filter)
            => _exportService.ExportDocumentRegisterAsync(filter);

        public Task<byte[]> ExportComplianceReportAsync()
            => _exportService.ExportComplianceReportAsync();
    }
}
