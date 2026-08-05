using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class CommandsPage : Page
{
    private static readonly KeyGesture DemoGesture =
        new(Key.G, ModifierKeys.Control | ModifierKeys.Shift);

    private readonly ICommandRegistry commandRegistry;
    private readonly ICommandDispatcher commandDispatcher;
    private readonly IShortcutService shortcuts;
    private readonly IGalleryLocalization localization;
    private ICommandRegistration? commandRegistration;
    private IShortcutRegistration? shortcutRegistration;
    private bool commandEnabled = true;
    private int executionCount;

    public CommandsPage(
        ICommandRegistry commandRegistry,
        ICommandDispatcher commandDispatcher,
        IShortcutService shortcuts,
        IGalleryLocalization localization
    )
    {
        this.commandRegistry = commandRegistry;
        this.commandDispatcher = commandDispatcher;
        this.shortcuts = shortcuts;
        this.localization = localization;
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
            CommandOutput.WriteLine(
                localization.Format("Registered '{0}' with priority {1}.", key, 100)
            );
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
            CommandOutput.WriteLine(FormatResult(localization.Get("Command"), result, canExecute));
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }
    }

    private void RemoveCommand_Click(object sender, RoutedEventArgs e)
    {
        if (commandRegistration is null)
        {
            CommandOutput.WriteLine(
                localization.Get("This page does not currently own a command registration.")
            );
            return;
        }

        try
        {
            commandRegistration.Dispose();
            commandRegistration = null;
            CommandOutput.WriteLine(
                localization.Get("The runtime command registration was removed.")
            );
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
                    ? localization.Format(
                        "The next registered handler will be {0}.",
                        localization.Get(commandEnabled ? "enabled" : "disabled")
                    )
                    : localization.Format(
                        "The command is now {0}.",
                        localization.Get(commandEnabled ? "enabled" : "disabled")
                    )
            );
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
            ShortcutOutput.WriteLine(
                localization.Format("Ctrl+Shift+G now dispatches '{0}'.", key)
            );
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            ShortcutOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
            ShortcutOutput.WriteLine(FormatResult(localization.Get("Shortcut"), result, null));
        }
        catch (Exception error)
        {
            ShortcutOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }
    }

    private void RemoveShortcut_Click(object sender, RoutedEventArgs e)
    {
        if (shortcutRegistration is null)
        {
            ShortcutOutput.WriteLine(
                localization.Get("This page does not currently own a shortcut registration.")
            );
            return;
        }

        try
        {
            shortcutRegistration.Dispose();
            shortcutRegistration = null;
            ShortcutOutput.WriteLine(
                localization.Get("The Ctrl+Shift+G registration was removed.")
            );
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            ShortcutOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
            localization.Format(
                "Hello, {0}! Invocation #{1} from {2}.",
                context.Parameter ?? localization.Get("runtime"),
                count,
                context.Source
            )
        );
    }

    private void RefreshRegistryState()
    {
        RegistrySummaryText.Text = localization.Format(
            "Commands: {0}  |  Shortcuts: {1}",
            commandRegistry.Registrations.Count,
            shortcuts.Registrations.Count
        );

        var commandItems = commandRegistry.Registrations.Select(item =>
            localization.Format(
                "Command  |  {0}  |  priority {1}",
                item.CommandKey,
                item.Priority
            )
        );
        var shortcutItems = shortcuts.Registrations.Select(item =>
            localization.Format(
                "Shortcut  |  {0}  |  {1}  |  {2}",
                item.Gesture.GetDisplayStringForCulture(
                    System.Globalization.CultureInfo.CurrentCulture
                ),
                item.CommandKey,
                item.Scope
            )
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

    private string FormatResult(string label, CommandResult result, bool? canExecute)
    {
        var canExecuteText = canExecute is null
            ? string.Empty
            : localization.Format("  |  Can execute: {0}", canExecute);
        var valueText = result.Value is null
            ? string.Empty
            : localization.Format("  |  Value: {0}", result.Value);
        var errorText = result.Exception is null
            ? string.Empty
            : localization.Format("  |  Error: {0}", result.Exception.Message);
        return localization.Format(
            "{0} status: {1}{2}{3}{4}",
            label,
            result.Status,
            canExecuteText,
            valueText,
            errorText
        );
    }
}
