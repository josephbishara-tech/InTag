using Microsoft.EntityFrameworkCore;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Manufacturing;

namespace InTagLogicLayer.Manufacturing
{
    public class ProductionCostEngine
    {
        private readonly IUnitOfWork _uow;

        public ProductionCostEngine(IUnitOfWork uow) { _uow = uow; }

        public async Task<ProductionCostResultVm> CalculateAsync(int orderId)
        {
            var order = await _uow.ProductionOrders.Query()
                .Include(o => o.Product)
                .Include(o => o.BOM).ThenInclude(b => b!.Lines).ThenInclude(l => l.ComponentProduct)
                .Include(o => o.Routing).ThenInclude(r => r!.Operations).ThenInclude(op => op.WorkCenter)
                .Include(o => o.ProductionLogs)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new KeyNotFoundException("Order not found.");

            // Material cost from BOM
            var materialCost = 0m;
            var materialLines = new List<CostLineVm>();
            if (order.BOM != null)
            {
                foreach (var line in order.BOM.Lines)
                {
                    var qty = line.Quantity * order.PlannedQuantity * (1 + line.ScrapFactor / 100);
                    var cost = qty * line.ComponentProduct.StandardCost;
                    materialCost += cost;
                    materialLines.Add(new CostLineVm
                    {
                        Category = "Material",
                        Description = $"{line.ComponentProduct.ProductCode} — {line.ComponentProduct.Name}",
                        Quantity = qty,
                        UnitCost = line.ComponentProduct.StandardCost,
                        TotalCost = cost
                    });
                }
            }

            // Labor/overhead cost from routing
            var laborCost = 0m;
            var laborLines = new List<CostLineVm>();
            if (order.Routing != null)
            {
                foreach (var op in order.Routing.Operations)
                {
                    var totalMinutes = op.SetupTimeMinutes + (op.RunTimePerUnitMinutes * order.PlannedQuantity);
                    var totalHours = totalMinutes / 60;
                    var cost = totalHours * op.WorkCenter.CostPerHour;
                    laborCost += cost;
                    laborLines.Add(new CostLineVm
                    {
                        Category = "Labor/Overhead",
                        Description = $"Op {op.Sequence}: {op.OperationName} @ {op.WorkCenter.Name}",
                        Quantity = totalHours,
                        UnitCost = op.WorkCenter.CostPerHour,
                        TotalCost = cost
                    });
                }
            }

            // Actual cost from production logs
            var actualLaborHours = order.ProductionLogs.Sum(l => ((l.RunTimeActual ?? 0) + (l.SetupTimeActual ?? 0)) / 60);
            var actualLaborCost = 0m;
            foreach (var log in order.ProductionLogs)
            {
                if (log.WorkCenterId.HasValue)
                {
                    var wc = order.Routing?.Operations.FirstOrDefault(o => o.WorkCenterId == log.WorkCenterId)?.WorkCenter;
                    if (wc != null)
                        actualLaborCost += ((log.RunTimeActual ?? 0) + (log.SetupTimeActual ?? 0)) / 60 * wc.CostPerHour;
                }
            }

            var totalPlanned = materialCost + laborCost;
            var goodUnits = order.CompletedQuantity - order.ScrapQuantity;
            var unitCostPlanned = order.PlannedQuantity > 0 ? totalPlanned / order.PlannedQuantity : 0;
            var unitCostActual = goodUnits > 0 ? (materialCost + actualLaborCost) / goodUnits : 0;

            return new ProductionCostResultVm
            {
                OrderNumber = order.OrderNumber,
                ProductName = order.Product.Name,
                PlannedQuantity = order.PlannedQuantity,
                CompletedQuantity = order.CompletedQuantity,
                ScrapQuantity = order.ScrapQuantity,
                GoodUnits = goodUnits,
                MaterialCost = materialCost,
                LaborCost = laborCost,
                TotalPlannedCost = totalPlanned,
                ActualLaborCost = actualLaborCost,
                TotalActualCost = materialCost + actualLaborCost,
                PlannedUnitCost = Math.Round(unitCostPlanned, 2),
                ActualUnitCost = Math.Round(unitCostActual, 2),
                CostVariance = Math.Round((materialCost + actualLaborCost) - totalPlanned, 2),
                MaterialLines = materialLines,
                LaborLines = laborLines
            };
        }
    }
}
