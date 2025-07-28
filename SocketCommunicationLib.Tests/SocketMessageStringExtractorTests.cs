using System.Text;
using Shouldly;
using static SocketCommunicationLib.Contract.ProtocolConstants;

namespace SocketCommunicationLib.Tests;

public class SocketMessageStringExtractorTests
{
    private readonly Encoding _encoding = Encoding.UTF8;
    
    [Fact]
    public void Sut_extract_pure_message()
    {
        var pureMessage = "Test Message";
        var message = $"{pureMessage}{Eom}";
        
        var sut = new SocketMessageStringExtractor(Eom, _encoding);
        
        var actual = sut.AppendAndExtract(message);

        actual.Count.ShouldBe(1);
        actual.ShouldContain(pureMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a")]
    [InlineData(Eom)]
    [InlineData($"a<|EOM|")]
    public void Sut_extract_nothing(string message)
    {
        var sut = new SocketMessageStringExtractor(Eom, _encoding);
        
        var actual = sut.AppendAndExtract(message);

        actual.ShouldBeEmpty();
    }
    
    [Fact]
    public void Sut_extract_two_messages()
    {
        var firstMessage = "First";
        var secondMessage = "Second";
        var message = $"{firstMessage}{Eom}{secondMessage}{Eom}";
        var sut = new SocketMessageStringExtractor(Eom, _encoding);
        
        var actual = sut.AppendAndExtract(message);

        actual.Count.ShouldBe(2);
        actual.ShouldBe(new[] { firstMessage, secondMessage });
    }

    [Fact]
    public void Sut_extract_after_multiple_append()
    {
        var first = "A";
        var second = "B";
        var sut = new SocketMessageStringExtractor(Eom, _encoding);

        var firstList = sut.AppendAndExtract(first);
        var secondList = sut.AppendAndExtract(second);
        var actual = sut.AppendAndExtract(Eom);

        firstList.ShouldBeEmpty();
        secondList.ShouldBeEmpty();
        actual.Count.ShouldBe(1);
        actual.ShouldContain($"{first}{second}");
    }

    [Fact]
    public void Sut_accept_to_append_bytes()
    {
        string pureMessage = "Test Message";
        byte[] inputBytes = _encoding.GetBytes($"{pureMessage}{Eom}");
        byte[] buffer = new byte[1024];
        
        int copyLength = Math.Min(buffer.Length, inputBytes.Length);
        Array.Copy(inputBytes, buffer, copyLength);
        
        var sut = new SocketMessageStringExtractor(Eom, _encoding);
        var actual = sut.AppendAndExtract(buffer, 0, copyLength);

        actual.Count.ShouldBe(1);
        actual.ShouldContain(pureMessage);
    }
    
    [Fact]
    public void Sut_append_bytes_two_messages()
    {
        var firstMessage = "First";
        var secondMessage = "Second";
        var message = $"{firstMessage}{Eom}{secondMessage}{Eom}";
        byte[] inputBytes = _encoding.GetBytes(message);
        byte[] buffer = new byte[1024];
        
        int copyLength = Math.Min(buffer.Length, inputBytes.Length);
        Array.Copy(inputBytes, buffer, copyLength);
        
        var sut = new SocketMessageStringExtractor(Eom, _encoding);
        var actual = sut.AppendAndExtract(buffer, 0, copyLength);

        actual.Count.ShouldBe(2);
        actual.ShouldBe(new[] { firstMessage, secondMessage });
    }
}