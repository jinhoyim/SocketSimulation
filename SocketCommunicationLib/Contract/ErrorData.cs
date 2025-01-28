namespace SocketCommunicationLib.Contract;

public record ErrorData(string Message);

public record ErrorData<T>(T Data, string Message);