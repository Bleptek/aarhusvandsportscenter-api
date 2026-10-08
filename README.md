# Aarhus Vandsportscenter - REST API

This serves as the backend for the aarhusvandsportscenter.dk website.

## About this project

Hosted at:

- https://aarhusvandsportscenter.dk/api/v1 (prod)
- https://dev.aarhusvandsportscenter.dk/api/v1 (dev)

**Highlights:**

- .NET 10 / ASP.NET Core controllers with MediatR commands and queries (`Domain/Commands`, `Domain/Queries`).
- MySQL 5.7 database administered with Entity Framework Core (Pomelo provider), code-first with migrations.
- JWT bearer authentication.
- Rental, rental product/category/price, content page and account endpoints under `/api/v1`.
- The API is documented using Swagger UI at `/swagger`.

### Project structure

```
src/Aarhusvandsportscenter.Api/
  Controllers/      HTTP endpoints and request/response models
  Domain/           Commands, queries, services and domain exceptions
  Infastructure/    Authorization, database (DbContext, migrations) and middleware
  app_offline/      app_offline.htm used while deploying
test/Aarhusvandsportscenter.Api.Tests/
  Controllers/      Acceptance tests for the endpoints
  TestUtils/        WebApplicationFactory setup
```

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for the local MySQL database)

## Getting started

1. Start a local database, see [Local MySQL database](#local-mysql-database-docker).
2. Review the [configuration](#configuration).
3. Run the app:

```sh
dotnet run --project ./src/Aarhusvandsportscenter.Api/
```

Migrations are applied automatically on startup. Open `/swagger` on the printed URL to explore the API.

## Local MySQL database (Docker)

The default connection string in `appsettings.json` expects a MySQL server on `localhost:3306` with the database `aarhusvandsportscenter_dk_db_local`, user `root` and password `abcd1234`.
Start a matching container with:

```sh
docker run -d --name avsc-mysql -p 3306:3306 -e MYSQL_ROOT_PASSWORD=abcd1234 -e MYSQL_DATABASE=aarhusvandsportscenter_dk_db_local mysql:5.7
```

Do **not** use `MYSQL_ALLOW_EMPTY_PASSWORD=yes`. Root then has no password and the app fails on startup with `Access denied for user 'root'@'172.17.0.1' (using password: YES)`.

The password is only applied when the container is first created. If the container already exists with a different password, remove it with `docker rm -f avsc-mysql` and run the command again. This deletes its data, but migrations recreate the schema on the next app start.
Use `docker stop avsc-mysql` / `docker start avsc-mysql` to stop and start it without losing data.

## Configuration

Configuration is read from `appsettings.json`, then the optional `appsettings.Local.json`, then environment variables and command line arguments (later sources win).

Put personal or secret values in `src/Aarhusvandsportscenter.Api/appsettings.Local.json`. It is git-ignored, so it is safe for secrets:

```json
{
  "ConnectionStrings": {
    "DbConnection": "server=localhost;port=3306;database=aarhusvandsportscenter_dk_db_local;user=root;password=abcd1234"
  },
  "SimplySmtp": {
    "Password": "<smtp password>"
  },
  "Authorization": {
    "JwtKey": "<long random string>"
  }
}
```

Environment variables work too, using `__` as the separator for nested keys, e.g. `SimplySmtp__Password`.
In VS Code a `launch.json` configuration can set them in an `env` object:

```json
"env": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "SimplySmtp:Password": ""
}
```

| Section | Purpose |
|---|---|
| `ConnectionStrings:DbConnection` | MySQL connection string. |
| `Authorization` | JWT `JwtKey`, `Issuer`, `Audience` and `ExpirationInSeconds`. |
| `SimplySmtp` | SMTP account and links used for outgoing mail (password reset, rental confirmation/cancellation, contact form). The app currently sends mail through this. |
| `SendGrid` | SendGrid settings and template ids. Not used while the SendGrid package is commented out in the csproj. |
| `CreateDefaultAdminAccounts` / `DefaultAdminAccounts` | Admin accounts to create on startup. |
| `Rental:DefaultCategoryName` | Name of the default rental category. |

The seeding steps in `Program.cs` (`SeedDb`, `EnsureDefaultAdminAccounts`, `EnsureDefaultRentalCategory`) are commented out. Enable the ones you need in `Main` when setting up an empty database.

## Entity Framework

Migrations are applied when the application starts and should generally not be applied manually using the command line.
This means that upon deploying the app, migrations will automatically update the database.

While you are working with a new database schema you should **not** connect to the Test or Production database.
Point the connection string at the local Docker database (see above), e.g. through `appsettings.Local.json`, so you don't accidentally mess up the schema on Test or Production.

The EF provider is configured for MySQL 5.7 (the version used by the hosting provider), so keep the local container on `mysql:5.7`.

Install the EF global tool:

```sh
dotnet tool install --global dotnet-ef
```

Add a new migration:

```sh
cd ./src/Aarhusvandsportscenter.Api/
dotnet ef migrations add InitialCreate -o ./Infastructure/Database/Migrations/
```

## Tests

We're using xUnit. The general strategy is to acceptance test the endpoints in the API with HTTP requests using `WebApplicationFactory`, with an in-memory EF database, so no MySQL is needed to run them.
We go with a most coverage with least effort mindset.

Run them from the editor's test explorer or with:

```sh
dotnet test
```

The same command is run as part of the deploy pipeline. Nothing will be deployed if any test fails.

## Deployment & hosting

The application is hosted at Simply.com (IIS, Windows).
GitHub Actions (`.github/workflows/deploy.yml`) is used as CI/CD. A push to one of these branches builds, tests and deploys:

| Branch | Environment | Configuration |
|---|---|---|
| `dev` | https://dev.aarhusvandsportscenter.dk | Debug |
| `master` | https://aarhusvandsportscenter.dk | Release |

The pipeline then:

1. Runs `dotnet build` and `dotnet test`.
2. Substitutes environment specific values (hostname/links, contact email, SendGrid key, connection string, JWT key) into `appsettings.json` from GitHub secrets.
3. Publishes a self-contained `win-x86` build.
4. Uploads it by FTP, using an `app_offline.htm` file while the files are replaced.

Secrets used: `DBCONNSTR_DEV`, `DBCONNSTR_PROD`, `AUTH_JWTKEY_DEV`, `AUTH_JWTKEY_PROD`, `SENDGRID_APIKEY`, `EMAIL_PASSWORD`, `FTP_SERVER`, `FTP_USERNAME` and `FTP_PASSWORD`.

In the csproj file the following applies to the Debug configuration:

```xml
<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <AspNetCoreModuleName>AspNetCoreModule</AspNetCoreModuleName>
    <AspNetCoreHostingModel>OutOfProcess</AspNetCoreHostingModel>
</PropertyGroup>
```

This enables running multiple applications in the IIS app-pool on the server.
The main application runs in-process and all others run out-of-process.
