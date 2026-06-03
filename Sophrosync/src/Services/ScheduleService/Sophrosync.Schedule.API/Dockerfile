FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Shared/Sophrosync.SharedKernel/Sophrosync.SharedKernel.csproj", "src/Shared/Sophrosync.SharedKernel/"]
COPY ["src/Services/ScheduleService/Sophrosync.Schedule.Domain/Sophrosync.Schedule.Domain.csproj", "src/Services/ScheduleService/Sophrosync.Schedule.Domain/"]
COPY ["src/Services/ScheduleService/Sophrosync.Schedule.Application/Sophrosync.Schedule.Application.csproj", "src/Services/ScheduleService/Sophrosync.Schedule.Application/"]
COPY ["src/Services/ScheduleService/Sophrosync.Schedule.Infrastructure/Sophrosync.Schedule.Infrastructure.csproj", "src/Services/ScheduleService/Sophrosync.Schedule.Infrastructure/"]
COPY ["src/Services/ScheduleService/Sophrosync.Schedule.API/Sophrosync.Schedule.API.csproj", "src/Services/ScheduleService/Sophrosync.Schedule.API/"]
RUN dotnet restore "src/Services/ScheduleService/Sophrosync.Schedule.API/Sophrosync.Schedule.API.csproj"

COPY . .
RUN dotnet publish "src/Services/ScheduleService/Sophrosync.Schedule.API/Sophrosync.Schedule.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Sophrosync.Schedule.API.dll"]
