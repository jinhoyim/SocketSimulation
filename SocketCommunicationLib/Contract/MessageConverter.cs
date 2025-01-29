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

                if (DataMessageDeserializers.Deserializers.TryGetValue(type, out var deserializer))
                {
                    try
                    {
                        var body = deserializer.Invoke(content) ?? string.Empty;
                        return new Message(type, body);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return new Message(DataProtocolConstants.Unknown, message);
                    }
                }
                
                return new Message(type, content);
            }
            case false:
                return new Message(DataProtocolConstants.Unknown, message);
        }
    }
}