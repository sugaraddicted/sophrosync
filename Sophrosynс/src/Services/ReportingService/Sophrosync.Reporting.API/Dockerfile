FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Shared/Sophrosync.SharedKernel/Sophrosync.SharedKernel.csproj", "src/Shared/Sophrosync.SharedKernel/"]
COPY ["src/Services/ReportingService/Sophrosync.Reporting.Domain/Sophrosync.Reporting.Domain.csproj", "src/Services/ReportingService/Sophrosync.Reporting.Domain/"]
COPY ["src/Services/ReportingService/Sophrosync.Reporting.Application/Sophrosync.Reporting.Application.csproj", "src/Services/ReportingService/Sophrosync.Reporting.Application/"]
COPY ["src/Services/ReportingService/Sophrosync.Reporting.Infrastructure/Sophrosync.Reporting.Infrastructure.csproj", "src/Services/ReportingService/Sophrosync.Reporting.Infrastructure/"]
COPY ["src/Services/ReportingService/Sophrosync.Reporting.API/Sophrosync.Reporting.API.csproj", "src/Services/ReportingService/Sophrosync.Reporting.API/"]
RUN dotnet restore "src/Services/ReportingService/Sophrosync.Reporting.API/Sophrosync.Reporting.API.csproj"

COPY . .
RUN dotnet publish "src/Services/ReportingService/Sophrosync.Reporting.API/Sophrosync.Reporting.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Sophrosync.Reporting.API.dll"]
