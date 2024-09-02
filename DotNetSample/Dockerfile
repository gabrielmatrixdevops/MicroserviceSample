FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DotNetSample.csproj", "."]
RUN dotnet restore "./DotNetSample.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./DotNetSample.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Generate the HTTPS developer certificate
RUN dotnet dev-certs https -ep /root/.aspnet/https/aspnetapp.pfx -p password
RUN dotnet dev-certs https --trust

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DotNetSample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /root/.aspnet/https /root/.aspnet/https
COPY --from=publish /app/publish .
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_URLS="https://+:8081;http://+:8080"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="password"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/root/.aspnet/https/aspnetapp.pfx"
ENTRYPOINT ["dotnet", "DotNetSample.dll"]