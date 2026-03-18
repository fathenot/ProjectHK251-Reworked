namespace ProjectHK251_Reworked.Domain
{
    public class Product
    {
        public long Id { get; init; }

        public required string Name { get; init; }

        public required string Sku { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}
