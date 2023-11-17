FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

WORKDIR /app
COPY ./src ./
RUN dotnet restore

WORKDIR /app/TransactionFetcher
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/runtime:6.0

WORKDIR /app
COPY --from=build-env /app/TransactionFetcher/out .

RUN apt-get update && \
    apt-get install -y dumb-init
    

# env variables go here


ENTRYPOINT ["/usr/bin/dumb-init", "--"]
CMD [ "dotnet", "/app/TransactionFetcher.dll" ]