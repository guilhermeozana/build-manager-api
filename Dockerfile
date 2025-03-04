FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app 

COPY Marelli-api/*.sln .
COPY Marelli-api/Admin/Marelli.Api/*.csproj Admin/Marelli.Api/
COPY Marelli-api/Marelli.Api/*.csproj Marelli.Api/
COPY Marelli-api/Marelli.Business/*.csproj Marelli.Business/
COPY Marelli-api/Marelli.BusinessTests/*.csproj Marelli.BusinessTests/
COPY Marelli-api/Marelli.Domain/*.csproj Marelli.Domain/
COPY Marelli-api/Marelli.Infra/*.csproj Marelli.Infra/
COPY Marelli-api/Marelli.Test/*.csproj Marelli.Test/

RUN dotnet restore

COPY Marelli-api/Admin/Marelli.Api/ Admin/Marelli.Api/
COPY Marelli-api/Marelli.Api/ Marelli.Api/
COPY Marelli-api/Marelli.Business/ Marelli.Business/
COPY Marelli-api/Marelli.BusinessTests/ Marelli.BusinessTests/
COPY Marelli-api/Marelli.Domain/ Marelli.Domain/
COPY Marelli-api/Marelli.Infra/ Marelli.Infra/
COPY Marelli-api/Marelli.Test/ Marelli.Test/

WORKDIR /app/Marelli.Api/
RUN dotnet publish -c Release -o WebApi 

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app 

COPY --from=build /app/Marelli.Api/WebApi ./
ENTRYPOINT ["dotnet", "Marelli.Api.dll"]
