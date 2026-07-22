using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;
using ArkheideSystem.Flourish.Views.Windows;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellWindowFrameTests
{
    [Fact]
    public void Apply_TogglesCustomAndNativeFramesWithoutRecreatingTheWindow()
    {
        RunInSta(() =>
        {
            var window = CreateTestWindow();
            var shellBorder = new Border();
            window.Content = shellBorder;
            var sut = new FlourishShellWindowFrame(window, shellBorder);
            sut.Apply(FlourishShellWindowFrameMode.Custom);
            window.Show();
            var chrome = sut.Chrome;
            var handle = new WindowInteropHelper(window).Handle;

            try
            {
                AssertCustomFrame(window, shellBorder, handle, chrome);
                for (var index = 0; index < 20; index++)
                {
                    sut.Apply(FlourishShellWindowFrameMode.Native);
                    AssertNativeFrame(window, shellBorder, handle, chrome);

                    // Reapplying a mode must be idempotent.
                    sut.Apply(FlourishShellWindowFrameMode.Native);
                    AssertNativeFrame(window, shellBorder, handle, chrome);

                    sut.Apply(FlourishShellWindowFrameMode.Custom);
                    AssertCustomFrame(window, shellBorder, handle, chrome);

                    sut.Apply(FlourishShellWindowFrameMode.Custom);
                    AssertCustomFrame(window, shellBorder, handle, chrome);
                }

                Assert.Equal(new Thickness(6), chrome.ResizeBorderThickness);
                Assert.False(chrome.UseAeroCaptionButtons);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Apply_NativeFrameCanBeTheInitialMode()
    {
        RunInSta(() =>
        {
            var window = CreateTestWindow();
            var shellBorder = new Border { BorderThickness = new Thickness(7) };
            window.Content = shellBorder;
            var frame = new FlourishShellWindowFrame(window, shellBorder);
            frame.Apply(FlourishShellWindowFrameMode.Native);
            window.Show();
            var handle = new WindowInteropHelper(window).Handle;

            try
            {
                AssertNativeFrame(window, shellBorder, handle, frame.Chrome);

                frame.Apply(FlourishShellWindowFrameMode.Custom);

                AssertCustomFrame(window, shellBorder, handle, frame.Chrome);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void MaterialEffect_RemainsIndependentAcrossFrameAndEffectChanges()
    {
        RunInSta(() =>
        {
            var window = CreateTestWindow();
            var shellBorder = new Border();
            window.Content = shellBorder;
            var frame = new FlourishShellWindowFrame(window, shellBorder);
            var material = new MaterialEffectService();
            var originalGlassFrame = frame.Chrome.GlassFrameThickness;
            frame.Apply(FlourishShellWindowFrameMode.Custom);
            window.Show();
            var handle = new WindowInteropHelper(window).Handle;
            material.Attach(window, MaterialEffect.Mica);

            try
            {
                Assert.Equal(MaterialEffect.Mica, material.CurrentEffect);
                Assert.Equal(originalGlassFrame, frame.Chrome.GlassFrameThickness);

                // Custom + Mica -> Native + Mica.
                frame.Apply(FlourishShellWindowFrameMode.Native);
                material.Reapply(window);
                AssertNativeFrame(window, shellBorder, handle, frame.Chrome);
                Assert.Equal(MaterialEffect.Mica, material.CurrentEffect);
                Assert.Equal(originalGlassFrame, frame.Chrome.GlassFrameThickness);

                // Changing the effect while the custom chrome is detached must not leave
                // stale glass settings when that chrome is attached again.
                material.SetEffect(MaterialEffect.None);
                Assert.Equal(MaterialEffect.None, material.CurrentEffect);
                Assert.Equal(originalGlassFrame, frame.Chrome.GlassFrameThickness);

                frame.Apply(FlourishShellWindowFrameMode.Custom);
                material.Reapply(window);
                AssertCustomFrame(window, shellBorder, handle, frame.Chrome);
                Assert.Equal(MaterialEffect.None, material.CurrentEffect);
                Assert.Equal(originalGlassFrame, frame.Chrome.GlassFrameThickness);

                // Exercise the inverse sequence: Native + None -> Native + Mica -> Custom + Mica.
                frame.Apply(FlourishShellWindowFrameMode.Native);
                material.Reapply(window);
                material.SetEffect(MaterialEffect.Mica);
                Assert.Equal(MaterialEffect.Mica, material.CurrentEffect);
                Assert.Equal(originalGlassFrame, frame.Chrome.GlassFrameThickness);

                frame.Apply(FlourishShellWindowFrameMode.Custom);
                material.Reapply(window);
                AssertCustomFrame(window, shellBorder, handle, frame.Chrome);
                Assert.Equal(MaterialEffect.Mica, material.CurrentEffect);
                Assert.Equal(originalGlassFrame, frame.Chrome.GlassFrameThickness);

                material.SetEffect(MaterialEffect.None);
                Assert.Equal(MaterialEffect.None, material.CurrentEffect);
                Assert.Equal(originalGlassFrame, frame.Chrome.GlassFrameThickness);
            }
            finally
            {
                material.Detach(window);
                window.Close();
            }
        });
    }

    [Fact]
    public void MaterialEffect_DwmCompositionChangeReappliesWithoutChangingRuntimeState()
    {
        const int wmDwmCompositionChanged = 0x031E;
        RunInSta(() =>
        {
            var originalBackground = new SolidColorBrush(Color.FromRgb(20, 40, 60));
            var replacementBackground = new SolidColorBrush(Color.FromRgb(180, 120, 40));
            var window = CreateTestWindow();
            window.Background = originalBackground;
            window.Content = new Border();
            window.Show();
            var handle = new WindowInteropHelper(window).Handle;
            var material = new MaterialEffectService();
            var changedCount = 0;
            material.Changed += (_, _) => changedCount++;
            material.Attach(window, MaterialEffect.None);

            try
            {
                window.Background = replacementBackground;
                Assert.Same(replacementBackground, window.Background);

                SendMessage(
                    handle,
                    wmDwmCompositionChanged,
                    IntPtr.Zero,
                    IntPtr.Zero
                );

                Assert.Same(originalBackground, window.Background);
                Assert.Equal(MaterialEffect.None, material.CurrentEffect);
                Assert.False(material.IsApplied);
                Assert.Equal(0, changedCount);
            }
            finally
            {
                material.Detach(window);
                window.Close();
            }
        });
    }

    [Fact]
    public void MaterialEffect_ResourceBackedBackgroundRestoresTheCurrentThemeValue()
    {
        const string backgroundResourceKey = "FlourishShellBackgroundBrush";
        RunInSta(() =>
        {
            var initialBackground = new SolidColorBrush(Color.FromRgb(20, 40, 60));
            var currentThemeBackground = new SolidColorBrush(Color.FromRgb(180, 120, 40));
            var replacementBackground = new SolidColorBrush(Color.FromRgb(5, 10, 15));
            var window = CreateTestWindow();
            window.Resources[backgroundResourceKey] = initialBackground;
            window.SetResourceReference(Window.BackgroundProperty, backgroundResourceKey);
            window.Content = new Border();
            window.Show();
            var material = new MaterialEffectService();
            material.Attach(window, MaterialEffect.None, backgroundResourceKey);

            try
            {
                // Model the local transparent value used while Mica is active, then
                // switch the palette before the effect is disabled.
                window.Background = replacementBackground;
                window.Resources[backgroundResourceKey] = currentThemeBackground;

                material.Reapply(window);

                Assert.Same(currentThemeBackground, window.Background);

                var nextThemeBackground = new SolidColorBrush(Color.FromRgb(70, 80, 90));
                window.Resources[backgroundResourceKey] = nextThemeBackground;
                Assert.Same(nextThemeBackground, window.Background);
            }
            finally
            {
                material.Detach(window);
                window.Close();
            }
        });
    }

    [Fact]
    public void WindowFrameFixService_AttachIsIdempotentAndTracksOneWindow()
    {
        RunInSta(() =>
        {
            var first = new Window { ShowInTaskbar = false };
            var second = new Window { ShowInTaskbar = false };
            new WindowInteropHelper(first).EnsureHandle();
            new WindowInteropHelper(second).EnsureHandle();
            var sut = new WindowFrameFixService();

            sut.Attach(first);
            sut.Attach(first);
            sut.RefreshFrame(first, useCustomFrame: true);
            sut.RefreshFrame(first, useCustomFrame: false);
            Assert.True(sut.IsAttachedTo(first));

            sut.Attach(second);
            Assert.False(sut.IsAttachedTo(first));
            Assert.True(sut.IsAttachedTo(second));

            first.Close();
            Assert.True(sut.IsAttachedTo(second));

            second.Close();
            Assert.False(sut.IsAttachedTo(second));
        });
    }

    [Fact]
    public void UpdateWindowState_RemovesCustomEdgeMetricsWhenMaximizedAndRestoresThem()
    {
        RunInSta(() =>
        {
            var window = CreateTestWindow();
            var shellBorder = new Border();
            window.Content = shellBorder;
            var frame = new FlourishShellWindowFrame(window, shellBorder);
            frame.Apply(FlourishShellWindowFrameMode.Custom);

            Assert.Equal(new Thickness(1), shellBorder.BorderThickness);

            window.WindowState = WindowState.Maximized;
            frame.UpdateWindowState();

            Assert.Equal(new Thickness(), shellBorder.BorderThickness);
            Assert.Equal(new Thickness(), frame.Chrome.ResizeBorderThickness);

            window.WindowState = WindowState.Normal;
            frame.UpdateWindowState();

            Assert.Equal(new Thickness(1), shellBorder.BorderThickness);
            Assert.Equal(new Thickness(6), frame.Chrome.ResizeBorderThickness);
        });
    }

    [Fact]
    public void Apply_CustomFrameStartsWithoutBorderWhenWindowIsAlreadyMaximized()
    {
        RunInSta(() =>
        {
            var window = CreateTestWindow();
            var shellBorder = new Border();
            window.Content = shellBorder;
            window.WindowState = WindowState.Maximized;
            var frame = new FlourishShellWindowFrame(window, shellBorder);

            frame.Apply(FlourishShellWindowFrameMode.Custom);

            Assert.Equal(new Thickness(), shellBorder.BorderThickness);
            Assert.Equal(new Thickness(), frame.Chrome.ResizeBorderThickness);
        });
    }

    [Fact]
    public void WindowFrameFixService_CustomFramePreservesWpfTrackSizeConstraints()
    {
        const int wmGetMinMaxInfo = 0x0024;
        RunInSta(() =>
        {
            var window = CreateTestWindow();
            window.MinWidth = 1280;
            window.MinHeight = 720;
            window.MaxWidth = 1600;
            window.MaxHeight = 900;
            var shellBorder = new Border();
            window.Content = shellBorder;
            var frame = new FlourishShellWindowFrame(window, shellBorder);
            var frameFix = new WindowFrameFixService();
            frame.Apply(FlourishShellWindowFrameMode.Custom);
            frameFix.Attach(window, useCustomFrame: true);
            window.Show();
            var handle = new WindowInteropHelper(window).Handle;
            var dpi = VisualTreeHelper.GetDpi(window);
            var minMaxInfo = CreateNativeMinMaxInfo();
            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<MinMaxInfo>());

            try
            {
                AssertCustomFrame(window, shellBorder, handle, frame.Chrome);
                Marshal.StructureToPtr(minMaxInfo, pointer, false);

                SendMessage(handle, wmGetMinMaxInfo, IntPtr.Zero, pointer);

                minMaxInfo = Marshal.PtrToStructure<MinMaxInfo>(pointer);
                AssertDevicePixels(window.MinWidth, dpi.DpiScaleX, minMaxInfo.MinTrackSize.X);
                AssertDevicePixels(window.MinHeight, dpi.DpiScaleY, minMaxInfo.MinTrackSize.Y);
                AssertDevicePixels(window.MaxWidth, dpi.DpiScaleX, minMaxInfo.MaxTrackSize.X);
                AssertDevicePixels(window.MaxHeight, dpi.DpiScaleY, minMaxInfo.MaxTrackSize.Y);
                Assert.True(minMaxInfo.MaxSize.X > 0);
                Assert.True(minMaxInfo.MaxSize.Y > 0);
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
                window.Close();
            }
        });
    }

    [Fact]
    public void ApplyFrameTransition_DoesNotShowAHiddenWindow()
    {
        RunInSta(() =>
        {
            var window = CreateTestWindow();
            var shellBorder = new Border();
            window.Content = shellBorder;
            var frame = new FlourishShellWindowFrame(window, shellBorder);
            var frameFix = new WindowFrameFixService();
            frame.Apply(FlourishShellWindowFrameMode.Custom);
            window.Show();
            var handle = new WindowInteropHelper(window).Handle;
            frameFix.Attach(window, useCustomFrame: true);
            window.Hide();

            try
            {
                AssertWindowIsHidden(window, handle);

                frameFix.ApplyFrameTransition(
                    window,
                    useCustomFrame: false,
                    () => frame.Apply(FlourishShellWindowFrameMode.Native)
                );
                AssertWindowIsHidden(window, handle);

                frameFix.ApplyFrameTransition(
                    window,
                    useCustomFrame: true,
                    () => frame.Apply(FlourishShellWindowFrameMode.Custom)
                );
                AssertWindowIsHidden(window, handle);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void AssertCustomFrame(
        Window window,
        Border shellBorder,
        IntPtr expectedHandle,
        WindowChrome expectedChrome
    )
    {
        Assert.Equal(expectedHandle, new WindowInteropHelper(window).Handle);
        Assert.Equal(WindowStyle.None, window.WindowStyle);
        Assert.False(window.AllowsTransparency);
        Assert.Same(expectedChrome, WindowChrome.GetWindowChrome(window));
        Assert.Equal(new Thickness(1), shellBorder.BorderThickness);
        Assert.Equal(new Thickness(), expectedChrome.GlassFrameThickness);
    }

    private static void AssertNativeFrame(
        Window window,
        Border shellBorder,
        IntPtr expectedHandle,
        WindowChrome persistentChrome
    )
    {
        Assert.Equal(expectedHandle, new WindowInteropHelper(window).Handle);
        Assert.Equal(WindowStyle.SingleBorderWindow, window.WindowStyle);
        Assert.False(window.AllowsTransparency);
        Assert.Null(WindowChrome.GetWindowChrome(window));
        Assert.Equal(new Thickness(), shellBorder.BorderThickness);
        Assert.Equal(new Thickness(), persistentChrome.GlassFrameThickness);
    }

    private static void AssertWindowIsHidden(Window window, IntPtr handle)
    {
        Assert.False(window.IsVisible);
        Assert.False(IsWindowVisible(handle));
    }

    private static MinMaxInfo CreateNativeMinMaxInfo() =>
        new()
        {
            MinTrackSize = new NativePoint(1, 1),
            MaxTrackSize = new NativePoint(100_000, 100_000),
        };

    private static void AssertDevicePixels(double logicalValue, double scale, int actual)
    {
        Assert.Equal((int)(logicalValue * scale + 0.5), actual);
    }

    private static Window CreateTestWindow() =>
        new()
        {
            Width = 320,
            Height = 200,
            Left = -10_000,
            Top = -10_000,
            ShowActivated = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
        };

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

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr windowHandle);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(
        IntPtr windowHandle,
        int message,
        IntPtr wParam,
        IntPtr lParam
    );

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public NativePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;

        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public NativePoint Reserved;

        public NativePoint MaxSize;

        public NativePoint MaxPosition;

        public NativePoint MinTrackSize;

        public NativePoint MaxTrackSize;
    }
}
