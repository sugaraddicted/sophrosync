FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Services/NotesService/Sophrosync.Notes.API/Sophrosync.Notes.API.csproj \
    -c Release -o /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "Sophrosync.Notes.API.dll"]
