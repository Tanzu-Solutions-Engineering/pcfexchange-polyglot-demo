cd client
dotnet publish -o ..\publish\Client
cd ..
cd MarketDataServer
dotnet publish -o ..\publish\MDS
cd ..
cd Exchange
dotnet publish -o ..\publish\Exchange
cd ..
mkdir publish\OMS
cd order-manager
mvnw clean package -DskipTests
