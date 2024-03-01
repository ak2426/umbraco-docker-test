FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY "UmbracoDocker.csproj" .
RUN dotnet restore "UmbracoDocker.csproj"
COPY . .
RUN dotnet build "UmbracoDocker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UmbracoDocker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN chown app:app .
COPY --from=publish --chown=app:app /app/publish .

USER app
RUN mkdir -p wwwroot/media
ENTRYPOINT ["dotnet", "UmbracoDocker.dll"]
