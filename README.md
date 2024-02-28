# Umbraco + Docker

Deploy [Umbraco CMS][umbraco-cms] using Docker.

## About

This project was created from scratch starting with the default Umbraco template.

```sh
dotnet new umbraco -n UmbracoDocker -o .
```

Umbraco has been unfortunately been designed to only support SQL Server + IIS in production mode.
This makes developing a Docker image for Umbraco somewhat challenging.
Below are some of the challenges and design considerations for this project.

### SQL Server in Docker

Most official Docker images for databases, such as [PostgreSQL][docker-postgres] and [MySQL][docker-mysql], have an option to specify the default database name via environment variable.
[Microsoft SQL Server][docker-mssql-server], however, has no such option and will not create any database automatically on startup.
This [issue][github-mssql-docker] has been unresolved for over 7 years!

To work around this, there is an extra service called `db-init` to create the database if it does not exist already.
Its sole purpose is to run the following T-SQL code and exit.

```sql
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'UmbracoCMS')
    CREATE DATABASE UmbracoCMS
```

In many production scenarios it is preferable to use an external database.
This Docker Compose solution is mainly provided for the sake of completeness.

## Usage

This section is a work in progress.

```sh
docker compose up
```

<!-- Link definitions -->

[docker-mysql]: <https://hub.docker.com/_/mysql> "mysql - Official Image | Docker Hub"
[docker-postgres]: <https://hub.docker.com/_/postgres> "postgres - Official Image | Docker Hub"
[docker-mssql-server]: <https://hub.docker.com/_/microsoft-mssql-server> "microsoft-mssql-server - Official Image | Docker Hub"
[github-mssql-docker]: <https://github.com/microsoft/mssql-docker/issues/2> "Creating a database automatically upon startup · Issue #2 · microsoft/mssql-docker"
[umbraco-cms]: <https://umbraco.com/> "Umbraco - the flexible open-source .NET (ASP.NET Core) CMS"
