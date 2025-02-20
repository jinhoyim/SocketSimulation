# SocketSimulation

## Features
.NET Socket 클래스로 서버/클라이언트 구성한다.
1. 서버에 5개의 클라이언트가 연결될 때 통신 시뮬레이션을 시작한다.
2. 통신 시뮬레이션이 시작되면 서버는 잠금 시간과 데이터를 생성하고 클라이언트에 전송한다.
3. 클라이언트가 잠금 시간과 데이터를 수신하면, 잠금시간만큼 대기한 후 서버에 조회를 요청한다.
4. 서버는 해당 데이터에 대한 잠금 시간이 지난 후 먼저 도착하는 조회 요청에 대해서 성공으로 응답한다.
5. 조회에 성공한 클라이언트는 다음 잠금시간(최대 100ms)과 데이터를 랜덤으로 생성하고 서버에 등록한다.
6. 서버는 등록을 요청한 클라이언트를 제외한 나머지 클라이언트에 잠금 시간과 데이터를 전송한다.
7. 과정은 설정한 횟수(2000) 만큼 반복된다.
8. 서버와 클라이언트는 데이터와 잠금 시간을 적절히 기록하고 클라이언트는 조회 성공/실패에 대한 결과를 기록한다.

## Implementation
로컬호스트로 동작, 간단한 콘솔 앱으로 데이터 교환을 중점으로 구현했다.

## Run
서버와 클라이언트는 별도의 콘솔 또는 터미널에서 실행한다.

### Server Run
```shell
dotnet run --project ./SocketServerApp/SocketServerApp.csproj
```

### Clients Run
```shell
dotnet run --project ./SocketClientApp/SocketClientApp.csproj --clientId=aaa
dotnet run --project ./SocketClientApp/SocketClientApp.csproj --clientId=bbb
dotnet run --project ./SocketClientApp/SocketClientApp.csproj --clientId=ccc
dotnet run --project ./SocketClientApp/SocketClientApp.csproj --clientId=ddd
dotnet run --project ./SocketClientApp/SocketClientApp.csproj --clientId=eee
```
