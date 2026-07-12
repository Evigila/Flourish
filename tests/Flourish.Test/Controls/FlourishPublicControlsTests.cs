using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Themes;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishPublicControlsTests
{
    private const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";
    private const string XamlNamespace = "http://schemas.arkheide.system/flourish";

    [Fact]
    public void CanonicalThemeAndControlContracts_ArePublic()
    {
        Type[] publicContractTypes =
        [
            typeof(FlourishThemeResources),
            typeof(FlourishButtonAppearance),
            typeof(FlourishButtonVariant),
            typeof(FlourishCardAppearance),
            typeof(FlourishGridSplitterVariant),
            typeof(FlourishListBoxAppearance),
            typeof(FlourishTextRole),
            typeof(HoverReveal),
            typeof(FlourishToolTipPlacement),
            .. GetPublicFlourishControlTypes(),
        ];

        Assert.All(publicContractTypes, type => Assert.True(type.IsPublic, type.FullName));
        Assert.All(
            new[]
            {
                nameof(HoverReveal.GetIsEnabled),
                nameof(HoverReveal.SetIsEnabled),
                nameof(HoverReveal.GetIsParticipant),
                nameof(HoverReveal.SetIsParticipant),
                nameof(HoverReveal.GetTemplateHandlesInteraction),
                nameof(HoverReveal.SetTemplateHandlesInteraction),
                nameof(HoverReveal.GetAnimationDuration),
                nameof(HoverReveal.SetAnimationDuration),
            },
            methodName => AssertPublicStaticMethod(typeof(HoverReveal), methodName)
        );
        Assert.All(
            new[]
            {
                nameof(FlourishToolTipPlacement.GetIsEnabled),
                nameof(FlourishToolTipPlacement.SetIsEnabled),
            },
            methodName =>
                AssertPublicStaticMethod(typeof(FlourishToolTipPlacement), methodName)
        );
    }

    [Fact]
    public void Assembly_MapsStableXamlNamespaceToCanonicalPublicApiSurfaces()
    {
        var assembly = typeof(FlourishButton).Assembly;
        var definitions = assembly
            .GetCustomAttributes<XmlnsDefinitionAttribute>()
            .Where(definition => definition.XmlNamespace == XamlNamespace)
            .Select(definition => definition.ClrNamespace)
            .ToHashSet(StringComparer.Ordinal);
        var prefixes = assembly.GetCustomAttributes<XmlnsPrefixAttribute>();

        Assert.Contains("ArkheideSystem.Flourish.Abstract", definitions);
        Assert.Contains("ArkheideSystem.Flourish.Controls", definitions);
        Assert.Contains("ArkheideSystem.Flourish.Themes", definitions);
        Assert.Contains(
            prefixes,
            prefix => prefix.XmlNamespace == XamlNamespace && prefix.Prefix == "flourish"
        );
    }

    [Fact]
    public void CanonicalThemeResources_LoadTheSingleGenericThemeEntryPoint()
    {
        RunInSta(() =>
        {
            _ = Application.LoadComponent(new Uri(GenericThemeSource, UriKind.Relative));
            var resources = new FlourishThemeResources();

            Assert.Equal(GenericThemeSource, resources.Source.OriginalString);
        });
    }

    [Fact]
    public void LegacyResourceTypes_RemainLoadableButAreMarkedObsolete()
    {
        RunInSta(() =>
        {
            _ = Application.LoadComponent(new Uri(GenericThemeSource, UriKind.Relative));

#pragma warning disable CS0618
            Type[] compatibilityTypes =
            [
                typeof(ArkheideSystem.Flourish.Styles.FlourishStyles),
                typeof(FlourishControlResources),
            ];
            ResourceDictionary[] compatibilityResources =
            [
                new ArkheideSystem.Flourish.Styles.FlourishStyles(),
                new FlourishControlResources(),
            ];
#pragma warning restore CS0618

            Assert.All(
                compatibilityTypes,
                type =>
                {
                    Assert.NotNull(type.GetCustomAttribute<ObsoleteAttribute>());
                    Assert.Equal(
                        EditorBrowsableState.Never,
                        type.GetCustomAttribute<EditorBrowsableAttribute>()?.State
                    );
                }
            );
            Assert.All(
                compatibilityResources,
                resources => Assert.Equal(GenericThemeSource, resources.Source.OriginalString)
            );
        });
    }

    [Fact]
    public void CanonicalThemeResources_EnsureMergedIsIdempotent()
    {
        RunInSta(() =>
        {
            var resources = new ResourceDictionary();

            FlourishThemeResources.EnsureMerged(resources);
            FlourishThemeResources.EnsureMerged(resources);

            var merged = Assert.Single(resources.MergedDictionaries);
            Assert.IsType<FlourishThemeResources>(merged);
        });
    }

    [Fact]
    public void PublicVisualControls_UseTheirOwnDefaultStyleKeys()
    {
        RunInSta(() =>
        {
            foreach (var controlType in GetPublicFlourishControlTypes())
            {
                var control = Assert.IsAssignableFrom<FrameworkElement>(
                    Activator.CreateInstance(controlType)
                );
                Assert.Equal(controlType, GetDefaultStyleKey(control));
            }
        });
    }

    [Fact]
    public void SemanticControlProperties_ExposeStableDefaults()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton();
            var card = new FlourishCard();
            var gridSplitter = new FlourishGridSplitter();
            var listBox = new FlourishListBox();
            var scrollViewer = new FlourishScrollViewer();
            var search = new FlourishSearchBox();
            var text = new FlourishTextBlock();

            Assert.Equal(FlourishButtonAppearance.Standard, button.Appearance);
            Assert.Equal(FlourishButtonVariant.Standard, button.Variant);
            Assert.Equal(FlourishCardAppearance.Standard, card.Appearance);
            Assert.Equal(FlourishGridSplitterVariant.Standard, gridSplitter.Variant);
            Assert.Equal(FlourishListBoxAppearance.Standard, listBox.Appearance);
            Assert.False(listBox.IsCompact);
            Assert.False(scrollViewer.IsCompact);
            Assert.Equal(string.Empty, search.Placeholder);
            Assert.Equal(FlourishTextRole.Body, text.Role);
        });
    }

    [Fact]
    public void SemanticEnumProperties_RejectUndefinedValues()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton();

            Assert.Throws<ArgumentException>(() =>
                button.Appearance = (FlourishButtonAppearance)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                button.Variant = (FlourishButtonVariant)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new FlourishCard().Appearance = (FlourishCardAppearance)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new FlourishGridSplitter().Variant = (FlourishGridSplitterVariant)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new FlourishListBox().Appearance = (FlourishListBoxAppearance)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new FlourishTextBlock().Role = (FlourishTextRole)(-1)
            );
        });
    }

    [Fact]
    public void HoverReveal_AttachedPropertiesExposeStableDefaultsAndRoundTrip()
    {
        RunInSta(() =>
        {
            var element = new Border();
            var duration = TimeSpan.FromMilliseconds(215);

            Assert.True(HoverReveal.GetIsEnabled(element));
            Assert.False(HoverReveal.GetIsParticipant(element));
            Assert.False(HoverReveal.GetTemplateHandlesInteraction(element));
            Assert.Equal(
                TimeSpan.FromMilliseconds(140),
                HoverReveal.GetAnimationDuration(element)
            );

            HoverReveal.SetIsEnabled(element, false);
            HoverReveal.SetIsParticipant(element, true);
            HoverReveal.SetTemplateHandlesInteraction(element, true);
            HoverReveal.SetAnimationDuration(element, duration);

            Assert.False(HoverReveal.GetIsEnabled(element));
            Assert.True(HoverReveal.GetIsParticipant(element));
            Assert.True(HoverReveal.GetTemplateHandlesInteraction(element));
            Assert.Equal(duration, HoverReveal.GetAnimationDuration(element));
        });
    }

    [Fact]
    public void HoverReveal_PolicyInheritsWithoutOptingDescendantsIntoTheBehavior()
    {
        RunInSta(() =>
        {
            var parent = new Grid();
            var child = new Button();
            parent.Children.Add(child);

            HoverReveal.SetIsEnabled(parent, false);
            HoverReveal.SetAnimationDuration(parent, TimeSpan.FromMilliseconds(90));
            HoverReveal.SetIsParticipant(parent, true);

            Assert.False(HoverReveal.GetIsEnabled(child));
            Assert.Equal(
                TimeSpan.FromMilliseconds(90),
                HoverReveal.GetAnimationDuration(child)
            );
            Assert.False(HoverReveal.GetIsParticipant(child));
        });
    }

    private static Type[] GetPublicFlourishControlTypes()
    {
        return typeof(FlourishButton)
            .Assembly.GetExportedTypes()
            .Where(type =>
                type.Namespace == "ArkheideSystem.Flourish.Controls"
                && type.Name.StartsWith("Flourish", StringComparison.Ordinal)
                && typeof(FrameworkElement).IsAssignableFrom(type)
                && !type.IsAbstract
            )
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AssertPublicStaticMethod(Type declaringType, string methodName)
    {
        Assert.True(
            declaringType
                .GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                ?.IsPublic,
            methodName
        );
    }

    private static object? GetDefaultStyleKey(FrameworkElement element)
    {
        var property = typeof(FrameworkElement).GetProperty(
            "DefaultStyleKey",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        return Assert.IsAssignableFrom<PropertyInfo>(property).GetValue(element);
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
}
