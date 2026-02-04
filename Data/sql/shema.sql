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
    Code VARCHAR(10)  DEFAULT 'GEN' NOT NULL,
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
    CostPrice DECIMAL(18,2) NULL,
    SalePrice DECIMAL(18,2) NULL,

    Stock INT NOT NULL DEFAULT 0,
    MinimumStock INT NOT NULL DEFAULT 5,

    IsActive BIT NOT NULL DEFAULT 0,
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


GO
/* ============================================================
   TABLA: StockEntry
   Descripción: Cada lote de productos entrantes.
   Permite controlar stock, costo unitario y total invertido.
   Se vincula con Product y User (quién registró el stock)
============================================================ */
CREATE TABLE StockEntry (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL, -- quién registró la compra

    Quantity INT NOT NULL, -- cantidad comprada
    RemainingQuantity INT NOT NULL, -- cantidad restante del lote

    UnitCost DECIMAL(18,2) NOT NULL, -- costo unitario de compra
    TotalCost AS (Quantity * UnitCost) PERSISTED,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1, -- activo mientras RemainingQuantity > 0

    CONSTRAINT FK_StockEntry_Product 
        FOREIGN KEY (ProductId) REFERENCES Product(Id),

    CONSTRAINT FK_StockEntry_User 
        FOREIGN KEY (UserId) REFERENCES [User](Id)
);
GO


/* ============================================================
   TABLA: StockMovement
   Descripción: Movimientos de stock (entradas o salidas)
   Útil para auditoría y control histórico
============================================================ */
CREATE TABLE StockMovement (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL, -- quién hizo el movimiento

    MovementType VARCHAR(10) NOT NULL, -- 'IN' / 'OUT'
    Quantity INT NOT NULL, -- cantidad de producto movido
    ReferenceType VARCHAR(50) NOT NULL, -- 'SALE', 'PURCHASE', 'ADJUSTMENT'
    ReferenceId INT NULL, -- opcional, id de la venta o stock entry relacionado

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_StockMovement_Product FOREIGN KEY (ProductId) REFERENCES Product(Id),
    CONSTRAINT FK_StockMovement_User FOREIGN KEY (UserId) REFERENCES [User](Id)
);
GO

/* ============================================================
   TABLA: Customer
   Descripción: Clientes para registro de ventas
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
   Descripción: Venta realizada
============================================================ */
CREATE TABLE Sale (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NULL, -- cliente opcional
    SellerUserId INT NOT NULL, -- vendedor que realizó la venta

    SaleDate DATETIME NOT NULL DEFAULT GETDATE(),
    Total DECIMAL(18,2) NOT NULL, -- total de la venta
    PaymentMethod VARCHAR(50) NOT NULL, -- forma de pago

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Sale_Customer FOREIGN KEY (CustomerId) REFERENCES Customer(Id),
    CONSTRAINT FK_Sale_User FOREIGN KEY (SellerUserId) REFERENCES [User](Id)
);
GO

/* ============================================================
   TABLA: SaleDetail
   Descripción: Detalle de cada producto vendido
============================================================ */
CREATE TABLE SaleDetail (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL, -- precio de venta por unidad
    SubTotal AS (Quantity * UnitPrice) PERSISTED, -- subtotal calculado

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_SaleDetail_Sale FOREIGN KEY (SaleId) REFERENCES Sale(Id),
    CONSTRAINT FK_SaleDetail_Product FOREIGN KEY (ProductId) REFERENCES Product(Id)
);
GO
