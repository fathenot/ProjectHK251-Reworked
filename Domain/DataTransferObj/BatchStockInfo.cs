namespace ProjectHK251_Reworked.Domain.DataTransferObj
{
    public class BatchStockInfo
    {
        public long BatchId { get; init; }

        public string ProductName { get; init; } = default!;

        public string Sku { get; init; } = default!;

        public string BatchCode { get; init; } = default!;

        public int CurrentQuantity { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? LastMovementAt { get; init; }
    }
}
