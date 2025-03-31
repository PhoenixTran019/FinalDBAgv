create database Sahafa
go

use Sahafa
go

CREATE TABLE Supplier (
    SupplierID nvarchar(255) PRIMARY KEY,
    SupplierName NVARCHAR(100),
    Phone NVARCHAR(15),
    Address NVARCHAR(255),
    Email NVARCHAR(100)
);


CREATE TABLE Authors (
    AuthorID nvarchar(255) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
	DOB date,
    Bio NVARCHAR(MAX)
);
go

CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
	DOB date,
    Email NVARCHAR(255) UNIQUE,
    Phone NVARCHAR(20),
    Address NVARCHAR(255)
);
go

CREATE TABLE Employees (
    EmployeeID int Identity (1,2) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) UNIQUE,
    Phone NVARCHAR(20),
	DOB date,
    EmployeeType int -- 1 is Manager, 2 is Staff
);
go

CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL foreign key references Customers (CustomerID),
    EmployeeID int Not null foreign key references Employees (EmployeeID),
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10,2)
);
go

CREATE TABLE OrderDetails (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL Foreign Key References Orders(OrderID),
	ProductID nvarchar(255),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL
);
go

Create Table Book 
(
	BookID nvarchar(255) primary key,
	Title nvarchar(255),
	AuthorID nvarchar(255) Foreign Key References Authors (AuthorID),
	SupplierID nvarchar(255) Foreign Key References Supplier (SupplierID),
	PublicationYear int,
	Price Decimal (18,2),
	StockQuantity int,
	image nvarchar(255),
	Description nvarchar (maX)
);
go

Create Table Stationery
(
	StationeryID nvarchar(255) primary key,
	Name nvarchar(255),
	SupplierID nvarchar (255) Foreign key References Supplier(SupplierID),
	Price Decimal (18,2),
	StockQuantity int
);
go

Create Table AccountEmployee
(
	Username Nvarchar(255) primary key,
	EmployeeID int Foreign Key References Employees (EmployeeID),
	Password nvarchar(50),
	registrationdate date
);
go

Create Table AccountCustomer
(
	Username Nvarchar(255) Primary key,
	CustomerID int Foreign Key References Customers (CustomerID),
	Password nvarchar (50),
	registrationdate date
);
go

EXEC sp_columns 'Book'
EXEC sp_columns 'Authors'
EXEC sp_columns 'Supplier'
SELECT * FROM sys.tables WHERE name = 'Authors';
EXEC sp_rename 'Authors_Old', 'Authors';
go


go
ALTER TABLE Book DROP CONSTRAINT FK__Book__AuthorID__48CFD27E;
go
ALTER TABLE Book ALTER COLUMN AuthorID INT NULL;
go
ALTER TABLE Book
ALTER COLUMN AuthorID INT NULL;
go
ALTER TABLE Book
ADD CONSTRAINT FK_Book_Author
FOREIGN KEY (AuthorID) REFERENCES Authors(AuthorID)
ON DELETE SET NULL
ON UPDATE CASCADE;
go
SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('Book');

SELECT name FROM sys.tables WHERE name = 'Authors';

ALTER TABLE Book ALTER COLUMN AuthorID NVARCHAR(255) NULL;

SELECT * FROM Authors;

go
CREATE TABLE BookType (
    BookTypeID INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(255) NOT NULL
);

go
CREATE TABLE Book_BookType (
    BookID nvarchar(255),
    BookTypeID INT,
    PRIMARY KEY (BookID, BookTypeID),
    
    FOREIGN KEY (BookTypeID) REFERENCES BookType(BookTypeID) ON DELETE CASCADE,
	FOREIGN KEY (BookID) 
  REFERENCES Book(BookID) 
  ON DELETE CASCADE
  ON UPDATE CASCADE
);
go

create table RequestBook
(
	RequestID int Identity (1,1) Primary key,
	EmployeeID int Foreign Key References Employees (EmployeeID),
	RequestDate Datetime Default Getdate (),
	Status nvarchar(50) check (Status in ('Waitting', 'Approved', 'Rejected')) Default 'Waitting',
	ApprovedBy int Foreign key References Employees (EmployeeID),
	ApprovalDate Datetime null,
	Note nvarchar(max) null
);
go

