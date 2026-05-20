# itBudgetingV4

Simplified internal IT budgeting & forecasting platform built with ASP.NET Core MVC + EF Core.

## Default users
- CIO Secretary: `secretary` / `secret123`
- CIO: `cio` / `cio123`

## Run
```bash
dotnet restore ItBudgetingV4.sln
dotnet run --project /home/runner/work/itBudgetingV4/itBudgetingV4/ItBudgetingV4.Web/ItBudgetingV4.Web.csproj
```

In development the app uses an in-memory database (`UseInMemoryDatabase=true`).
Set `UseInMemoryDatabase=false` and configure `ConnectionStrings:DefaultConnection` to use SQL Server.
