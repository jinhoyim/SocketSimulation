using SocketCommunicationLib.Model;
using static SocketCommunicationLib.Contract.DataProtocolConstants;

namespace SocketCommunicationLib.Contract;

public static class DataMessageDeserializers
{
    public static readonly Dictionary<string, Func<string, object?>> Deserializers = new()
    {
        { QueryData, s => s },
        { ErrorEmptyData, JsonUtils.Deserialize<ErrorData<string>> },
        { ErrorDataLocked, JsonUtils.Deserialize<ErrorData<string>> },
        { NextData, JsonUtils.Deserialize<NextDataValue> },
        { DataLockTime, JsonUtils.Deserialize<DataRecord> },
        { DataWithNext, JsonUtils.Deserialize<DataRecordWithNext> },
        { ErrorBadRequest, JsonUtils.Deserialize<ErrorData> },
    };
}