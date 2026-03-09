using InTagRepositoryLayer.Common;

namespace InTagLogicLayer.Manufacturing
{
    /// <summary>
    /// Format: PO-YYYYMM-NNNNN (e.g. PO-202603-00042)
    /// </summary>
    public static class ProductionOrderNumberGenerator
    {
        public static async Task<string> GenerateAsync(IUnitOfWork uow)
        {
            var prefix = $"PO-{DateTimeOffset.UtcNow:yyyyMM}-";
            var existing = await uow.ProductionOrders.FindAsync(
                o => o.OrderNumber.StartsWith(prefix));

            var maxSeq = 0;
            foreach (var o in existing)
            {
                var numPart = o.OrderNumber.Replace(prefix, "");
                if (int.TryParse(numPart, out var seq) && seq > maxSeq)
                    maxSeq = seq;
            }

            return $"{prefix}{(maxSeq + 1):D5}";
        }
    }

    public static class LotNumberGenerator
    {
        public static async Task<string> GenerateAsync(IUnitOfWork uow)
        {
            var prefix = $"LOT-{DateTimeOffset.UtcNow:yyyyMM}-";
            var existing = await uow.LotBatches.FindAsync(
                l => l.LotNumber.StartsWith(prefix));

            var maxSeq = 0;
            foreach (var l in existing)
            {
                var numPart = l.LotNumber.Replace(prefix, "");
                if (int.TryParse(numPart, out var seq) && seq > maxSeq)
                    maxSeq = seq;
            }

            return $"{prefix}{(maxSeq + 1):D5}";
        }
    }

    public static class BOMCodeGenerator
    {
        public static async Task<string> GenerateAsync(string productCode, IUnitOfWork uow)
        {
            var prefix = $"BOM-{productCode}-";
            var existing = await uow.BillOfMaterials.FindAsync(
                b => b.BOMCode.StartsWith(prefix));

            var maxSeq = 0;
            foreach (var b in existing)
            {
                var numPart = b.BOMCode.Replace(prefix, "");
                if (int.TryParse(numPart, out var seq) && seq > maxSeq)
                    maxSeq = seq;
            }

            return $"{prefix}{(maxSeq + 1):D3}";
        }
    }

    public static class RoutingCodeGenerator
    {
        public static async Task<string> GenerateAsync(string productCode, IUnitOfWork uow)
        {
            var prefix = $"RTG-{productCode}-";
            var existing = await uow.Routings.FindAsync(
                r => r.RoutingCode.StartsWith(prefix));

            var maxSeq = 0;
            foreach (var r in existing)
            {
                var numPart = r.RoutingCode.Replace(prefix, "");
                if (int.TryParse(numPart, out var seq) && seq > maxSeq)
                    maxSeq = seq;
            }

            return $"{prefix}{(maxSeq + 1):D3}";
        }
    }
}
