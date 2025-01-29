namespace SocketCommunicationLib.Model;

public record LockTime(int Hour, int Minute, int Second, int Millisecond)
{
    public  static LockTime Empty { get; } = new(0, 0, 0, 0);

    public static LockTime From(DateTime time) => new(time.Hour, time.Minute, time.Second, time.Millisecond);
    
    private TimeSpan ToTimeSpan() => new(0, Hour, Minute, Second, Millisecond);
    
    public TimeSpan TimeLeftToExpire(DateTime utcNow)
    {
        var target = utcNow.TimeOfDay;
        if (utcNow.Hour == 23 && Hour == 0)
        {
            target = target.Add(TimeSpan.FromDays(-1));
        }
        else if (utcNow.Hour == 0 && Hour == 23)
        {
            target = target.Add(TimeSpan.FromDays(1));
        }
        return ToTimeSpan() - target;
    }
    
    public bool IsExpired(DateTime utcNow) => TimeLeftToExpire(utcNow) <= TimeSpan.Zero;
    
    public override string ToString() => $"{Hour:00}:{Minute:00}:{Second:00}:{Millisecond:000}";
}