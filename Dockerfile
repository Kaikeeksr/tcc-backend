FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json AttendanceManagement.slnx ./
COPY src/AttendanceManagement.Api/AttendanceManagement.Api.csproj src/AttendanceManagement.Api/
COPY src/AttendanceManagement.Application/AttendanceManagement.Application.csproj src/AttendanceManagement.Application/
COPY src/AttendanceManagement.Domain/AttendanceManagement.Domain.csproj src/AttendanceManagement.Domain/
COPY src/AttendanceManagement.Infrastructure/AttendanceManagement.Infrastructure.csproj src/AttendanceManagement.Infrastructure/
RUN dotnet restore src/AttendanceManagement.Api/AttendanceManagement.Api.csproj

COPY src/AttendanceManagement.Api/ src/AttendanceManagement.Api/
COPY src/AttendanceManagement.Application/ src/AttendanceManagement.Application/
COPY src/AttendanceManagement.Domain/ src/AttendanceManagement.Domain/
COPY src/AttendanceManagement.Infrastructure/ src/AttendanceManagement.Infrastructure/

RUN dotnet publish src/AttendanceManagement.Api/AttendanceManagement.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# $PORT vem do host (Render/etc.); 8080 e o fallback local.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet AttendanceManagement.Api.dll"]
