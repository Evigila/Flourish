using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ArkheideSystem.Flourish.WPF;
using Xunit;
using ThemeMode = ArkheideSystem.Flourish.WPF.Abstract.ThemeMode;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ArkheideSystem.Tests.Flourish.WPF;

internal static class NativeTest
{
    public static void Run(Action action) => Run(() => { action(); return Task.CompletedTask; });
    public static void Run(Func<Task> action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
            var frame = new DispatcherFrame();
            dispatcher.UnhandledException += (_, args) => { failure = args.Exception; args.Handled = true; frame.Continue = false; };
            dispatcher.BeginInvoke(new Action(async () =>
            {
                try { await action(); }
                catch (Exception error) { failure = error; }
                finally { frame.Continue = false; }
            }));
            Dispatcher.PushFrame(frame);
            dispatcher.InvokeShutdown();
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "Native STA check exceeded 30 seconds.");
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    public static Grid Stage(FrameworkElement content, double width = 1000, double height = 700, ThemeMode mode = ThemeMode.Light)
    {
        var stage = new Grid();
        FrameworkResources.Apply(stage);
        stage.Resources.MergedDictionaries.Add(DesignResources.Load(mode));
        stage.SetResourceReference(Panel.BackgroundProperty, "Flourish.Brush.Canvas");
        stage.Children.Add(content);
        Layout(stage, width, height);
        return stage;
    }

    public static void Layout(FrameworkElement element, double width, double height)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();
    }

    public static IDisposable Connect(Visual root) => new OffscreenSource(root);

    private sealed class OffscreenSource : PresentationSource, IDisposable
    {
        private readonly VisualTarget target = new(new HostVisual());
        private bool disposed;
        public OffscreenSource(Visual root) { AddSource(); RootVisual = root; }
        public override Visual RootVisual
        {
            get => target.RootVisual;
            set { var previous = target.RootVisual; target.RootVisual = value; RootChanged(previous, value); }
        }
        public override bool IsDisposed => disposed;
        protected override CompositionTarget GetCompositionTargetCore() => target;
        public void Dispose()
        {
            if (disposed) return;
            RootVisual = null!; RemoveSource(); target.Dispose(); disposed = true;
        }
    }

    public static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        if (root is T found) yield return found;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
            foreach (var child in Descendants<T>(VisualTreeHelper.GetChild(root, index))) yield return child;
    }

    public static T Part<T>(Control owner, string name) where T : DependencyObject
    {
        owner.ApplyTemplate();
        return Assert.IsAssignableFrom<T>(owner.Template.FindName(name, owner));
    }

    public static string Repository()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Cannot find repository root.");
    }

    public static void Export(FrameworkElement element, string name, int width = 1000, int height = 700)
    {
        Layout(element, width, height);
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);
        var bytes = new byte[width * height * 4];
        bitmap.CopyPixels(bytes, width * 4, 0);
        Assert.Contains(bytes.Where((_, index) => index % 4 == 3), alpha => alpha != 0);
        var directory = Path.Combine(Repository(), "artifacts", "wpf-render");
        Directory.CreateDirectory(directory);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(Path.Combine(directory, name + ".png"));
        encoder.Save(stream);
    }
}
