FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Shared/Sophrosync.SharedKernel/Sophrosync.SharedKernel.csproj", "src/Shared/Sophrosync.SharedKernel/"]
COPY ["src/Gateway/Sophrosync.Gateway/Sophrosync.Gateway.csproj", "src/Gateway/Sophrosync.Gateway/"]
RUN dotnet restore "src/Gateway/Sophrosync.Gateway/Sophrosync.Gateway.csproj"

COPY . .
RUN dotnet publish "src/Gateway/Sophrosync.Gateway/Sophrosync.Gateway.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Sophrosync.Gateway.dll"]
