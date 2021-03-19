# HTTP-сервер на C# .NET 5 с использованием архитектуры thread per request

## Запуск (.NET 5.0 SDK):
```
dotnet build
./bin/Debug/net5.0/http-server
```

## Запуск через Docker:
```
docker build -t server .
docker run -p 8080:8080 server
```
