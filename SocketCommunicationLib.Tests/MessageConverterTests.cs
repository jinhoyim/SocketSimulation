using Shouldly;
using SocketCommunicationLib.Contract;

namespace SocketCommunicationLib.Tests;

public class MessageConverterTests
{
    [Theory]
    [InlineData("<|TEST|>Content1234", "<|TEST|>", "Content1234")]
    [InlineData("<|TERMINATED|>Content1234", "<|TERMINATED|>", "Content1234")]
    [InlineData("<|QUERYDATA|>Content1234", "<|QUERYDATA|>", "Content1234")]
    [InlineData("<|ERROR|>Content1234", "<|ERROR|>", "Content1234")]
    [InlineData("<|SUCCESS|>Content1234", "<|SUCCESS|>", "Content1234")]
    public void Sut_correctly_convert_message(string input, string type, string content)
    {
        var sut = new MessageConverter();
        var message = sut.Convert(input);

        message.Type.ShouldBe(type);
        message.Content.ShouldBe(content);
    }
}