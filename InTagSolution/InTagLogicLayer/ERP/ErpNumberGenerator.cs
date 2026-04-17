using InTagRepositoryLayer.Common;
using Microsoft.EntityFrameworkCore;

namespace InTagLogicLayer.ERP
{
    public static class ErpNumberGenerator
    {
        public static async Task<string> GenerateAsync(string prefix, IUnitOfWork uow,
            Func<IQueryable<string>> numberQuery)
        {
            var datePrefix = $"{prefix}-{DateTimeOffset.UtcNow:yyyyMM}-";
            var existingNumbers = await numberQuery()
                .Where(n => n.StartsWith(datePrefix))
                .ToListAsync();

            var maxSeq = 0;
            foreach (var num in existingNumbers)
            {
                var seqPart = num.Replace(datePrefix, "");
                if (int.TryParse(seqPart, out var seq) && seq > maxSeq)
                    maxSeq = seq;
            }

            return $"{datePrefix}{(maxSeq + 1):D5}";
        }
    }
}
