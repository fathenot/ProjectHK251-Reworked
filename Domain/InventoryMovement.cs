namespace ProjectHK251_Reworked.Domain
{
    public sealed class InventoryMovement
    {
        public long Id { get; init; }

        public long BatchId { get; init; }

        public int Quantity { get; init; }

        public MovementType MovementType { get; init; }

        public DateTime CreatedAt { get; init; }

        public required string ProductNameSnapshot { get; init; }

        public required string BatchCodeSnapshot { get; init; }

        public required string RequestDocument {  get; init; }

        public string RequestId { get; init; } = "";
    }
}
