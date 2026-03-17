using InTagRepositoryLayer.Common;

namespace InTagLogicLayer.Workflow
{
    public static class WorkflowNumberGenerator
    {
        public static async Task<string> GenerateAsync(IUnitOfWork uow)
        {
            var prefix = $"WF-{DateTimeOffset.UtcNow:yyyyMM}-";
            var existing = await uow.WorkflowInstances.FindAsync(
                i => i.InstanceNumber.StartsWith(prefix));

            var maxSeq = 0;
            foreach (var i in existing)
            {
                var numPart = i.InstanceNumber.Replace(prefix, "");
                if (int.TryParse(numPart, out var seq) && seq > maxSeq)
                    maxSeq = seq;
            }
            return $"{prefix}{(maxSeq + 1):D5}";
        }
    }
}
