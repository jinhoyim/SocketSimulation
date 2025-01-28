namespace SocketCommunicationLib.Contract;

public record ErrorData<T>(T Data, string Message);