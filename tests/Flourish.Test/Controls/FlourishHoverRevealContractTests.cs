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
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string FlourishRoot = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish"
    );

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

            AssertAttribute(template, hoverChrome[0], "Background", HoverRevealBrush, violations);
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
    public void ButtonPressedState_UsesASeparateDarkerFillWithoutAnOutline()
    {
        var file = Path.Combine(FlourishRoot, "Controls", "Button.xaml");
        var document = LoadXaml(file);
        var template = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "ControlTemplate"
                && (string?)element.Attribute(XName.Get("Key", XamlNamespace))
                    == "FlourishButtonTemplate"
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
        AssertSetter(pressedTrigger, "HoverChrome", "Visibility", "Collapsed");
        AssertSetter(pressedTrigger, "PressedChrome", "Opacity", "1");

        var disabledTrigger = FindTrigger(template, "IsEnabled", "False");
        AssertSetter(disabledTrigger, "HoverChrome", "Visibility", "Collapsed");
        AssertSetter(disabledTrigger, "PressedChrome", "Visibility", "Collapsed");
    }

    [Theory]
    [InlineData("Colors.Light.xaml", "#1F0F6CBD", "#330F6CBD")]
    [InlineData("Colors.Dark.xaml", "#33A7D8F9", "#4D479EF5")]
    public void Palettes_KeepHoverBrighterAndPressedVisuallyDeeper(
        string fileName,
        string expectedHover,
        string expectedPressed
    )
    {
        var document = LoadXaml(
            Path.Combine(FlourishRoot, "Themes", "Colors", fileName)
        );

        Assert.Equal(expectedHover, GetBrushColor(document, "FlourishHoverRevealBrush"));
        Assert.Equal(
            expectedPressed,
            GetBrushColor(document, "FlourishPressedRevealBrush")
        );
        Assert.NotEqual(expectedHover, expectedPressed);
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

    private static void AssertSetter(
        XElement trigger,
        string targetName,
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
        XElement Template
    );
}
