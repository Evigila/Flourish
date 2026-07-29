using System.Globalization;
using System.Threading.Channels;
using System.Windows;
using ArkheideSystem.Flourish.Internal.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishPreferencePersistenceService(
    FlourishDataOptions dataOptions,
    FlourishShellOptions shellOptions,
    IFlourishSettingsStore appSettings,
    IFlourishLocalization localization,
    IWindowService window,
    INavigationPanelService navigationPanel,
    INavigationService navigation,
    IMotionService motion,
    IScrollService scroll,
    IFontService font,
    IContentLayoutService contentLayout,
    IMaterialEffectService material,
    IAppearanceService appearance,
    ITrayService tray,
    IProfileService profile,
    ILogger<FlourishPreferencePersistenceService> logger
) : IHostedService
{
    private static readonly TimeSpan CoalescingDelay = TimeSpan.FromMilliseconds(250);
    private readonly Channel<bool> changes = Channel.CreateBounded<bool>(
        new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        }
    );
    private Task? worker;
    private WindowState lastRestorableWindowState =
        shellOptions.WindowState == WindowState.Maximized
            ? WindowState.Maximized
            : WindowState.Normal;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Subscribe();
        worker = Task.Run(ProcessChangesAsync, CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Unsubscribe();
        changes.Writer.TryWrite(true);
        changes.Writer.TryComplete();
        if (worker is not null)
        {
            await worker.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private void Subscribe()
    {
        localization.Changed += PreferenceChanged;
        window.Changed += Window_StateChanged;
        navigationPanel.Changed += PreferenceChanged;
        navigation.Navigated += PreferenceChanged;
        motion.Changed += PreferenceChanged;
        scroll.Changed += PreferenceChanged;
        font.Changed += PreferenceChanged;
        contentLayout.Changed += PreferenceChanged;
        material.Changed += PreferenceChanged;
        appearance.Changed += PreferenceChanged;
        tray.Changed += PreferenceChanged;
        profile.ProfileChanged += PreferenceChanged;
    }

    private void Unsubscribe()
    {
        localization.Changed -= PreferenceChanged;
        window.Changed -= Window_StateChanged;
        navigationPanel.Changed -= PreferenceChanged;
        navigation.Navigated -= PreferenceChanged;
        motion.Changed -= PreferenceChanged;
        scroll.Changed -= PreferenceChanged;
        font.Changed -= PreferenceChanged;
        contentLayout.Changed -= PreferenceChanged;
        material.Changed -= PreferenceChanged;
        appearance.Changed -= PreferenceChanged;
        tray.Changed -= PreferenceChanged;
        profile.ProfileChanged -= PreferenceChanged;
    }

    private void PreferenceChanged(object? sender, EventArgs args) => changes.Writer.TryWrite(false);

    private void Window_StateChanged(object? sender, FlourishWindowStateChangedEventArgs args)
    {
        if (args.State.WindowState is WindowState.Normal or WindowState.Maximized)
        {
            lastRestorableWindowState = args.State.WindowState;
        }

        changes.Writer.TryWrite(false);
    }

    private async Task ProcessChangesAsync()
    {
        await foreach (var isFinal in changes.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            var final = isFinal;
            if (!final)
            {
                await Task.Delay(CoalescingDelay).ConfigureAwait(false);
                while (changes.Reader.TryRead(out var next))
                {
                    final |= next;
                }
            }

            try
            {
                await PersistCurrentAsync().ConfigureAwait(false);
            }
            catch (Exception error)
            {
                logger.LogError(error, "Failed to persist Flourish runtime preferences.");
            }
        }
    }

    private ValueTask<FlourishSettingsUpdateResult> PersistCurrentAsync()
    {
        var panel = navigationPanel.Current;
        var currentMotion = motion.Current;
        var currentScroll = scroll.GetCurrent();
        var currentLayout = contentLayout.Current;
        var currentAppearance = appearance.Current;
        var currentMaterial = material.CurrentEffect;
        var currentTray = tray.Current;
        var currentNavigationKey = navigation.CurrentNavigationKey;
        var currentNameOrder = profile.NameOrder;

        return appSettings.UpdateAsync(editor =>
        {
            if (dataOptions.UsePersistedLocale)
            {
                editor.Set(FlourishPreferenceKeys.Locale, localization.CurrentLocale);
            }

            if (shellOptions.UsePersistedWindowSize)
            {
                editor.Set(
                    FlourishPreferenceKeys.WindowSize,
                    new
                    {
                        Width = shellOptions.WindowWidth,
                        Height = shellOptions.WindowHeight,
                    }
                );
            }

            if (
                shellOptions.UsePersistedWindowPosition
                && shellOptions.WindowLeft is { } left
                && shellOptions.WindowTop is { } top
            )
            {
                editor.Set(FlourishPreferenceKeys.WindowPosition, new { Left = left, Top = top });
            }

            if (shellOptions.UsePersistedWindowState)
            {
                editor.Set(FlourishPreferenceKeys.WindowState, lastRestorableWindowState.ToString());
            }

            if (shellOptions.UsePersistedWindowTopmost)
            {
                editor.Set(FlourishPreferenceKeys.WindowTopmost, shellOptions.WindowTopmost);
            }

            if (shellOptions.UsePersistedTrayExit)
            {
                editor.Set(
                    FlourishPreferenceKeys.WindowCloseBehavior,
                    (
                        currentTray.IsEnabled
                            ? WindowCloseBehavior.MinimizeToTray
                            : WindowCloseBehavior.Prompt
                    ).ToString()
                );
            }

            if (shellOptions.UsePersistedNavigationDirection)
            {
                editor.Set(
                    $"{FlourishPreferenceKeys.Navigation}:Direction",
                    panel.Direction.ToString()
                );
            }

            if (shellOptions.UsePersistedNavigationOpenState)
            {
                editor.Set($"{FlourishPreferenceKeys.Navigation}:IsOpen", panel.IsOpen);
            }

            if (shellOptions.UsePersistedNavigationWidth)
            {
                editor.Set($"{FlourishPreferenceKeys.Navigation}:OpenWidth", panel.OpenWidth);
            }

            if (
                shellOptions.UsePersistedLastNavigation
                && !string.IsNullOrWhiteSpace(currentNavigationKey)
            )
            {
                editor.Set(
                    $"{FlourishPreferenceKeys.Navigation}:LastKey",
                    currentNavigationKey
                );
            }

            if (shellOptions.UsePersistedMotion)
            {
                editor.Set($"{FlourishPreferenceKeys.Motion}:Enabled", currentMotion.IsEnabled);
            }

            if (shellOptions.Motion.UsePersistedPageTransition)
            {
                editor.Set(
                    $"{FlourishPreferenceKeys.Motion}:PageTransition",
                    new
                    {
                        Transition = currentMotion.PageTransition.ToString(),
                        DurationMilliseconds = currentMotion.PageTransitionDuration.TotalMilliseconds,
                    }
                );
            }

            if (shellOptions.Motion.UsePersistedNavigationPanelTransition)
            {
                editor.Set(
                    $"{FlourishPreferenceKeys.Motion}:NavigationPanelTransition",
                    new
                    {
                        Transition = currentMotion.NavigationPanelTransition.ToString(),
                        DurationMilliseconds =
                            currentMotion.NavigationPanelTransitionDuration.TotalMilliseconds,
                    }
                );
            }

            if (shellOptions.Motion.UsePersistedHoverReveal)
            {
                editor.Set(
                    $"{FlourishPreferenceKeys.Motion}:HoverReveal",
                    new
                    {
                        Enabled = currentMotion.IsHoverRevealEnabled,
                        DurationMilliseconds =
                            currentMotion.HoverRevealAnimationDuration.TotalMilliseconds,
                    }
                );
            }

            if (shellOptions.Motion.UsePersistedReducedMotion)
            {
                editor.Set(
                    $"{FlourishPreferenceKeys.Motion}:RespectSystemReducedMotion",
                    currentMotion.RespectSystemReducedMotion
                );
            }

            if (shellOptions.UsePersistedSmoothScroll)
            {
                editor.Set(
                    FlourishPreferenceKeys.SmoothScrolling,
                    currentScroll.IsSmoothScrollingEnabled
                );
            }

            if (shellOptions.UsePersistedFont)
            {
                editor.Set(
                    FlourishPreferenceKeys.Font,
                    new
                    {
                        Family = font.FontFamily,
                        IconFamily = font.IconFontFamily,
                        Small = font.SmallFontSize,
                        Standard = font.StandardFontSize,
                        Icon = font.IconFontSize,
                        Large = font.LargeFontSize,
                        ExtraLarge = font.ExtraLargeFontSize,
                        Header = font.HeaderSizeFontSize,
                    }
                );
            }

            if (shellOptions.UsePersistedContentLayout)
            {
                editor.Set(
                    FlourishPreferenceKeys.ContentLayout,
                    new
                    {
                        Enabled = currentLayout.IsCenterContentEnabled,
                        Width = currentLayout.ContentWidth,
                    }
                );
            }

            if (shellOptions.UsePersistedMaterialEffect)
            {
                editor.Set(
                    FlourishPreferenceKeys.Material,
                    new
                    {
                        Enabled = currentMaterial != MaterialEffect.None,
                        Effect = currentMaterial.ToString(),
                    }
                );
            }

            if (shellOptions.UsePersistedThemeColors)
            {
                editor.Set(
                    FlourishPreferenceKeys.ThemeColors,
                    new
                    {
                        Enabled = currentAppearance.ThemeColors is not null,
                        Primary = FormatColor(currentAppearance.ThemeColors?.Primary),
                        Secondary = FormatColor(currentAppearance.ThemeColors?.Secondary),
                        Accent = FormatColor(currentAppearance.ThemeColors?.Accent),
                    }
                );
            }

            if (shellOptions.UsePersistedCornerRadius)
            {
                editor.Set(
                    FlourishPreferenceKeys.CornerRadius,
                    new
                    {
                        Enabled = currentAppearance.CornerRadius is not null,
                        Value = currentAppearance.CornerRadius ?? 0,
                    }
                );
            }

            if (shellOptions.UsePersistedNameOrder)
            {
                editor.Set(FlourishPreferenceKeys.NameOrder, currentNameOrder.ToString());
            }
        });
    }

    private static string? FormatColor(System.Windows.Media.Color? color) =>
        color is { } value
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}"
            )
            : null;
}
