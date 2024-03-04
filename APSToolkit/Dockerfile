FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["APSToolkit/APSToolkit.csproj", "APSToolkit/"]
RUN dotnet restore "APSToolkit/APSToolkit.csproj"
COPY . .
WORKDIR "/src/APSToolkit"
RUN dotnet build "APSToolkit.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "APSToolkit.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "APSToolkit.dll"]
