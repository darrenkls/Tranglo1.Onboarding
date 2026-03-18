#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM 423693868245.dkr.ecr.ap-southeast-1.amazonaws.com/dotnet/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["BackgroundTasks/ActiveDirectoryManager/ActiveDirectoryManager.csproj", "BackgroundTasks/ActiveDirectoryManager/"]
COPY ["Tranglo1.CustomerIdentity.Domain/Tranglo1.CustomerIdentity.Domain.csproj", "Tranglo1.CustomerIdentity.Domain/"]
COPY ["Tranglo1.CustomerIdentity.Infrastructure/Tranglo1.CustomerIdentity.Infrastructure.csproj", "Tranglo1.CustomerIdentity.Infrastructure/"]
RUN dotnet restore "BackgroundTasks/ActiveDirectoryManager/ActiveDirectoryManager.csproj"
COPY . .
WORKDIR "/src/BackgroundTasks/ActiveDirectoryManager"
RUN dotnet build "ActiveDirectoryManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ActiveDirectoryManager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ActiveDirectoryManager.dll"]