# Umbraco + Docker

Deploy [Umbraco CMS][umbraco-cms] in headless production environment using Docker.

## About

Umbraco is a decoupled content management system (CMS) written in ASP.NET Core.
The term *decoupled* refers to an architecture that separates the frontend template system in such a way that using it is not required.
As a result, Umbraco can be used in nearly headless manner by changing a few configuration settings.

The main advantages of headless operation include:

- Being able to use any frontend technology stack
- Increased security due to a smaller attack surface
- Transferability of design principles to other headless CMS platforms
- Reduced dependency on plugins for functionality

This goal of this project is to provide a lean and portable Docker image that can be used to deploy Umbraco in a headless production environment.

## Usage

The following commands give an example of how to build and run the Docker image after cloning this repository.

```sh
docker build -t umbraco .
docker volume create umbraco-media

docker run -it --rm \
    -e "ConnectionStrings__umbracoDbDSN=$MY_CONNECTION_STRING" \
    -p "8080:8080" \
    -v umbraco-media:/app/wwwroot/media \
    umbraco
```

Navigate to <http://localhost:8080/umbraco> and login with the following credentials (currently hardcoded in [`appsettings.Production.json`](appsettings.Production.json)):

- Email: `admin@example.com`
- Password: `admin-password`

### Docker Compose

Create a file called `.env` containing the following keys with your own values.

```sh
MSSQL_SA_PASSWORD="Pass@word"
```

> [!IMPORTANT]
> As stated [here][docker-mssql-server], `MSSQL_SA_PASSWORD` must satisfy the password complexity policy:
> > This password needs to include at least 8 characters of at least three of these four categories: uppercase letters, lowercase letters, numbers and non-alphanumeric symbols.

<!-- block quote separator for markdownlint -->

> [!CAUTION]
> Any special characters in `MSSQL_SA_PASSWORD`, such as `;` or `"`, must be properly escaped to avoid breaking the connection string.
> Refer to Microsoft's documentation on [connection string syntax][connection-string-syntax].

Then run the following command to create and start the containers.

```sh
docker compose up
```

## Design Challenges

Umbraco has clearly been made by and for those already knee-deep in Microsoft's software ecosystem.
The only supported database types are SQLite, SQL Server, and Azure SQL.
Production deployment is mainly intended for IIS or Azure.

Containerization with Docker is more or less an exercise in Linux system administration that is often at odds with Microsoft's design principles.
Here are some of the design challenges faced and their workarounds.

### Application Configuration

The usage of `appsettings*.json` files in ASP.NET encourages the bad practice of exposing application secrets in code repositories.
It is possible, however, to configure these settings with environment variables.
To handle nested JSON structures, a double underscore is used to concatenate keys, e.g., `ConnectionStrings__umbracoDbDSN`.

### Runtime Mode Validation

Umbraco has its own [runtime modes][umbraco-runtime-modes], separate from the usual [ASP.NET Core environments][aspnetcore-environments].
Selecting `Production` mode will enable several runtime mode validators, two of which are problematic:

1. `UmbracoApplicationUrlValidator` -
    Most web applications are designed to work regardless of what IP address and port number they happen to be running on.
    Likewise, it is unreasonable for Umbraco to require external information such as the application URL.
2. `UseHttpsValidator` -
    Unless you have specific security requirements, HTTPS is better left to a reverse-proxy server, such as [nginx](https://nginx.org), rather than the application itself.

Fortunately, these validators can be disabled with the following code in `Program.cs`:

```cs
using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

umbracoBuilder.RuntimeModeValidators()
    .Remove<UmbracoApplicationUrlValidator>()
    .Remove<UseHttpsValidator>();
```

### SQL Server in Docker

Most official Docker images for databases, such as [PostgreSQL][docker-postgres] and [MySQL][docker-mysql], have an option to specify the default database name via environment variable.
[Microsoft SQL Server][docker-mssql-server], however, has no such option and will not create any database automatically on startup.
This [issue][github-mssql-docker] has been unresolved for over 7 years!

To work around this, there is an extra service in [`docker-compose.yml`](docker-compose.yml) called `db-init` to create the database if it does not exist already.
Its sole purpose is to run the following T-SQL code and exit.

```sql
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'UmbracoCMS')
    CREATE DATABASE UmbracoCMS
```

In many production scenarios it is preferable to use an external database.
This Docker Compose solution is mainly provided for the sake of completeness.

<!-- Link definitions -->

[aspnetcore-environments]: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-8.0> "Use multiple environments in ASP.NET Core | Microsoft Learn"
[connection-string-syntax]: <https://learn.microsoft.com/en-us/sql/connect/ado-net/connection-strings?view=sql-server-ver16#connection-string-syntax> "Connection strings - ADO.NET Provider for SQL Server | Microsoft Learn"
[docker-mysql]: <https://hub.docker.com/_/mysql> "mysql - Official Image | Docker Hub"
[docker-postgres]: <https://hub.docker.com/_/postgres> "postgres - Official Image | Docker Hub"
[docker-mssql-server]: <https://hub.docker.com/_/microsoft-mssql-server> "microsoft-mssql-server - Official Image | Docker Hub"
[github-mssql-docker]: <https://github.com/microsoft/mssql-docker/issues/2> "Creating a database automatically upon startup · Issue #2 · microsoft/mssql-docker"
[umbraco-cms]: <https://umbraco.com/> "Umbraco - the flexible open-source .NET (ASP.NET Core) CMS"
[umbraco-runtime-modes]: <https://docs.umbraco.com/umbraco-cms/fundamentals/setup/server-setup/runtime-modes> "Runtime Modes - Umbraco CMS"
