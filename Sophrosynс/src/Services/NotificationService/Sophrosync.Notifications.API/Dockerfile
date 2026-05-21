FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Shared/Sophrosync.SharedKernel/Sophrosync.SharedKernel.csproj", "src/Shared/Sophrosync.SharedKernel/"]
COPY ["src/Services/NotificationService/Sophrosync.Notifications.Domain/Sophrosync.Notifications.Domain.csproj", "src/Services/NotificationService/Sophrosync.Notifications.Domain/"]
COPY ["src/Services/NotificationService/Sophrosync.Notifications.Application/Sophrosync.Notifications.Application.csproj", "src/Services/NotificationService/Sophrosync.Notifications.Application/"]
COPY ["src/Services/NotificationService/Sophrosync.Notifications.Infrastructure/Sophrosync.Notifications.Infrastructure.csproj", "src/Services/NotificationService/Sophrosync.Notifications.Infrastructure/"]
COPY ["src/Services/NotificationService/Sophrosync.Notifications.API/Sophrosync.Notifications.API.csproj", "src/Services/NotificationService/Sophrosync.Notifications.API/"]
RUN dotnet restore "src/Services/NotificationService/Sophrosync.Notifications.API/Sophrosync.Notifications.API.csproj"

COPY . .
RUN dotnet publish "src/Services/NotificationService/Sophrosync.Notifications.API/Sophrosync.Notifications.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Sophrosync.Notifications.API.dll"]
