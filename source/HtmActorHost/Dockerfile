FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["HtmAkkaHost/HtmAkkaHost.csproj", "HtmAkkaHost/"]
COPY ["NeoCortexUtils/NeoCortexUtils.csproj", "NeoCortexUtils/"]
COPY ["NeoCortexApi/NeoCortexApi.csproj", "NeoCortexApi/"]
COPY ["NeoCortexEntities/NeoCortexEntities.csproj", "NeoCortexEntities/"]
COPY ["NeoCortexArrayLib/NeoCortexArrayLib.csproj", "NeoCortexArrayLib/"]
COPY ["DistributedComputeLib/DistributedComputeLib.csproj", "DistributedComputeLib/"]
RUN dotnet restore "HtmAkkaHost/HtmAkkaHost.csproj"
COPY . .
WORKDIR "/src/HtmAkkaHost"
RUN dotnet build "HtmAkkaHost.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "HtmAkkaHost.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HtmAkkaHost.dll"]