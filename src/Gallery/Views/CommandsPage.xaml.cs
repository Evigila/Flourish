using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Gallery.Views;

public partial class CommandsPage : Page
{
    private static readonly KeyGesture DemoGesture =
        new(Key.G, ModifierKeys.Control | ModifierKeys.Shift);

    private readonly ICommandRegistry commandRegistry;
    private readonly ICommandDispatcher commandDispatcher;
    private readonly IShortcutService shortcuts;
    private ICommandRegistration? commandRegistration;
    private IShortcutRegistration? shortcutRegistration;
    private bool commandEnabled = true;
    private int executionCount;

    public CommandsPage(
        ICommandRegistry commandRegistry,
        ICommandDispatcher commandDispatcher,
        IShortcutService shortcuts
    )
    {
        this.commandRegistry = commandRegistry;
        this.commandDispatcher = commandDispatcher;
        this.shortcuts = shortcuts;
        InitializeComponent();
        CommandEnabledBox.Checked += CommandEnabled_Changed;
        CommandEnabledBox.Unchecked += CommandEnabled_Changed;

        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshRegistryState();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Page_Unloaded(sender, e);
        commandRegistry.Changed += Registry_Changed;
        shortcuts.Changed += Registry_Changed;
        RefreshRegistryState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        commandRegistry.Changed -= Registry_Changed;
        shortcuts.Changed -= Registry_Changed;
        commandRegistration?.Dispose();
        commandRegistration = null;
        shortcutRegistration?.Dispose();
        shortcutRegistration = null;
    }

    private void Registry_Changed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshRegistryState);
    }

    private void RegisterCommand_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var key = RequireCommandKey();
            commandRegistration?.Dispose();
            commandRegistration = commandRegistry.Register(
                key,
                ExecuteDemoHandlerAsync,
                _ => commandEnabled,
                new CommandRegistrationOptions
                {
                    DuplicatePolicy = CommandDuplicatePolicy.Replace,
                    Priority = 100,
                }
            );
            CommandOutput.WriteLine($"Registered '{key}' with priority 100.");
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private async void ExecuteCommand_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var key = RequireCommandKey();
            var canExecute = commandDispatcher.CanExecute(
                key,
                CommandParameterBox.Text,
                CommandSource.Application
            );
            var result = await commandDispatcher.ExecuteAsync(
                key,
                CommandParameterBox.Text,
                CommandSource.Application
            );
            CommandOutput.WriteLine(FormatResult("Command", result, canExecute));
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void RemoveCommand_Click(object sender, RoutedEventArgs e)
    {
        if (commandRegistration is null)
        {
            CommandOutput.WriteLine("This page does not currently own a command registration.");
            return;
        }

        try
        {
            commandRegistration.Dispose();
            commandRegistration = null;
            CommandOutput.WriteLine("The runtime command registration was removed.");
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void CommandEnabled_Changed(object sender, RoutedEventArgs e)
    {
        try
        {
            commandEnabled = CommandEnabledBox.IsChecked == true;
            commandRegistration?.NotifyCanExecuteChanged();
            CommandOutput.WriteLine(
                commandRegistration is null
                    ? $"The next registered handler will be {(commandEnabled ? "enabled" : "disabled")}."
                    : $"The command is now {(commandEnabled ? "enabled" : "disabled")}."
            );
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void RegisterShortcut_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var key = RequireCommandKey();
            shortcutRegistration?.Dispose();
            shortcutRegistration = shortcuts.Register(
                DemoGesture,
                key,
                CommandParameterBox.Text,
                new ShortcutRegistrationOptions
                {
                    Scope = ShortcutScope.Application,
                    ConflictPolicy = ShortcutConflictPolicy.Replace,
                    Priority = 100,
                }
            );
            ShortcutOutput.WriteLine($"Ctrl+Shift+G now dispatches '{key}'.");
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            ShortcutOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private async void ExecuteShortcut_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await shortcuts.ExecuteAsync(
                DemoGesture,
                new ShortcutResolutionContext(pageKey: nameof(CommandsPage))
            );
            ShortcutOutput.WriteLine(FormatResult("Shortcut", result, null));
        }
        catch (Exception error)
        {
            ShortcutOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void RemoveShortcut_Click(object sender, RoutedEventArgs e)
    {
        if (shortcutRegistration is null)
        {
            ShortcutOutput.WriteLine("This page does not currently own a shortcut registration.");
            return;
        }

        try
        {
            shortcutRegistration.Dispose();
            shortcutRegistration = null;
            ShortcutOutput.WriteLine("The Ctrl+Shift+G registration was removed.");
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            ShortcutOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private async ValueTask<CommandResult> ExecuteDemoHandlerAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        await Task.Delay(180, cancellationToken);
        var count = Interlocked.Increment(ref executionCount);
        return CommandResult.HandledWith(
            $"Hello, {context.Parameter ?? "runtime"}! Invocation #{count} from {context.Source}."
        );
    }

    private void RefreshRegistryState()
    {
        RegistrySummaryText.Text =
            $"Commands: {commandRegistry.Registrations.Count}  |  Shortcuts: {shortcuts.Registrations.Count}";

        var commandItems = commandRegistry.Registrations.Select(item =>
            $"Command  |  {item.CommandKey}  |  priority {item.Priority}"
        );
        var shortcutItems = shortcuts.Registrations.Select(item =>
            $"Shortcut  |  {item.Gesture.GetDisplayStringForCulture(System.Globalization.CultureInfo.CurrentCulture)}  |  {item.CommandKey}  |  {item.Scope}"
        );
        RegistryList.ItemsSource = commandItems.Concat(shortcutItems).ToArray();
    }

    private string RequireCommandKey()
    {
        var key = CommandKeyBox.Text.Trim();
        if (key.Length == 0)
        {
            throw new ArgumentException("Enter a command key.");
        }

        return key;
    }

    private static string FormatResult(string label, CommandResult result, bool? canExecute)
    {
        var canExecuteText = canExecute is null ? string.Empty : $"  |  Can execute: {canExecute}";
        var valueText = result.Value is null ? string.Empty : $"  |  Value: {result.Value}";
        var errorText = result.Exception is null ? string.Empty : $"  |  Error: {result.Exception.Message}";
        return $"{label} status: {result.Status}{canExecuteText}{valueText}{errorText}";
    }
}
