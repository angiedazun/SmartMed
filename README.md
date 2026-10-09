# SmartMed – Pharmacy Management System

![C#](https://img.shields.io/badge/C%23-.NET%20Framework%204.8-512BD4?logo=dotnet&logoColor=white)
![WinForms](https://img.shields.io/badge/UI-Windows%20Forms-0078D4?logo=windows&logoColor=white)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server%20Express-CC2927?logo=microsoftsqlserver&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)

SmartMed is a desktop application that streamlines day-to-day pharmacy operations. Administrators manage medicine stock, suppliers, discounts, orders and reports, while customers can browse medicines, place orders and track their purchase history, all from a modern, themed interface.

Developed as a university Application Development coursework project.

---

## Features

### Administrator
- **Dashboard** with key business metrics and charts
- **Medicine management**: codes, dosage, pricing, stock levels, minimum-stock alerts, expiry dates, prescription-required flag
- **Categories & suppliers** management
- **Discount campaigns** with percentage, start/end dates and active status
- **Order management** and order status tracking
- **Customer management**
- **Reports** with export to **PDF** (iTextSharp) and **Excel** (EPPlus)
- **Settings** and activity logging

### Customer
- Self-registration and secure login
- Search and browse available medicines
- Shopping cart and checkout
- Prescription handling for restricted medicines
- Order history and profile management

### Security & Quality
- Passwords hashed with **BCrypt** (work factor 11)
- Input validation utilities and strong-password policy
- Layered architecture separating UI, services, repositories and data access

---

## Tech Stack

| Layer          | Technology                                              |
| -------------- | ------------------------------------------------------- |
| Language       | C# (.NET Framework 4.8)                                 |
| UI             | Windows Forms, Guna.UI2.WinForms, FontAwesome.Sharp     |
| Database       | Microsoft SQL Server (Express) via `System.Data.SqlClient` |
| Reporting      | iTextSharp 5.5 (PDF), EPPlus 6.2 (Excel)                |
| Security       | BCrypt.Net-Next                                         |

---

## Project Structure

```
SmartMed/
├── Data/          # Database connection and helper classes
├── Database/      # SmartMed.sql – schema and seed data
├── Forms/
│   ├── Admin/     # Admin dashboard and management panels
│   ├── Customer/  # Customer storefront, cart, checkout, orders
│   └── Login/     # Login and registration forms
├── Models/        # Domain entities (Medicine, Order, Customer, ...)
├── Repository/    # Data-access layer (one repository per entity)
├── Services/      # Business logic (login, orders, medicines, notifications)
├── UI/            # Shared theme, state and UI helpers
├── Utilities/     # PDF/Excel exporters, password hasher, validation
├── Assets/        # Icons, logo and images
└── Program.cs     # Application entry point
```

---

## Getting Started

### Prerequisites
- Windows 10 or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **.NET desktop development** workload
- .NET Framework 4.8 Developer Pack
- SQL Server Express (instance name `SQLEXPRESS`) and SQL Server Management Studio (SSMS)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/angiedazun/SmartMed.git
   cd SmartMed
   ```

2. **Create the database**
   Open `Database/SmartMed.sql` in SSMS and execute it. This creates the `SmartMedDB` database, all tables and sample seed data.

3. **Check the connection string**
   The default connection in [`Data/DBConnection.cs`](Data/DBConnection.cs) is:
   ```
   Server=.\SQLEXPRESS;Database=SmartMedDB;Integrated Security=True;
   ```
   Update the server name there if your SQL Server instance is different.

4. **Build and run**
   Open `SmartMed.csproj` in Visual Studio, restore NuGet packages, then press **F5**. Or from the command line:
   ```bash
   dotnet build
   ```

### Default Administrator Login

| Username | Password    |
| -------- | ----------- |
| `admin`  | `Admin@123` |

> **Note:** Change the default password after first login. Customers can create their own accounts from the registration screen.

---

## Database Overview

`Admin`, `Customer`, `Category`, `Supplier`, `Discount`, `Medicine`, `Order`, `OrderDetail`, `Prescription`, `ActivityLog`

---

## Author

**Anjana Dasun Dissanayake**
GitHub: [@angiedazun](https://github.com/angiedazun)
