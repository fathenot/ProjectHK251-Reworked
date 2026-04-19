namespace ProjectHK251_Reworked.Domain.DataTransferObj
{
    public class InventoryMovementResults
    {
        public long Id { get; init; }

        public long BatchId { get; init; }

        public int Quantity { get; init; }

        public required String MovementType { get; init; }

        public DateTime CreatedAt { get; init; }

        public required string ProductNameSnapshot { get; init; }

        public required string BatchCodeSnapshot { get; init; }

        public required string RequestDocument { get; init; }


        /// <summary>
        /// generate results from history of movement
        /// </summary>
        /// <param name="movement"></param>
        public InventoryMovementResults(InventoryMovement movement)
        {
            this.Id = movement.Id;
            this.BatchId = movement.BatchId;
            this.Quantity = Math.Abs(movement.Quantity);
            this.CreatedAt = movement.CreatedAt;
            this.ProductNameSnapshot = movement.ProductNameSnapshot;
            this.BatchCodeSnapshot = movement.BatchCodeSnapshot;
            this.RequestDocument = movement.RequestDocument;

            // generate quantity
            switch (movement.MovementType)
            {
                // You need to complete the switch cases here as appropriate.
                case Domain.MovementType.Import:
                    this.MovementType = "Import";
                    break;
                case Domain.MovementType.Export:
                    this.MovementType = "Export";
                    break;
                case Domain.MovementType.Adjustment:
                    this.MovementType = "Adjustment";
                    break;
                 default:
                    throw new InvalidDataException("invalid movement type");
            }
        }

    }
}
