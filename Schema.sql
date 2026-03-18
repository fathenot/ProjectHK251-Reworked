CREATE TABLE product (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    sku VARCHAR(100) UNIQUE NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    CONSTRAINT uk_product_sku UNIQUE (sku)
) ENGINE=InnoDB;

CREATE TABLE batch (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    batch_code VARCHAR(100) NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    current_quantity INT NOT NULL DEFAULT 0,
    CONSTRAINT fk_batch_product
        FOREIGN KEY (product_id)
        REFERENCES product(id)
        ON DELETE RESTRICT,

    CONSTRAINT uk_batch_product_code
        UNIQUE (product_id, batch_code)
) ENGINE=InnoDB;

CREATE TABLE inventory_movement (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    batch_id BIGINT UNSIGNED NOT NULL,

    quantity INT NOT NULL,
    movement_type TINYINT UNSIGNED NOT NULL,

    product_name_snapshot VARCHAR(255) NOT NULL,
    batch_code_snapshot VARCHAR(100) NOT NULL,
    reference_doc_id VARCHAR(100) NOT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    request_id CHAR(36) NOT NULL,
    CONSTRAINT fk_movement_batch
        FOREIGN KEY (batch_id)
        REFERENCES batch(id)
        ON DELETE RESTRICT,

    CONSTRAINT chk_movement_type
    CHECK (movement_type IN (1, 2)),
	
	CONSTRAINT chk_req
	UNIQUE(request_id, movement_type, batch_id)
) ENGINE=InnoDB;

CREATE TABLE inventory_document (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    reference_doc_id VARCHAR(100) NOT NULL,
    document_type TINYINT UNSIGNED NOT NULL, -- 1=IMPORT,2=EXPORT
    request_id CHAR(36) NOT NULL,
    status TINYINT UNSIGNED NOT NULL DEFAULT 1, -- 1=POSTED

    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    CONSTRAINT uk_document_request UNIQUE (request_id)
);

CREATE TABLE inventory_document_line (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    document_id BIGINT UNSIGNED NOT NULL,
    batch_id BIGINT UNSIGNED NOT NULL,
    quantity INT NOT NULL,

    CONSTRAINT fk_line_document
        FOREIGN KEY (document_id) REFERENCES inventory_document(id),

    CONSTRAINT fk_line_batch
        FOREIGN KEY (batch_id) REFERENCES batch(id)
);

ALTER TABLE batch
ADD CONSTRAINT chk_non_negative_quantity
CHECK (current_quantity >= 0);

CREATE INDEX idx_movement_created_at 
ON inventory_movement(created_at);

CREATE INDEX idx_batch_product_id 
ON batch(product_id);

CREATE INDEX idx_movement_batch_created ON inventory_movement(batch_id, created_at);

DELIMITER //
CREATE PROCEDURE sp_create_batch()

DELIMITER //

DELIMITER //

CREATE PROCEDURE sp_import_inventory (
    IN p_batch_id BIGINT UNSIGNED,
    IN p_quantity INT,
    IN p_reference_doc_id VARCHAR(100),
    IN p_request_id CHAR(36)
)
BEGIN
    DECLARE v_product_name VARCHAR(255);
    DECLARE v_batch_code VARCHAR(100);

    IF p_quantity <= 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Quantity must be greater than zero';
    END IF;

    -- Lock batch row
    SELECT p.name, b.batch_code
    INTO v_product_name, v_batch_code
    FROM batch b
    JOIN product p ON p.id = b.product_id
    WHERE b.id = p_batch_id
      AND b.is_active = 1
    FOR UPDATE;

    IF v_product_name IS NULL THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Batch not found or inactive';
    END IF;

    -- Update snapshot balance
    UPDATE batch
    SET current_quantity = current_quantity + p_quantity
    WHERE id = p_batch_id;

    -- Insert ledger
    INSERT INTO inventory_movement (
        batch_id,
        quantity,
        movement_type,
        reference_doc_id,
        product_name_snapshot,
        batch_code_snapshot,
        request_id,
        created_at
    )
    VALUES (
        p_batch_id,
        p_quantity,
        1,
        p_reference_doc_id,
        v_product_name,
        v_batch_code,
        p_request_id,
        NOW(6)
    );

END //

DELIMITER ;

DELIMITER //

CREATE PROCEDURE sp_export_inventory(
    IN p_batch_id BIGINT UNSIGNED,
    IN p_quantity INT,
    IN p_reference_doc_id VARCHAR(100),
    IN p_request_id CHAR(36)
)
BEGIN
    DECLARE v_product_name VARCHAR(255);
    DECLARE v_batch_code VARCHAR(100);

    IF p_quantity <= 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Quantity must be greater than zero';
    END IF;

    -- Lock & snapshot
    SELECT p.name, b.batch_code
    INTO v_product_name, v_batch_code
    FROM batch b
    JOIN product p ON p.id = b.product_id
    WHERE b.id = p_batch_id
      AND b.is_active = 1
    FOR UPDATE;

    IF v_product_name IS NULL THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Batch not found or inactive';
    END IF;

    -- Atomic decrease
    UPDATE batch
    SET current_quantity = current_quantity - p_quantity
    WHERE id = p_batch_id
      AND current_quantity >= p_quantity;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Quantity is not enough for export';
    END IF;

    -- Insert ledger
    INSERT INTO inventory_movement (
        batch_id,
        quantity,
        movement_type,
        reference_doc_id,
        product_name_snapshot,
        batch_code_snapshot,
        request_id,
        created_at
    )
    VALUES (
        p_batch_id,
        -p_quantity,
        2,
        p_reference_doc_id,
        v_product_name,
        v_batch_code,
        p_request_id,
        NOW(6)
    );

END //

DELIMITER ;