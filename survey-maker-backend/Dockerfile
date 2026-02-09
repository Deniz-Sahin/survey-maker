# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Debug -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy published output
COPY --from=build /out .

# Expose port (optional but nice)
EXPOSE 80

# Run app
ENTRYPOINT ["dotnet", "survey-maker-backend.dll"]
