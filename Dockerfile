FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /src

# Copy the project file and restore dependencies
COPY */*.csproj ./
RUN dotnet restore

# Copy the rest of the project
COPY llm-thought-taker/ llm-thought-taker/

RUN dotnet publish ./llm-thought-taker/ -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

CMD ["dotnet", "llm-thought-taker.dll"]