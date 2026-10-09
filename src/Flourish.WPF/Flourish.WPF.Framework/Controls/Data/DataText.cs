using System.Globalization;
using System.Windows;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

internal sealed class DataText
{
    private readonly FrameworkElement owner;
    private readonly Action refresh;
    private ITextProvider? provider;
    private bool listening;
    internal DataText(FrameworkElement owner, Action refresh)
    {
        this.owner = owner; this.refresh = refresh;
        owner.Loaded += (_, _) => { Subscribe(); refresh(); };
        owner.Unloaded += (_, _) => Unsubscribe();
    }
    internal ITextProvider? Provider
    {
        get => provider;
        set { Unsubscribe(); provider = value; Subscribe(); refresh(); }
    }
    internal CultureInfo Culture => provider?.FormatCulture ?? CultureInfo.CurrentCulture;
    internal string Get(string key, string fallback, params object?[] arguments)
        => provider?.Get(new TextReference("Flourish", key, fallback), arguments)
            ?? string.Format(Culture, fallback, arguments);
    private void Subscribe() { if (owner.IsLoaded && provider is not null && !listening) { provider.Changed += Changed; listening = true; } }
    private void Unsubscribe() { if (provider is not null && listening) provider.Changed -= Changed; listening = false; }
    private void Changed(object? sender, EventArgs args)
    {
        if (owner.Dispatcher.CheckAccess()) { if (owner.IsLoaded) refresh(); }
        else if (!owner.Dispatcher.HasShutdownStarted) owner.Dispatcher.BeginInvoke(new Action(() => { if (owner.IsLoaded) refresh(); }));
    }
}
