using SocketCommunicationLib.Model;

namespace SocketClientApp.Processing;

public class NextDataGenerator
{
    private readonly Random _random;
    private readonly int _maxMilliseconds;

    public NextDataGenerator(int maxMilliseconds)
    {
        _random = new Random();
        _maxMilliseconds = maxMilliseconds;
    }
    
    public NextDataValue CreateNewData(string nextId)
    {
        var nextMilliseconds = _random.Next(_maxMilliseconds);
        var nextDateTime = DateTime.Now.AddMilliseconds(nextMilliseconds);
        LockTime lockTime = LockTime.From(nextDateTime);
        return new NextDataValue(nextId, lockTime, lockTime.Millisecond);
    }
}