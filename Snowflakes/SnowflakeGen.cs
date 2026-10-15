namespace Snowflakes;

public sealed class SnowflakeGen : ISnowflakeGen
{
    private const int TimestampBits = 41;
    private const int MachineIdBits = 10;
    private const int IncrementBits = 12;

    private const long MaxTimestamp = (1L << TimestampBits) - 1;
    private const long MaxMachineId = (1L << MachineIdBits) - 1;
    private const long MaxIncrement = (1L << IncrementBits) - 1;

    private readonly long Epoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

    private readonly long _machineId;
    private long _increment;
    private long _lastTimestamp;
    private readonly Lock _lock = new();

    public SnowflakeGen(long machineId)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(machineId);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(machineId, MaxMachineId);
        _machineId = machineId;
    }

    public SnowflakeGen(long machineId, DateTimeOffset epoch) : this(machineId)
    {
        Epoch = epoch.ToUnixTimeMilliseconds();
    }

    public Snowflake NextId()
    {
        lock (_lock)
        {
            UpdateIncrement(GetCurrentTimestamp());
            return CreateSnowflake();
        }
    }

    private void UpdateIncrement(long currentTimestamp)
    {
        if (currentTimestamp < _lastTimestamp)
        {
            throw new InvalidOperationException($"Clock moved backwards by {_lastTimestamp - currentTimestamp} ms.");
        }

        if (currentTimestamp == _lastTimestamp)
        {
            _increment = (_increment + 1) & MaxIncrement;
            if (_increment == 0)
            {
                currentTimestamp = WaitForNextTimestamp(_lastTimestamp);
            }
        }
        else
        {
            _increment = 0;
        }

        _lastTimestamp = currentTimestamp;
    }

    private static long WaitForNextTimestamp(long lastTimestamp)
    {
        SpinWait spinner = new();
        long timestamp;

        do
        {
            spinner.SpinOnce(sleep1Threshold: -1);
            timestamp = GetCurrentTimestamp();
        } while (timestamp <= lastTimestamp);

        return timestamp;
    }

    private static long GetCurrentTimestamp()
        => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Epoch;

    private Snowflake CreateSnowflake()
    {
        // MachineID and Increment are guaranteed to be within bounds cause of UpdateIncrement
        ArgumentOutOfRangeException.ThrowIfNegative(_lastTimestamp);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_lastTimestamp, MaxTimestamp);

        return new Snowflake(
            (_lastTimestamp << (MachineIdBits + IncrementBits))
            | (_machineId << IncrementBits)
            | _increment);
    }
}