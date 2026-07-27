namespace ArkheideSystem.Flourish.Internal.Composition;

/// <summary>
/// Prevents startup builders from mutating configuration after their configuration phase ends.
/// </summary>
internal abstract class FlourishBuilderMutationGuard
{
    private int isFrozen;

    protected void ThrowIfFrozen()
    {
        if (Volatile.Read(ref isFrozen) != 0)
        {
            throw new InvalidOperationException(
                "The builder can no longer be modified after its configuration phase has completed."
            );
        }
    }

    internal void Freeze()
    {
        Interlocked.Exchange(ref isFrozen, 1);
    }

    protected bool TryFreeze()
    {
        return Interlocked.Exchange(ref isFrozen, 1) == 0;
    }
}
