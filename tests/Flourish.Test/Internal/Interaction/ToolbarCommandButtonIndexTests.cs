using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Interaction;

namespace ArkheideSystem.Flourish.Test.Internal.Interaction;

public sealed class ToolbarCommandButtonIndexTests
{
    [Fact]
    public void Refresh_WithCommandKeyOnlyQueriesAndUpdatesMatchingButtons()
    {
        RunInSta(() =>
        {
            var dispatcher = new RecordingCommandDispatcher();
            dispatcher.SetAvailability("save", true);
            dispatcher.SetAvailability("export", true);
            var sut = new ToolbarCommandButtonIndex(dispatcher);
            var saveButton = new Button();
            var exportButton = new Button();
            sut.Track(saveButton, new FlourishToolbarItem("Save", "S", "save"));
            sut.Track(exportButton, new FlourishToolbarItem("Export", "E", "export"));
            dispatcher.ResetCalls();

            dispatcher.SetAvailability("save", false);
            sut.Refresh("save");

            Assert.False(saveButton.IsEnabled);
            Assert.True(exportButton.IsEnabled);
            Assert.Equal(1, dispatcher.GetCalls("save"));
            Assert.Equal(0, dispatcher.GetCalls("export"));
        });
    }

    [Fact]
    public void Refresh_WithoutCommandKeyUpdatesAllTrackedCommands()
    {
        RunInSta(() =>
        {
            var dispatcher = new RecordingCommandDispatcher();
            dispatcher.SetAvailability("save", true);
            dispatcher.SetAvailability("export", true);
            var sut = new ToolbarCommandButtonIndex(dispatcher);
            var saveButton = new Button();
            var exportButton = new Button();
            sut.Track(saveButton, new FlourishToolbarItem("Save", "S", "save"));
            sut.Track(exportButton, new FlourishToolbarItem("Export", "E", "export"));
            dispatcher.ResetCalls();
            dispatcher.SetAvailability("save", false);
            dispatcher.SetAvailability("export", false);

            sut.Refresh(commandKey: null);

            Assert.False(saveButton.IsEnabled);
            Assert.False(exportButton.IsEnabled);
            Assert.Equal(1, dispatcher.GetCalls("save"));
            Assert.Equal(1, dispatcher.GetCalls("export"));
        });
    }

    [Fact]
    public void Track_StaticDisabledItemNeverQueriesOrEnablesItsCommand()
    {
        RunInSta(() =>
        {
            var dispatcher = new RecordingCommandDispatcher();
            dispatcher.SetAvailability("save", true);
            var sut = new ToolbarCommandButtonIndex(dispatcher);
            var button = new Button();
            var item = new FlourishToolbarItem("Save", "S", "save")
            {
                IsEnabled = false,
            };

            sut.Track(button, item);
            sut.Refresh("save");

            Assert.False(button.IsEnabled);
            Assert.Equal(0, dispatcher.GetCalls("save"));
        });
    }

    [Fact]
    public void Clear_StopsPreviouslyTrackedButtonsFromRefreshing()
    {
        RunInSta(() =>
        {
            var dispatcher = new RecordingCommandDispatcher();
            dispatcher.SetAvailability("save", true);
            var sut = new ToolbarCommandButtonIndex(dispatcher);
            var button = new Button();
            sut.Track(button, new FlourishToolbarItem("Save", "S", "save"));
            dispatcher.ResetCalls();
            dispatcher.SetAvailability("save", false);

            sut.Clear();
            sut.Refresh("save");
            sut.Refresh(commandKey: null);

            Assert.True(button.IsEnabled);
            Assert.Equal(0, dispatcher.GetCalls("save"));
        });
    }

    private static void RunInSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                error = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    private sealed class RecordingCommandDispatcher : ICommandDispatcher
    {
        private readonly Dictionary<string, bool> availability = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> calls = new(StringComparer.Ordinal);

        public bool CanExecute(
            string commandKey,
            object? parameter = null,
            CommandSource source = CommandSource.Application
        )
        {
            Assert.Equal(CommandSource.Toolbar, source);
            calls[commandKey] = GetCalls(commandKey) + 1;
            return availability.GetValueOrDefault(commandKey);
        }

        public ValueTask<CommandResult> ExecuteAsync(
            string commandKey,
            object? parameter = null,
            CommandSource source = CommandSource.Application,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(CommandResult.NotHandled);
        }

        internal int GetCalls(string commandKey)
        {
            return calls.GetValueOrDefault(commandKey);
        }

        internal void ResetCalls()
        {
            calls.Clear();
        }

        internal void SetAvailability(string commandKey, bool canExecute)
        {
            availability[commandKey] = canExecute;
        }
    }
}
