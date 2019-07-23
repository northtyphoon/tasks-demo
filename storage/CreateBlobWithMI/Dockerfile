FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["CreateBlobWithMI.csproj", ""]
RUN dotnet restore "CreateBlobWithMI.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "CreateBlobWithMI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "CreateBlobWithMI.csproj" -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "/app/CreateBlobWithMI.dll"]