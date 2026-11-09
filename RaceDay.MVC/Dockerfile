FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RaceDay.MVC/RaceDay.MVC.csproj", "RaceDay.MVC/"]
RUN dotnet restore "RaceDay.MVC/RaceDay.MVC.csproj"
COPY . .
WORKDIR "/src/RaceDay.MVC"
RUN dotnet build "RaceDay.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RaceDay.MVC.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]
