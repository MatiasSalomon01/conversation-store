FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8081

RUN apk update && apk add --no-cache tzdata
ENV TZ=America/Asuncion

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /
COPY ["WebApi/WebApi.csproj", "WebApi/"]
RUN dotnet restore "WebApi/WebApi.csproj"
COPY / .

FROM build AS publish
WORKDIR "/WebApi"
RUN dotnet publish "WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WebApi.dll"]
