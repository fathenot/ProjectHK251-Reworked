using ProjectHK251_Reworked.Domain;
using ProjectHK251_Reworked.Domain.DataTransferObj;
using ProjectHK251_Reworked.Infrastructure;
using ProjectHK251_Reworked.Domain.AppException;

namespace ProjectHK251_Reworked.Application
{
    public class ImportService
    {
        private readonly ProductRepository _productRepository;
        private readonly BatchRepository _batchRepository;
        private readonly MovementRepository _movementRepository;

        internal ImportService(ProductRepository productRepository, BatchRepository batchRepository, MovementRepository movementRepository)
        {
            _productRepository = productRepository;
            _batchRepository = batchRepository;
            _movementRepository = movementRepository;
        }

        public async Task<Product?> CreateProduct(Product product, DbSession? dbSession = null)
        {
            if (dbSession == null)
            {
                var session = new DbSession("");
                await session.OpenAsync();
                var result = _productRepository.InsertProduct(product, session);
                return await _productRepository.SearchById(result.Result, session);
            }
            else
            {
                var result = await _productRepository.InsertProduct(product, dbSession);
                return await _productRepository.SearchById(result, dbSession);
            }
        }

        public async Task ImportToWarehouse(ImportRequest request, DbSession session, string requestId)
        {
            if (string.IsNullOrWhiteSpace(request.ReferenceNo))
                throw new BusinessException("ReferenceNo is required.");

            if (request.Items == null || request.Items.Count == 0)
                throw new BusinessException("Items must not be empty.");

            if (string.IsNullOrWhiteSpace(requestId))
                throw new BusinessException("RequestId is required.");

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                    throw new BusinessException("Quantity must be greater than zero.");

                var product = await _productRepository.getProductBySKU(item.Sku, session);
                long id;
                if (product == null)
                {
                    id = await _productRepository.InsertProduct(item.ConvertToProduct(), session);
                }
                else
                {
                    id = product.Id;
                }

                var importingBatch = await _batchRepository.findBatchByBatchCodeAndProduct(item.BatchCode, id, session);
                if (importingBatch == null)
                {
                    var newBatch = new Batch { BatchCode = item.BatchCode, ProductId = id };
                    var batchId = await _batchRepository.InsertBatch(newBatch, session);
                    await _movementRepository.Import(batchId, item.Quantity, request.ReferenceNo, requestId);
                }
                else
                {
                    await _movementRepository.Import(importingBatch.Id, item.Quantity, request.ReferenceNo, requestId);
                }
            }
        }
    }
}
