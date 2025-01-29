using System.Diagnostics.CodeAnalysis;

namespace SocketCommunicationLib.Model;

public class DataRecord
{
    public DataRecord(string id, LockTime lockTime, string createdClientId, int value)
    {
        Id = id;
        LockTime = lockTime;
        CreatedClientId = createdClientId;
        Value = value;
    }

    public static DataRecord Empty => new DataRecord(string.Empty, LockTime.Empty, string.Empty, 0);
    public string Id { get; }
    public LockTime LockTime { get; }
    public string CreatedClientId { get; }
    public int Value { get; }

    public bool TryUpdate(
        string clientId,
        LockTime lockTime,
        int value,
        [MaybeNullWhen(false)] out DataRecord updatedRecord,
        [MaybeNullWhen(true)] out string errorMessage)
    {
        if (!HasModifyPermission(clientId))
        {
            updatedRecord = null;
            errorMessage = $"Id: {Id}, Has not modify permission for record.";
            return false;
        }

        updatedRecord = UpdateImmutable(lockTime, value);
        errorMessage = null;
        return true;
    }

    private DataRecord UpdateImmutable(LockTime lockTime, int value)
    {
        return new DataRecord(Id, lockTime, CreatedClientId, value);
    }

    private bool HasModifyPermission(string clientId)
    {
        return string.IsNullOrEmpty(CreatedClientId) || CreatedClientId == clientId;
    }

    public void Deconstruct(out string id, out LockTime lockTime, out string createdClientId, out int value)
    {
        id = Id;
        lockTime = LockTime;
        createdClientId = CreatedClientId;
        value = Value;
    }

    public static DataRecord Create(string id, string createdClientId)
    {
        return new DataRecord(id, LockTime.Empty, createdClientId, 0);
    }

    public override string ToString()
    {
        return $"Id: {Id}, LockTime: {LockTime}, CreatedClientId: {CreatedClientId}, Value: {Value}";
    }
}
