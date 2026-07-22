using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishHoverRevealContractTests
{
    private const string XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";
    private const string HoverRevealBrush =
        "{DynamicResource FlourishHoverRevealBrush}";
    private const string HoverRevealBrushBinding =
        "{Binding Path=(controls:HoverReveal.OverrideColor), RelativeSource={RelativeSource TemplatedParent}}";
    private const string HoverRevealBrushTemplateBinding =
        "{TemplateBinding controls:HoverReveal.OverrideColor}";
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string FlourishRoot = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish"
    );
    private static readonly HashSet<string> ApprovedBrandRamp = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "#061724",
        "#082338",
        "#0A2E4A",
        "#0C3B5E",
        "#0E4775",
        "#0F548C",
        "#115EA3",
        "#0F6CBD",
        "#2886DE",
        "#479EF5",
        "#62ABF5",
        "#77B7F7",
        "#96C6FA",
        "#B4D6FA",
        "#CFE4FA",
        "#EBF3FC",
    };

    [Fact]
    public void ParticipatingControlTemplates_UseOneBorderlessUnifiedRevealLayer()
    {
        var templates = FindParticipatingTemplates();

        Assert.Equal(
            new[] { "Button.xaml", "ComboBoxItem.xaml", "ListBoxItem.xaml" },
            templates.Select(template => Path.GetFileName(template.File)).Order()
        );

        var violations = new List<string>();
        foreach (var template in templates)
        {
            var hoverChrome = FindNamedDescendants(template.Template, "HoverChrome");
            var revealScale = FindNamedDescendants(template.Template, "HoverRevealScale");

            if (hoverChrome.Length != 1)
            {
                violations.Add(
                    $"{template.Identifier}: expected one HoverChrome, found {hoverChrome.Length}"
                );
                continue;
            }

            if (revealScale.Length != 1)
            {
                violations.Add(
                    $"{template.Identifier}: expected one HoverRevealScale, found {revealScale.Length}"
                );
                continue;
            }

            AssertAttribute(
                template,
                hoverChrome[0],
                "Background",
                Path.GetFileName(template.File) == "Button.xaml"
                    ? HoverRevealBrushTemplateBinding
                    : HoverRevealBrushBinding,
                violations
            );
            var overrideColorSetter = template.Style
                .Elements()
                .SingleOrDefault(element =>
                    element.Name.LocalName == "Setter"
                    && (string?)element.Attribute("Property")
                        == "controls:HoverReveal.OverrideColor"
                );
            if ((string?)overrideColorSetter?.Attribute("Value") != HoverRevealBrush)
            {
                violations.Add(
                    $"{template.Identifier}: reveal override color is not bound to {HoverRevealBrush}"
                );
            }
            AssertAttribute(template, hoverChrome[0], "BorderThickness", "0", violations);
            AssertAttribute(template, hoverChrome[0], "Opacity", "0", violations);
            AssertAttribute(template, revealScale[0], "ScaleX", "0", violations);
            AssertAttribute(template, revealScale[0], "ScaleY", "0", violations);
        }

        AssertNoViolations(violations);
    }

    [Fact]
    public void MouseOverFallbacks_OnlyRevealWhenMotionIsDisabled()
    {
        var violations = new List<string>();

        foreach (var template in FindParticipatingTemplates())
        {
            var hoverFallbacks = template.Template
                .Descendants()
                .Where(element => element.Name.LocalName is "Trigger" or "MultiTrigger")
                .Where(trigger =>
                    trigger
                        .DescendantsAndSelf()
                        .Any(element =>
                            element.Name.LocalName == "Setter"
                            && (string?)element.Attribute("TargetName") == "HoverChrome"
                            && (string?)element.Attribute("Property") == "Opacity"
                            && (string?)element.Attribute("Value") == "1"
                        )
                )
                .ToArray();

            if (hoverFallbacks.Length != 1)
            {
                violations.Add(
                    $"{template.Identifier}: expected one mouse-over fallback, found {hoverFallbacks.Length}"
                );
                continue;
            }

            var conditions = GetConditions(hoverFallbacks[0]);
            if (!conditions.Contains(("IsMouseOver", "True")))
            {
                violations.Add($"{template.Identifier}: fallback does not require IsMouseOver=True");
            }

            if (!conditions.Contains(("controls:HoverReveal.IsEnabled", "False")))
            {
                violations.Add(
                    $"{template.Identifier}: fallback does not require HoverReveal.IsEnabled=False"
                );
            }
        }

        AssertNoViolations(violations);
    }

    [Fact]
    public void ParticipatingTemplates_DeclareThatTheyOwnStaticInteractionStates()
    {
        var violations = new List<string>();
        foreach (var template in FindParticipatingTemplates())
        {
            var setter = template.Style
                .Elements()
                .SingleOrDefault(element =>
                    element.Name.LocalName == "Setter"
                    && (string?)element.Attribute("Property")
                        == "controls:HoverReveal.TemplateHandlesInteraction"
                );
            if ((string?)setter?.Attribute("Value") != "True")
            {
                violations.Add(
                    $"{template.Identifier}: template interaction ownership is not enabled"
                );
            }
        }

        AssertNoViolations(violations);
    }

    [Fact]
    public void ParticipatingStyles_ConsumeTheGlobalMotionPolicyThroughDynamicResources()
    {
        const string expectedPolicy =
            "{DynamicResource FlourishHoverRevealEnabled}";
        var violations = new List<string>();

        foreach (var template in FindParticipatingTemplates())
        {
            var setter = template.Style
                .Elements()
                .SingleOrDefault(element =>
                    element.Name.LocalName == "Setter"
                    && (string?)element.Attribute("Property")
                        == "controls:HoverReveal.IsMotionEnabled"
                );
            if ((string?)setter?.Attribute("Value") != expectedPolicy)
            {
                violations.Add(
                    $"{template.Identifier}: motion policy is not bound to {expectedPolicy}"
                );
            }
        }

        AssertNoViolations(violations);
    }

    [Fact]
    public void ButtonPressedState_UsesASeparateDarkerFillWithoutAnOutline()
    {
        var file = Path.Combine(FlourishRoot, "Controls", "Button.xaml");
        var document = LoadXaml(file);
        var template = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "ControlTemplate"
                && (string?)element.Attribute(XName.Get("Key", XamlNamespace))
                    == "ButtonTemplate"
            );
        var pressedChrome = Assert.Single(
            FindNamedDescendants(template, "PressedChrome")
        );

        Assert.Equal(
            "{DynamicResource FlourishPressedRevealBrush}",
            (string?)pressedChrome.Attribute("Background")
        );
        Assert.Equal("0", (string?)pressedChrome.Attribute("BorderThickness"));
        Assert.Equal("0", (string?)pressedChrome.Attribute("Opacity"));

        var pressedTrigger = FindTrigger(template, "IsPressed", "True");
        AssertSetter(pressedTrigger, "HoverChrome", "Opacity", "0");
        AssertSetter(pressedTrigger, "PressedChrome", "Opacity", "1");
        Assert.DoesNotContain(
            pressedTrigger.Descendants(),
            element =>
                element.Name.LocalName == "Setter"
                && (string?)element.Attribute("TargetName") == "InteractionRoot"
                && (string?)element.Attribute("Property") == "RenderTransform"
        );

        var disabledTrigger = FindTrigger(template, "IsEnabled", "False");
        AssertSetter(disabledTrigger, "HoverChrome", "Visibility", "Collapsed");
        AssertSetter(disabledTrigger, "PressedChrome", "Visibility", "Collapsed");

        foreach (
            var (fileName, templateKey) in new[]
            {
                ("CardButton.xaml", "CardButtonTemplate"),
                ("WindowCaptionButton.xaml", "WindowCaptionButtonTemplate"),
            }
        )
        {
            var familyDocument = LoadXaml(
                Path.Combine(FlourishRoot, "Controls", fileName)
            );
            var familyTemplate = familyDocument
                .Descendants()
                .Single(element =>
                    element.Name.LocalName == "ControlTemplate"
                    && (string?)element.Attribute(XName.Get("Key", XamlNamespace))
                        == templateKey
                );
            var familyPressedTrigger = FindTrigger(
                familyTemplate,
                "IsPressed",
                "True"
            );

            Assert.DoesNotContain(
                familyPressedTrigger.Descendants(),
                element =>
                    element.Name.LocalName == "Setter"
                    && (string?)element.Attribute("TargetName")
                        == "InteractionRoot"
                    && (string?)element.Attribute("Property")
                        == "RenderTransform"
            );
        }
    }

    [Fact]
    public void ButtonVariants_ShareRevealAndKeepCaptionDangerSpecialized()
    {
        var document = LoadXaml(
            Path.Combine(FlourishRoot, "Controls", "Button.xaml")
        );
        var template = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "ControlTemplate"
                && (string?)element.Attribute(XName.Get("Key", XamlNamespace))
                    == "ButtonTemplate"
            );

        var dangerTrigger = FindTrigger(template, "Variant", "Danger");
        AssertSetter(
            dangerTrigger,
            "PressedChrome",
            "Background",
            "{DynamicResource FlourishDangerStrokeBrush}"
        );
        Assert.DoesNotContain(
            document.Descendants().Attributes("Value"),
            attribute => attribute.Value.Contains(
                "FlourishWindowCaptionClose",
                StringComparison.Ordinal
            )
        );

        var implicitStyle = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Style"
                && element.Attribute(XName.Get("Key", XamlNamespace)) is null
                && (string?)element.Attribute("TargetType")
                    == "{x:Type controls:Button}"
            );
        var dangerStyleTrigger = FindTrigger(implicitStyle, "Variant", "Danger");
        AssertSetter(
            dangerStyleTrigger,
            null,
            "Background",
            "{DynamicResource FlourishDangerBackgroundBrush}"
        );
        AssertSetter(
            dangerStyleTrigger,
            null,
            "Foreground",
            "{DynamicResource FlourishDangerForegroundBrush}"
        );
        AssertSetter(
            dangerStyleTrigger,
            null,
            "controls:HoverReveal.OverrideColor",
            "{DynamicResource FlourishDangerHoverRevealBrush}"
        );

        var captionDocument = LoadXaml(
            Path.Combine(FlourishRoot, "Controls", "WindowCaptionButton.xaml")
        );
        var captionTemplate = captionDocument
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "ControlTemplate"
                && (string?)element.Attribute(XName.Get("Key", XamlNamespace))
                    == "WindowCaptionButtonTemplate"
            );
        var dangerHoverTrigger = captionTemplate
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "MultiTrigger"
                && HasCondition(element, "Variant", "Danger")
                && HasCondition(element, "IsMouseOver", "True")
            );
        AssertSetter(
            dangerHoverTrigger,
            null,
            "Foreground",
            "{DynamicResource FlourishWindowCaptionCloseForegroundBrush}"
        );

        var captionStyle = captionDocument
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Style"
                && element.Attribute(XName.Get("Key", XamlNamespace)) is null
                && (string?)element.Attribute("TargetType")
                    == "{x:Type controls:WindowCaptionButton}"
            );
        var captionDangerTrigger = FindTrigger(captionStyle, "Variant", "Danger");
        AssertSetter(
            captionDangerTrigger,
            null,
            "controls:HoverReveal.OverrideColor",
            "{DynamicResource FlourishWindowCaptionCloseHoverBrush}"
        );
        Assert.DoesNotContain(
            captionDangerTrigger.Descendants(),
            element =>
                element.Name.LocalName == "Setter"
                && (string?)element.Attribute("Property")
                    == "controls:HoverReveal.IsMotionEnabled"
        );

        var cardDocument = LoadXaml(
            Path.Combine(FlourishRoot, "Controls", "CardButton.xaml")
        );
        foreach (
            var (templateDocument, templateKey) in new[]
            {
                (document, "ButtonTemplate"),
                (captionDocument, "WindowCaptionButtonTemplate"),
                (cardDocument, "CardButtonTemplate"),
            }
        )
        {
            var hoverChrome = Assert.Single(
                FindNamedDescendants(
                    templateDocument
                        .Descendants()
                        .Single(element =>
                            element.Name.LocalName == "ControlTemplate"
                            && (string?)element.Attribute(XName.Get("Key", XamlNamespace))
                                == templateKey
                        ),
                    "HoverChrome"
                )
            );
            Assert.Equal(
                HoverRevealBrushTemplateBinding,
                (string?)hoverChrome.Attribute("Background")
            );
        }
    }

    [Fact]
    public void ButtonFocusVisual_IsKeyboardOnlyAndDoesNotUseTemplateFocusState()
    {
        var document = LoadXaml(
            Path.Combine(FlourishRoot, "Controls", "Button.xaml")
        );
        var implicitStyle = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Style"
                && element.Attribute(XName.Get("Key", XamlNamespace)) is null
                && (string?)element.Attribute("TargetType")
                    == "{x:Type controls:Button}"
            );
        var focusVisualSetter = implicitStyle
            .Elements()
            .Single(element =>
                element.Name.LocalName == "Setter"
                && (string?)element.Attribute("Property") == "FocusVisualStyle"
            );

        Assert.Equal(
            "{StaticResource ButtonFocusVisualStyle}",
            (string?)focusVisualSetter.Attribute("Value")
        );
        Assert.Contains(
            document.Descendants(),
            element =>
                element.Name.LocalName == "Style"
                && (string?)element.Attribute(XName.Get("Key", XamlNamespace))
                    == "ButtonFocusVisualStyle"
        );
        Assert.DoesNotContain(
            document.Descendants(),
            element =>
                (element.Name.LocalName is "Trigger" or "Condition")
                && (string?)element.Attribute("Property") == "IsKeyboardFocused"
        );
    }

    [Fact]
    public void WindowCaptionButtons_ReserveDangerVariantForCloseCommands()
    {
        var titleBar = LoadXaml(
            Path.Combine(FlourishRoot, "Views", "Windows", "TitleBar.xaml")
        );
        var messageBox = LoadXaml(
            Path.Combine(
                FlourishRoot,
                "Views",
                "Windows",
                "FlourishMessageBoxWindow.xaml"
            )
        );

        AssertButtonVariant(titleBar, "MinimizeButton", "Text");
        AssertButtonVariant(titleBar, "MaximizeButton", "Text");
        AssertButtonVariant(titleBar, "CloseButton", "Danger");
        AssertButtonVariant(messageBox, "CloseButton", "Danger");
        AssertCloseButtonHasIcon(titleBar);
        AssertCloseButtonHasIcon(messageBox);
    }

    [Fact]
    public void SelectedItemTemplates_UseTheDedicatedReadableForegroundToken()
    {
        var selectedTriggers = FindParticipatingTemplates()
            .Select(template =>
                template.Template
                    .Descendants()
                    .SingleOrDefault(element =>
                        element.Name.LocalName == "Trigger"
                        && (string?)element.Attribute("Property") == "IsSelected"
                        && (string?)element.Attribute("Value") == "True"
                    )
            )
            .OfType<XElement>()
            .ToArray();

        Assert.Equal(2, selectedTriggers.Length);
        foreach (var trigger in selectedTriggers)
        {
            Assert.Contains(
                trigger.Elements(),
                element =>
                    element.Name.LocalName == "Setter"
                    && (string?)element.Attribute("Property") == "Foreground"
                    && (string?)element.Attribute("Value")
                        == "{DynamicResource FlourishControlSelectedForegroundBrush}"
            );
            Assert.Contains(
                trigger.Elements(),
                element =>
                    element.Name.LocalName == "Setter"
                    && (string?)element.Attribute("Property") == "Background"
                    && (string?)element.Attribute("Value")
                        == "{DynamicResource FlourishControlSelectedBrush}"
            );
        }
    }

    [Theory]
    [InlineData(
        "Colors.Light.xaml",
        "#592886DE",
        "#CFE4FA",
        "#0C3B5E",
        "#660F6CBD",
        "#33D13438"
    )]
    [InlineData(
        "Colors.Dark.xaml",
        "#662886DE",
        "#0F548C",
        "#FFFFFF",
        "#730F6CBD",
        "#33E37D83"
    )]
    public void Palettes_UseBrighterThemeColorsWithADeeperPressedState(
        string fileName,
        string expectedHover,
        string expectedSelected,
        string expectedSelectedForeground,
        string expectedPressed,
        string expectedDangerHover
    )
    {
        var document = LoadXaml(
            Path.Combine(FlourishRoot, "Themes", "Colors", fileName)
        );

        Assert.Equal(expectedHover, GetBrushColor(document, "FlourishHoverRevealBrush"));
        Assert.Equal(
            expectedSelected,
            GetBrushColor(document, "FlourishControlSelectedBrush")
        );
        Assert.Equal(
            expectedSelectedForeground,
            GetBrushColor(document, "FlourishControlSelectedForegroundBrush")
        );
        Assert.Equal(
            expectedPressed,
            GetBrushColor(document, "FlourishPressedRevealBrush")
        );
        Assert.Equal(
            expectedDangerHover,
            GetBrushColor(document, "FlourishDangerHoverRevealBrush")
        );
        Assert.NotEqual(expectedHover, expectedPressed);
        var controlBackground = ParseColor(
            GetBrushColor(document, "FlourishControlBackgroundBrush")
        ).Rgb;
        Assert.True(
            GetRelativeLuminance(
                Composite(ParseColor(expectedHover), controlBackground)
            )
                > GetRelativeLuminance(
                    Composite(ParseColor(expectedPressed), controlBackground)
                )
        );
    }

    [Theory]
    [InlineData(
        "Colors.Light.xaml",
        "#EBF3FC",
        "#115EA3",
        "#CFE4FA",
        "#96C6FA",
        "#0A2E4A"
    )]
    [InlineData(
        "Colors.Dark.xaml",
        "#082338",
        "#62ABF5",
        "#0C3B5E",
        "#061724",
        "#EBF3FC"
    )]
    public void TonalButtonPalette_UsesExpectedBrandInspiredTokens(
        string fileName,
        string expectedBackground,
        string expectedForeground,
        string expectedHover,
        string expectedPressed,
        string expectedPressedForeground
    )
    {
        var document = LoadXaml(
            Path.Combine(FlourishRoot, "Themes", "Colors", fileName)
        );

        Assert.Equal(expectedBackground, GetBrushColor(document, "FlourishTonalButtonBackgroundBrush"));
        Assert.Equal(expectedForeground, GetBrushColor(document, "FlourishTonalButtonForegroundBrush"));
        Assert.Equal(expectedHover, GetBrushColor(document, "FlourishTonalButtonHoverBrush"));
        Assert.Equal(expectedPressed, GetBrushColor(document, "FlourishTonalButtonPressedBrush"));
        Assert.Equal(
            expectedPressedForeground,
            GetBrushColor(document, "FlourishTonalButtonPressedForegroundBrush")
        );
    }

    [Theory]
    [InlineData("Colors.Light.xaml")]
    [InlineData("Colors.Dark.xaml")]
    public void InteractiveAccentColors_ComeFromTheApprovedBrandRamp(
        string fileName
    )
    {
        var document = LoadXaml(
            Path.Combine(FlourishRoot, "Themes", "Colors", fileName)
        );

        foreach (
            var key in new[]
            {
                "FlourishHoverRevealBrush",
                "FlourishPressedRevealBrush",
                "FlourishControlSelectedBrush",
            }
        )
        {
            var rgb = ParseColor(GetBrushColor(document, key)).Rgb;
            Assert.Contains(ToHex(rgb), ApprovedBrandRamp);
        }
    }

    [Theory]
    [InlineData("Colors.Light.xaml")]
    [InlineData("Colors.Dark.xaml")]
    public void SelectedStates_MaintainReadableTextContrast(string fileName)
    {
        var document = LoadXaml(
            Path.Combine(FlourishRoot, "Themes", "Colors", fileName)
        );
        var selected = ParseColor(
            GetBrushColor(document, "FlourishControlSelectedBrush")
        ).Rgb;
        var foreground = ParseColor(
            GetBrushColor(document, "FlourishControlSelectedForegroundBrush")
        ).Rgb;
        var hover = ParseColor(GetBrushColor(document, "FlourishHoverRevealBrush"));
        var selectedHover = Composite(hover, selected);

        AssertReadableContrast(foreground, selected, fileName, "selected");
        AssertReadableContrast(
            foreground,
            selectedHover,
            fileName,
            "selected + hover"
        );
    }

    private static ParticipatingTemplate[] FindParticipatingTemplates()
    {
        var controlsRoot = Path.Combine(FlourishRoot, "Controls");
        var result = new List<ParticipatingTemplate>();

        foreach (
            var file in Directory.EnumerateFiles(
                controlsRoot,
                "*.xaml",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            var document = LoadXaml(file);
            foreach (
                var style in document
                    .Descendants()
                    .Where(element => element.Name.LocalName == "Style")
                    .Where(style =>
                        style
                            .Elements()
                            .Any(element =>
                                element.Name.LocalName == "Setter"
                                && ((string?)element.Attribute("Property"))?.EndsWith(
                                    "HoverReveal.IsParticipant",
                                    StringComparison.Ordinal
                                ) == true
                                && (string?)element.Attribute("Value") == "True"
                            )
                    )
            )
            {
                var template = style
                    .Descendants()
                    .FirstOrDefault(element => element.Name.LocalName == "ControlTemplate");
                if (template is null)
                {
                    var templateReference = style
                        .Elements()
                        .FirstOrDefault(element =>
                            element.Name.LocalName == "Setter"
                            && (string?)element.Attribute("Property") == "Template"
                        )
                        ?.Attribute("Value")
                        ?.Value;
                    const string staticResourcePrefix = "{StaticResource ";
                    if (
                        templateReference?.StartsWith(
                            staticResourcePrefix,
                            StringComparison.Ordinal
                        ) == true
                        && templateReference.EndsWith('}')
                    )
                    {
                        var key = templateReference[
                            staticResourcePrefix.Length..^1
                        ];
                        template = document
                            .Descendants()
                            .SingleOrDefault(element =>
                                element.Name.LocalName == "ControlTemplate"
                                && (string?)element.Attribute(
                                    XName.Get("Key", XamlNamespace)
                                ) == key
                            );
                    }
                }
                if (template is not null)
                {
                    result.Add(
                        new ParticipatingTemplate(
                            file,
                            $"{RelativePath(file)}::{(string?)style.Attribute("TargetType")}",
                            style,
                            template
                        )
                    );
                }
            }
        }

        return result
            .OrderBy(template => template.Identifier, StringComparer.Ordinal)
            .ToArray();
    }

    private static XElement[] FindNamedDescendants(XElement root, string name)
    {
        return root
            .Descendants()
            .Where(element =>
                (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == name
            )
            .ToArray();
    }

    private static HashSet<(string Property, string Value)> GetConditions(
        XElement trigger
    )
    {
        var result = new HashSet<(string Property, string Value)>();
        if (trigger.Name.LocalName == "Trigger")
        {
            result.Add(
                (
                    (string?)trigger.Attribute("Property") ?? string.Empty,
                    (string?)trigger.Attribute("Value") ?? string.Empty
                )
            );
        }

        foreach (
            var condition in trigger
                .Descendants()
                .Where(element => element.Name.LocalName == "Condition")
        )
        {
            result.Add(
                (
                    (string?)condition.Attribute("Property") ?? string.Empty,
                    (string?)condition.Attribute("Value") ?? string.Empty
                )
            );
        }

        return result;
    }

    private static XElement FindTrigger(
        XElement template,
        string property,
        string value
    )
    {
        return template
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Trigger"
                && (string?)element.Attribute("Property") == property
                && (string?)element.Attribute("Value") == value
            );
    }

    private static bool HasCondition(
        XElement trigger,
        string property,
        string value
    )
    {
        return trigger
            .Descendants()
            .Any(element =>
                element.Name.LocalName == "Condition"
                && (string?)element.Attribute("Property") == property
                && (string?)element.Attribute("Value") == value
            );
    }

    private static void AssertSetter(
        XElement trigger,
        string? targetName,
        string property,
        string value
    )
    {
        Assert.Contains(
            trigger.Elements(),
            element =>
                element.Name.LocalName == "Setter"
                && (string?)element.Attribute("TargetName") == targetName
                && (string?)element.Attribute("Property") == property
                && (string?)element.Attribute("Value") == value
        );
    }

    private static void AssertCloseButtonHasIcon(XDocument document)
    {
        var closeButton = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "WindowCaptionButton"
                && (string?)element.Attribute(XName.Get("Name", XamlNamespace))
                    == "CloseButton"
            );

        Assert.False(string.IsNullOrWhiteSpace((string?)closeButton.Attribute("Icon")));
    }

    private static void AssertButtonVariant(
        XDocument document,
        string name,
        string variant
    )
    {
        var button = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "WindowCaptionButton"
                && (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == name
            );

        Assert.Equal(variant, (string?)button.Attribute("Variant"));
        Assert.Null(button.Attribute("Appearance"));
    }

    private static void AssertAttribute(
        ParticipatingTemplate template,
        XElement element,
        string property,
        string expected,
        ICollection<string> violations
    )
    {
        var actual = (string?)element.Attribute(property);
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            violations.Add(
                $"{template.Identifier}: {GetNodeName(element)}.{property} is "
                    + $"{actual ?? "<missing>"}, expected {expected}"
            );
        }
    }

    private static string GetBrushColor(XDocument document, string key)
    {
        var brush = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "SolidColorBrush"
                && (string?)element.Attribute(XName.Get("Key", XamlNamespace)) == key
            );
        return (string)brush.Attribute("Color")!;
    }

    private static ParsedColor ParseColor(string value)
    {
        var hex = value.TrimStart('#');
        var offset = hex.Length == 8 ? 2 : 0;
        var alpha = offset == 2 ? Convert.ToByte(hex[..2], 16) : byte.MaxValue;
        return new ParsedColor(
            alpha,
            new RgbColor(
                Convert.ToByte(hex.Substring(offset, 2), 16),
                Convert.ToByte(hex.Substring(offset + 2, 2), 16),
                Convert.ToByte(hex.Substring(offset + 4, 2), 16)
            )
        );
    }

    private static RgbColor Composite(ParsedColor foreground, RgbColor background)
    {
        var alpha = foreground.Alpha / 255d;
        return new RgbColor(
            Blend(foreground.Rgb.Red, background.Red, alpha),
            Blend(foreground.Rgb.Green, background.Green, alpha),
            Blend(foreground.Rgb.Blue, background.Blue, alpha)
        );
    }

    private static byte Blend(byte foreground, byte background, double alpha)
    {
        return (byte)Math.Round(
            (foreground * alpha) + (background * (1d - alpha))
        );
    }

    private static void AssertReadableContrast(
        RgbColor foreground,
        RgbColor background,
        string fileName,
        string state
    )
    {
        var contrast = GetContrastRatio(foreground, background);
        Assert.True(
            contrast >= 4.5,
            $"{fileName} {state} contrast was {contrast:F2}:1, expected at least 4.5:1."
        );
    }

    private static double GetContrastRatio(RgbColor first, RgbColor second)
    {
        var firstLuminance = GetRelativeLuminance(first);
        var secondLuminance = GetRelativeLuminance(second);
        return (Math.Max(firstLuminance, secondLuminance) + 0.05)
            / (Math.Min(firstLuminance, secondLuminance) + 0.05);
    }

    private static double GetRelativeLuminance(RgbColor color)
    {
        return (0.2126 * Linearize(color.Red))
            + (0.7152 * Linearize(color.Green))
            + (0.0722 * Linearize(color.Blue));
    }

    private static double Linearize(byte channel)
    {
        var value = channel / 255d;
        return value <= 0.04045
            ? value / 12.92
            : Math.Pow((value + 0.055) / 1.055, 2.4);
    }

    private static string ToHex(RgbColor color)
    {
        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
    }

    private static string GetNodeName(XElement element)
    {
        return (string?)element.Attribute(XName.Get("Name", XamlNamespace))
            ?? element.Name.LocalName;
    }

    private static void AssertNoViolations(IReadOnlyCollection<string> violations)
    {
        Assert.True(
            violations.Count == 0,
            "HoverReveal visual contract violations:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, violations)
        );
    }

    private static XDocument LoadXaml(string file)
    {
        return XDocument.Load(file, LoadOptions.SetLineInfo);
    }

    private static string RelativePath(string path)
    {
        return Path.GetRelativePath(RepositoryRoot, path).Replace('\\', '/');
    }

    private static string FindRepositoryRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (
                File.Exists(Path.Combine(directory.FullName, "Flourish.slnx"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish"))
            )
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the Flourish repository above {AppContext.BaseDirectory}."
        );
    }

    private sealed record ParticipatingTemplate(
        string File,
        string Identifier,
        XElement Style,
        XElement Template
    );

    private readonly record struct ParsedColor(byte Alpha, RgbColor Rgb);

    private readonly record struct RgbColor(byte Red, byte Green, byte Blue);
}
