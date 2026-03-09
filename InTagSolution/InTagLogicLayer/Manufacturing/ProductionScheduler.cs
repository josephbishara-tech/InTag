using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Manufacturing;

namespace InTagLogicLayer.Manufacturing
{
    public class ProductionScheduler
    {
        private readonly IUnitOfWork _uow;

        public ProductionScheduler(IUnitOfWork uow) { _uow = uow; }

        /// <summary>
        /// Forward-schedules a production order from a given start date,
        /// assigning each routing operation to its work center with capacity checks.
        /// </summary>
        public async Task<ScheduleResultVm> ScheduleOrderAsync(int orderId, DateTimeOffset startDate)
        {
            var order = await _uow.ProductionOrders.Query()
                .Include(o => o.Routing).ThenInclude(r => r!.Operations.OrderBy(op => op.Sequence))
                    .ThenInclude(op => op.WorkCenter)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new KeyNotFoundException("Order not found.");
            if (order.Routing == null) throw new InvalidOperationException("Order has no routing assigned.");

            var operations = order.Routing.Operations.OrderBy(o => o.Sequence).ToList();
            var scheduledOps = new List<ScheduledOperationVm>();
            var currentDate = startDate;

            foreach (var op in operations)
            {
                var totalMinutes = op.SetupTimeMinutes + (op.RunTimePerUnitMinutes * order.PlannedQuantity);
                var hoursNeeded = totalMinutes / 60;
                var availablePerDay = op.WorkCenter.CapacityHoursPerDay * op.WorkCenter.NumberOfStations;

                if (availablePerDay <= 0) availablePerDay = 8; // fallback

                var daysNeeded = Math.Ceiling(hoursNeeded / availablePerDay);
                var opStart = currentDate;
                var opEnd = opStart.AddDays((double)daysNeeded);

                // Check for existing load on this work center
                var existingLoad = await GetWorkCenterLoadAsync(op.WorkCenterId, opStart, opEnd);
                var utilization = availablePerDay > 0
                    ? Math.Round((existingLoad + hoursNeeded) / ((decimal)daysNeeded * availablePerDay) * 100, 1)
                    : 0;

                scheduledOps.Add(new ScheduledOperationVm
                {
                    Sequence = op.Sequence,
                    OperationName = op.OperationName,
                    WorkCenterCode = op.WorkCenter.Code,
                    WorkCenterName = op.WorkCenter.Name,
                    StartDate = opStart,
                    EndDate = opEnd,
                    TotalMinutes = totalMinutes,
                    DaysRequired = (int)daysNeeded,
                    WorkCenterUtilization = utilization,
                    IsOverloaded = utilization > 100
                });

                // Next operation starts after this one (minus overlap)
                if (op.OverlapQuantity > 0 && op.RunTimePerUnitMinutes > 0)
                {
                    var overlapTime = op.OverlapQuantity * op.RunTimePerUnitMinutes / 60;
                    currentDate = opEnd.AddHours(-(double)overlapTime);
                }
                else
                {
                    currentDate = opEnd;
                }
            }

            // Update order dates
            order.PlannedStartDate = startDate;
            order.PlannedEndDate = scheduledOps.Any() ? scheduledOps.Last().EndDate : startDate;
            _uow.ProductionOrders.Update(order);
            await _uow.SaveChangesAsync();

            return new ScheduleResultVm
            {
                OrderNumber = order.OrderNumber,
                PlannedStart = startDate,
                PlannedEnd = order.PlannedEndDate!.Value,
                TotalDays = (int)(order.PlannedEndDate!.Value - startDate).TotalDays,
                Operations = scheduledOps,
                HasOverloadedWorkCenters = scheduledOps.Any(o => o.IsOverloaded)
            };
        }

        /// <summary>
        /// Returns work center capacity utilization for a date range.
        /// </summary>
        public async Task<IReadOnlyList<WorkCenterCapacityVm>> GetCapacityOverviewAsync(
            DateTimeOffset from, DateTimeOffset to)
        {
            var workCenters = await _uow.WorkCenters.Query()
                .Where(w => w.Status == WorkCenterStatus.Active)
                .ToListAsync();

            var result = new List<WorkCenterCapacityVm>();

            foreach (var wc in workCenters)
            {
                var totalDays = Math.Max(1, (int)(to - from).TotalDays);
                var totalCapacity = wc.CapacityHoursPerDay * wc.NumberOfStations * totalDays;
                var loadedHours = await GetWorkCenterLoadAsync(wc.Id, from, to);
                var utilization = totalCapacity > 0 ? Math.Round(loadedHours / totalCapacity * 100, 1) : 0;

                result.Add(new WorkCenterCapacityVm
                {
                    WorkCenterId = wc.Id,
                    Code = wc.Code,
                    Name = wc.Name,
                    TotalCapacityHours = totalCapacity,
                    LoadedHours = loadedHours,
                    AvailableHours = totalCapacity - loadedHours,
                    Utilization = utilization,
                    Status = wc.Status.ToString()
                });
            }

            return result.OrderByDescending(c => c.Utilization).ToList();
        }

        private async Task<decimal> GetWorkCenterLoadAsync(
            int workCenterId, DateTimeOffset from, DateTimeOffset to)
        {
            // Sum actual logged time + scheduled remaining time for active orders
            var loggedHours = await _uow.ProductionLogs.Query()
                .Where(l => l.WorkCenterId == workCenterId
                            && l.LogDate >= from && l.LogDate <= to)
                .SumAsync(l => (l.RunTimeActual ?? 0) + (l.SetupTimeActual ?? 0)) / 60;

            // Planned but not yet started operations
            var scheduledMinutes = await _uow.RoutingOperations.Query()
                .Where(o => o.WorkCenterId == workCenterId)
                .Join(_uow.ProductionOrders.Query()
                    .Where(p => (p.Status == ProductionOrderStatus.Released || p.Status == ProductionOrderStatus.Planned)
                                && p.PlannedStartDate >= from && p.PlannedStartDate <= to),
                    o => o.Routing.ProductId,
                    p => p.ProductId,
                    (o, p) => o.SetupTimeMinutes + o.RunTimePerUnitMinutes * p.PlannedQuantity
                ).SumAsync(m => m);

            return loggedHours + scheduledMinutes / 60;
        }
    }
}
