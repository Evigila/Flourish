using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class SplitButtonChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("split links share selection while disclosure expansion remains independent", async () =>
        {
            var parameters = new Dictionary<string, object?>
            {
                [nameof(SplitButton.Href)] = "/reports", [nameof(SplitButton.Text)] = "Reports",
                [nameof(SplitButton.Icon)] = "description", [nameof(SplitButton.Selected)] = true,
                [nameof(SplitButton.Expanded)] = false, [nameof(SplitButton.SecondaryControls)] = "report-options",
                [nameof(SplitButton.SecondaryLabel)] = "Expand report options",
                [nameof(SplitButton.PrimaryClass)] = "host-link", [nameof(SplitButton.SecondaryClass)] = "host-toggle",
                [nameof(SplitButton.PrimaryAttributes)] = new Dictionary<string, object> { ["title"] = "All reports", ["aria-current"] = "page" },
                [nameof(SplitButton.SecondaryAttributes)] = new Dictionary<string, object> { ["data-action"] = "report-options" }
            };
            await WithButton(parameters, async (button, html) =>
            {
                var primary = Tag(html(), "a", "f-split-button-primary");
                var secondary = Tag(html(), "button", "f-split-button-secondary");
                Require(Attribute(primary, "href") == "/reports", "The main link lost its destination.");
                Require(HasClass(primary, "is-selected") && HasClass(secondary, "is-selected"), "Selection differs between the two controls.");
                Require(HasClass(primary, "host-link") && HasClass(secondary, "host-toggle"), "Host class hooks were lost.");
                Require(Attribute(primary, "aria-current") == "page" && Attribute(primary, "title") == "All reports", "Main link attributes were lost.");
                Require(Attribute(secondary, "aria-expanded") == "false" && Attribute(secondary, "aria-controls") == "report-options", "The disclosure relationship was lost.");
                Require(Attribute(secondary, "aria-label") == "Expand report options" && Attribute(secondary, "data-action") == "report-options", "Secondary attributes or accessible name were lost.");
                Require(Attribute(secondary, "type") == "button" && Attribute(secondary, "aria-current") is null, "The secondary action became a submit or route action.");
                Require(Attribute(Tag(html(), "span", "f-expansion-indicator"), "data-expanded") == "false", "Collapsed splits lost the shared expansion marker.");
                parameters[nameof(SplitButton.Selected)] = false;
                parameters[nameof(SplitButton.Expanded)] = true;
                parameters[nameof(SplitButton.PrimaryAttributes)] = new Dictionary<string, object> { ["title"] = "All reports" };
                await button.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(!HasClass(Tag(html(), "a", "f-split-button-primary"), "is-selected") && !HasClass(Tag(html(), "button", "f-split-button-secondary"), "is-selected"), "Expanding the secondary region selected the two controls.");
                Require(Attribute(Tag(html(), "button", "f-split-button-secondary"), "aria-expanded") == "true", "Controlled expansion did not update.");
                Require(Attribute(Tag(html(), "span", "f-expansion-indicator"), "data-expanded") == "true", "The shared expansion marker did not follow controlled expansion.");
            });
        }));
        tests.Add(("split actions keep submit and secondary-button semantics separate", async () =>
        {
            await WithButton(new Dictionary<string, object?>
            {
                [nameof(SplitButton.Type)] = "submit", [nameof(SplitButton.Text)] = "Save",
                [nameof(SplitButton.SecondaryLabel)] = "Other save actions",
                [nameof(SplitButton.SecondaryContent)] = (RenderFragment)(builder => builder.AddContent(0, "Options"))
            }, (_, html) =>
            {
                var primary = Tag(html(), "button", "f-split-button-primary");
                var secondary = Tag(html(), "button", "f-split-button-secondary");
                Require(Attribute(primary, "type") == "submit" && Attribute(secondary, "type") == "button", "Both halves submit the host form.");
                Require(Attribute(secondary, "aria-expanded") is null && Attribute(secondary, "aria-controls") is null, "A plain secondary action invents a disclosure region.");
                Require(html().Contains("Options", StringComparison.Ordinal), "Custom secondary content was lost.");
                Require(!html().Contains("f-secondary-item", StringComparison.Ordinal), "The generic split button acquired navigation-specific classes.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("split callbacks remain independent and disabled links cannot dispatch", async () =>
        {
            var primaryCalls = 0;
            var secondaryCalls = 0;
            var parameters = new Dictionary<string, object?>
            {
                [nameof(SplitButton.Href)] = "/reports", [nameof(SplitButton.Text)] = "Reports",
                [nameof(SplitButton.PrimaryAttributes)] = new Dictionary<string, object> { ["HREF"] = "/must-not-navigate" },
                [nameof(SplitButton.OnClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => { primaryCalls++; }),
                [nameof(SplitButton.OnSecondaryClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => { secondaryCalls++; })
            };
            await WithButton(parameters, async (button, html) =>
            {
                await Invoke(button, "ClickAsync");
                Require(primaryCalls == 1 && secondaryCalls == 0, "A main click also dispatched the secondary action.");
                await Invoke(button, "SecondaryClickAsync");
                Require(primaryCalls == 1 && secondaryCalls == 1, "A secondary click also dispatched the main action.");
                parameters[nameof(SplitButton.Disabled)] = true;
                await button.SetParametersAsync(ParameterView.FromDictionary(parameters));
                var primary = Tag(html(), "a", "f-split-button-primary");
                var secondary = Tag(html(), "button", "f-split-button-secondary");
                Require(Attribute(primary, "href") is null && Attribute(primary, "aria-disabled") == "true" && Attribute(primary, "tabindex") == "-1", "A disabled main link remains navigable or tabbable.");
                Require(Attribute(primary, "role") == "link" && !Regex.IsMatch(primary, "\\shref=", RegexOptions.IgnoreCase), "Passthrough attributes revived a disabled link.");
                Require(Regex.IsMatch(secondary, "\\sdisabled(?:[\\s=>])") && Attribute(secondary, "aria-disabled") == "true", "The secondary button remains enabled.");
                await Invoke(button, "ClickAsync");
                await Invoke(button, "SecondaryClickAsync");
                Require(primaryCalls == 1 && secondaryCalls == 1, "Disabled state still allows a callback to dispatch.");
            });
        }));
        tests.Add(("split whitespace destinations use native buttons without empty icon placeholders", async () =>
        {
            await WithButton(new Dictionary<string, object?>
            {
                [nameof(SplitButton.Href)] = "   ", [nameof(SplitButton.Icon)] = " \t ",
                [nameof(SplitButton.Text)] = "Save", [nameof(SplitButton.SecondaryIcon)] = string.Empty,
                [nameof(SplitButton.SecondaryContent)] = (RenderFragment)(builder => builder.AddContent(0, "Options"))
            }, (_, html) =>
            {
                Require(!Regex.IsMatch(html(), "<a\\b"), "A whitespace destination became an empty navigation link.");
                Require(Attribute(Tag(html(), "button", "f-split-button-primary"), "type") == "button", "The primary action lost its native button fallback.");
                Require(!html().Contains("data-icon=", StringComparison.Ordinal), "An empty icon became a question-mark glyph.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("split native menus render usable SSR details without submitting or fabricating menu keyboard roles", async () =>
        {
            var primaryCalls = 0;
            await WithButton(new()
            {
                [nameof(SplitButton.Type)] = "submit", [nameof(SplitButton.Text)] = "Save",
                [nameof(SplitButton.MenuContent)] = MenuItems(),
                [nameof(SplitButton.SecondaryLabel)] = "Other save actions",
                [nameof(SplitButton.SecondaryControls)] = "save-options",
                [nameof(SplitButton.SecondaryAttributes)] = new Dictionary<string, object>
                {
                    ["id"] = "save-options-trigger", ["data-action"] = "save-options", ["title"] = "Show alternatives"
                },
                [nameof(SplitButton.OnClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => primaryCalls++)
            }, async (button, html) =>
            {
                var primary = Tag(html(), "button", "f-split-button-primary");
                var disclosure = Tag(html(), "details", "f-dropdown-surface");
                var summary = Tag(html(), "summary", "f-split-button-secondary");
                Require(Attribute(primary, "type") == "submit", "The primary form action lost submission semantics.");
                Require(HasClass(disclosure, "f-split-button-menu") && !HasBooleanAttribute(disclosure, "open"), "The native menu must start closed.");
                Require(Attribute(summary, "id") == "save-options-trigger" && Attribute(summary, "data-action") == "save-options" && Attribute(summary, "title") == "Show alternatives", "SecondaryAttributes did not reach the actual native summary.");
                Require(Attribute(summary, "aria-label") == "Other save actions" && Attribute(summary, "aria-controls") == "save-options", "The summary lost its accessible name or content relationship.");
                Require(Attribute(summary, "type") is null && Attribute(summary, "aria-expanded") is null && Attribute(summary, "role") is null, "A native summary submits or claims unsynchronized expansion/menu semantics.");
                Require(Attribute(Tag(html(), "div", "f-split-button-menu-content"), "id") == "save-options", "aria-controls refers to a missing native content region.");
                Require(!html().Contains("role=\"menu\"", StringComparison.Ordinal) && !html().Contains("role=\"menuitem\"", StringComparison.Ordinal), "The native disclosure falsely promises arrow-key menu behavior.");
                Require(Attribute(Tag(html(), "a", "alternate-save"), "href") == "/save-copy", "A standard navigation action lost its real destination.");
                Require(Attribute(Tag(html(), "button", "menu-action"), "type") == "button", "Opening or using a secondary action submits the primary form.");
                await Invoke(button, "SecondaryClickAsync");
                Require(primaryCalls == 0, "Native expansion dispatched the primary submit callback.");
                await Invoke(button, "ClickAsync");
                Require(primaryCalls == 1, "The native-menu mode disabled its independent primary action.");
            });
        }));
        tests.Add(("native menu relationships map explicit summary controls or generate a stable real content ID", async () =>
        {
            foreach (var controls in new string?[] { null, "mapped-menu" })
            {
                var parameters = new Dictionary<string, object?> { [nameof(SplitButton.MenuContent)] = MenuItems() };
                if (controls is not null)
                    parameters[nameof(SplitButton.SecondaryAttributes)] = new Dictionary<string, object> { ["aria-controls"] = controls };
                await WithButton(parameters, async (button, html) =>
                {
                    var actual = Attribute(Tag(html(), "summary", "f-split-button-secondary"), "aria-controls");
                    Require(!string.IsNullOrWhiteSpace(actual) && Attribute(Tag(html(), "div", "f-split-button-menu-content"), "id") == actual, "The generated or supplied summary relationship is unresolved.");
                    Require(controls is null || actual == controls, "The supplied native controls relationship was silently rewritten.");
                    await button.SetParametersAsync(ParameterView.FromDictionary(parameters));
                    Require(Attribute(Tag(html(), "div", "f-split-button-menu-content"), "id") == actual, "Rerendering changed the native menu content ID.");
                });
            }
        }));
        tests.Add(("disabled or busy splits remove native menus and suppress both callbacks", async () =>
        {
            foreach (var state in new[] { nameof(SplitButton.Disabled), nameof(SplitButton.Busy) })
            {
                var calls = 0;
                var mountedMenus = 0;
                var parameters = new Dictionary<string, object?>
                {
                    [nameof(SplitButton.Href)] = "/reports", [nameof(SplitButton.Text)] = "Reports", [state] = true,
                    [nameof(SplitButton.PrimaryAttributes)] = new Dictionary<string, object> { ["HREF"] = "/unsafe-destination" },
                    [nameof(SplitButton.BusyLabel)] = "Preparing report",
                    [nameof(SplitButton.MenuContent)] = (RenderFragment)(builder => { mountedMenus++; builder.AddContent(0, MenuItems()); }),
                    [nameof(SplitButton.OnClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => calls++)
                };
                await WithButton(parameters, async (button, html) =>
                {
                    var primary = Tag(html(), "a", "f-split-button-primary");
                    var secondary = Tag(html(), "button", "f-split-button-secondary");
                    Require(Attribute(primary, "href") is null && Attribute(primary, "tabindex") == "-1" && Attribute(primary, "aria-disabled") == "true", "An unavailable primary link still navigates.");
                    Require(HasBooleanAttribute(secondary, "disabled") && Attribute(secondary, "aria-controls") is null, "An unavailable native trigger remains active or points to absent content.");
                    Require(!html().Contains("<details", StringComparison.Ordinal) && !html().Contains("/save-copy", StringComparison.Ordinal) && mountedMenus == 0, "Unavailable menu content still exposes links, expands, or initializes host components.");
                    Require(Attribute(primary, "aria-busy") == (state == nameof(SplitButton.Busy) ? "true" : "false"), "Busy was not announced on the primary action.");
                    if (state == nameof(SplitButton.Busy)) Require(html().Contains("Preparing report", StringComparison.Ordinal) && html().Contains("f-spinner", StringComparison.Ordinal), "Busy status lost its standard spinner or accessible label.");
                    await Invoke(button, "ClickAsync");
                    await Invoke(button, "SecondaryClickAsync");
                    Require(calls == 0, "An unavailable native split dispatched a callback.");
                });
            }
        }));
        tests.Add(("busy controlled splits keep legacy expansion while blocking submits and secondary callbacks", async () =>
        {
            var calls = 0;
            await WithButton(new()
            {
                [nameof(SplitButton.Type)] = "submit", [nameof(SplitButton.Busy)] = true,
                [nameof(SplitButton.Expanded)] = true, [nameof(SplitButton.SecondaryControls)] = "existing-region",
                [nameof(SplitButton.OnClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => calls++),
                [nameof(SplitButton.OnSecondaryClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => calls++)
            }, async (button, html) =>
            {
                var primary = Tag(html(), "button", "f-split-button-primary");
                var secondary = Tag(html(), "button", "f-split-button-secondary");
                Require(Attribute(primary, "type") == "submit" && HasBooleanAttribute(primary, "disabled") && Attribute(primary, "aria-busy") == "true", "A busy primary submit remains available.");
                Require(Attribute(secondary, "aria-expanded") == "true" && Attribute(secondary, "aria-controls") == "existing-region" && HasBooleanAttribute(secondary, "disabled"), "Busy state changed the host's legacy controlled expansion.");
                await Invoke(button, "ClickAsync"); await Invoke(button, "SecondaryClickAsync");
                Require(calls == 0, "A busy controlled split dispatched either action.");
            });
        }));
        tests.Add(("split widths are positive pixels with explicit full-width precedence and navigation sizing isolation", async () =>
        {
            var parameters = new Dictionary<string, object?> { [nameof(SplitButton.Width)] = 320 };
            await WithButton(parameters, async (button, html) =>
            {
                Require(Attribute(Tag(html(), "div", "f-split-button"), "style") == "width:320px", "Fixed Width did not use invariant pixel units.");
                parameters[nameof(SplitButton.FullWidth)] = true;
                await button.SetParametersAsync(ParameterView.FromDictionary(parameters));
                var root = Tag(html(), "div", "f-split-button");
                Require(HasClass(root, "f-split-button-full-width") && Attribute(root, "style") is null, "FullWidth did not take precedence over fixed Width.");
            });
            foreach (var width in new[] { 0, -1 })
                await Reject(new() { [nameof(SplitButton.Width)] = width, [nameof(SplitButton.FullWidth)] = true }, nameof(SplitButton.Width));
            var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var layout = File.ReadAllText(Path.Combine(rootPath, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/split-button.css"));
            Require(layout.Contains(".f-split-button:not(.f-secondary-row) > :is(.f-split-button-secondary,.f-split-button-menu)", StringComparison.Ordinal) && layout.Contains("flex:0 0 48px; width:48px; height:48px", StringComparison.Ordinal), "Fixed square secondary layout escaped the library or changed existing navigation-row sizing.");
        }));
        tests.Add(("native split panels beat interactive hiding in the complete Framework and optional Design cascade", () =>
        {
            var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var framework = ExpandCssImports(Path.Combine(rootPath, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css"));
            var design = ExpandCssImports(Path.Combine(rootPath, "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/design.css"));
            foreach (var css in new[] { framework, framework + "\n" + design })
            {
                Require(PanelDisplay(css, nativeSplit: true, open: true, interactiveOpen: false) == "block", "Native details opened but the interactive popover hiding rules still conceal its panel.");
                Require(PanelDisplay(css, nativeSplit: true, open: false, interactiveOpen: false) == "none", "A closed native split leaks its panel into layout.");
                Require(PanelDisplay(css, nativeSplit: false, open: false, interactiveOpen: false) == "none", "The native override revived an unrelated closed interactive panel.");
                Require(PanelDisplay(css, nativeSplit: false, open: false, interactiveOpen: true) == "block", "The native override broke an unrelated data-f-open interactive panel.");
                Require(PanelDisplay(css, nativeSplit: false, open: true, interactiveOpen: false, nativeActionMenu: true) == "block", "A native row action disclosure is still hidden by the interactive panel cascade.");
                Require(PanelDisplay(css, nativeSplit: false, open: false, interactiveOpen: false, nativeActionMenu: true) == "none", "A closed native row action disclosure leaks its panel into layout.");
            }
            return Task.CompletedTask;
        }));
        tests.Add(("native menus reject conflicting controlled callbacks and static expansion attributes", async () =>
        {
            await Reject(new() { [nameof(SplitButton.MenuContent)] = MenuItems(), [nameof(SplitButton.Expanded)] = false }, nameof(SplitButton.Expanded));
            await Reject(new()
            {
                [nameof(SplitButton.MenuContent)] = MenuItems(),
                [nameof(SplitButton.OnSecondaryClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => { })
            }, nameof(SplitButton.OnSecondaryClick));
            foreach (var attribute in new[] { "aria-expanded", "onclick" })
                await Reject(new()
                {
                    [nameof(SplitButton.MenuContent)] = MenuItems(),
                    [nameof(SplitButton.SecondaryAttributes)] = new Dictionary<string, object> { [attribute] = "false" }
                }, nameof(SplitButton.SecondaryAttributes));
            await Reject(new() { [nameof(SplitButton.MenuContent)] = MenuItems(), [nameof(SplitButton.SecondaryControls)] = "two IDs" }, nameof(SplitButton.SecondaryControls));
            await Reject(new()
            {
                [nameof(SplitButton.MenuContent)] = MenuItems(), [nameof(SplitButton.SecondaryControls)] = "actual-menu",
                [nameof(SplitButton.SecondaryAttributes)] = new Dictionary<string, object> { ["aria-controls"] = "other-menu" }
            }, nameof(SplitButton.SecondaryAttributes));
        }));
        tests.Add(("dropdown surfaces keep native default behavior and forward trigger attributes to summary only", async () =>
        {
            var services = new ServiceCollection(); services.AddLogging(); services.AddFlourishFramework();
            using var provider = services.BuildServiceProvider();
            await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var trigger = (RenderFragment)(builder => builder.AddContent(0, "Open choices"));
                var plain = await renderer.RenderComponentAsync<DropdownSurface>(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(DropdownSurface.Trigger)] = trigger }));
                Require(Attribute(Tag(plain.ToHtmlString(), "summary", "f-menu-trigger"), "class") == "f-menu-trigger", "Existing dropdown summary classes changed without opting into the new API.");
                var configured = await renderer.RenderComponentAsync<DropdownSurface>(ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    [nameof(DropdownSurface.Trigger)] = trigger, [nameof(DropdownSurface.TriggerClass)] = "custom-trigger",
                    [nameof(DropdownSurface.TriggerAttributes)] = new Dictionary<string, object> { ["id"] = "actual-summary", ["data-trigger"] = "choices" },
                    [nameof(DropdownSurface.AdditionalAttributes)] = new Dictionary<string, object> { ["id"] = "outer-disclosure", ["open"] = true }
                }));
                var html = configured.ToHtmlString();
                var summary = Tag(html, "summary", "f-menu-trigger"); var details = Tag(html, "details", "f-dropdown-surface");
                Require(HasClass(summary, "custom-trigger") && Attribute(summary, "id") == "actual-summary" && Attribute(summary, "data-trigger") == "choices", "Trigger hooks did not reach the real summary.");
                Require(Attribute(details, "id") == "outer-disclosure" && HasBooleanAttribute(details, "open") && Attribute(details, "data-trigger") is null, "Trigger and disclosure attributes leaked into each other.");
            });
        }));
    }

    private static RenderFragment MenuItems() => builder =>
    {
        builder.OpenComponent<Button>(0); builder.AddAttribute(1, nameof(Button.Href), "/save-copy");
        builder.AddAttribute(2, nameof(Button.Text), "Save copy"); builder.AddAttribute(3, nameof(Button.Class), "alternate-save"); builder.CloseComponent();
        builder.OpenComponent<Button>(4); builder.AddAttribute(5, nameof(Button.Type), "button");
        builder.AddAttribute(6, nameof(Button.Text), "Save draft"); builder.AddAttribute(7, nameof(Button.Class), "menu-action"); builder.CloseComponent();
    };
    private static async Task Reject(Dictionary<string, object?> parameters, string name)
    {
        try { await WithButton(parameters, (_, _) => Task.CompletedTask); }
        catch (ArgumentException error) when (error.ParamName == name) { return; }
        throw new InvalidOperationException("Invalid split configuration was not rejected through " + name + '.');
    }
    private static bool HasBooleanAttribute(string tag, string name) => Regex.IsMatch(tag, $"(?:^|\\s){Regex.Escape(name)}(?:[\\s=>])");

    private static string ExpandCssImports(string path)
    {
        var source = Regex.Replace(File.ReadAllText(path), @"/\*[\s\S]*?\*/", string.Empty);
        return Regex.Replace(source, "@import\\s+url\\(['\"]([^'\"]+)['\"]\\)\\s*;", match =>
            ExpandCssImports(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, match.Groups[1].Value))));
    }
    // Deliberately narrow display cascade, not a pretend browser engine: resolve all panel-display
    // rules from both fully expanded stylesheets, fail on unknown selectors and honor specificity/order.
    private static string PanelDisplay(string css, bool nativeSplit, bool open, bool interactiveOpen, bool nativeActionMenu = false)
    {
        var winner = string.Empty;
        var winnerImportance = -1;
        var winnerSpecificity = -1;
        foreach (Match rule in Regex.Matches(css, @"(?<selector>[^{}]+)\{(?<body>[^{}]*)\}"))
        {
            var displays = Regex.Matches(rule.Groups["body"].Value, @"(?:^|;)\s*display\s*:\s*(?<value>[^;}]+)", RegexOptions.IgnoreCase);
            if (displays.Count == 0) continue;
            foreach (var part in rule.Groups["selector"].Value.Split(','))
            {
                var selector = Regex.Replace(part.Trim(), @"\s+", " ");
                if (!selector.Contains(".f-menu-panel", StringComparison.Ordinal) || selector.Contains("::", StringComparison.Ordinal)) continue;
                var applies = selector switch
                {
                    ".f-menu-panel" => true,
                    ".f-menu-panel:not(:popover-open):not([data-f-open])" => !interactiveOpen,
                    ".f-menu-panel[data-f-open]" => interactiveOpen,
                    ".f-data-display-options.f-menu-panel[data-f-open]" => false,
                    ".f-dropdown-surface > .f-menu-panel" => nativeSplit || nativeActionMenu,
                    ".f-dropdown-surface[open] > .f-menu-panel" => (nativeSplit || nativeActionMenu) && open,
                    ".f-split-button > .f-split-button-menu[open] > .f-menu-panel" => nativeSplit && open,
                    ".f-action-menu[data-f-native-menu]:not([data-f-enhanced])[open] > .f-menu-panel" => nativeActionMenu && open,
                    _ => throw new InvalidOperationException("Review the new panel display selector in the cascade contract: " + selector)
                };
                if (!applies) continue;
                var specificity = Regex.Matches(selector, @"\.[\w-]+|\[[^\]]+\]|(?<!:):(?!not\()[\w-]+").Count;
                foreach (Match display in displays)
                {
                    var value = display.Groups["value"].Value.Trim();
                    var importance = value.Contains("!important", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                    if (importance < winnerImportance || importance == winnerImportance && specificity < winnerSpecificity) continue;
                    winner = Regex.Replace(value, "\\s*!important\\s*", string.Empty, RegexOptions.IgnoreCase).Trim();
                    winnerImportance = importance; winnerSpecificity = specificity;
                }
            }
        }
        return winner;
    }

    private static async Task WithButton(Dictionary<string, object?> parameters, Func<SplitButton, Func<string>, Task> verify)
    {
        var activator = new ButtonActivator();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton<IComponentActivator>(activator);
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<SplitButton>(ParameterView.FromDictionary(parameters));
            await verify(activator.Button ?? throw new InvalidOperationException("Split button was not created."), output.ToHtmlString);
        });
    }

    private static Task Invoke(SplitButton button, string methodName)
    {
        var method = typeof(SplitButton).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Split button handler was not found.");
        return (Task)(method.Invoke(button, [new MouseEventArgs()]) ?? throw new InvalidOperationException("Split button handler returned no task."));
    }
    private static string Tag(string html, string name, string cssClass) => Regex.Matches(html, $"<{name}\\b[^>]*>")
        .Select(match => match.Value).Single(tag => HasClass(tag, cssClass));
    private static string? Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, $"(?:^|\\s){Regex.Escape(name)}=\"([^\"]*)\"");
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : null;
    }
    private static bool HasClass(string tag, string cssClass) => Attribute(tag, "class")?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(cssClass) == true;
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class ButtonActivator : IComponentActivator
    {
        internal SplitButton? Button { get; private set; }
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            if (component is SplitButton button) Button = button;
            return component;
        }
    }
}
