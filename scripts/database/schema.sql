-- =============================================
-- SCHEMA PostgreSQL - Galaxium System
-- Sistema de punto de venta (POS)
-- =============================================

-- =============================================
-- TABLA: Role
-- =============================================
CREATE TABLE IF NOT EXISTS "Role" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(50) NOT NULL
);

-- =============================================
-- TABLA: User
-- =============================================
CREATE TABLE IF NOT EXISTS "User" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" INT NOT NULL,
    "FullName" VARCHAR(100) NOT NULL,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_User_Role" FOREIGN KEY ("RoleId") REFERENCES "Role"("Id")
);

-- =============================================
-- TABLA: RefreshToken
-- =============================================
CREATE TABLE IF NOT EXISTS "RefreshToken" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INT NOT NULL,
    "Token" VARCHAR(300) NOT NULL,
    "ExpiresAt" TIMESTAMP NOT NULL,
    "IsRevoked" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "RevokedAt" TIMESTAMP,
    "ReplacedByToken" VARCHAR(300),
    CONSTRAINT "FK_RefreshToken_User" FOREIGN KEY ("UserId") REFERENCES "User"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_RefreshToken_UserId" ON "RefreshToken"("UserId");

-- =============================================
-- TABLA: PasswordResetCode
-- =============================================
CREATE TABLE IF NOT EXISTS "PasswordResetCode" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INT NOT NULL,
    "CodeHash" VARCHAR(255) NOT NULL,
    "ExpiresAt" TIMESTAMP NOT NULL,
    "IsUsed" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_PasswordResetCode_User" FOREIGN KEY ("UserId") REFERENCES "User"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_PasswordResetCode_UserId" ON "PasswordResetCode"("UserId");

