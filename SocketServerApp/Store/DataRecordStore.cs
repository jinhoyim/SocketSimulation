using SocketCommunicationLib.Contract;

namespace SocketServerApp.Store;

public class DataRecordStore
{
    private int _saveCount = 0;
    private int _increment = 0;

    public int Save()
    {
        return Interlocked.Increment(ref _saveCount);
    }

    public DataRecord InitialDataRecord()
    {
        var initValue = 1;
        var lockTime = LockTime.From(DateTime.Now.AddSeconds(2));
        var number = Interlocked.Increment(ref _increment);
        var id = number.ToString();
        return new DataRecord(id, lockTime, string.Empty, initValue);
    }
}