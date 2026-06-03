FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Shared/Sophrosync.SharedKernel/Sophrosync.SharedKernel.csproj", "src/Shared/Sophrosync.SharedKernel/"]
COPY ["src/Services/NotesService/Sophrosync.Notes.Domain/Sophrosync.Notes.Domain.csproj", "src/Services/NotesService/Sophrosync.Notes.Domain/"]
COPY ["src/Services/NotesService/Sophrosync.Notes.Application/Sophrosync.Notes.Application.csproj", "src/Services/NotesService/Sophrosync.Notes.Application/"]
COPY ["src/Services/NotesService/Sophrosync.Notes.Infrastructure/Sophrosync.Notes.Infrastructure.csproj", "src/Services/NotesService/Sophrosync.Notes.Infrastructure/"]
COPY ["src/Services/NotesService/Sophrosync.Notes.API/Sophrosync.Notes.API.csproj", "src/Services/NotesService/Sophrosync.Notes.API/"]
RUN dotnet restore "src/Services/NotesService/Sophrosync.Notes.API/Sophrosync.Notes.API.csproj"

COPY . .
RUN dotnet publish "src/Services/NotesService/Sophrosync.Notes.API/Sophrosync.Notes.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Sophrosync.Notes.API.dll"]