create table RequestBookDetail
(
	DetailID int Identity (1,1) Primary key,
	RequestID int Foreign key references RequestBook (RequestID),
	BookID nvarchar (255) Foreign key References Book (BookID),
	Quantity int not null check (Quantity >0),
	ReceivedQuantity int null check (ReceivedQuantity >= 0),
);
go

create table RequestStationery
(
	RequestID int Identity (1,1) Primary key,
	EmployeeID int Foreign key references Employees (EmployeeID),
	RequestDate Datetime Default Getdate(),
	Status nvarchar(50) check (Status in ('Waitting', 'Approved', 'Rejected')) Default 'Waitting',
	ApprovedBy int Foreign key References Employees (EmployeeID),
	ApprovalDate Datetime null,
	Note nvarchar(max) null
);
go

Create table RequestStationeryDetail
(
	DetailID int identity (1,1) primary key,
	RequestID int Foreign key references RequestStationery (RequestID),
	StationeryID nvarchar(255) foreign key references Stationery (StationeryID),
	Quantity int not null check (Quantity > 0),
	ReceivedQuantity int null check (ReceivedQuantity >= 0)
);
go

SELECT name, OBJECT_NAME(parent_object_id) AS ReferencingTable
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('dbo.RequestStationery');

ALTER TABLE Book_BookType
  DROP CONSTRAINT FK__Book_Book__BookT__395884C4;

ALTER TABLE Book_BookType
  ADD CONSTRAINT FK_Book_BookType
      FOREIGN KEY (BookID) REFERENCES Book(BookID)
      ON UPDATE CASCADE
      ON DELETE CASCADE;

	  sp_help 'book'

go
drop table Book_BookType
go

CREATE TABLE Book_BookType
(
    BookID VARCHAR(255) NOT NULL,
    BookTypeID INT NOT NULL,

    -- Tùy chọn: Nếu muốn, có thể thêm các cột khác (ví dụ: CreatedDate, v.v.)

    -- Tạo khóa chính (nếu cần), thường là cặp (BookID, BookTypeID)
    CONSTRAINT PK_Book_BookType PRIMARY KEY (BookID, BookTypeID),

    -- Tạo khóa ngoại tham chiếu đến Book(BookID)
    CONSTRAINT FK_Book_BookType_Book
        FOREIGN KEY (BookID) 
        REFERENCES Book(BookID)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    -- Tạo khóa ngoại tham chiếu đến BookType(BookTypeID)
    CONSTRAINT FK_Book_BookType_BookType
        FOREIGN KEY (BookTypeID)
        REFERENCES BookType(BookTypeID)
        -- Tùy chọn: ON UPDATE CASCADE, ON DELETE CASCADE
);

go
create table StationeryType
(
	TypeID int identity (1,1) primary key,
	TypeName nvarchar(255)
);

go
ALTER TABLE Stationery
ADD TypeID INT
go

ALTER TABLE Stationery
ADD CONSTRAINT FK_Stationery_StationeryType
FOREIGN KEY (TypeID) REFERENCES StationeryType(TypeID);
go

ALTER TABLE Book
DROP CONSTRAINT FK__Book__SupplierID__49C3F6B7;

ALTER TABLE Book
ADD CONSTRAINT FK_Book_Supplier
    FOREIGN KEY (SupplierID) REFERENCES Supplier(SupplierID)
    ON UPDATE CASCADE
    ON DELETE SET NULL;
	go

	 sp_help 'stationery'
ALTER TABLE Stationery
DROP CONSTRAINT FK__Stationer__Suppl__4CA06362; -- tên ràng buộc hiện có

ALTER TABLE Stationery
ADD CONSTRAINT FK_Stationery_Supplier
    FOREIGN KEY (SupplierID) REFERENCES Supplier(SupplierID)
    ON UPDATE CASCADE
    ON DELETE SET NULL;
go
sp_help 'Stationery'

go
ALTER TABLE Stationery
DROP CONSTRAINT FK_Stationery_Supplier;

ALTER TABLE Stationery
ADD CONSTRAINT FK_Stationery_Supplier
    FOREIGN KEY (SupplierID) REFERENCES Supplier(SupplierID)
    ON UPDATE CASCADE
    ON DELETE SET NULL;