FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY ./artifacts/api/ ./
ENTRYPOINT ["dotnet", "LegacyMigration.Api.dll"]
