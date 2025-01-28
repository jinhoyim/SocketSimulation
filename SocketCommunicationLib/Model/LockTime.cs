namespace SocketCommunicationLib.Model;

public record LockTime(int Hour, int Minute, int Second, int Millisecond)
{
    public  static LockTime Empty { get; } = new(0, 0, 0, 0);

    public static LockTime From(DateTime time) => new(time.Hour, time.Minute, time.Second, time.Millisecond);
    
    private TimeSpan ToTimeSpan() => new(0, Hour, Minute, Second, Millisecond);
    
    public TimeSpan TimeLeftToExpire(DateTime utcNow)
    {
        TimeSpan expireTime = utcNow.Hour == 23 && Hour == 0 ?
            ToTimeSpan().Add(TimeSpan.FromDays(1)) : ToTimeSpan();

        var delay = expireTime - utcNow.TimeOfDay;
        return delay;
    }
    
    public bool IsExpired(DateTime utcNow) => TimeLeftToExpire(utcNow) <= TimeSpan.Zero;
    
    public override string ToString() => $"{Hour:00}:{Minute:00}:{Second:00}:{Millisecond:000}";
}