FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/CollectA.Api/CollectA.Api.csproj", "src/CollectA.Api/"]
COPY ["src/CollectA.Application/CollectA.Application.csproj", "src/CollectA.Application/"]
COPY ["src/CollectA.Domain/CollectA.Domain.csproj", "src/CollectA.Domain/"]
COPY ["src/CollectA.Infrastructure/CollectA.Infrastructure.csproj", "src/CollectA.Infrastructure/"]
RUN dotnet restore "src/CollectA.Api/CollectA.Api.csproj"
COPY . .
WORKDIR "/src/src/CollectA.Api"
RUN dotnet build "CollectA.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CollectA.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CollectA.Api.dll"]
