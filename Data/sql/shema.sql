-- =============================================
-- SEED DE ROLES BASE PARA POSTGRESQL
-- =============================================
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Role" WHERE "Name" = 'Administrador') THEN
        INSERT INTO "Role" ("Name") VALUES ('Administrador');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "Role" WHERE "Name" = 'Encargado de inventario') THEN
        INSERT INTO "Role" ("Name") VALUES ('Encargado de inventario');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "Role" WHERE "Name" = 'Cajero') THEN
        INSERT INTO "Role" ("Name") VALUES ('Cajero');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "Role" WHERE "Name" = 'Supervisor') THEN
        INSERT INTO "Role" ("Name") VALUES ('Supervisor');
    END IF;
END$$;
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

IF DB_ID('GalaxiumBD') IS NULL
BEGIN
    CREATE DATABASE GalaxiumBD;
END
GO
USE GalaxiumBD;
GO

/* ============================================================
   TABLA: Role
   ============================================================ */
IF OBJECT_ID('Role', 'U') IS NULL
BEGIN
    CREATE TABLE Role
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name VARCHAR(50) NOT NULL
        -- ADMINISTRADOR, VENDEDOR
    );
END
GO

/* ============================================================
   TABLA: User
   ============================================================ */
IF OBJECT_ID('[User]', 'U') IS NULL
BEGIN
    CREATE TABLE [User]
    (
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
END
GO

--✔ Logout por usuario
--✔ Logout global
--✔ Rotación de tokens
--✔ Auditoría
--✔ Múltiples dispositivos
IF OBJECT_ID('RefreshToken', 'U') IS NULL
BEGIN
    CREATE TABLE RefreshToken
    (
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
END
GO

/* ============================================================
    TABLA: PasswordResetCode
    Descripción: códigos temporales para recuperación de contraseña
============================================================ */
IF OBJECT_ID('PasswordResetCode', 'U') IS NULL
BEGIN
    CREATE TABLE PasswordResetCode
    (
         Id INT IDENTITY(1,1) PRIMARY KEY,
         UserId INT NOT NULL,
         CodeHash VARCHAR(255) NOT NULL,
         ExpiresAt DATETIME NOT NULL,
         IsUsed BIT NOT NULL DEFAULT 0,
         CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

         CONSTRAINT FK_PasswordResetCode_User
              FOREIGN KEY (UserId) REFERENCES [User](Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PasswordResetCode_UserId'
      AND object_id = OBJECT_ID('PasswordResetCode')
)
BEGIN
    CREATE INDEX IX_PasswordResetCode_UserId ON PasswordResetCode(UserId);
END
GO



/* ============================================================
   TABLA: ProductCategory
   ============================================================ */
IF OBJECT_ID('ProductCategory', 'U') IS NULL
BEGIN
    CREATE TABLE ProductCategory
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name VARCHAR(100) NOT NULL,
        Code VARCHAR(10) DEFAULT 'GEN' NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

/* ============================================================
   TABLA: Product
   ============================================================ */
--    Product: guarda datos básicos del producto, stock total actual, costo y precio de venta.

-- ProductPhoto: almacena fotos, con posibilidad de varias por producto y una primaria.

-- StockEntry: controla cada lote o entrada de stock con cantidad, costo unitario y total invertido. Permite saber cuánto has invertido y cuánto stock queda por lote (con IsActive).

-- StockMovement: registra cada movimiento de stock (entradas y salidas), útil para auditoría y control histórico.
IF OBJECT_ID('Product', 'U') IS NULL
BEGIN
    CREATE TABLE Product
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CategoryId INT NOT NULL,
        CreatedByUserId INT NOT NULL,
         Barcode VARCHAR(50) NULL UNIQUE, 
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
END
GO

IF OBJECT_ID('ProductPhoto', 'U') IS NULL
BEGIN
    CREATE TABLE ProductPhoto
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL,
        PhotoUrl VARCHAR(500) NOT NULL,
        IsPrimary BIT NOT NULL DEFAULT 0,
        UploadedAt DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_ProductPhoto_Product FOREIGN KEY (ProductId) REFERENCES Product(Id)
    );
END


GO
/* ============================================================
   TABLA: StockEntry
   Descripción: Cada lote de productos entrantes.
   Permite controlar stock, costo unitario y total invertido.
   Se vincula con Product y User (quién registró el stock)
============================================================ */
IF OBJECT_ID('StockEntry', 'U') IS NULL
BEGIN
    CREATE TABLE StockEntry
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,

        ProductId INT NOT NULL,
        UserId INT NOT NULL,

        -- Cantidad comprada
        Quantity INT NOT NULL,

        -- Cantidad restante del lote
        RemainingQuantity INT NOT NULL,

        -- Costo unitario
        UnitCost DECIMAL(18,2) NOT NULL,

        -- Costo total del lote (calculado y persistido)
        TotalCost AS (Quantity * UnitCost) PERSISTED,

        -- Tipo referencia
        ReferenceType INT NOT NULL DEFAULT 1,

        -- Id externo (venta, ajuste, etc.)
        ReferenceId INT NULL,

        -- Auditoría
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

        -- Activo mientras tenga stock
        IsActive BIT NOT NULL DEFAULT 1,

        /* ============================
           FOREIGN KEYS
        ============================ */

        CONSTRAINT FK_StockEntry_Product
            FOREIGN KEY (ProductId)
            REFERENCES Product(Id),

        CONSTRAINT FK_StockEntry_User
            FOREIGN KEY (UserId)
            REFERENCES [User](Id)
    );
END
GO

/* ============================================================
   TABLA: Customer
   Descripción: Clientes para registro de ventas
============================================================ */
IF OBJECT_ID('Customer', 'U') IS NULL
BEGIN
    CREATE TABLE Customer
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName VARCHAR(150) NOT NULL,
        Phone VARCHAR(20),
        Email VARCHAR(100),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO
/* ============================================================
   TABLA: PaymentMethod
   Descripción: Catálogo de métodos de pago
============================================================ */
IF OBJECT_ID('PaymentMethod', 'U') IS NULL
BEGIN
    CREATE TABLE PaymentMethod
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,

        Name VARCHAR(50) NOT NULL,
        -- Ej: Efectivo, Transferencia, Tarjeta

        Description VARCHAR(150) NULL,

        IsActive BIT NOT NULL DEFAULT 1,

        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

/* ============================================================
   TABLA: Sale
   Descripción: Cabecera de venta
============================================================ */
IF OBJECT_ID('Sale', 'U') IS NULL
BEGIN
    CREATE TABLE Sale
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,

        CustomerId INT NULL,
        UserId INT NOT NULL,
        --el administrador o vendedor que realiza la venta
        -- Método de pago (FK catálogo)
        PaymentMethodId INT NOT NULL,

        SaleDate DATETIME NOT NULL DEFAULT GETDATE(),

        -- Totales financieros
        SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
        Discount DECIMAL(18,2)  NOT NULL DEFAULT 0,
        AmountPaid DECIMAL(18,2) NULL DEFAULT 0,
        -- dinero recibido
        ChangeAmount DECIMAL(18,2) NULL DEFAULT 0,
        -- vuelto entregado
        Total DECIMAL(18,2) NOT NULL,

        -- Estado de la venta
        Status VARCHAR(30) NOT NULL DEFAULT 'COMPLETED',
        -- COMPLETED | CANCELLED | REFUNDED

        -- Facturación
        InvoiceNumber VARCHAR(50) NULL,

        -- Auditoría
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

        /* ============================
           FOREIGN KEYS
        ============================ */
        CONSTRAINT FK_Sale_Customer 
            FOREIGN KEY (CustomerId) REFERENCES Customer(Id),

        CONSTRAINT FK_Sale_User 
            FOREIGN KEY (UserId) REFERENCES [User](Id),

        CONSTRAINT FK_Sale_PaymentMethod 
            FOREIGN KEY (PaymentMethodId) REFERENCES PaymentMethod(Id)
    );
