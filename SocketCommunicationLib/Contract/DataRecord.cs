namespace SocketCommunicationLib.Contract;

public record DataRecord(string Id, LockTime LockTime, string ClientId, int Value);