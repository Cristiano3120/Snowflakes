namespace Snowflakes;

public readonly struct Snowflake(long value)
{
    public long Value { get; } = value;
    public DateTimeOffset CreationTimeUtc(long epoch) => DateTimeOffset.FromUnixTimeMilliseconds((Value >> 22) + epoch);

    public static implicit operator long(Snowflake snowflake)
        => snowflake.Value;

    public override string ToString()
        => Value.ToString();
}