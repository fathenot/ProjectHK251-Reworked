namespace ProjectHK251_Reworked.Domain.DataTransferObj
{
    public sealed class ImportRequest
    {
        public required string ReferenceNo { get; init; }     // mã chứng từ nhập
        public DateTime ImportDate { get; init; }
        public required List<ImportItem> Items { get; init; }
    }

    public sealed class ImportItem
    {
        public required string Sku { get; init; }
        public required string BatchCode { get; init; }
        public required int Quantity { get; init; }
        public required string ItemName { get; init; }

        public Product ConvertToProduct()
        {
            return new Product
            {
                Name = this.ItemName,
                Sku = this.Sku
            };
        }
    }
}
