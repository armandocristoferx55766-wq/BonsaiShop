
USE BonsaiShop;
GO

-- 1. BANG DANH MUC
CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500)
);
GO

-- 2. BANG SAN PHAM
CREATE TABLE Products
(
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Image NVARCHAR(255),
    Description NVARCHAR(1000),
    Quantity INT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Products_Categories
    FOREIGN KEY (CategoryID)
    REFERENCES Categories(CategoryID),

    CONSTRAINT CK_Products_Price
    CHECK (Price >= 0),

    CONSTRAINT CK_Products_Quantity
    CHECK (Quantity >= 0)
);
GO

-- 3. BANG NGUOI DUNG
CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Phone VARCHAR(20),
    Address NVARCHAR(300),
    Role VARCHAR(20) NOT NULL DEFAULT 'Customer'
);
GO

-- 4. BANG DON HANG
CREATE TABLE Orders
(
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    ShippingAddress NVARCHAR(300) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Ch? xác nh?n',

    CONSTRAINT FK_Orders_Users
    FOREIGN KEY (UserID)
    REFERENCES Users(UserID),

    CONSTRAINT CK_Orders_TotalAmount
    CHECK (TotalAmount >= 0)
);
GO

-- 5. BANG CHI TIET DON HANG
CREATE TABLE OrderDetails
(
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,

    CONSTRAINT FK_OrderDetails_Orders
    FOREIGN KEY (OrderID)
    REFERENCES Orders(OrderID),

    CONSTRAINT FK_OrderDetails_Products
    FOREIGN KEY (ProductID)
    REFERENCES Products(ProductID),

    CONSTRAINT CK_OrderDetails_Quantity
    CHECK (Quantity > 0),

    CONSTRAINT CK_OrderDetails_UnitPrice
    CHECK (UnitPrice >= 0)
);
GO

-- Chuyen tat ca tài khoan va Customer
UPDATE Users
SET Role = 'Customer';

-- Chi cap quyen Admin cho tài khoan huy@gmail.com
UPDATE Users
SET Role = 'Admin'
WHERE Email = 'huy@gmail.com';

-- Kiam tra ket qua
SELECT UserID, FullName, Email, Role
FROM Users;
