CREATE TRIGGER TR_AddStock_OnStockEntry
ON StockEntry
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Aumentar stock del producto
    UPDATE P
    SET P.Stock = P.Stock + I.Quantity
    FROM Product P
    INNER JOIN inserted I ON P.Id = I.ProductId;

    -- Registrar movimiento de stock
    INSERT INTO StockMovement (
        ProductId,
        UserId,
        MovementType,
        Quantity,
        Reference
    )
    SELECT
        ProductId,
        UserId,
        'IN',
        Quantity,
        'PURCHASE'
    FROM inserted;
END;
GO
