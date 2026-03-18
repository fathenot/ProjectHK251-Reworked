namespace ProjectHK251_Reworked.Domain;

public sealed class Batch
{
    public long Id { get; init; }

    public required long ProductId { get; init; }

    public required string BatchCode { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
}
