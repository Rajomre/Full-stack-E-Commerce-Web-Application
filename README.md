# 🛒 Shopping Web Application (E-Commerce Platform)

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET MVC](https://img.shields.io/badge/ASP.NET-MVC%205-0078D7?style=for-the-badge&logo=microsoft)](https://dotnet.microsoft.com/apps/aspnet/mvc)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-6-68217A?style=for-the-badge&logo=visualstudio)](https://docs.microsoft.com/en-us/ef/ef6/)
[![SQL Server](https://img.shields.io/badge/Microsoft%20SQL%20Server-CC292B?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/en-us/sql-server)

> A full-featured, secure, and production-grade E-Commerce Web Application built with **ASP.NET MVC 5**, **Entity Framework 6**, and **SQL Server**. Features a modern **Glassmorphism UI (Black / White / Red)**, complete dual-role architecture (`Admin` vs `User`), persistent database shopping cart, automated inventory management, and self-service order lifecycle.

---

## 📌 Technical Specifications & Stack

| Layer | Technology / Tool | Description |
| :--- | :--- | :--- |
| **Backend Framework** | ASP.NET MVC 5 (.NET Framework 4.8) | Robust MVC architecture with separation of concerns |
| **Language** | C# | Strongly typed business & controller logic |
| **ORM / Data Access** | Entity Framework 6 (EF6) | `ShoppingDbContext` Code-First / Database mapping |
| **Database** | Microsoft SQL Server | Relational database (`ShoppingWebApplicationDB`) |
| **Frontend Engine** | Razor View Engine (`.cshtml`) | Dynamic HTML rendering with partial views & layouts |
| **Styling & Theme** | CSS3 & Modern Glassmorphism | Custom dark sidebar, frosted glass cards, Red accent (`#e50914`) |
| **Security & Auth** | Custom `PasswordHasher` + Session RBAC | Secure PBKDF2/SHA-based hashing with session-based access control |
| **Client Interaction**| JavaScript / jQuery & HTML5 | Client-side validation, interactive quantity selectors, invoice printing |

---

## 🏗️ System Architecture & Workflow

```text
Browser / Client Request
          │
          ▼
ASP.NET MVC Controllers (Account, Dashboard, Product, Cart, Wishlist, Admin)
          │
          ▼
ViewModels / Entity Models (Data Validation & Transfer)
          │
          ▼
Entity Framework 6 (ShoppingDbContext)
          │
          ▼
Microsoft SQL Server Database (Relational Schema)
