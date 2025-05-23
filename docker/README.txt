To run all the containers:
docker-compose -f docker-compose.base.yml -f docker-compose.mssql.yml -f docker-compose.rabbitmq.yml -f docker-compose.web.demo.yml up
docker-compose -f docker-compose.base.yml -f docker-compose.mssql.yml -f docker-compose.rabbitmq.yml -f docker-compose.web.demo.yml down

To run only the mssql and rabbitmq containers:
docker-compose -f docker-compose.base.yml -f docker-compose.mssql.yml -f docker-compose.rabbitmq.yml up
docker-compose -f docker-compose.base.yml -f docker-compose.mssql.yml -f docker-compose.rabbitmq.yml down

To install a fresh development certificate:
dotnet dev-certs https --clean
dotnet dev-certs https -ep %APPDATA%\ASP.NET\Https\aspnetapp.pfx -p SecureP4ssw0rd!
dotnet dev-certs https --trust
