# ThriftDome

Thrift by dotnet

# Server

dotnet2.1 

# client

1. dotnet 2.1
2. framework 4.5


# thrift -version

Thrift version 0.11.0

# CMD

## dotnet 
thrift -r --gen netcore tutorial.thrift

## framework

thrift -r --gen csharp:async tutorial.thrift  

### Thrift Transport

1. TStreamTransport
2. TBufferedTransport

public override IAsyncResult BeginFlush()
public override IAsyncResult EndFlush()
