/* ============================================================
   BASE DE DATOS: GalaxiumBD
   DESCRIPCIÓN:
   Sistema básico para un local de ventas:
   - Productos
   - Stock
   - Ventas
   - Usuarios / Administradores
   - Control de quién hace cada acción
   ============================================================ */

CREATE DATABASE GalaxiumBD;
GO
USE GalaxiumBD;
GO

/* ============================================================
   TABLA: Role
   ============================================================ */
CREATE TABLE Role (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(50) NOT NULL -- ADMINISTRADOR, VENDEDOR
);
GO

/* ============================================================
   TABLA: User
   ============================================================ */
CREATE TABLE [User] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoleId INT NOT NULL,
    FullName VARCHAR(150) NOT NULL,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_User_Role
        FOREIGN KEY (RoleId) REFERENCES Role(Id)
);
GO

--✔ Logout por usuario
--✔ Logout global
--✔ Rotación de tokens
--✔ Auditoría
--✔ Múltiples dispositivos
CREATE TABLE RefreshToken (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Token VARCHAR(500) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    RevokedAt DATETIME NULL,
    ReplacedByToken VARCHAR(500) NULL,

    CONSTRAINT FK_RefreshToken_User
        FOREIGN KEY (UserId) REFERENCES [User](Id)
);
GO



/* ============================================================
   TABLA: ProductCategory
   ============================================================ */
CREATE TABLE ProductCategory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Code VARCHAR(10) NOT NULL DEFAULT 'GEN',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

/* ============================================================
   TABLA: Product
   ============================================================ */
--    Product: guarda datos básicos del producto, stock total actual, costo y precio de venta.

-- ProductPhoto: almacena fotos, con posibilidad de varias por producto y una primaria.

-- StockEntry: controla cada lote o entrada de stock con cantidad, costo unitario y total invertido. Permite saber cuánto has invertido y cuánto stock queda por lote (con IsActive).

-- StockMovement: registra cada movimiento de stock (entradas y salidas), útil para auditoría y control histórico.
CREATE TABLE Product (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    CreatedByUserId INT NOT NULL,

    Name VARCHAR(150) NOT NULL,
    SKU VARCHAR(50) NOT NULL UNIQUE,
    CostPrice DECIMAL(18,2) NOT NULL,
    SalePrice DECIMAL(18,2) NOT NULL,

    Stock INT NOT NULL DEFAULT 0,
    MinimumStock INT NOT NULL DEFAULT 5,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Product_Category
        FOREIGN KEY (CategoryId) REFERENCES ProductCategory(Id),

    CONSTRAINT FK_Product_User
        FOREIGN KEY (CreatedByUserId) REFERENCES [User](Id)
);
GO

CREATE TABLE ProductPhoto (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    PhotoUrl VARCHAR(500) NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    UploadedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_ProductPhoto_Product FOREIGN KEY (ProductId) REFERENCES Product(Id)
);
CREATE TABLE StockEntry (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL,

    Quantity INT NOT NULL,
    UnitCost DECIMAL(18,2) NOT NULL, -- precio unitario de compra
    TotalCost AS (Quantity * UnitCost) PERSISTED,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1, -- para saber si aún quedan productos en ese lote

    CONSTRAINT FK_StockEntry_Product FOREIGN KEY (ProductId) REFERENCES Product(Id),
    CONSTRAINT FK_StockEntry_User FOREIGN KEY (UserId) REFERENCES [User](Id)
);
GO

GO
CREATE TABLE StockMovement (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    MovementType VARCHAR(10) NOT NULL, -- 'IN' o 'OUT'
    Quantity INT NOT NULL,
    Reference VARCHAR(100) NULL, -- ej: 'PURCHASE', 'SALE', 'ADJUSTMENT'
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_StockMovement_Product FOREIGN KEY (ProductId) REFERENCES Product(Id),
    CONSTRAINT FK_StockMovement_User FOREIGN KEY (UserId) REFERENCES [User](Id)
);
GO

/* ======
======================================================
   TABLA: Customer
   ============================================================ */
CREATE TABLE Customer (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(150) NOT NULL,
    Phone VARCHAR(20),
    Email VARCHAR(100),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

/* ============================================================
   TABLA: Sale
   ============================================================ */
CREATE TABLE Sale (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NULL,
    SellerUserId INT NOT NULL,

    SaleDate DATETIME NOT NULL DEFAULT GETDATE(),
    Total DECIMAL(18,2) NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Sale_Customer
        FOREIGN KEY (CustomerId) REFERENCES Customer(Id),

    CONSTRAINT FK_Sale_User
        FOREIGN KEY (SellerUserId) REFERENCES [User](Id)
);
GO

/* ============================================================
   TABLA: SaleDetail
   ============================================================ */
CREATE TABLE SaleDetail (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL,
    ProductId INT NOT NULL,
    UserId INT NOT NULL,

    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    SubTotal AS (Quantity * UnitPrice),

    CONSTRAINT FK_SaleDetail_Sale
        FOREIGN KEY (SaleId) REFERENCES Sale(Id),

    CONSTRAINT FK_SaleDetail_Product
        FOREIGN KEY (ProductId) REFERENCES Product(Id),

    CONSTRAINT FK_SaleDetail_User
        FOREIGN KEY (UserId) REFERENCES [User](Id)
);
GO

/* ============================================================
   TABLA: StockMovement
   ============================================================ */
CREATE TABLE StockMovement (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL,

    MovementType VARCHAR(10) NOT NULL, -- IN / OUT
    Quantity INT NOT NULL,
    Reference VARCHAR(100), -- SALE, ADJUSTMENT, PURCHASE

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_StockMovement_Product
        FOREIGN KEY (ProductId) REFERENCES Product(Id),

    CONSTRAINT FK_StockMovement_User
        FOREIGN KEY (UserId) REFERENCES [User](Id)
);
GO

/* ============================================================
   TRIGGER: Descontar stock al vender
   ============================================================ */
CREATE TRIGGER TR_DiscountStock_OnSale
ON SaleDetail
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Descontar stock
    UPDATE P
    SET P.Stock = P.Stock - I.Quantity
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
        'OUT',
        Quantity,
        'SALE'
    FROM inserted;
END;
GO
/* ============================================================
   FIN ESQUEMA BASE DE DATOS
   ============================================================ */