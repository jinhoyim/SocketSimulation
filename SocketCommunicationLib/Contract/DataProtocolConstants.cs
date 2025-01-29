namespace SocketCommunicationLib.Contract;

public static class DataProtocolConstants
{
    // Id 데이터 조회
    public const string QueryData = "<|DATA|QUERYDATA|>";

    // 쿼리 결과 에러
    public const string ErrorEmptyData = "<|DATA|ERROREMPTYDATA|>";
    public const string ErrorDataLocked = "<|DATA|ERRORDATALOCKED|>";
    
    public const string NextData = "<|DATA|NEXTDATA|>"; // 다음 데이터 생성
    public const string ErrorNotFoundData = "<|DATA|ERRORNOTFOUNDDATA|>";
    public const string ErrorNotModifyPermission = "<|DATA|ERRORNOTMODIFYPERMISSION|>";
    
    public const string DataLockTime = "<|DATA|LOCKTIME|>"; // Id 에 대한 LockTime 전송
    public const string DataWithNext = "<|DATA|DATAWITHNEXT|>"; // 조회 요청에 대한 결과로 데이터와 NextId 전송

    // 알 수 없는 요청
    public const string ErrorBadRequest = "<|DATA|BADREQUEST|>";
    public const string ErrorUnsupportedRequest = "<|DATA|UNSUPPORTEDREQUEST|>";
    public const string Unknown = "<|DATA|UNKNOWN>";
}