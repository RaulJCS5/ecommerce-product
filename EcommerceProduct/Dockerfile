# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EcommerceProduct.API/EcommerceProduct.API.csproj", "EcommerceProduct.API/"]
RUN dotnet restore "./EcommerceProduct.API/EcommerceProduct.API.csproj"
COPY . .
WORKDIR "/src/EcommerceProduct.API"
RUN dotnet build "./EcommerceProduct.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EcommerceProduct.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final

# begin for http only
# ENV ASPNETCORE_URLS=http://+:5001
# ENV ASPNET_HTTP_PORTS=5001
# end for http only

# begin for https and http
ENV ASPNETCORE_URLS=https://+:5001;http://+:5000
ENV ASPNET_HTTPS_PORTS=5001
ENV ASPNET_HTTP_PORTS=5000
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=<YourPasswordHere>
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/aspnetapp.pfx
EXPOSE 5000
# end for https and http
EXPOSE 5001
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcommerceProduct.API.dll"]
