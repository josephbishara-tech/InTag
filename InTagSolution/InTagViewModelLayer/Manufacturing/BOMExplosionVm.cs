namespace InTagViewModelLayer.Manufacturing
{
    public class BOMExplosionResultVm
    {
        public string BOMCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal OrderQuantity { get; set; }
        public IReadOnlyList<BOMExplosionLineVm> FlatRequirements { get; set; } = new List<BOMExplosionLineVm>();
        public IReadOnlyList<BOMExplosionNodeVm> Tree { get; set; } = new List<BOMExplosionNodeVm>();
        public decimal TotalMaterialCost { get; set; }
        public int UniqueComponents { get; set; }
        public int MaxDepth { get; set; }
    }

    public class BOMExplosionLineVm
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string UOM { get; set; } = null!;
        public decimal TotalQuantity { get; set; }
        public decimal TotalQuantityWithScrap { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public bool IsRawMaterial { get; set; }
        public int Level { get; set; }
    }

    public class BOMExplosionNodeVm
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal QuantityWithScrap { get; set; }
        public string UOM { get; set; } = null!;
        public int Level { get; set; }
        public bool IsPhantom { get; set; }
        public decimal UnitCost { get; set; }
        public List<BOMExplosionNodeVm> Children { get; set; } = new();
    }
}
