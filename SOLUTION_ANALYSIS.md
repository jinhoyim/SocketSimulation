# SocketSimulation 솔루션 분석

## 1. 전체 아키텍처
`SocketSimulation` 솔루션은 원시 소켓을 사용하여 서버와 여러 클라이언트 간의 통신을 시뮬레이션하도록 설계된 .NET 애플리케이션입니다. 이 솔루션은 다음 네 가지 주요 프로젝트로 구성됩니다:
- `SocketServerApp`: 서버용 콘솔 애플리케이션.
- `SocketClientApp`: 클라이언트용 콘솔 애플리케이션.
- `SocketCommunicationLib`: 공통 통신 로직, 데이터 계약 및 유틸리티를 포함하는 공유 라이브러리.
- `SocketCommunicationLib.Tests`: `SocketCommunicationLib`에 대한 단위 테스트.

서버는 들어오는 클라이언트 연결을 수신 대기합니다. 5개의 클라이언트가 연결되면 서버가 잠금 시간과 데이터를 클라이언트에 전송하는 시뮬레이션이 시작됩니다. 클라이언트는 지정된 잠금 시간만큼 기다린 다음 서버에 쿼리합니다. 서버는 잠금 시간이 만료된 후 가장 먼저 성공적으로 쿼리한 요청에 응답합니다. 성공한 클라이언트는 새로운 잠금 시간과 데이터를 생성하여 서버에 등록하고, 서버는 이를 다른 클라이언트에 브로드캐스트합니다. 이 과정은 구성된 반복 횟수만큼 반복됩니다.

## 2. 프로젝트 분석

### 2.1. `SocketServerApp`
- **역할**: 소켓 통신 및 시뮬레이션 관리를 위한 서버 측 로직을 구현합니다.
- **주요 구성 요소**:
    - `Program.cs`: 진입점이며, `Microsoft.Extensions.Hosting`을 사용하여 의존성 주입(DI)을 설정합니다. `ServerConfig`를 구성하고 `ISocketServer`, `AllClientsCommunicator`, `ServerTerminator`, `OutputWriter`, `DataStore`, `StartStateStore`, `ServerListener`, `ClientIdentifierFactory`, `ClientHandler`, `ClientCommunicatorFactory`, `ServerWorkerFactory`와 같은 다양한 서비스를 등록합니다.
    - `Server.cs`: `ISocketServer`를 구현합니다. `ServerListener`를 통해 들어오는 클라이언트 연결을 수락하고 각 클라이언트를 새 작업에서 `ClientHandler`로 디스패치합니다.
    - `ServerListener.cs`: 새 소켓 연결을 수신 대기하고 수락하는 것을 처리합니다.
    - `ClientHandler.cs`: 개별 클라이언트 세션을 관리하며, 클라이언트로부터 메시지를 처리합니다.
    - `DataStore.cs` / `IDataStore` / `SaveLoggingDataStore.cs`: 시뮬레이션을 위한 공유 데이터 및 잠금 시간을 관리합니다. `SaveLoggingDataStore`는 `DataStore`를 래핑하여 로깅 기능을 추가합니다.
    - `AllClientsCommunicator.cs`: 연결된 모든 클라이언트와의 통신을 관리합니다.
    - `Processing` 폴더: 서버가 수신하는 다양한 유형의 메시지에 대한 핸들러를 포함합니다 (예: `NextDataHandler`, `QueryDataHandler`).

### 2.2. `SocketClientApp`
- **역할**: 소켓 통신 및 시뮬레이션 참여를 위한 클라이언트 측 로직을 구현합니다.
- **주요 구성 요소**:
    - `Program.cs`: 진입점이며, DI를 설정합니다. `clientId` 명령줄 인수가 필요합니다. `ClientConfig`를 구성하고 `ISocketClient`, `ConnectorFactory`, `ClientCommunicatorFactory`, `ClientJobProcessorFactory`, `SocketListenerFactory`, `ClientWorker`, `CountStore`, `OutputWriter`, `LockTimesStore`, `NextDataGenerator`와 같은 서비스를 등록합니다.
    - `Client.cs`: `ISocketClient`를 구현합니다. `ConnectorFactory`를 사용하여 서버에 연결한 다음, 주요 클라이언트 작업을 `ClientWorker`에 위임합니다.
    - `ClientWorker.cs`: 클라이언트의 핵심 시뮬레이션 루프를 포함하며, 쿼리 전송 및 업데이트 수신을 처리합니다.
    - `Connector.cs` / `ConnectorFactory.cs`: 서버에 대한 소켓 연결을 설정하고 관리하는 것을 처리합니다.
    - `Store` 폴더: `CountStore.cs` (성공/실패한 쿼리 추적), `LockTimesStore.cs` (서버로부터 수신한 잠금 시간 저장).
    - `Processing` 폴더: `ClientJobProcessor.cs` (클라이언트 측 처리 오케스트레이션), `NextDataGenerator.cs` (새로운 데이터 및 잠금 시간 생성), `QueryHandler.cs` (쿼리 전송 처리), `ErrorHandler.cs`, `QuerySuccessfulHandler.cs`.

