FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Tatuagem.Api/Tatuagem.Api.csproj", "Tatuagem.Api/"]
COPY ["Tatuagem.Testes/Tatuagem.Testes.csproj", "Tatuagem.Testes/"]
RUN dotnet restore "Tatuagem.Api/Tatuagem.Api.csproj"

COPY . .
WORKDIR "/src/Tatuagem.Api"
RUN dotnet build "Tatuagem.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Tatuagem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tatuagem.Api.dll"]