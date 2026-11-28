namespace AffiliateMessages.Api;

public sealed class ConversionState
{
    private int state;

    public bool IsActive => Volatile.Read(ref state) == 1;

    public bool TryBegin()
    {
        return Interlocked.CompareExchange(ref state, 1, 0) == 0;
    }

    public bool TryBeginShutdown()
    {
        return Interlocked.CompareExchange(ref state, 2, 0) == 0;
    }

    public void End()
    {
        Interlocked.CompareExchange(ref state, 0, 1);
    }
}
