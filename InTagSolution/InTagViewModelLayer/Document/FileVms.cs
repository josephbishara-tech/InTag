namespace InTagViewModelLayer.Document
{
    public class FileUploadResultVm
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public long FileSize { get; set; }
        public string Hash { get; set; } = null!;
        public bool ContentIndexed { get; set; }
    }

    public class FileDownloadVm
    {
        public Stream Content { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
    }

    public class DocumentSearchResultVm
    {
        public IReadOnlyList<DocumentSearchHitVm> Hits { get; set; } = new List<DocumentSearchHitVm>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class DocumentSearchHitVm
    {
        public int DocumentId { get; set; }
        public string DocNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Snippet { get; set; }
        public string FileName { get; set; } = null!;
        public string MatchSource { get; set; } = null!; // "metadata" or "content"
    }
}
