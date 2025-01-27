using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using static SocketCommunicationLib.ProtocolConstants;

namespace SocketCommunicationLib.Tests;

public class MessageStringExtractorTests
{
    private readonly Encoding _encoding = Encoding.UTF8;
    
    [Fact]
    public void Sut_extract_pure_message()
    {
        var pureMessage = "Test Message";
        var message = $"{pureMessage}{Eom}";
        
        var sut = new MessageStringExtractor(Eom, _encoding);
        
        var actual = sut.AppendAndExtract(message);

        using (new AssertionScope())
        {
            actual.Should().HaveCount(1);
            actual.Should().Contain(pureMessage);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a")]
    [InlineData(Eom)]
    [InlineData($"a<|EOM|")]
    public void Sut_extract_nothing(string message)
    {
        var sut = new MessageStringExtractor(Eom, _encoding);
        
        var actual = sut.AppendAndExtract(message);

        using (new AssertionScope())
        {
            actual.Should().BeEmpty();
        }
    }
    
    [Fact]
    public void Sut_extract_two_messages()
    {
        var firstMessage = "First";
        var secondMessage = "Second";
        var message = $"{firstMessage}{Eom}{secondMessage}{Eom}";
        var sut = new MessageStringExtractor(Eom, _encoding);
        
        var actual = sut.AppendAndExtract(message);

        using (new AssertionScope())
        {
            actual.Should().HaveCount(2);
            actual.Should().ContainInOrder(firstMessage, secondMessage);
        }
    }

    [Fact]
    public void Sut_extract_after_multiple_append()
    {
        var first = "A";
        var second = "B";
        var sut = new MessageStringExtractor(Eom, _encoding);

        var firstList = sut.AppendAndExtract(first);
        var secondList = sut.AppendAndExtract(second);
        var actual = sut.AppendAndExtract(Eom);

        using (new AssertionScope())
        {
            firstList.Should().BeEmpty();
            secondList.Should().BeEmpty();
            actual.Should().HaveCount(1);
            actual.Should().Contain($"{first}{second}");
        }
    }

    [Fact]
    public void Sut_accept_to_append_bytes()
    {
        string pureMessage = "Test Message";
        byte[] inputBytes = _encoding.GetBytes($"{pureMessage}{Eom}");
        byte[] buffer = new byte[1024];
        
        int copyLength = Math.Min(buffer.Length, inputBytes.Length);
        Array.Copy(inputBytes, buffer, copyLength);
        
        var sut = new MessageStringExtractor(Eom, _encoding);
        var actual = sut.AppendAndExtract(buffer, 0, copyLength);

        using (new AssertionScope())
        {
            actual.Should().HaveCount(1);
            actual.Should().Contain(pureMessage);
        }
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
        
        var sut = new MessageStringExtractor(Eom, _encoding);
        var actual = sut.AppendAndExtract(buffer, 0, copyLength);

        using (new AssertionScope())
        {
            actual.Should().HaveCount(2);
            actual.Should().ContainInOrder(firstMessage, secondMessage);
        }
    }
}