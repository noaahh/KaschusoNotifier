FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/KaschusoNotifier.csproj", "KaschusoNotifier/"]
RUN dotnet restore "KaschusoNotifier/KaschusoNotifier.csproj"

WORKDIR "/src/KaschusoNotifier"
COPY . .

FROM build AS publish
RUN dotnet publish "KaschusoNotifier.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "KaschusoNotifier.dll"]