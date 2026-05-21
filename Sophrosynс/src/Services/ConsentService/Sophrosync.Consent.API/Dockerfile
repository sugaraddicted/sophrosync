FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Shared/Sophrosync.SharedKernel/Sophrosync.SharedKernel.csproj", "src/Shared/Sophrosync.SharedKernel/"]
COPY ["src/Services/ConsentService/Sophrosync.Consent.Domain/Sophrosync.Consent.Domain.csproj", "src/Services/ConsentService/Sophrosync.Consent.Domain/"]
COPY ["src/Services/ConsentService/Sophrosync.Consent.Application/Sophrosync.Consent.Application.csproj", "src/Services/ConsentService/Sophrosync.Consent.Application/"]
COPY ["src/Services/ConsentService/Sophrosync.Consent.Infrastructure/Sophrosync.Consent.Infrastructure.csproj", "src/Services/ConsentService/Sophrosync.Consent.Infrastructure/"]
COPY ["src/Services/ConsentService/Sophrosync.Consent.API/Sophrosync.Consent.API.csproj", "src/Services/ConsentService/Sophrosync.Consent.API/"]
RUN dotnet restore "src/Services/ConsentService/Sophrosync.Consent.API/Sophrosync.Consent.API.csproj"

COPY . .
RUN dotnet publish "src/Services/ConsentService/Sophrosync.Consent.API/Sophrosync.Consent.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Sophrosync.Consent.API.dll"]
