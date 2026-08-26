using System;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using System.Globalization;
using System.Threading.Channels;
using System.Windows;
using ArkheideSystem.Flourish.Appearance;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Layout;
using ArkheideSystem.Flourish.Motion;
using ArkheideSystem.Flourish.Navigation;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Windowing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArkheideSystem.Flourish.Hosting;

internal sealed class FlourishPreferencePersistenceService(
    FlourishDataOptions dataOptions,
    FlourishWindowOptions windowOptions,
    FlourishNavigationOptions navigationOptions,
    FlourishMotionOptions motionOptions,
    FlourishLayoutOptions layoutOptions,
    FlourishAppearanceOptions appearanceOptions,
    FlourishProfileOptions profileOptions,
    IFlourishSettingsStore appSettings,
    IFlourishLocalization localization,
    IWindowService window,
    NavigationPanelService navigationPanel,
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
        windowOptions.WindowState == WindowState.Maximized
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
        profile.Changed += PreferenceChanged;
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
        profile.Changed -= PreferenceChanged;
    }

    private void PreferenceChanged(object? sender, EventArgs args) => changes.Writer.TryWrite(false);

    private void Window_StateChanged(object? sender, FlourishStateChangedEventArgs<FlourishWindowState> args)
    {
        if (args.Current.WindowState is WindowState.Normal or WindowState.Maximized)
        {
            lastRestorableWindowState = args.Current.WindowState;
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
        var currentScroll = scroll.Current;
        var currentLayout = contentLayout.Current;
        var currentAppearance = appearance.Current;
        var currentMaterial = material.Current.RequestedEffect;
        var currentTray = tray.Current;
        var currentNavigationKey = navigation.Current.NavigationKey;
        var currentNameOrder = profile.Current.NameOrder;

        return appSettings.UpdateAsync(editor =>
        {
            if (dataOptions.UsePersistedLocale)
            {
                editor.Set(FlourishPreferenceKeys.Locale, localization.Current.Locale);
            }

            if (windowOptions.UsePersistedWindowSize)
            {
                editor.Set(
                    FlourishPreferenceKeys.WindowSize,
                    new
                    {
                        Width = windowOptions.WindowWidth,
                        Height = windowOptions.WindowHeight,
                    }
                );
            }

            if (
                windowOptions.UsePersistedWindowPosition
                && windowOptions.WindowLeft is { } left
                && windowOptions.WindowTop is { } top
            )
            {
                editor.Set(FlourishPreferenceKeys.WindowPosition, new { Left = left, Top = top });
            }

            if (windowOptions.UsePersistedWindowState)
            {
                editor.Set(FlourishPreferenceKeys.WindowState, lastRestorableWindowState.ToString());
            }

            if (windowOptions.UsePersistedWindowTopmost)
            {
                editor.Set(FlourishPreferenceKeys.WindowTopmost, windowOptions.WindowTopmost);
            }

            if (windowOptions.UsePersistedTrayExit)
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

            if (navigationOptions.UsePersistedNavigationDirection)
            {
                editor.Set(
                    $"{FlourishPreferenceKeys.Navigation}:Direction",
                    panel.Direction.ToString()
                );
            }

            if (navigationOptions.UsePersistedNavigationOpenState)
            {
                editor.Set($"{FlourishPreferenceKeys.Navigation}:IsOpen", panel.IsOpen);
            }

            if (navigationOptions.UsePersistedNavigationWidth)
            {
                editor.Set($"{FlourishPreferenceKeys.Navigation}:OpenWidth", panel.OpenWidth);
            }

            if (
                navigationOptions.UsePersistedLastNavigation
                && !string.IsNullOrWhiteSpace(currentNavigationKey)
            )
            {
                editor.Set(
                    $"{FlourishPreferenceKeys.Navigation}:LastKey",
                    currentNavigationKey
                );
            }

            if (motionOptions.UsePersistedMotion)
            {
                editor.Set($"{FlourishPreferenceKeys.Motion}:Enabled", currentMotion.IsEnabled);
            }

            if (motionOptions.UsePersistedPageTransition)
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

            if (motionOptions.UsePersistedNavigationPanelTransition)
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

            if (motionOptions.UsePersistedHoverReveal)
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

            if (motionOptions.UsePersistedReducedMotion)
            {
                editor.Set(
                    $"{FlourishPreferenceKeys.Motion}:RespectSystemReducedMotion",
                    currentMotion.RespectSystemReducedMotion
                );
            }

            if (layoutOptions.UsePersistedSmoothScroll)
            {
                editor.Set(
                    FlourishPreferenceKeys.SmoothScrolling,
                    currentScroll.IsSmoothScrollingEnabled
                );
            }

            if (appearanceOptions.UsePersistedFont)
            {
                var currentFont = font.Current;
                editor.Set(
                    FlourishPreferenceKeys.Font,
                    new
                    {
                        Family = currentFont.FontFamily,
                        IconFamily = currentFont.IconFontFamily,
                        Small = currentFont.SmallFontSize,
                        Standard = currentFont.StandardFontSize,
                        Icon = currentFont.IconFontSize,
                        Large = currentFont.LargeFontSize,
                        ExtraLarge = currentFont.ExtraLargeFontSize,
                        Header = currentFont.HeaderSizeFontSize,
                    }
                );
            }

            if (layoutOptions.UsePersistedContentLayout)
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

            if (appearanceOptions.UsePersistedMaterialEffect)
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

            if (appearanceOptions.UsePersistedThemeColors)
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

            if (appearanceOptions.UsePersistedCornerRadius)
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

            if (profileOptions.UsePersistedNameOrder)
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
