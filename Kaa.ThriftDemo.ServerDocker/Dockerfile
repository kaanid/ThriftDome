FROM microsoft/dotnet:2.1-runtime

COPY . .

# Configure the listening port to 9100
EXPOSE 9090/tcp

# Start the app
ENTRYPOINT dotnet Kaa.ThriftDemo.Service.dll