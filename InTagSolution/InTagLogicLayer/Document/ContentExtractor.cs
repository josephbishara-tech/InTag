using System.Text;

namespace InTagLogicLayer.Document
{
    /// <summary>
    /// Extracts text content from uploaded files for full-text search indexing.
    /// Supports plain text and basic extraction. For production, integrate
    /// Apache Tika or Azure Cognitive Services for PDF/DOCX extraction.
    /// </summary>
    public static class ContentExtractor
    {
        private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".csv", ".md", ".json", ".xml", ".html", ".htm", ".log", ".yaml", ".yml"
        };

        /// <summary>
        /// Extract searchable text content from a file stream.
        /// Returns null if extraction is not supported for the file type.
        /// </summary>
        public static async Task<string?> ExtractAsync(Stream stream, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (TextExtensions.Contains(ext))
            {
                return await ExtractPlainTextAsync(stream);
            }

            // For PDF/DOCX: placeholder — integrate iTextSharp, DocumentFormat.OpenXml,
            // or Azure AI Document Intelligence in production
            if (ext == ".pdf")
            {
                return await ExtractPdfPlaceholderAsync(stream);
            }

            return null;
        }

        private static async Task<string?> ExtractPlainTextAsync(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var content = await reader.ReadToEndAsync();

            // Truncate for indexing (max 50KB of text)
            if (content.Length > 50_000)
                content = content[..50_000];

            stream.Position = 0;
            return content;
        }

        private static Task<string?> ExtractPdfPlaceholderAsync(Stream stream)
        {
            // TODO: Integrate PDF text extraction library
            // Options: iTextSharp, PdfPig, Azure AI Document Intelligence
            stream.Position = 0;
            return Task.FromResult<string?>(null);
        }
    }
}
