using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies command availability that should be queried again.
/// </summary>
public sealed class CommandCanExecuteChangedEventArgs : EventArgs
{
    internal CommandCanExecuteChangedEventArgs(string? commandKey)
    {
        CommandKey = commandKey;
    }

    /// <summary>
    /// Gets the affected command key, or <see langword="null" /> when all commands should be queried again.
    /// </summary>
    public string? CommandKey { get; }
}
