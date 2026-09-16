# GameChanger

GameChanger helps one person run a focused improvement cycle: define a fixed period, add goals, review progress weekly, and learn from the result.

The first product slice creates a personal cycle in `Draft` status through the React UI and `/api/v1/cycles`, with data stored in SQL Server.

## Technology

- React 19, Vinext, and Tailwind CSS 4
- ASP.NET Core on .NET 10
- Entity Framework Core 10 and SQL Server
- xUnit integration tests with SQL Server Testcontainers

## Run locally

Prerequisites: Node.js 22.13 or newer, .NET SDK 10, and Docker Desktop.

### 1. Start SQL Server

Choose a strong local password, then start the pinned development database from PowerShell:

```powershell
$gameChangerSqlPassword = Read-Host "SQL Server password"
docker run --name gamechanger-sql -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=$gameChangerSqlPassword" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04
```

Store the matching connection string in .NET user secrets. It is never committed to the repository:

```powershell
dotnet user-secrets set "ConnectionStrings:GameChanger" "Server=localhost,1433;Database=GameChanger;User Id=sa;Password=$gameChangerSqlPassword;TrustServerCertificate=True" --project .\src\GameChanger.Api\GameChanger.Api.csproj
```

On later runs, restart the existing container with `docker start gamechanger-sql`.

### 2. Start the API

```powershell
dotnet run --project .\src\GameChanger.Api\GameChanger.Api.csproj
```

In Development, the API applies pending EF Core migrations and listens at `http://localhost:5080`. It stops immediately with a configuration message if the database connection string is missing.

### 3. Start the web app

In another terminal:

```powershell
npm run install:ci
Copy-Item .env.example .env.local
npm run dev
```

Open `http://localhost:5173`.

## Validate changes

```powershell
npm run lint
npm run build
dotnet restore GameChanger.slnx --locked-mode
dotnet build GameChanger.slnx --configuration Release --no-restore
dotnet test GameChanger.slnx --configuration Release --no-build --no-restore
```

The API tests start a disposable SQL Server 2022 container. See [the development workflow](./docs/development-workflow.md) for branch, pull-request, and independent-review expectations.