END
GO

/* ============================================================
   TABLA: SaleDetail
   Descripción: Detalle de productos vendidos
============================================================ */
IF OBJECT_ID('SaleDetail', 'U') IS NULL
BEGIN
    CREATE TABLE SaleDetail
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,

        SaleId INT NOT NULL,
        ProductId INT NOT NULL,

        Quantity INT NOT NULL,

        -- Precio de venta histórico
        UnitPrice DECIMAL(18,2) NOT NULL,

        -- Costo histórico (para utilidad)
        UnitCost DECIMAL(18,2) NOT NULL,

        -- Subtotal persistido
        SubTotal AS (Quantity * UnitPrice) PERSISTED,

        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

        /* ============================
           FOREIGN KEYS
        ============================ */
        CONSTRAINT FK_SaleDetail_Sale 
            FOREIGN KEY (SaleId) REFERENCES Sale(Id),

        CONSTRAINT FK_SaleDetail_Product 
            FOREIGN KEY (ProductId) REFERENCES Product(Id)
    );
END
GO

/* ============================================================
   ACTUALIZACION DE ESQUEMA 2026-03-15
   Descripcion:
   Sincroniza la base existente con los cambios nuevos del backend.
   Este bloque es seguro para ejecutarse sobre una base ya creada.
   ============================================================ */

/* ============================================================
   ROLE BASE PARA RBAC
   ============================================================ */
IF NOT EXISTS (SELECT 1 FROM Role WHERE Name = 'Administrador')
    INSERT INTO Role (Name) VALUES ('Administrador');
GO

