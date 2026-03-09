using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Manufacturing;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Manufacturing;

namespace InTagLogicLayer.Manufacturing
{
    /// <summary>
    /// Multi-level BOM explosion: recursively resolves phantom assemblies
    /// and calculates total material requirements for a given quantity.
    /// </summary>
    public class BOMExplosionEngine
    {
        private readonly IUnitOfWork _uow;

        public BOMExplosionEngine(IUnitOfWork uow) { _uow = uow; }

        public async Task<BOMExplosionResultVm> ExplodeAsync(int bomId, decimal orderQuantity, int maxDepth = 10)
        {
            var visited = new HashSet<int>(); // circular reference guard
            var flatLines = new List<BOMExplosionLineVm>();
            var tree = new List<BOMExplosionNodeVm>();

            var bom = await _uow.BillOfMaterials.Query()
                .Include(b => b.Product)
                .Include(b => b.Lines).ThenInclude(l => l.ComponentProduct).ThenInclude(p => p.BOMs).ThenInclude(b => b.Lines)
                .FirstOrDefaultAsync(b => b.Id == bomId);

            if (bom == null) throw new KeyNotFoundException("BOM not found.");

            await ExplodeRecursive(bom, orderQuantity / bom.OutputQuantity, 0, maxDepth, visited, flatLines, tree);

            // Consolidate flat list (sum quantities by component)
            var consolidated = flatLines
                .GroupBy(l => l.ProductId)
                .Select(g => new BOMExplosionLineVm
                {
                    ProductId = g.Key,
                    ProductCode = g.First().ProductCode,
                    ProductName = g.First().ProductName,
                    UOM = g.First().UOM,
                    TotalQuantity = g.Sum(l => l.TotalQuantity),
                    TotalQuantityWithScrap = g.Sum(l => l.TotalQuantityWithScrap),
                    UnitCost = g.First().UnitCost,
                    TotalCost = g.Sum(l => l.TotalCost),
                    IsRawMaterial = g.First().IsRawMaterial,
                    Level = g.Min(l => l.Level)
                })
                .OrderBy(l => l.ProductCode)
                .ToList();

            return new BOMExplosionResultVm
            {
                BOMCode = bom.BOMCode,
                ProductName = bom.Product.Name,
                OrderQuantity = orderQuantity,
                FlatRequirements = consolidated,
                Tree = tree,
                TotalMaterialCost = consolidated.Sum(l => l.TotalCost),
                UniqueComponents = consolidated.Count,
                MaxDepth = tree.Any() ? GetMaxTreeDepth(tree) : 0
            };
        }

        private async Task ExplodeRecursive(
            BillOfMaterial bom,
            decimal multiplier,
            int level,
            int maxDepth,
            HashSet<int> visited,
            List<BOMExplosionLineVm> flatLines,
            List<BOMExplosionNodeVm> treeNodes)
        {
            if (level > maxDepth) return;
            if (!visited.Add(bom.Id))
                throw new InvalidOperationException($"Circular BOM reference detected at {bom.BOMCode}.");

            foreach (var line in bom.Lines.Where(l => l.IsActive))
            {
                var qty = line.Quantity * multiplier;
                var qtyWithScrap = qty * (1 + line.ScrapFactor / 100);

                var node = new BOMExplosionNodeVm
                {
                    ProductId = line.ComponentProductId,
                    ProductCode = line.ComponentProduct.ProductCode,
                    ProductName = line.ComponentProduct.Name,
                    Quantity = qty,
                    QuantityWithScrap = qtyWithScrap,
                    UOM = line.UOM.ToString(),
                    Level = level,
                    IsPhantom = line.IsPhantom,
                    UnitCost = line.ComponentProduct.StandardCost,
                    Children = new List<BOMExplosionNodeVm>()
                };

                // If phantom, explode sub-BOM
                if (line.IsPhantom)
                {
                    var subBom = line.ComponentProduct.BOMs
                        .FirstOrDefault(b => b.Status == BOMStatus.Active);

                    if (subBom != null)
                    {
                        var subBomFull = await _uow.BillOfMaterials.Query()
                            .Include(b => b.Product)
                            .Include(b => b.Lines).ThenInclude(l => l.ComponentProduct).ThenInclude(p => p.BOMs).ThenInclude(b => b.Lines)
                            .FirstOrDefaultAsync(b => b.Id == subBom.Id);

                        if (subBomFull != null)
                        {
                            await ExplodeRecursive(subBomFull, qtyWithScrap / subBomFull.OutputQuantity,
                                level + 1, maxDepth, visited, flatLines, node.Children);
                        }
                    }
                }
                else
                {
                    // Leaf component — add to flat requirements
                    flatLines.Add(new BOMExplosionLineVm
                    {
                        ProductId = line.ComponentProductId,
                        ProductCode = line.ComponentProduct.ProductCode,
                        ProductName = line.ComponentProduct.Name,
                        UOM = line.UOM.ToString(),
                        TotalQuantity = qty,
                        TotalQuantityWithScrap = qtyWithScrap,
                        UnitCost = line.ComponentProduct.StandardCost,
                        TotalCost = qtyWithScrap * line.ComponentProduct.StandardCost,
                        IsRawMaterial = line.ComponentProduct.IsRawMaterial,
                        Level = level
                    });
                }

                treeNodes.Add(node);
            }

            visited.Remove(bom.Id); // allow same BOM at different branches
        }

        private static int GetMaxTreeDepth(List<BOMExplosionNodeVm> nodes)
        {
            var max = nodes.Any() ? nodes.Max(n => n.Level) : 0;
            foreach (var node in nodes)
            {
                if (node.Children.Any())
                    max = Math.Max(max, GetMaxTreeDepth(node.Children));
            }
            return max;
        }
    }
}
