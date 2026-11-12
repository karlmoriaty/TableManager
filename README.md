# TableManager

A desktop ERP/CRM for small businesses: products and stock, customers and contacts,
sales orders, and reports — all in one local SQLite database.

## Features

- **Dashboard** — product, customer and order counts (charts are still a placeholder).
- **Products** — CRUD, stock lookup, stock adjustments.
- **Customers** — customer records, contacts, follow-up notes.
- **Orders** — create sales orders, track status, query and summarize.
- **Stock** — inbound and outbound movements, low-stock warnings.
- **Export and backup** — Excel (`.xlsx`) exports for products, customers, orders
  and stock records; compressed database backups and restore.

## Architecture

```
TableManager.UI    WPF views, ViewModels, converters (CommunityToolkit.Mvvm)
TableManager.BLL   services: products, customers, orders, stock, export, database
TableManager.DAL   SQLite context, models, repositories
```

The UI talks to services, services talk to repositories; there is no ORM — the DAL
runs SQL against Microsoft.Data.Sqlite directly.

## Stack

C# 12, WPF on .NET 8, Microsoft.Data.Sqlite, CommunityToolkit.Mvvm, EPPlus for Excel
export.

## Build and run

```bash
dotnet build TableManager.sln
dotnet run --project TableManager.UI/TableManager.UI.csproj
```

On first start the database is created next to the executable as `tablemanager.db`.
Backups are written to a `Backups` folder next to the executable.

## Notes

- Single-user, single-machine: no server, no sync, no multi-user locking.
- The UI is in Chinese and ships as Chinese Excel headers.
- No unit tests yet.

## License

MIT
