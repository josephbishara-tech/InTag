namespace InTagEntitiesLayer.Enums
{
    public enum TransactionType
    {
        Receipt = 0,
        Issue = 1,
        Transfer = 2,
        Adjustment = 3,
        CycleCount = 4,
        Return = 5,
        Scrap = 6,
        ProductionConsumption = 7,
        ProductionOutput = 8
    }

    public enum ValuationMethod
    {
        WeightedAverage = 0,
        FIFO = 1,
        LIFO = 2,
        StandardCost = 3
    }

    public enum StockStatus
    {
        Available = 0,
        Reserved = 1,
        Quarantine = 2,
        InTransit = 3,
        Damaged = 4
    }

    public enum ReorderStatus
    {
        Normal = 0,
        BelowReorderPoint = 1,
        BelowMinimum = 2,
        OutOfStock = 3,
        Overstock = 4
    }
}
