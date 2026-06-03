FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY CareLink.sln ./
COPY CareLink.Api/CareLink.Api.csproj CareLink.Api/
COPY CareLink.Application/CareLink.Application.csproj CareLink.Application/
COPY CareLink.Persistence/CareLink.Persistence.csproj CareLink.Persistence/
COPY CareLink.Security/CareLink.Security.csproj CareLink.Security/
COPY CareLink.Domain/CareLink.Domain.csproj CareLink.Domain/
RUN dotnet restore CareLink.Api/CareLink.Api.csproj
COPY . .
RUN dotnet publish CareLink.Api/CareLink.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CareLink.Api.dll"]
