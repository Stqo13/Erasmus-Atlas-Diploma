# ErasmusAtlas (Diploma Project)

ErasmusAtlas is a web app for Erasmus+ students to explore and share Erasmus experiences (posts) and discover Erasmus-related projects on an interactive map.

- **Backend:** ASP.NET Core MVC + Services + Repository pattern  
- **Auth:** ASP.NET Core Identity (custom Account views/actions)  
- **Database:** SQL Server + spatial support (geolocation points)  
- **Map:** MapLibre GL (markers grouped by location; topic-colored markers; clickable marker drawer)

---

## Features

### ✅ Implemented
- Custom **Register / Login / Logout** (Razor views, no scaffolding)
- **Seeded database** via JSON-based seeding + EF Core configurations
- **Cities + Posts** seeded with rich content (topic sentences + city snippets)
- **Interactive Map**
  - Marker clusters based on rounded location precision
  - Marker color based on dominant topic within the cluster
  - Click marker → right drawer with scrollable post previews

### 🚧 In progress / planned
- Posts CRUD pages + filtering/search
- Projects pages + applications workflow
- Roles & moderation
- Institutions directory + project linking
- Dashboard stats driven by DB

---

## Tech Stack

- **.NET:** ASP.NET Core MVC
- **ORM:** Entity Framework Core
- **Auth:** ASP.NET Core Identity
- **DB:** SQL Server (geography / NetTopologySuite)
- **Map:** MapLibre GL JS
- **UI:** Razor views + custom CSS (no Bootstrap)

---

## Solution Structure

```
ErasmusAtlas/
  ErasmusAtlas                 -> Web app (Controllers, Views, wwwroot)
  ErasmusAtlas.Core            -> Services layer (business logic)
  ErasmusAtlas.Infrastructure  -> DbContext, entity models, repositories, migrations, seed JSON
  ErasmusAtlas.ViewModels      -> View models
  ErasmusAtlas.Common          -> Constants, helpers, custom errors
```

---

## Prerequisites

- .NET SDK (recommended: latest LTS)
- SQL Server (LocalDB or full instance)
- Visual Studio (recommended) or VS Code

---

## Database & Migrations

### Packages
You should have:
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `NetTopologySuite` (spatial types)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Newtonsoft.Json` (seed JSON)

### Migrations (Package Manager Console)
Run inside the project that contains your `DbContext` (typically **ErasmusAtlas.Infrastructure**):

```powershell
Add-Migration InitialCreate
Update-Database
```

> If you already have migrations, just run:
```powershell
Update-Database
```

---

## Seeding

Seeding is done via:
- `IEntityTypeConfiguration<T>` classes
- JSON files located in:
  - `ErasmusAtlas.Infrastructure/Data`

Seeding runs during migrations / startup depending on configuration.

---

## Running the App

1. Configure connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ErasmusAtlas;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

2. Run the web project:

- Visual Studio: Set **ErasmusAtlas** as Startup Project → Run  
- CLI:

```bash
dotnet run --project ErasmusAtlas
```

3. Open:
- Home/Dashboard: `/`
- Map page: `/Map`

---

## Map Endpoint Format

The Map page consumes a non-GeoJSON endpoint shaped like:

```json
{
  "items": [
    {
      "lat": 41.9028,
      "lng": 12.4964,
      "posts": [
        {
          "id": "uuid",
          "title": "Housing tips in Rome",
          "body": "Rent early!",
          "topics": ["Housing"],
          "created_at": "2025-01-10"
        }
      ]
    }
  ]
}
```

Markers are grouped by rounded lat/lng precision for performance.

---

## Roles (planned)

Planned roles for Identity:
- `Student` (default)
- `Moderator` (review/approve content)
- `Admin` (full access)

---

## Contributing

This is a diploma project, but PRs and suggestions are welcome.

- Keep code organized by layer (Controllers → Services → Repositories).
- Use ViewModels for all UI binding.
- Use DataAnnotations for model validation.
- No Bootstrap — CSS is written manually.

---

## License

MIT (or set your preferred license).

