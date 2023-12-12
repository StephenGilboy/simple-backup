FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SimpleBackup.App/SimpleBackup.App.csproj", "src/SimpleBackup.App/"]
RUN dotnet restore "src/SimpleBackup.App/SimpleBackup.App.csproj"
COPY . .
WORKDIR "/src/src/SimpleBackup.App"
RUN dotnet build "SimpleBackup.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleBackup.App.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleBackup.App.dll"]
