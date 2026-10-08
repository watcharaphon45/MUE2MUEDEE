# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy the project files
COPY src/Mue2MueDee.Domain/Mue2MueDee.Domain.csproj src/Mue2MueDee.Domain/
COPY src/Mue2MueDee.Application/Mue2MueDee.Application.csproj src/Mue2MueDee.Application/
COPY src/Mue2MueDee.Infrastructure/Mue2MueDee.Infrastructure.csproj src/Mue2MueDee.Infrastructure/
COPY src/Mue2MueDee.Web/Mue2MueDee.Web.csproj src/Mue2MueDee.Web/

# Restore dependencies
RUN dotnet restore src/Mue2MueDee.Web/Mue2MueDee.Web.csproj

# Copy everything else and build
COPY . .
WORKDIR /app/src/Mue2MueDee.Web
RUN dotnet publish -c Release -o /out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Expose port (.NET 8/10 uses 8080 by default in containers)
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "Mue2MueDee.Web.dll"]
