-- =============================================
-- SmartMed Pharmacy Management System
-- Database Setup Script
-- =============================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'SmartMedDB')
    DROP DATABASE SmartMedDB;
GO

CREATE DATABASE SmartMedDB;
GO

USE SmartMedDB;
GO

-- =============================================
-- ADMIN TABLE
-- =============================================
CREATE TABLE Admin (
    AdminID     INT IDENTITY(1,1) PRIMARY KEY,
    Username    NVARCHAR(50) NOT NULL UNIQUE,
    Password    NVARCHAR(256) NOT NULL,
    FullName    NVARCHAR(100) NOT NULL,
    Email       NVARCHAR(100) NOT NULL,
    Role        NVARCHAR(20) NOT NULL DEFAULT 'Admin', -- Admin, SuperAdmin
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive    BIT DEFAULT 1
);
GO

-- =============================================
-- CUSTOMER TABLE
-- =============================================
CREATE TABLE Customer (
    CustomerID      INT IDENTITY(1,1) PRIMARY KEY,
    FirstName       NVARCHAR(50) NOT NULL,
    LastName        NVARCHAR(50) NOT NULL,
    Email           NVARCHAR(100) NOT NULL UNIQUE,
    Phone           NVARCHAR(20),
    Address         NVARCHAR(255),
    Password        NVARCHAR(256) NOT NULL,
    ProfileImage    NVARCHAR(500),
    RegisteredDate  DATETIME DEFAULT GETDATE(),
    Status          NVARCHAR(20) DEFAULT 'Active' -- Active, Disabled
);
GO

-- =============================================
-- CATEGORY TABLE
-- =============================================
CREATE TABLE Category (
    CategoryID      INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName    NVARCHAR(100) NOT NULL UNIQUE,
    Description     NVARCHAR(500)
);
GO

-- =============================================
-- SUPPLIER TABLE
-- =============================================
CREATE TABLE Supplier (
    SupplierID      INT IDENTITY(1,1) PRIMARY KEY,
    SupplierName    NVARCHAR(100) NOT NULL,
    Company         NVARCHAR(100),
    Phone           NVARCHAR(20),
    Email           NVARCHAR(100),
    Address         NVARCHAR(255),
    CreatedDate     DATETIME DEFAULT GETDATE(),
    IsActive        BIT DEFAULT 1
);
GO

-- =============================================
-- DISCOUNT TABLE
-- =============================================
CREATE TABLE Discount (
    DiscountID      INT IDENTITY(1,1) PRIMARY KEY,
    DiscountName    NVARCHAR(100) NOT NULL,
    Percentage      DECIMAL(5,2) NOT NULL,
    StartDate       DATE NOT NULL,
    EndDate         DATE NOT NULL,
    IsActive        BIT DEFAULT 1
);
GO

-- =============================================
-- MEDICINE TABLE
-- =============================================
CREATE TABLE Medicine (
    MedicineID          INT IDENTITY(1,1) PRIMARY KEY,
    MedicineCode        NVARCHAR(20) NOT NULL UNIQUE,
    MedicineName        NVARCHAR(150) NOT NULL,
    CategoryID          INT NOT NULL REFERENCES Category(CategoryID),
    SupplierID          INT NOT NULL REFERENCES Supplier(SupplierID),
    Dosage              NVARCHAR(100),
    Unit                NVARCHAR(50),
    Price               DECIMAL(10,2) NOT NULL,
    Stock               INT NOT NULL DEFAULT 0,
    MinimumStock        INT NOT NULL DEFAULT 10,
    ExpiryDate          DATE NOT NULL,
    Description         NVARCHAR(1000),
    PrescriptionRequired BIT DEFAULT 0,
    Image               NVARCHAR(500),
    DiscountID          INT REFERENCES Discount(DiscountID),
    Status              NVARCHAR(20) DEFAULT 'Active', -- Active, Inactive, Expired
    CreatedDate         DATETIME DEFAULT GETDATE()
);
GO

-- =============================================
-- ORDER TABLE
-- =============================================
CREATE TABLE [Order] (
    OrderID         INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID      INT NOT NULL REFERENCES Customer(CustomerID),
    OrderDate       DATETIME DEFAULT GETDATE(),
    Total           DECIMAL(10,2) NOT NULL,
    Discount        DECIMAL(10,2) DEFAULT 0,
    FinalAmount     DECIMAL(10,2) NOT NULL,
    Status          NVARCHAR(30) DEFAULT 'Pending', -- Pending, Approved, Ready, Delivered, Cancelled, Rejected
    PaymentMethod   NVARCHAR(30) DEFAULT 'Cash',    -- Cash, Card, Online
    PickupDate      DATE,
    Notes           NVARCHAR(500)
);
GO

-- =============================================
-- ORDER DETAILS TABLE
-- =============================================
CREATE TABLE OrderDetail (
    OrderDetailID   INT IDENTITY(1,1) PRIMARY KEY,
    OrderID         INT NOT NULL REFERENCES [Order](OrderID),
    MedicineID      INT NOT NULL REFERENCES Medicine(MedicineID),
    Quantity        INT NOT NULL,
    Price           DECIMAL(10,2) NOT NULL,
    Subtotal        DECIMAL(10,2) NOT NULL
);
GO

-- =============================================
-- PRESCRIPTION TABLE
-- =============================================
CREATE TABLE Prescription (
    PrescriptionID  INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID      INT NOT NULL REFERENCES Customer(CustomerID),
    OrderID         INT REFERENCES [Order](OrderID),
    Image           NVARCHAR(500),
    DoctorName      NVARCHAR(100),
    IssueDate       DATE,
    Status          NVARCHAR(20) DEFAULT 'Pending', -- Pending, Approved, Rejected
    UploadDate      DATETIME DEFAULT GETDATE()
);
GO

