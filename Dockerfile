# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY MakeItShort.sln .
COPY MakeItShort.API/MakeItShort.API.csproj MakeItShort.API/
COPY MakeItShort.API.Test/MakeItShort.API.Test.csproj MakeItShort.API.Test/

# Restore dependencies
RUN dotnet restore MakeItShort.sln

# Copy the rest of the source code
COPY . .

# Build and publish
WORKDIR /src/MakeItShort.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080
ENTRYPOINT ["dotnet", "MakeItShort.API.dll"]