-- =============================================
-- TABLA: ProductCategory
-- =============================================
CREATE TABLE IF NOT EXISTS "ProductCategory" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Code" VARCHAR(20),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- =============================================
-- TABLA: Product
-- =============================================
CREATE TABLE IF NOT EXISTS "Product" (
    "Id" SERIAL PRIMARY KEY,
    "CategoryId" INT NOT NULL,
    "CreatedByUserId" INT NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "SKU" VARCHAR(50) NOT NULL UNIQUE,
    "Barcode" VARCHAR(50) UNIQUE,
    "CostPrice" DECIMAL(18,2),
    "SalePrice" DECIMAL(18,2),
    "Stock" INT NOT NULL DEFAULT 0,
    "MinimumStock" INT NOT NULL DEFAULT 5,
    "UnitOfMeasure" INTEGER NOT NULL DEFAULT 1,
    "IsActive" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_Product_Category" FOREIGN KEY ("CategoryId") REFERENCES "ProductCategory"("Id"),
    CONSTRAINT "FK_Product_User" FOREIGN KEY ("CreatedByUserId") REFERENCES "User"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Product_CategoryId" ON "Product"("CategoryId");
CREATE INDEX IF NOT EXISTS "IX_Product_SKU" ON "Product"("SKU");
CREATE INDEX IF NOT EXISTS "IX_Product_Barcode" ON "Product"("Barcode");

-- =============================================
-- TABLA: ProductPhoto
-- =============================================
CREATE TABLE IF NOT EXISTS "ProductPhoto" (
    "Id" SERIAL PRIMARY KEY,
    "ProductId" INT NOT NULL,
    "PhotoUrl" VARCHAR(300) NOT NULL,
    "IsPrimary" BOOLEAN NOT NULL DEFAULT FALSE,
    "UploadedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_ProductPhoto_Product" FOREIGN KEY ("ProductId") REFERENCES "Product"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_ProductPhoto_ProductId" ON "ProductPhoto"("ProductId");

-- =============================================
-- TABLA: Supplier
-- =============================================
CREATE TABLE IF NOT EXISTS "Supplier" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(150) NOT NULL,
    "Phone" VARCHAR(30),
    "Email" VARCHAR(150),
    "Address" VARCHAR(300),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);


-- =============================================
-- TABLA: Customer
-- =============================================
CREATE TABLE IF NOT EXISTS "Customer" (
    "Id" SERIAL PRIMARY KEY,
    "FullName" VARCHAR(100) NOT NULL,
    "Phone" VARCHAR(30),
    "Email" VARCHAR(150),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- =============================================
-- TABLA: PaymentMethod
-- =============================================
CREATE TABLE IF NOT EXISTS "PaymentMethod" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(200),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- INSERT INTO "PaymentMethod" ("Id", "Name", "Description", "IsActive", "CreatedAt") VALUES 
--     (1, 'Efectivo', 'Pago en efectivo', TRUE, NOW());
-- -- =============================================
-- TABLA: Sale
-- =============================================
CREATE TABLE IF NOT EXISTS "Sale" (
    "Id" SERIAL PRIMARY KEY,
    "CustomerId" INT,
    "UserId" INT NOT NULL,
    "PaymentMethodId" INT NOT NULL,
    "SaleDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "SubTotal" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Discount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "AmountPaid" DECIMAL(18,2) DEFAULT 0,
    "ChangeAmount" DECIMAL(18,2) DEFAULT 0,
    "Total" DECIMAL(18,2) NOT NULL,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'COMPLETED',
    "InvoiceNumber" VARCHAR(50),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_Sale_Customer" FOREIGN KEY ("CustomerId") REFERENCES "Customer"("Id"),
    CONSTRAINT "FK_Sale_User" FOREIGN KEY ("UserId") REFERENCES "User"("Id"),
    CONSTRAINT "FK_Sale_PaymentMethod" FOREIGN KEY ("PaymentMethodId") REFERENCES "PaymentMethod"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Sale_SaleDate" ON "Sale"("SaleDate");
CREATE INDEX IF NOT EXISTS "IX_Sale_CustomerId" ON "Sale"("CustomerId");
CREATE INDEX IF NOT EXISTS "IX_Sale_UserId" ON "Sale"("UserId");

-- =============================================
-- TABLA: SaleDetail
-- =============================================
CREATE TABLE IF NOT EXISTS "SaleDetail" (
    "Id" SERIAL PRIMARY KEY,
    "SaleId" INT NOT NULL,
    "ProductId" INT NOT NULL,
    "Quantity" INT NOT NULL,
    "UnitPrice" DECIMAL(18,2) NOT NULL,
    "UnitCost" DECIMAL(18,2) NOT NULL,
    "SubTotal" DECIMAL(18,2) GENERATED ALWAYS AS (Quantity * UnitPrice) STORED,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_SaleDetail_Sale" FOREIGN KEY ("SaleId") REFERENCES "Sale"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SaleDetail_Product" FOREIGN KEY ("ProductId") REFERENCES "Product"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_SaleDetail_SaleId" ON "SaleDetail"("SaleId");
CREATE INDEX IF NOT EXISTS "IX_SaleDetail_ProductId" ON "SaleDetail"("ProductId");

-- =============================================
-- TABLA: Purchase
-- =============================================
CREATE TABLE IF NOT EXISTS "Purchase" (
    "Id" SERIAL PRIMARY KEY,
    "SupplierId" INT NOT NULL,
    "UserId" INT NOT NULL,
    "PurchaseDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Total" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'COMPLETED',
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_Purchase_Supplier" FOREIGN KEY ("SupplierId") REFERENCES "Supplier"("Id"),
    CONSTRAINT "FK_Purchase_User" FOREIGN KEY ("UserId") REFERENCES "User"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Purchase_PurchaseDate" ON "Purchase"("PurchaseDate" DESC);
CREATE INDEX IF NOT EXISTS "IX_Purchase_SupplierId" ON "Purchase"("SupplierId");

-- =============================================
-- TABLA: PurchaseDetail
-- =============================================
CREATE TABLE IF NOT EXISTS "PurchaseDetail" (
    "Id" SERIAL PRIMARY KEY,
    "PurchaseId" INT NOT NULL,
    "ProductId" INT NOT NULL,
    "Quantity" INT NOT NULL,
    "UnitPrice" DECIMAL(18,2) NOT NULL,
    "Total" DECIMAL(18,2) NOT NULL,
    CONSTRAINT "FK_PurchaseDetail_Purchase" FOREIGN KEY ("PurchaseId") REFERENCES "Purchase"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PurchaseDetail_Product" FOREIGN KEY ("ProductId") REFERENCES "Product"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_PurchaseDetail_PurchaseId" ON "PurchaseDetail"("PurchaseId");
CREATE INDEX IF NOT EXISTS "IX_PurchaseDetail_ProductId" ON "PurchaseDetail"("ProductId");

-- =============================================
-- TABLA: StockAlert
-- =============================================
CREATE TABLE IF NOT EXISTS "StockAlert" (
    "Id" SERIAL PRIMARY KEY,
    "ProductId" INT NOT NULL,
    "AlertType" INTEGER NOT NULL,
    "Message" VARCHAR(300) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "ResolvedAt" TIMESTAMP,
    CONSTRAINT "FK_StockAlert_Product" FOREIGN KEY ("ProductId") REFERENCES "Product"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_StockAlert_ProductId_IsActive" ON "StockAlert"("ProductId", "IsActive");

-- =============================================
-- SEED: Roles base
-- =============================================
INSERT INTO "Role" ("Id", "Name") VALUES 
    (1, 'Administrator'),
    (2, 'Cashier'),
    (3, 'Supervisor')
ON CONFLICT DO NOTHING;

-- =============================================
-- SEED: PaymentMethod (SOLO EFECTIVO como solicitaste)
-- =============================================
INSERT INTO "PaymentMethod" ("Id", "Name", "Description", "IsActive", "CreatedAt") VALUES 
    (1, 'Efectivo', 'Pago en efectivo', TRUE, NOW())
ON CONFLICT DO NOTHING;

-- =============================================
-- COMENTARIOS PARA DOCUMENTACION
-- =============================================
COMMENT ON TABLE "Role" IS 'Roles para control de acceso (RBAC)';
COMMENT ON TABLE "User" IS 'Usuarios del sistema';
COMMENT ON TABLE "Product" IS 'Productos del inventario';
COMMENT ON TABLE "PaymentMethod" IS 'Métodos de pago disponibles';
COMMENT ON TABLE "Sale" IS 'Cabecera de ventas';
COMMENT ON TABLE "SaleDetail" IS 'Detalle de productos vendidos';
COMMENT ON COLUMN "Product.UnitOfMeasure" IS '1=Unit, 2=Box, 3=Kilo';
COMMENT ON COLUMN "StockEntry.ReferenceType" IS '1=Purchase, 2=Sale, 3=Adjustment, 4=Return';
COMMENT ON COLUMN "StockAlert.AlertType" IS '1=LowStock, 2=Exhausted, 3=NoMovement';
