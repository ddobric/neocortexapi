# Fetch base image from DockerHub
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Copy everything
COPY . ./

# Restore dependent projects
RUN dotnet restore NeoCortexApi/NeoCortexApi.csproj
RUN dotnet restore NeoCortexArrayLib/NeoCortexArrayLib.csproj
RUN dotnet restore NeoCortexEntities/NeoCortexEntities.csproj
RUN dotnet restore NeoCortexUtils/NeoCortexUtils.csproj
RUN dotnet restore GridCell/GridCell.csproj
RUN dotnet restore Samples/NeoCortexApiSample/NeoCortexApiSample.csproj

# Restore solution
RUN dotnet restore NeoCortexApi.sln


RUN dotnet build Samples/NeoCortexApiSample/NeoCortexApiSample.csproj

RUN dotnet publish Samples/NeoCortexApiSample/NeoCortexApiSample.csproj -c Release -o output

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

COPY --from=build-env /output .

ENTRYPOINT ["dotnet", "NeoCortexApiSample.dll"]