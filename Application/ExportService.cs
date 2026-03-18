using ProjectHK251_Reworked.Domain.AppException;
using ProjectHK251_Reworked.Domain.DataTransferObj;
using ProjectHK251_Reworked.Infrastructure;

namespace ProjectHK251_Reworked.Application
{
    public sealed class ExportService
    {
        private readonly ProductRepository _productRepository;
        private readonly BatchRepository _batchRepository;
        private readonly MovementRepository _movementRepository;

        public ExportService(
            ProductRepository productRepository,
            BatchRepository batchRepository,
            MovementRepository movementRepository)
        {
            _productRepository = productRepository;
            _batchRepository = batchRepository;
            _movementRepository = movementRepository;
        }

        public async Task ExportFromWarehouse(ExportRequest request, DbSession session)
        {
            if (string.IsNullOrWhiteSpace(request.ReferenceNo))
                throw new BusinessException("ReferenceNo is required.");

            if (request.Items == null || request.Items.Count == 0)
                throw new BusinessException("Items must not be empty.");

            var requestId = Guid.NewGuid().ToString();

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                    throw new BusinessException("Quantity must be greater than zero.");

                var product = await _productRepository.getProductBySKU(item.Sku, session);
                if (product == null)
                    throw new BusinessException($"Product not found for SKU: {item.Sku}");

                var batch = await _batchRepository.findBatchByBatchCodeAndProduct(item.BatchCode, product.Id, session);
                if (batch == null)
                    throw new BusinessException($"Batch not found for SKU {item.Sku} and batch {item.BatchCode}");

                await _movementRepository.Export(batch.Id, item.Quantity, request.ReferenceNo, requestId);
            }
        }
    }
}
