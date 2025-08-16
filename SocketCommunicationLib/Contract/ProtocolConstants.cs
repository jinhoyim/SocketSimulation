namespace SocketCommunicationLib.Contract;

public static class ProtocolConstants
{
    // 모든 메시지 마지막 구분자
    public const string Eom = "<|SYSTEM|EOM|>";

    // 서버 종료
    public const string ServerTerminated = "<|SYSTEM|SERVERTERMINATED|>";

    // 접속에 사용
    public const string ReadyConnect = "<|CONNECT|READY|>";
    public const string Connect = "<|CONNECT|REQUEST|>";
    public const string Success = "<|CONNECT|SUCCESS|>"; // ClientId 식별 성공
    public const string Error = "<|CONNECT|ERROR|>"; // ClientId 식별 실패
}