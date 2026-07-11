using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Shell;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Services;
using ArkheideSystem.Flourish.Windows;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellWindowFrameTests
{
    [Fact]
    public void Apply_KeepsOneCustomFrameForTheWindowLifetime()
    {
        RunInSta(() =>
        {
            var window = new Window { ShowInTaskbar = false };
            var shellBorder = new Border();
            var sut = new FlourishShellWindowFrame(window, shellBorder);
            sut.Apply();
            var chrome = WindowChrome.GetWindowChrome(window);
            var handle = new WindowInteropHelper(window).EnsureHandle();

            try
            {
                for (var index = 0; index < 20; index++)
                {
                    sut.Apply();

                    Assert.Equal(handle, new WindowInteropHelper(window).Handle);
                    Assert.Equal(WindowStyle.None, window.WindowStyle);
                    Assert.Same(chrome, WindowChrome.GetWindowChrome(window));
                    Assert.Same(sut.Chrome, chrome);
                    Assert.Equal(new Thickness(1), shellBorder.BorderThickness);
                }

                Assert.Equal(new Thickness(6), chrome!.ResizeBorderThickness);
                Assert.False(chrome.UseAeroCaptionButtons);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void SurfaceVisibilityDoesNotAffectAttachedFrameOrMaterial()
    {
        RunInSta(() =>
        {
            var window = new Window { ShowInTaskbar = false };
            var shellBorder = new Border();
            var titleBarContent = new Border();
            var titleBarPresenter = new FlourishTitlebarFeaturePresenter(titleBarContent);
            var frame = new FlourishShellWindowFrame(window, shellBorder);
            var material = new MaterialEffectService();
            frame.Apply();
            new WindowInteropHelper(window).EnsureHandle();
            var effect = material.IsSupported(MaterialEffect.Mica)
                ? MaterialEffect.Mica
                : MaterialEffect.None;
            material.Attach(window, effect);
            var chrome = WindowChrome.GetWindowChrome(window);
            var glassFrame = chrome!.GlassFrameThickness;
            var isApplied = material.IsApplied;

            try
            {
                for (var index = 0; index < 20; index++)
                {
                    var isTitleBarEnabled = index % 2 != 0;
                    titleBarPresenter.SetEnabled(isTitleBarEnabled);

                    Assert.Equal(isTitleBarEnabled, titleBarPresenter.IsEnabled);
                    Assert.Equal(
                        isTitleBarEnabled ? Visibility.Visible : Visibility.Collapsed,
                        titleBarContent.Visibility
                    );
                    Assert.Same(chrome, WindowChrome.GetWindowChrome(window));
                    Assert.Equal(WindowStyle.None, window.WindowStyle);
                    Assert.Equal(effect, material.CurrentEffect);
                    Assert.Equal(isApplied, material.IsApplied);
                    Assert.Equal(glassFrame, chrome.GlassFrameThickness);
                }
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
}
