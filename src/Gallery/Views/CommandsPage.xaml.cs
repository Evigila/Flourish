using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CKey = ArkheideSystem.Essential.Culture.Key;
using Localizer = ArkheideSystem.Essential.Culture.Localizer;
using InputKey = System.Windows.Input.Key;
using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArkheideSystem.Gallery.Views;

public partial class CommandsPage : Page
{
    private static readonly KeyGesture DemoGesture = new(
        InputKey.G,
        ModifierKeys.Control | ModifierKeys.Shift
    );

    private readonly ICommandRegistry commandRegistry;
    private readonly ICommandDispatcher commandDispatcher;
    private readonly IShortcutService shortcuts;
    private IRegistration? commandRegistration;
    private IRegistration? shortcutRegistration;
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
            CommandOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_Registered0WithPriority1_10A41D09, key, 100)
            );
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
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
            CommandOutput.WriteLine(
                FormatResult(Localizer.Parse(CKey.Runtime_Command_71316697), result, canExecute)
            );
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void RemoveCommand_Click(object sender, RoutedEventArgs e)
    {
        if (commandRegistration is null)
        {
            CommandOutput.WriteLine(
                Localizer.Parse(
                    CKey.Runtime_ThisPageDoesNotCurrentlyOwnACommandRegistration_9C294C80
                )
            );
            return;
        }

        try
        {
            commandRegistration.Dispose();
            commandRegistration = null;
            CommandOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_TheRuntimeCommandRegistrationWasRemoved_9BF1D72B)
            );
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void CommandEnabled_Changed(object sender, RoutedEventArgs e)
    {
        try
        {
            commandEnabled = CommandEnabledBox.IsChecked == true;
            if (commandRegistration is not null)
            {
                commandRegistry.NotifyCanExecuteChanged(RequireCommandKey());
            }
            CommandOutput.WriteLine(
                commandRegistration is null
                    ? Localizer.Parse(
                        CKey.Runtime_TheNextRegisteredHandlerWillBe0_861E241D,
                        Localizer.Parse(
                            commandEnabled
                                ? CKey.Runtime_Enabled_FB9CF756
                                : CKey.Runtime_Disabled_17EB3C01
                        )
                    )
                    : Localizer.Parse(
                        CKey.Runtime_TheCommandIsNow0_25507420,
                        Localizer.Parse(
                            commandEnabled
                                ? CKey.Runtime_Enabled_FB9CF756
                                : CKey.Runtime_Disabled_17EB3C01
                        )
                    )
            );
        }
        catch (Exception error)
        {
            CommandOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
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
                Localizer.Parse(CKey.Runtime_CtrlShiftGNowDispatches0_2B41B29B, key)
            );
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            ShortcutOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
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
            ShortcutOutput.WriteLine(
                FormatResult(Localizer.Parse(CKey.Runtime_Shortcut_5753EA37), result, null)
            );
        }
        catch (Exception error)
        {
            ShortcutOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void RemoveShortcut_Click(object sender, RoutedEventArgs e)
    {
        if (shortcutRegistration is null)
        {
            ShortcutOutput.WriteLine(
                Localizer.Parse(
                    CKey.Runtime_ThisPageDoesNotCurrentlyOwnAShortcutRegistration_B41D35DD
                )
            );
            return;
        }

        try
        {
            shortcutRegistration.Dispose();
            shortcutRegistration = null;
            ShortcutOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_TheCtrlShiftGRegistrationWasRemoved_D62E6ACC)
            );
            RefreshRegistryState();
        }
        catch (Exception error)
        {
            ShortcutOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
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
            Localizer.Parse(
                CKey.Runtime_Hello0Invocation1From2_4B6A51EC,
                context.Parameter ?? Localizer.Parse(CKey.Runtime_Runtime_D92C6A81),
                count,
                context.Source
            )
        );
    }

    private void RefreshRegistryState()
    {
        RegistrySummaryText.Text = Localizer.Parse(
            CKey.Runtime_Commands0Shortcuts1_EE620469,
            commandRegistry.Current.Count,
            shortcuts.Current.Count
        );

        var commandItems = commandRegistry.Current.Select(item =>
            Localizer.Parse(
                CKey.Runtime_Command0Priority1_6F653DF7,
                item.CommandKey,
                item.Priority
            )
        );
        var shortcutItems = shortcuts.Current.Select(item =>
            Localizer.Parse(
                CKey.Runtime_Shortcut012_B5D726B0,
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
            : Localizer.Parse(CKey.Runtime_CanExecute0_43748D6F, canExecute);
        var valueText = result.Value is null
            ? string.Empty
            : Localizer.Parse(CKey.Runtime_Value0_BF1A56EF, result.Value);
        var errorText = result.Exception is null
            ? string.Empty
            : Localizer.Parse(CKey.Runtime_Error0_EB96953F, result.Exception.Message);
        return Localizer.Parse(
            CKey.Runtime_Text0Status1234_535E9DA8,
            label,
            result.Status,
            canExecuteText,
            valueText,
            errorText
        );
    }
}
