using Shouldly;
using SocketCommunicationLib.Model;

namespace SocketCommunicationLib.Tests;

public class LockTimeTests
{
    [Fact]
    public void Sut_return_remain_time()
    {
        var date = DateTime.Now.Date;
        var timeSpan = new TimeSpan(31, 0, 0, 1, 500);
        var target = date.Add(timeSpan);
        var sut = new LockTime(0, 0, 3, 0);

        var actual = sut.TimeLeftToExpire(target);

        actual.ShouldBe(TimeSpan.FromMilliseconds(1500));
        sut.IsExpired(target).ShouldBeFalse();
    }

    [Fact]
    public void Sut_correct_time_left_with_next_day()
    {
        var date = DateTime.Now.Date;
        var timeSpan = new TimeSpan(31, 23, 59, 59, 200);
        var target = date.Add(timeSpan);
        var sut = new LockTime(0, 0, 1, 0);

        var actual = sut.TimeLeftToExpire(target);

        actual.ShouldBe(TimeSpan.FromMilliseconds(1800));
        sut.IsExpired(target).ShouldBeFalse();
    }

    [Fact]
    public void Sut_correct_expired()
    {
        var date = DateTime.Now.Date;
        var timeSpan = new TimeSpan(31, 10, 10, 11, 100);
        var target = date.Add(timeSpan);
        var sut = new LockTime(10, 10, 9, 300);

        var actual = sut.TimeLeftToExpire(target);

        actual.ShouldBe(TimeSpan.FromMilliseconds(-1800));
        sut.IsExpired(target).ShouldBeTrue();
    }

    [Fact]
    public void Sut_correct_expired_with_next_day()
    {
        var date = DateTime.Now.Date;
        var timeSpan = new TimeSpan(31, 0, 0, 1, 100);
        var target = date.Add(timeSpan);
        var sut = new LockTime(23, 59, 59, 500);

        var actual = sut.TimeLeftToExpire(target);

        actual.ShouldBe(TimeSpan.FromMilliseconds(-1600));
        sut.IsExpired(target).ShouldBeTrue();
    }
}