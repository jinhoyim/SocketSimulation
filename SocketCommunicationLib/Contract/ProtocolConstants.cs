namespace SocketCommunicationLib.Contract;

public static class ProtocolConstants
{
    public const string Eom = "<|EOM|>";
    public const string Connect = "<|CONNECT|>";
    public const string Success = "<|SUCCESS|>";
    public const string Error = "<|ERROR|>";
    public const string ReadyConnect = "<|READYCONNECT|>";
    public const string ServerTerminated = "<|SERVERTERMINATED|>";
    public const string QueryData = "<|QUERYDATA|>";
    public const string ErrorEmptyData = "<|ERROREMPTYDATA|>";
    public const string ErrorDataLocked = "<|ERRORDATALOCKED|>";
    public const string LockTime = "<|LOCKTIME|JSON|>";
    public const string DataRecordWithNext = "<|DataRecordWithNext|JSON|>";
    public const string ErrorBadRequest = "<|ERRORBADREQUEST|>";
    public const string NextData = "<|NEXTDATA|JSON|>";
}