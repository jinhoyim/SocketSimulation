using System.Text.Json;
using SocketClientApp.Communication;
using SocketCommunicationLib;
using SocketCommunicationLib.Channel;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Processing;

public class ClientJobProcessor
{
    private readonly IChannel<string> _channel;
    private readonly SocketCommunicator _communicator;
    private readonly CancellationTokenSource _cts;
    private readonly MessageConverter _messageConverter;
    private readonly LockTimeHandler _lockTimeHandler;

    public ClientJobProcessor(
        IChannel<string> channel,
        SocketCommunicator communicator,
        MessageConverter messageConverter,
        LockTimeHandler lockTimeHandler,
        CancellationTokenSource cts)
    {
        _channel = channel;
        _communicator = communicator;
        _cts = cts;
        _messageConverter = messageConverter;
        _lockTimeHandler = lockTimeHandler;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (var request in _channel.ReadAllAsync(cancellationToken))
        {
            var message = _messageConverter.Convert(request);
            if (message == Message.Empty) continue;
            
            switch (message.Type)
            {
                case ProtocolConstants.LockTime:
                    await _lockTimeHandler.HandleLockTimeAsync(message.Content, cancellationToken);
                    break;
                case ProtocolConstants.DataRecordWithNext:
                    await HandleQueryResultAsync(message.Content, cancellationToken);
                    break;
                case ProtocolConstants.ErrorEmptyData:
                    Console.WriteLine($"Error: {ProtocolConstants.ErrorEmptyData}");
                    break;
                case ProtocolConstants.ErrorDataLocked:
                    Console.WriteLine($"Error: {ProtocolConstants.ErrorDataLocked}");
                    break;
                case ProtocolConstants.ErrorBadRequest:
                    Console.WriteLine($"Error: {ProtocolConstants.ErrorBadRequest}");
                    break;
                case ProtocolConstants.ServerTerminated:
                    await _cts.CancelAsync();
                    break;
                
            // else if ReceiveData
            // Logging Data + LockTime + SuccessCount
            // Create Data + LockTime
            
            // else if NotYetExpiredLockTime
            // Logging NotYetExpiredLockTime Count
            // delay LockTime
            // retry query QueryData
            
            // else if DataIsEmpty
            // Logging FailedCount
                
            }
        }
    }

    private async Task HandleQueryResultAsync(string content, CancellationToken cancellationToken)
    {
        // string content의 타입 정보를 함께 받거나,
        // 내부에 정보를 포함하여 역직렬화된 결과를 바로 받을 수 있게 바꿀 것
        // 데이터와 LockTime, 성공 횟수 기록
        var withNext = Deserialize<DataRecordWithNext>(content);
        if (withNext is null) return;
        
        Console.WriteLine("HandleQueryResultAsync");

        var record = withNext.DataRecord;
        // save record
        Console.WriteLine($"Save Record: {record}");
        
        // SuccessfulCount ++
        Console.WriteLine("Increse Successful Count");
        
        // 수신한 NextId를 사용하여 2초 이내 랜덤 LockTime, 새로운 데이터를 생성해서 서버에 저장 요청
        Console.WriteLine($"SendNextData: {withNext.NextId}");
    }

    private T? Deserialize<T>(string json)
    {
        return JsonUtils.Deserialize<T>(json);
    }
}