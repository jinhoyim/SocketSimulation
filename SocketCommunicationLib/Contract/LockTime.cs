namespace SocketCommunicationLib.Contract;

public record LockTime(int Hour, int Minute, int Second, int Millisecond)
{
    public  static LockTime Empty { get; } = new(0, 0, 0, 0);

    public static LockTime From(DateTime time) => new(time.Hour, time.Minute, time.Second, time.Millisecond);
}