FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Shared/Sophrosync.SharedKernel/Sophrosync.SharedKernel.csproj", "src/Shared/Sophrosync.SharedKernel/"]
COPY ["src/Services/ClientService/Sophrosync.Clients.Domain/Sophrosync.Clients.Domain.csproj", "src/Services/ClientService/Sophrosync.Clients.Domain/"]
COPY ["src/Services/ClientService/Sophrosync.Clients.Application/Sophrosync.Clients.Application.csproj", "src/Services/ClientService/Sophrosync.Clients.Application/"]
COPY ["src/Services/ClientService/Sophrosync.Clients.Infrastructure/Sophrosync.Clients.Infrastructure.csproj", "src/Services/ClientService/Sophrosync.Clients.Infrastructure/"]
COPY ["src/Services/ClientService/Sophrosync.Clients.API/Sophrosync.Clients.API.csproj", "src/Services/ClientService/Sophrosync.Clients.API/"]
RUN dotnet restore "src/Services/ClientService/Sophrosync.Clients.API/Sophrosync.Clients.API.csproj"

COPY . .
RUN dotnet publish "src/Services/ClientService/Sophrosync.Clients.API/Sophrosync.Clients.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Sophrosync.Clients.API.dll"]
