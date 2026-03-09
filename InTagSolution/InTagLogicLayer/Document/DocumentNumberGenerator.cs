using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;

namespace InTagLogicLayer.Document
{
    /// <summary>
    /// Generates ISO-compliant document numbers.
    /// Format: {TypePrefix}-{CategoryPrefix}-{SequentialNumber}
    /// Example: SOP-QA-00042, WI-OPS-00105, CTR-FIN-00003
    /// </summary>
    public static class DocumentNumberGenerator
    {
        private static readonly Dictionary<DocumentType, string> TypePrefixes = new()
        {
            [DocumentType.SOP] = "SOP",
            [DocumentType.WorkInstruction] = "WI",
            [DocumentType.Contract] = "CTR",
            [DocumentType.EngineeringDrawing] = "DWG",
            [DocumentType.QualityRecord] = "QR",
            [DocumentType.Form] = "FRM",
            [DocumentType.Template] = "TPL",
            [DocumentType.Policy] = "POL",
            [DocumentType.Manual] = "MAN",
            [DocumentType.Report] = "RPT"
        };

        private static readonly Dictionary<DocumentCategory, string> CategoryPrefixes = new()
        {
            [DocumentCategory.Quality] = "QA",
            [DocumentCategory.Safety] = "SAF",
            [DocumentCategory.Operations] = "OPS",
            [DocumentCategory.Engineering] = "ENG",
            [DocumentCategory.Maintenance] = "MNT",
            [DocumentCategory.HR] = "HR",
            [DocumentCategory.Finance] = "FIN",
            [DocumentCategory.Regulatory] = "REG",
            [DocumentCategory.General] = "GEN"
        };

        public static async Task<string> GenerateAsync(
            DocumentType type,
            DocumentCategory category,
            IUnitOfWork uow)
        {
            var typePrefix = TypePrefixes.GetValueOrDefault(type, "DOC");
            var catPrefix = CategoryPrefixes.GetValueOrDefault(category, "GEN");
            var prefix = $"{typePrefix}-{catPrefix}-";

            // Find the highest existing number with this prefix
            var existingDocs = await uow.Documents.FindAsync(
                d => d.DocNumber.StartsWith(prefix));

            var maxSequence = 0;
            foreach (var doc in existingDocs)
            {
                var numPart = doc.DocNumber.Replace(prefix, "");
                if (int.TryParse(numPart, out var seq) && seq > maxSequence)
                    maxSequence = seq;
            }

            return $"{prefix}{(maxSequence + 1):D5}";
        }

        /// <summary>
        /// Increments revision number: "1.0" → "1.1", "1.9" → "2.0"
        /// </summary>
        public static string IncrementVersion(string currentVersion)
        {
            if (string.IsNullOrEmpty(currentVersion)) return "1.0";

            var parts = currentVersion.Split('.');
            if (parts.Length != 2 || !int.TryParse(parts[0], out var major) || !int.TryParse(parts[1], out var minor))
                return "1.0";

            minor++;
            if (minor > 9)
            {
                major++;
                minor = 0;
            }

            return $"{major}.{minor}";
        }
    }
}