### 2.3. `SocketCommunicationLib`
- **역할**: 메시지 직렬화/역직렬화 및 데이터 계약을 포함한 소켓 통신을 위한 공통 기능을 제공합니다.
- **주요 구성 요소**:
    - `SocketCommunicator.cs`: 소켓을 통해 메시지를 전송하기 위한 기본 클래스입니다. `JsonUtils`를 사용하여 데이터를 JSON으로 직렬화하고 `ProtocolConstants.Eom` (메시지 끝) 구분 기호를 추가합니다.
    - `JsonUtils.cs`: JSON 직렬화 및 역직렬화를 위한 유틸리티.
    - `SocketListener.cs`: 서버와 클라이언트 모두에서 데이터를 수신하는 데 사용되는 일반 소켓 리스너.
    - `SocketMessageStringExtractor.cs`: `Eom` 구분 기호를 사용하여 바이트 스트림에서 완전한 메시지를 추출합니다.
    - `Contract` 폴더: 메시지 구조 및 상수를 정의합니다.
        - `Message.cs`: `Type` 및 `Content`를 가진 일반 메시지를 나타내는 레코드 타입.
        - `DataProtocolConstants.cs`, `ProtocolConstants.cs`: 통신 프로토콜에 사용되는 상수 (예: 메시지 유형, `Eom`)를 정의합니다.
        - `DataMessageDeserializers.cs`, `MessageConverter.cs`: 다양한 메시지 유형의 역직렬화를 처리합니다.
    - `Model` 폴더: 서버와 클라이언트 간에 교환되는 데이터 모델을 정의합니다 (예: `DataRecord`, `DataRecordWithNext`, `LockTime`, `NextDataValue`).

### 2.4. `SocketCommunicationLib.Tests`
- **역할**: `SocketCommunicationLib`에 대한 단위 테스트를 포함합니다.
- **주요 구성 요소**:
    - `LockTimeTests.cs`: `LockTime` 기능과 관련된 테스트.
    - `MessageConverterTests.cs`: 올바른 메시지 직렬화/역직렬화를 위한 `MessageConverter` 테스트.
    - `SocketMessageStringExtractorTests.cs`: 바이트 스트림에서 메시지를 추출하기 위한 `SocketMessageStringExtractor` 테스트.
- **테스트 프레임워크**: 테스트에는 `xUnit`을 사용하고, 어설션 구문에는 `FluentAssertions`를 사용합니다.

## 3. 통신 프로토콜
통신 프로토콜은 텍스트 기반이며, 데이터 직렬화를 위해 JSON을 사용합니다. 메시지는 접두사 (메시지 유형/목적을 나타냄)와 JSON 페이로드로 전송되며, `ProtocolConstants.cs`에 정의된 특수 메시지 끝 (`Eom`) 상수로 종료됩니다. `SocketCommunicator`는 전송을 처리하고, `SocketMessageStringExtractor`는 완전한 메시지의 수신 및 추출을 처리합니다. `SocketCommunicationLib.Contract`의 `Message` 레코드는 일반 메시지 구조를 정의합니다.

## 4. 시뮬레이션 로직
1.  **서버 시작**: 서버가 시작되고 들어오는 연결을 수신 대기합니다.
2.  **클라이언트 연결**: 클라이언트는 고유한 `clientId`를 가지고 서버에 연결합니다.
3.  **시뮬레이션 시작**: 5개의 클라이언트가 연결되면 서버가 시뮬레이션을 시작합니다.
4.  **서버 데이터 전송**: 서버는 초기 잠금 시간과 데이터를 생성하여 연결된 모든 클라이언트에 전송합니다.
5.  **클라이언트 쿼리**: 클라이언트는 데이터를 수신하고, 지정된 잠금 시간만큼 기다린 다음 서버에 쿼리 요청을 보냅니다.
6.  **서버 응답**: 서버는 쿼리를 처리합니다. 잠금 시간이 만료된 후 가장 먼저 성공적으로 쿼리한 클라이언트는 성공 응답을 받습니다.
7.  **클라이언트 등록**: 성공한 클라이언트는 새로운 잠금 시간과 데이터를 생성하여 서버에 등록합니다.
8.  **서버 브로드캐스트**: 서버는 새로 등록된 데이터와 잠금 시간을 *다른* 모든 연결된 클라이언트에 브로드캐스트합니다.
9.  **반복**: 5-8단계는 구성된 반복 횟수만큼 반복됩니다 (`README.md`에 따라 2000회).
10. **로깅**: 서버와 클라이언트 모두 데이터, 잠금 시간, 쿼리 성공/실패를 로깅합니다.

## 5. 의존성
프로젝트는 주로 표준 .NET 라이브러리와 의존성 주입, 구성 및 호스팅을 위한 `Microsoft.Extensions.*` 패키지를 사용합니다.
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.Options`
- `System.Text.Json` (JSON 직렬화/역직렬화용)
- `xunit`, `xunit.runner.visualstudio`, `FluentAssertions`, `coverlet.collector` (테스트용)