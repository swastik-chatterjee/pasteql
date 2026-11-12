FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/PasteQL/PasteQL.csproj", "src/PasteQL/"]
RUN dotnet restore "src/PasteQL/PasteQL.csproj"

COPY . .
WORKDIR "/src/src/PasteQL"
RUN dotnet publish "PasteQL.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PasteQL.dll"]
