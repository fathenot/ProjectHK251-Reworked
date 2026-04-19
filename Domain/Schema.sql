-- Schema derived from Domain entities (Product, Batch, InventoryMovement)
-- Target: SQL Server

CREATE TABLE product (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    sku NVARCHAR(100) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2(6) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT uk_product_sku UNIQUE (sku)
);

CREATE TABLE batch (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    product_id BIGINT NOT NULL,
    batch_code NVARCHAR(100) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2(6) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT fk_batch_product FOREIGN KEY (product_id)
        REFERENCES product(id),
    CONSTRAINT uk_batch_product_code UNIQUE (product_id, batch_code)
);

CREATE TABLE inventory_movement (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    batch_id BIGINT NOT NULL,
    quantity INT NOT NULL,
    movement_type TINYINT NOT NULL,
    created_at DATETIME2(6) NOT NULL DEFAULT SYSUTCDATETIME(),
    product_name_snapshot NVARCHAR(255) NOT NULL,
    batch_code_snapshot NVARCHAR(100) NOT NULL,
    request_document NVARCHAR(100) NOT NULL,
    request_id CHAR(36) NOT NULL,
    CONSTRAINT fk_movement_batch FOREIGN KEY (batch_id)
        REFERENCES batch(id),
    CONSTRAINT chk_movement_type CHECK (movement_type IN (1, 2, 3)),
    CONSTRAINT uk_movement_idempotency UNIQUE (request_id, movement_type, batch_id)
);

CREATE INDEX idx_batch_product_id ON batch(product_id);
CREATE INDEX idx_movement_created_at ON inventory_movement(created_at);
CREATE INDEX idx_movement_batch_created ON inventory_movement(batch_id, created_at);
