using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.ERP
{
    public class Pricelist : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        public DateTimeOffset? ValidFrom { get; set; }

        public DateTimeOffset? ValidTo { get; set; }

        public bool IsDefault { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<PricelistLine> Lines { get; set; } = new List<PricelistLine>();
    }

    public class PricelistLine : BaseEntity
    {
        [Required]
        public int PricelistId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal MinQuantity { get; set; } = 1;

        public DateTimeOffset? ValidFrom { get; set; }

        public DateTimeOffset? ValidTo { get; set; }

        // Navigation
        public Pricelist Pricelist { get; set; } = null!;
    }
}
