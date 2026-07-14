using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using FlourishNavigationService = ArkheideSystem.Flourish.Services.NavigationService;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FrameNavigationContentHostTests
{
    private const string XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void ShellFrames_UseTheParentJournalPolicy()
    {
        var document = XDocument.Load(GetShellXamlPath());
        var name = XName.Get("Name", XamlNamespace);
        var frames = document
            .Descendants()
            .Where(element => element.Name.LocalName == "Frame")
            .Where(element =>
                (string?)element.Attribute(name) is "RootFrame" or "ProfileFrame"
            )
            .ToArray();

        Assert.Equal(2, frames.Length);
        Assert.All(
            frames,
            frame =>
                Assert.Equal(
                    nameof(JournalOwnership.UsesParentJournal),
                    (string?)frame.Attribute("JournalOwnership")
                )
        );
    }

    [Fact]
    public void Navigation_UsesOnlyTheBoundedFlourishHistory()
    {
        RunInSta(() =>
        {
            var frame = new Frame
            {
                JournalOwnership = JournalOwnership.UsesParentJournal,
                NavigationUIVisibility = NavigationUIVisibility.Hidden,
            };
            var window = new Window
            {
                Width = 320,
                Height = 200,
                Left = -10_000,
                Top = -10_000,
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Content = frame,
            };
            var options = new FlourishShellOptions();
            Register(options, HomeKey, typeof(HomePage));
            Register(options, SettingsKey, typeof(SettingsPage));
            Register(options, GalleryKey, typeof(GalleryPage));
            var sut = new FlourishNavigationService(
                new TestPageProvider(),
                new PageHistoryService(maximumEntries: 2),
                new NavigationRouteRegistry(options)
            );
            sut.Initialize(frame);
            window.Show();

            try
            {
                NavigateAndAssert(sut, frame, HomeKey, 0, typeof(HomePage));
                NavigateAndAssert(sut, frame, SettingsKey, 1, typeof(SettingsPage));
                NavigateAndAssert(sut, frame, GalleryKey, 2, typeof(GalleryPage));
                NavigateAndAssert(sut, frame, HomeKey, 3, typeof(HomePage));

                Assert.True(sut.GoBack());
                PumpDispatcher();
                AssertNavigationState(sut, frame, GalleryKey, 2, typeof(GalleryPage));

                Assert.True(sut.GoBack());
                PumpDispatcher();
                AssertNavigationState(sut, frame, SettingsKey, 1, typeof(SettingsPage));
                Assert.False(sut.GoBack());

                Assert.True(sut.GoForward());
                PumpDispatcher();
                AssertNavigationState(sut, frame, GalleryKey, 2, typeof(GalleryPage));

                Assert.True(sut.GoForward());
                PumpDispatcher();
                AssertNavigationState(sut, frame, HomeKey, 3, typeof(HomePage));
                Assert.False(sut.GoForward());
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void NavigateAndAssert(
        FlourishNavigationService service,
        Frame frame,
        string navigationKey,
        object parameter,
        Type expectedPageType
    )
    {
        Assert.True(service.Navigate(navigationKey, parameter));
        PumpDispatcher();
        AssertNavigationState(service, frame, navigationKey, parameter, expectedPageType);
    }

    private static void AssertNavigationState(
        FlourishNavigationService service,
        Frame frame,
        string navigationKey,
        object parameter,
        Type expectedPageType
    )
    {
        Assert.Equal(navigationKey, service.CurrentNavigationKey);
        Assert.Equal(parameter, service.CurrentParameter);
        Assert.IsType(expectedPageType, frame.Content);
        Assert.False(frame.CanGoBack);
        Assert.False(frame.CanGoForward);
        Assert.Empty(frame.BackStack?.Cast<object>() ?? []);
        Assert.Empty(frame.ForwardStack?.Cast<object>() ?? []);
    }

    private static void Register(
        FlourishShellOptions options,
        string navigationKey,
        Type pageType
    )
    {
        options.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute(navigationKey, pageType)
        );
    }

    private static void PumpDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(
            DispatcherPriority.ApplicationIdle,
            new Action(() => { })
        );
    }

    private static string GetShellXamlPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine(
                directory.FullName,
                "src",
                "Flourish",
                "Views",
                "Windows",
                "FlourishShellWindow.xaml"
            );
            if (File.Exists(path))
            {
                return path;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
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
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    private sealed class TestPageProvider : INavigationPageProvider
    {
        public Page GetPage(Type sourcePageType)
        {
            if (sourcePageType == typeof(HomePage))
            {
                return new HomePage();
            }

            if (sourcePageType == typeof(SettingsPage))
            {
                return new SettingsPage();
            }

            if (sourcePageType == typeof(GalleryPage))
            {
                return new GalleryPage();
            }

            throw new InvalidOperationException(
                $"Unexpected page type '{sourcePageType.FullName}'."
            );
        }
    }

    private sealed class HomePage : Page { }

    private sealed class SettingsPage : Page { }

    private sealed class GalleryPage : Page { }

    private const string HomeKey = "Home";
    private const string SettingsKey = "Settings";
    private const string GalleryKey = "Gallery";
}