IF NOT EXISTS (SELECT 1 FROM Role WHERE Name = 'Encargado de inventario')
    INSERT INTO Role (Name) VALUES ('Encargado de inventario');
GO

IF NOT EXISTS (SELECT 1 FROM Role WHERE Name = 'Cajero')
    INSERT INTO Role (Name) VALUES ('Cajero');
GO

IF NOT EXISTS (SELECT 1 FROM Role WHERE Name = 'Supervisor')
    INSERT INTO Role (Name) VALUES ('Supervisor');
GO

/* ============================================================
   PRODUCT: Unidad de medida
   ============================================================ */
IF COL_LENGTH('Product', 'UnitOfMeasure') IS NULL
BEGIN
    ALTER TABLE Product
    ADD UnitOfMeasure VARCHAR(30) NOT NULL
        CONSTRAINT DF_Product_UnitOfMeasure DEFAULT 'Unit';
END
GO

/* ============================================================
   SUPPLIER
   ============================================================ */
IF OBJECT_ID('Supplier', 'U') IS NULL
BEGIN
    CREATE TABLE Supplier
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name VARCHAR(150) NOT NULL,
        Phone VARCHAR(30) NULL,
        Email VARCHAR(150) NULL,
        Address VARCHAR(300) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

/* ============================================================
   PURCHASE
   ============================================================ */
IF OBJECT_ID('Purchase', 'U') IS NULL
BEGIN
    CREATE TABLE Purchase
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SupplierId INT NOT NULL,
        UserId INT NOT NULL,
        PurchaseDate DATETIME NOT NULL DEFAULT GETDATE(),
        Total DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status VARCHAR(30) NOT NULL DEFAULT 'COMPLETED',
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_Purchase_Supplier
            FOREIGN KEY (SupplierId) REFERENCES Supplier(Id),

        CONSTRAINT FK_Purchase_User
            FOREIGN KEY (UserId) REFERENCES [User](Id)
    );
END
GO

/* ============================================================
   PURCHASE DETAIL
   ============================================================ */
IF OBJECT_ID('PurchaseDetail', 'U') IS NULL
BEGIN
    CREATE TABLE PurchaseDetail
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PurchaseId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        Total DECIMAL(18,2) NOT NULL,

        CONSTRAINT FK_PurchaseDetail_Purchase
            FOREIGN KEY (PurchaseId) REFERENCES Purchase(Id),

        CONSTRAINT FK_PurchaseDetail_Product
            FOREIGN KEY (ProductId) REFERENCES Product(Id)
    );
END
GO

/* ============================================================
   STOCK ENTRY: motivo y proveedor
   ============================================================ */
IF COL_LENGTH('StockEntry', 'Reason') IS NULL
BEGIN
    ALTER TABLE StockEntry
    ADD Reason VARCHAR(300) NULL;
END
GO

IF COL_LENGTH('StockEntry', 'SupplierId') IS NULL
BEGIN
    ALTER TABLE StockEntry
    ADD SupplierId INT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_StockEntry_Supplier'
)
BEGIN
    ALTER TABLE StockEntry
    ADD CONSTRAINT FK_StockEntry_Supplier
        FOREIGN KEY (SupplierId) REFERENCES Supplier(Id);
END
GO

/* ============================================================
   STOCK ALERT
   ============================================================ */
IF OBJECT_ID('StockAlert', 'U') IS NULL
BEGIN
    CREATE TABLE StockAlert
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL,
        AlertType VARCHAR(30) NOT NULL,
        Message VARCHAR(300) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ResolvedAt DATETIME NULL,

        CONSTRAINT FK_StockAlert_Product
            FOREIGN KEY (ProductId) REFERENCES Product(Id)
    );
END
GO

/* ============================================================
   INDICES RECOMENDADOS
   ============================================================ */
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Sale_SaleDate'
      AND object_id = OBJECT_ID('Sale')
)
BEGIN
    CREATE INDEX IX_Sale_SaleDate ON Sale(SaleDate);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_StockEntry_ProductId_CreatedAt'
      AND object_id = OBJECT_ID('StockEntry')
)
BEGIN
    CREATE INDEX IX_StockEntry_ProductId_CreatedAt ON StockEntry(ProductId, CreatedAt DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_StockAlert_ProductId_IsActive'
      AND object_id = OBJECT_ID('StockAlert')
)
BEGIN
    CREATE INDEX IX_StockAlert_ProductId_IsActive ON StockAlert(ProductId, IsActive);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Purchase_PurchaseDate'
      AND object_id = OBJECT_ID('Purchase')
)
BEGIN
    CREATE INDEX IX_Purchase_PurchaseDate ON Purchase(PurchaseDate DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Purchase_SupplierId'
      AND object_id = OBJECT_ID('Purchase')
)
BEGIN
    CREATE INDEX IX_Purchase_SupplierId ON Purchase(SupplierId);
END
GO
