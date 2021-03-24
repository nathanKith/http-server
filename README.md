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
## Нагрузочное тестирование через ab:
```sh
~ ❯ ab -n 100000 -c 8 http://127.0.0.1:8080/httptest/wikipedia_russia.html
...
Server Software:        superserver/1.0.0
Server Hostname:        127.0.0.1
Server Port:            8080

Document Path:          /httptest/wikipedia_russia.html
Document Length:        954824 bytes

Concurrency Level:      8
Time taken for tests:   143.728 seconds
Complete requests:      100000
Failed requests:        0
Requests per second:    695.76 [#/sec] (mean)
Time per request:       11.498 [ms] (mean)
Time per request:       1.437 [ms] (mean, across all concurrent requests)
Transfer rate:          648861.13 [Kbytes/sec] received
```
## Нагрузочное тестирование nginx через ab:
```sh
~ ❯ ab -n 100000 -c 8 http://127.0.0.1/httptest/wikipedia_russia.html
...
Server Software:        nginx/1.17.10
Server Hostname:        127.0.0.1
Server Port:            80

Document Path:          /httptest/wikipedia_russia.html
Document Length:        954824 bytes

Concurrency Level:      8
Time taken for tests:   27.882 seconds
Complete requests:      100000
Failed requests:        0
Requests per second:    3586.53 [#/sec] (mean)
Time per request:       2.231 [ms] (mean)
Time per request:       0.279 [ms] (mean, across all concurrent requests)
Transfer rate:          3345113.06 [Kbytes/sec] received
```

### Как видим разница в Rps составляет ~ _5,16_
