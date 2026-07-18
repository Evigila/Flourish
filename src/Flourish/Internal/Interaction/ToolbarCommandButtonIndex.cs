using ArkheideSystem.Flourish.Abstract;
using WpfButton = System.Windows.Controls.Button;

namespace ArkheideSystem.Flourish.Internal.Interaction;

internal sealed class ToolbarCommandButtonIndex(ICommandDispatcher commandDispatcher)
{
    private readonly ICommandDispatcher commandDispatcher =
        commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
    private readonly List<Entry> entries = [];
    private readonly Dictionary<string, List<Entry>> entriesByCommandKey = new(
        StringComparer.Ordinal
    );

    internal void Track(WpfButton button, FlourishToolbarItem item)
    {
        ArgumentNullException.ThrowIfNull(button);
        ArgumentNullException.ThrowIfNull(item);

        button.IsEnabled = CanExecute(item);
        if (string.IsNullOrWhiteSpace(item.CommandKey))
        {
            return;
        }

        var entry = new Entry(button, item);
        entries.Add(entry);
        if (!entriesByCommandKey.TryGetValue(item.CommandKey, out var commandEntries))
        {
            commandEntries = [];
            entriesByCommandKey.Add(item.CommandKey, commandEntries);
        }

        commandEntries.Add(entry);
    }

    internal void Refresh(string? commandKey)
    {
        if (commandKey is null)
        {
            Refresh(entries);
            return;
        }

        if (entriesByCommandKey.TryGetValue(commandKey, out var commandEntries))
        {
            Refresh(commandEntries);
        }
    }

    internal void Untrack(IEnumerable<WpfButton> buttons)
    {
        ArgumentNullException.ThrowIfNull(buttons);
        var removedButtons = buttons.ToHashSet();
        if (removedButtons.Count == 0)
        {
            return;
        }

        entries.RemoveAll(entry => removedButtons.Contains(entry.Button));
        foreach (var commandEntries in entriesByCommandKey.Values)
        {
            commandEntries.RemoveAll(entry => removedButtons.Contains(entry.Button));
        }

        foreach (
            var commandKey in entriesByCommandKey
                .Where(pair => pair.Value.Count == 0)
                .Select(pair => pair.Key)
                .ToArray()
        )
        {
            entriesByCommandKey.Remove(commandKey);
        }
    }

    internal void Clear()
    {
        entries.Clear();
        entriesByCommandKey.Clear();
    }

    private void Refresh(List<Entry> refreshEntries)
    {
        for (var index = 0; index < refreshEntries.Count; index++)
        {
            var entry = refreshEntries[index];
            entry.Button.IsEnabled = CanExecute(entry.Item);
        }
    }

    private bool CanExecute(FlourishToolbarItem item)
    {
        return item.IsEnabled
            && (
                string.IsNullOrWhiteSpace(item.CommandKey)
                || commandDispatcher.CanExecute(
                    item.CommandKey,
                    source: CommandSource.Toolbar
                )
            );
    }

    private readonly record struct Entry(WpfButton Button, FlourishToolbarItem Item);
}
