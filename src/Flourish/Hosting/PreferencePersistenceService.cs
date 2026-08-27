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

internal sealed class PreferencePersistenceService(
    ApplicationDataOptions dataOptions,
    WindowOptions windowOptions,
    NavigationOptions navigationOptions,
    MotionOptions motionOptions,
    LayoutOptions layoutOptions,
    AppearanceOptions appearanceOptions,
    ProfileOptions profileOptions,
    ISettingsStore appSettings,
    ILocalizationService localization,
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
    ILogger<PreferencePersistenceService> logger
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

    private void Window_StateChanged(object? sender, StateChangedEventArgs<WindowStateSnapshot> args)
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

    private ValueTask<SettingsUpdateResult> PersistCurrentAsync()
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
                editor.Set(PreferenceConfigurationKeys.Locale, localization.Current.Locale);
            }

            if (windowOptions.UsePersistedWindowSize)
            {
                editor.Set(
                    PreferenceConfigurationKeys.WindowSize,
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
                editor.Set(PreferenceConfigurationKeys.WindowPosition, new { Left = left, Top = top });
            }

            if (windowOptions.UsePersistedWindowState)
            {
                editor.Set(PreferenceConfigurationKeys.WindowState, lastRestorableWindowState.ToString());
            }

            if (windowOptions.UsePersistedWindowTopmost)
            {
                editor.Set(PreferenceConfigurationKeys.WindowTopmost, windowOptions.WindowTopmost);
            }

            if (windowOptions.UsePersistedTrayExit)
            {
                editor.Set(
                    PreferenceConfigurationKeys.WindowCloseBehavior,
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
                    $"{PreferenceConfigurationKeys.Navigation}:Direction",
                    panel.Direction.ToString()
                );
            }

            if (navigationOptions.UsePersistedNavigationOpenState)
            {
                editor.Set($"{PreferenceConfigurationKeys.Navigation}:IsOpen", panel.IsOpen);
            }

            if (navigationOptions.UsePersistedNavigationWidth)
            {
                editor.Set($"{PreferenceConfigurationKeys.Navigation}:OpenWidth", panel.OpenWidth);
            }

            if (
                navigationOptions.UsePersistedLastNavigation
                && !string.IsNullOrWhiteSpace(currentNavigationKey)
            )
            {
                editor.Set(
                    $"{PreferenceConfigurationKeys.Navigation}:LastKey",
                    currentNavigationKey
                );
            }

            if (motionOptions.UsePersistedMotion)
            {
                editor.Set($"{PreferenceConfigurationKeys.Motion}:Enabled", currentMotion.IsEnabled);
            }

            if (motionOptions.UsePersistedPageTransition)
            {
                editor.Set(
                    $"{PreferenceConfigurationKeys.Motion}:PageTransition",
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
                    $"{PreferenceConfigurationKeys.Motion}:NavigationPanelTransition",
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
                    $"{PreferenceConfigurationKeys.Motion}:HoverReveal",
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
                    $"{PreferenceConfigurationKeys.Motion}:RespectSystemReducedMotion",
                    currentMotion.RespectSystemReducedMotion
                );
            }

            if (layoutOptions.UsePersistedSmoothScroll)
            {
                editor.Set(
                    PreferenceConfigurationKeys.SmoothScrolling,
                    currentScroll.IsSmoothScrollingEnabled
                );
            }

            if (appearanceOptions.UsePersistedFont)
            {
                var currentFont = font.Current;
                editor.Set(
                    PreferenceConfigurationKeys.Font,
                    new
                    {
                        Family = currentFont.FontFamily,
                        IconFamily = currentFont.IconFontFamily,
                        Small = currentFont.SmallFontSize,
                        Standard = currentFont.StandardFontSize,
                        StandardIcon = currentFont.IconFontSize,
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
                    PreferenceConfigurationKeys.ContentLayout,
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
                    PreferenceConfigurationKeys.Material,
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
                    PreferenceConfigurationKeys.ThemeColors,
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
                    PreferenceConfigurationKeys.CornerRadius,
                    new
                    {
                        Enabled = currentAppearance.CornerRadius is not null,
                        Value = currentAppearance.CornerRadius ?? 0,
                    }
                );
            }

            if (profileOptions.UsePersistedNameOrder)
            {
                editor.Set(PreferenceConfigurationKeys.NameOrder, currentNameOrder.ToString());
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
