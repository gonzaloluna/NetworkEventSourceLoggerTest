FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app
ADD . .

RUN dotnet restore -s https://api.nuget.org/v3/index.json

RUN dotnet publish \
  --self-contained true \
  --runtime linux-x64 \
  -c Release \
  -o ./output
RUN chmod +x -R ./output/

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
RUN adduser \
  --disabled-password \
  --home /app \
  --gecos '' app \
  && chown -R app /app

USER app
WORKDIR /app
COPY --from=build-env /app/output .

ENV DOTNET_RUNNING_IN_CONTAINER=true \
  ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["./WebApplication1", "--urls", "http://0.0.0.0:8080"]
