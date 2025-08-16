using System.Text;

namespace SocketCommunicationLib;

public class SocketMessageStringExtractor
{
    private readonly string _delimiter;
    private readonly StringBuilder _stringBuilder = new();
    private readonly Encoding _encoding;

    public SocketMessageStringExtractor(string delimiter, Encoding encoding)
    {
        _delimiter = delimiter;
        _encoding = encoding;
    }

    public List<string> AppendAndExtract(string message)
    {
        _stringBuilder.Append(message);
        return SplitAndAppendRemain();
    }

    public List<string> AppendAndExtract(byte[] buffer, int index, int count)
    {
        var text = _encoding.GetString(buffer, index, count);
        return AppendAndExtract(text);
    }

    private List<string> SplitAndAppendRemain()
    {
        var data = _stringBuilder.ToString();
        int startIndex = 0;
        int index;
        List<string> list = new();
        while ((index = data.IndexOf(_delimiter, startIndex, StringComparison.Ordinal)) != -1)
        {
            var msg = data.Substring(startIndex, index - startIndex).Trim();
            if (!string.IsNullOrEmpty(msg))
            {
                list.Add(msg);
            }

            startIndex = index + _delimiter.Length;
        }

        _stringBuilder.Clear();
        if (startIndex < data.Length)
        {
            _stringBuilder.Append(data.Substring(startIndex));
        }

        return list;
    }
}