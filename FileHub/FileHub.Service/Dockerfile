FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FileHub.Service/FileHub.Service.csproj", "FileHub.Service/"]
RUN dotnet restore "FileHub.Service/FileHub.Service.csproj"
COPY . .
WORKDIR "/src/FileHub.Service"
RUN dotnet build "FileHub.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileHub.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileHub.Service.dll"]
