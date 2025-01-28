using System.Text.RegularExpressions;

namespace SocketCommunicationLib.Contract;

public class MessageConverter
{
    private const string Pattern = @"(<\|.*?\|>)(.*)";
    private readonly Regex _regex;
    
    public MessageConverter()
    {
        _regex = new Regex(Pattern, RegexOptions.Compiled, TimeSpan.FromSeconds(2));
    }

    public Message Convert(string message)
    {
        Match match = _regex.Match(message);
        switch (match.Success)
        {
            case true:
            {
                var type = match.Groups[1].Value;
                var content = match.Groups[2].Value;
                return new Message(type, content);
            }
            default:
                return Message.Empty;
        }
    }
}