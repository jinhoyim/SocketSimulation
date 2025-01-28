namespace SocketCommunicationLib.Contract;

public record DataRecordWithNext(DataRecord DataRecord, string? NextId);