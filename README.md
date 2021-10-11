# Aarhus Vandsportscenter - REST API

This serves as the backend for the aarhusvandsportscenter.dk website.

## About this project

Hosted at:

- http://aarhusvandsportscenter.dk/api/v1 (prod)
- http://århusvandsportscenter.dk/api/v1 (prod)
- http://dev.aarhusvandsportscenter.dk/api/v1 (dev)

**Highlights:**

- .Net 5
- The API is documented using Swagger UI at /swagger.
- The project administers an SQL Server database using Entity Framework and the code-first principle with migrations.

### Prerequisites

- .Net SDK 5.0.1

## Getting Started

### Environment variables

Read through the appsettings.json configuration file for any values missing.
This is essential before the project can run without error.
Using VS Code, a launch.json file can have a configuration containing an env object as such:

```
"env": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "someprop:nestedprop": ""
}
```

## Entity Framework

Migrations are applied when the application starts and should generally not be applied manually using the command line.
This means that upon deploying the app, migrations will automatically update the database.
it is important to configure the connection string in appsettings.json temporarily.
While you are working with a new database scheme you should **not** connect to the Test or Production database.
It should point at a local database running on your machine so you dont accidentally mess up the database scheme on Test or Production.
I use the following connection string to point at a locally running LocalDb, which can be installed as part of the SQL Server Express installation.

Install the EF global tool:
```sh
dotnet tool install --global dotnet-ef
```

To add new migrations
```sh
cd ./src/Aarhusvandsportscenter.Api/
dotnet ef migrations add InitialCreate -o ./Infastructure/Database/Migrations/
```

## Run the app

Using Dotnet

```sh
dotnet run --project ./src/Aarhusvandsportscenter.Api/
```

## Tests

We're using Xunit and NSubstitute as mocking framework.
We go with a most coverage with least effort mindset.
The general strategy for testing is to success acceptance test all endpoints in the API with HTTP requests using WebApplicationFactory.

To run tests one can use a test explorer in the code editor or run the command: `dotnet test`.
The same command is run as part of the deploy pipeline. Nothing will be deployed if any test fails.

## Deployment & hosting

The application is currently hosted at Simply.com.
We use github actions as CI/CD tool. Upon pushing to the Dev branch, any changes will be deployed to our Test server.
In the csproj file the following applies to the Debug configuration:

```xml
<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <AspNetCoreModuleName>AspNetCoreModule</AspNetCoreModuleName>
    <AspNetCoreHostingModel>OutOfProcess</AspNetCoreHostingModel>
</PropertyGroup>
```

This enables running multiples applications in the IIS app-pool on the server.
The main application runs In-process and all others run Out-of-process.
