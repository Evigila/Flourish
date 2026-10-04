using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Views.Windows;
using ArkheideSystem.Tests.Flourish.WPF.Infrastructure;

using WpfButton = System.Windows.Controls.Button;

namespace ArkheideSystem.Tests.Flourish.WPF.Internal.Interaction;

public sealed class ToolbarCommandButtonIndexTests
{
    [Fact]
    public void Refresh_WithCommandKeyOnlyQueriesAndUpdatesMatchingButtons()
    {
        StaTest.Run(() =>
        {
            var dispatcher = new RecordingCommandDispatcher();
            dispatcher.SetAvailability("save", true);
            dispatcher.SetAvailability("export", true);
            var sut = new ToolbarCommandButtonIndex(dispatcher);
            var saveButton = new WpfButton();
            var exportButton = new WpfButton();
            sut.Track(saveButton, new ToolbarItem("Save", "S", "save"));
            sut.Track(exportButton, new ToolbarItem("Export", "E", "export"));
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
        StaTest.Run(() =>
        {
            var dispatcher = new RecordingCommandDispatcher();
            dispatcher.SetAvailability("save", true);
            dispatcher.SetAvailability("export", true);
            var sut = new ToolbarCommandButtonIndex(dispatcher);
            var saveButton = new WpfButton();
            var exportButton = new WpfButton();
            sut.Track(saveButton, new ToolbarItem("Save", "S", "save"));
            sut.Track(exportButton, new ToolbarItem("Export", "E", "export"));
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
        StaTest.Run(() =>
        {
            var dispatcher = new RecordingCommandDispatcher();
            dispatcher.SetAvailability("save", true);
            var sut = new ToolbarCommandButtonIndex(dispatcher);
            var button = new WpfButton();
            var item = new ToolbarItem("Save", "S", "save") { IsEnabled = false };

            sut.Track(button, item);
            sut.Refresh("save");

            Assert.False(button.IsEnabled);
            Assert.Equal(0, dispatcher.GetCalls("save"));
        });
    }

    [Fact]
    public void Clear_StopsPreviouslyTrackedButtonsFromRefreshing()
    {
        StaTest.Run(() =>
        {
            var dispatcher = new RecordingCommandDispatcher();
            dispatcher.SetAvailability("save", true);
            var sut = new ToolbarCommandButtonIndex(dispatcher);
            var button = new WpfButton();
            sut.Track(button, new ToolbarItem("Save", "S", "save"));
            dispatcher.ResetCalls();
            dispatcher.SetAvailability("save", false);

            sut.Clear();
            sut.Refresh("save");
            sut.Refresh(commandKey: null);

            Assert.True(button.IsEnabled);
            Assert.Equal(0, dispatcher.GetCalls("save"));
        });
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
