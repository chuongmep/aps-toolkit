FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ExploreForgeGeometry/ExploreForgeGeometry.csproj", "ExploreForgeGeometry/"]
RUN dotnet restore "ExploreForgeGeometry/ExploreForgeGeometry.csproj"
COPY . .
WORKDIR "/src/ExploreForgeGeometry"
RUN dotnet build "ExploreForgeGeometry.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ExploreForgeGeometry.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExploreForgeGeometry.dll"]
