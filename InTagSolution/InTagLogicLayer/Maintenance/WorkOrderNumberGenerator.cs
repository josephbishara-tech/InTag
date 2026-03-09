using InTagRepositoryLayer.Common;

namespace InTagLogicLayer.Maintenance
{
    /// <summary>
    /// Format: WO-YYYYMM-NNNNN (e.g. WO-202603-00042)
    /// </summary>
    public static class WorkOrderNumberGenerator
    {
        public static async Task<string> GenerateAsync(IUnitOfWork uow)
        {
            var prefix = $"WO-{DateTimeOffset.UtcNow:yyyyMM}-";
            var existing = await uow.WorkOrders.FindAsync(
                w => w.WorkOrderNumber.StartsWith(prefix));

            var maxSeq = 0;
            foreach (var w in existing)
            {
                var numPart = w.WorkOrderNumber.Replace(prefix, "");
                if (int.TryParse(numPart, out var seq) && seq > maxSeq)
                    maxSeq = seq;
            }

            return $"{prefix}{(maxSeq + 1):D5}";
        }
    }
}
