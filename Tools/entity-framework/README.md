
# Entity Framework Core

Microsoft Documentation: https://learn.microsoft.com/en-us/ef/core

The Entity Framework (EF) DbContext already implements both the Repository pattern and the Unit of Work pattern. This is stated in the
official DbContext summary. If you implement the repository pattern around EF Core, you are creating an abstraction over an abstraction,
leading to over-engineered solutions.  See [this article](https://antondevtips.com/blog/ef-core-in-clean-architecture-the-pragmatic-way)

## Docker

This example uses Postgres. To run a Docker container, use the following command:

```
docker run --name mypostgres -e POSTGRES_PASSWORD=mysecretpassword -p 5432:5432 -d --rm postgres:18-alpine
```


## Rider DB Tool

Create connections with the following URLs:

- `BookStore` => jdbc:postgresql://localhost:5432/BookStore
- `postgres` => jdbc:postgresql://localhost:5432/postgres


## Entity Framework Command-line Tools

The dotnet-tools.json file was created by doing:

```
dotnet new tool-manifest
dotnet tool install dotnet-ef 
```

This allows for a local rather than global install so different versions of dotnet-ef can be installed, and
for developers to install the correct version by running `dotnet tool restore`

## Migrations

Created migrations by doing `dotnet ef migrations add InitialCreate`

Got a note saying to undo this action, use `dotnet ef migrations remove`

To apply the migrations, use `dotnet ef database update`