-- =============================================
-- ACTIVITY LOG TABLE
-- =============================================
CREATE TABLE ActivityLog (
    LogID       INT IDENTITY(1,1) PRIMARY KEY,
    UserType    NVARCHAR(20), -- Admin, Customer
    UserID      INT,
    Action      NVARCHAR(500),
    LogDate     DATETIME DEFAULT GETDATE()
);
GO

-- =============================================
-- SEED DATA
-- =============================================

-- Default Admin (Password: Admin@123 - BCrypt hashed)
INSERT INTO Admin (Username, Password, FullName, Email, Role)
VALUES ('admin', '$2a$11$rBnXvzqK8YlJzqK8YlJzqOQGpJzqK8YlJzqK8YlJzqK8YlJzqK8Yl', 'System Administrator', 'admin@smartmed.com', 'SuperAdmin');
GO

-- Categories
INSERT INTO Category (CategoryName, Description) VALUES
('Antibiotics',     'Medicines used to treat bacterial infections'),
('Analgesics',      'Pain relief medicines'),
('Antivirals',      'Medicines used to treat viral infections'),
('Vitamins',        'Vitamins and nutritional supplements'),
('Antidiabetics',   'Medicines for diabetes management'),
('Antihypertensives','Medicines for blood pressure control'),
('Antihistamines',  'Medicines for allergic reactions'),
('Antacids',        'Medicines for stomach acid and heartburn'),
('Antipyretics',    'Fever reducing medicines'),
('Dermatologicals', 'Skin care medicines and creams');
GO

-- Suppliers
INSERT INTO Supplier (SupplierName, Company, Phone, Email, Address) VALUES
('John Medical Supplies',   'JMS Corp',          '0112345678', 'john@jms.com',     'Colombo 03'),
('MediTrade Lanka',         'MediTrade Pvt Ltd', '0119876543', 'info@meditrade.lk', 'Kandy'),
('HealthCare Solutions',    'HCS Lanka',         '0117654321', 'hcs@health.lk',    'Gampaha'),
('PharmaCo Distributors',   'PharmaCo Ltd',      '0115432109', 'pharma@co.lk',     'Colombo 07');
GO

-- Discounts
INSERT INTO Discount (DiscountName, Percentage, StartDate, EndDate, IsActive) VALUES
('No Discount',     0,  '2026-01-01', '2030-12-31', 1),
('5% Off',          5,  '2026-01-01', '2026-12-31', 1),
('10% Off',         10, '2026-01-01', '2026-12-31', 1),
('15% Off',         15, '2026-06-01', '2026-07-31', 1);
GO

-- Sample Medicines
INSERT INTO Medicine (MedicineCode, MedicineName, CategoryID, SupplierID, Dosage, Unit, Price, Stock, MinimumStock, ExpiryDate, Description, PrescriptionRequired, DiscountID, Status)
VALUES
('MED-001', 'Amoxicillin 500mg',       1, 1, '500mg', 'Capsule', 25.00, 150, 20, '2027-06-30', 'Broad-spectrum antibiotic', 1, 1, 'Active'),
('MED-002', 'Paracetamol 500mg',       9, 1, '500mg', 'Tablet',  8.50,  300, 50, '2027-12-31', 'Fever and mild pain relief', 0, 1, 'Active'),
('MED-003', 'Ibuprofen 400mg',         2, 2, '400mg', 'Tablet',  15.00, 200, 30, '2027-08-31', 'Anti-inflammatory pain relief', 0, 2, 'Active'),
('MED-004', 'Cetirizine 10mg',         7, 2, '10mg',  'Tablet',  12.00, 120, 20, '2026-12-31', 'Antihistamine for allergies', 0, 1, 'Active'),
('MED-005', 'Metformin 500mg',         5, 3, '500mg', 'Tablet',  18.50, 100, 15, '2027-03-31', 'Type 2 diabetes management', 1, 1, 'Active'),
('MED-006', 'Amlodipine 5mg',          6, 3, '5mg',   'Tablet',  22.00, 80,  10, '2027-09-30', 'Blood pressure control', 1, 1, 'Active'),
('MED-007', 'Vitamin C 1000mg',        4, 4, '1000mg','Tablet',  30.00, 250, 40, '2028-01-31', 'Vitamin C supplement', 0, 3, 'Active'),
('MED-008', 'Omeprazole 20mg',         8, 4, '20mg',  'Capsule', 20.00, 160, 25, '2027-07-31', 'Proton pump inhibitor for acidity', 0, 1, 'Active'),
('MED-009', 'Azithromycin 500mg',      1, 1, '500mg', 'Tablet',  45.00, 90,  15, '2026-11-30', 'Antibiotic for respiratory infections', 1, 2, 'Active'),
('MED-010', 'Acyclovir 400mg',         3, 2, '400mg', 'Tablet',  35.00, 70,  10, '2027-05-31', 'Antiviral for herpes infections', 1, 1, 'Active'),
('MED-011', 'Aspirin 300mg',            2, 1, '300mg', 'Tablet',  10.00, 40,  20, '2026-01-15', 'Pain and fever relief - expired batch', 0, 1, 'Active'),
('MED-012', 'Loratadine 10mg',          7, 2, '10mg',  'Tablet',  14.00, 25,  15, '2026-03-01', 'Antihistamine - expired batch', 0, 1, 'Active'),
('MED-013', 'Ranitidine 150mg',         8, 3, '150mg', 'Tablet',  16.50, 10,  10, '2026-05-20', 'Acid reflux relief - expired batch', 0, 1, 'Active');
GO

PRINT 'SmartMed Database created successfully!';
GO
