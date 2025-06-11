FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/ioBox.Web.Demo.SqlServer/ioBox.Web.Demo.SqlServer.csproj", "src/ioBox.Web.Demo.SqlServer/"]
COPY ["src/ioBox.SqlServer/ioBox.SqlServer.csproj", "src/ioBox.SqlServer/"]
COPY ["src/ioBox/ioBox.csproj", "src/ioBox/"]
RUN dotnet restore "./src/ioBox.Web.Demo.SqlServer/ioBox.Web.Demo.SqlServer.csproj"
COPY . .
WORKDIR "/src/src/ioBox.Web.Demo.SqlServer"
RUN dotnet build "./ioBox.Web.Demo.SqlServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ioBox.Web.Demo.SqlServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ioBox.Web.Demo.SqlServer.dll"]
