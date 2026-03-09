using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;

namespace InTagLogicLayer.Inventory
{
    public static class InventoryNumberGenerator
    {
        public static async Task<string> GenerateTransactionAsync(TransactionType type, IUnitOfWork uow)
        {
            var prefix = type switch
            {
                TransactionType.Receipt => "RCV",
                TransactionType.Issue => "ISS",
                TransactionType.Transfer => "TRF",
                TransactionType.Adjustment => "ADJ",
                TransactionType.CycleCount => "CC",
                TransactionType.Return => "RTN",
                TransactionType.Scrap => "SCP",
                TransactionType.ProductionConsumption => "PCN",
                TransactionType.ProductionOutput => "POT",
                _ => "TXN"
            };

            var full = $"{prefix}-{DateTimeOffset.UtcNow:yyyyMM}-";
            var existing = await uow.InventoryTransactions.FindAsync(
                t => t.TransactionNumber.StartsWith(full));

            var maxSeq = 0;
            foreach (var t in existing)
            {
                var numPart = t.TransactionNumber.Replace(full, "");
                if (int.TryParse(numPart, out var seq) && seq > maxSeq)
                    maxSeq = seq;
            }
            return $"{full}{(maxSeq + 1):D5}";
        }

        public static async Task<string> GenerateCycleCountAsync(IUnitOfWork uow)
        {
            var prefix = $"CC-{DateTimeOffset.UtcNow:yyyyMM}-";
            var existing = await uow.CycleCounts.FindAsync(c => c.CountNumber.StartsWith(prefix));

            var maxSeq = 0;
            foreach (var c in existing)
            {
                var numPart = c.CountNumber.Replace(prefix, "");
                if (int.TryParse(numPart, out var seq) && seq > maxSeq)
                    maxSeq = seq;
            }
            return $"{prefix}{(maxSeq + 1):D5}";
        }
    }
}
