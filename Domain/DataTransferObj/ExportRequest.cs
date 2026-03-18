namespace ProjectHK251_Reworked.Domain.DataTransferObj
{
    public sealed class ExportRequest
    {
        public required string ReferenceNo { get; init; }     // ma chung tu xuat
        public DateTime ExportDate { get; init; }
        public required List<ExportItem> Items { get; init; }
    }

    public sealed class ExportItem
    {
        public required string Sku { get; init; }
        public required string BatchCode { get; init; }
        public required int Quantity { get; init; }
        public required string ItemName { get; init; }
    }
}
