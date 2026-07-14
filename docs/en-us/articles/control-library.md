---
title: Control library
description: Load Flourish theme resources and use explicit custom controls, semantic variants, and hover behavior.
---

# Control library

Flourish provides themed WPF custom controls for application pages, Shell extension regions, dialogs, and independently hosted windows. Use a `Flourish*` control when an element should use the Flourish theme and interaction states.

Loading Flourish resources does not install implicit styles for WPF base types. A WPF `<Button>`, `<TextBox>`, or `<ListBox>` keeps its native appearance; `<flourish:FlourishButton>`, `<flourish:FlourishTextBox>`, and `<flourish:FlourishListBox>` opt in to the Flourish templates.

## Load the control resources

The runtime created by `FlourishBuilder` adds the Flourish control and theme resources to `Application.Resources` before it shows the Shell. Applications that create controls only after `IFlourish.Show(Application)` or `Run(Application)` do not need another resource declaration.

Add `FlourishThemeResources` explicitly when controls must work in the WPF designer, before the Shell starts, or without a Flourish Shell:

```xml
<Application
  x:Class="Foobar.App"
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:flourish="http://schemas.arkheide.system/flourish"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <flourish:FlourishThemeResources />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

`http://schemas.arkheide.system/flourish` is the public XAML namespace for Flourish controls and theme resources. Add one `FlourishThemeResources` instance at application scope. Do not also merge Flourish theme dictionaries by URI.

## Available controls

The public control library provides the following control families:

| Flourish control | Purpose |
| --- | --- |
| `FlourishButton` | Actions, including primary, low-emphasis, card, and destructive appearances. |
| `FlourishCard` | Groups related content on a themed surface. |
| `FlourishTextBlock`, `FlourishLabel` | Semantic text roles and access-key-aware form labels. |
| `FlourishTextBox`, `FlourishPasswordBox`, `FlourishSearchBox` | Text, password, and search input. |
| `FlourishCheckBox`, `FlourishRadioButton` | Independent and mutually exclusive choices. |
| `FlourishComboBox`, `FlourishComboBoxItem` | Drop-down selection and generated item containers. |
| `FlourishListBox`, `FlourishListBoxItem` | List selection and generated item containers. |
| `FlourishScrollViewer`, `FlourishScrollBar` | Scrolling surfaces and scroll bars. |
| `FlourishToolTip`, `FlourishGridSplitter` | Themed tooltips and layout resizing. |

See the [Controls API](xref:ArkheideSystem.Flourish.Controls) for properties, variants, and attached behaviors.

## Use the controls

Reference Flourish controls explicitly in page XAML:

```xml
<StackPanel
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:flourish="http://schemas.arkheide.system/flourish">
  <flourish:FlourishTextBlock
    Role="PageTitle"
    Text="Account" />

  <flourish:FlourishTextBlock
    Role="Subtitle"
    Text="Manage the current profile and sign-in state." />

  <flourish:FlourishTextBlock
    Role="SectionTitle"
    Text="Account details" />
  <flourish:FlourishTextBlock
    Role="Description"
    Text="Edit the information visible to the current user." />

  <flourish:FlourishCard>
    <StackPanel>
      <flourish:FlourishTextBlock
        Role="FieldLabel"
        Text="Display name" />
      <flourish:FlourishTextBox Text="Foo Bar" />
      <flourish:FlourishSearchBox Placeholder="Search accounts" />
      <StackPanel>
        <flourish:FlourishButton
          Appearance="Subtle"
          Content="Cancel" />
        <flourish:FlourishButton
          Appearance="Primary"
          Content="Save" />
      </StackPanel>
    </StackPanel>
  </flourish:FlourishCard>
</StackPanel>
```

### FlourishButton

`FlourishButton.Appearance` describes the action:

- `Standard` is the default action.
- `Primary` identifies the main action in a group.
- `Subtle` reduces visual emphasis.
- `Card` presents the complete button as an interactive card.
- `Danger` identifies a destructive action and uses warning feedback.

Layout containers control external placement such as `Margin`. Pointer and keyboard focus use distinct states, and keyboard focus remains visible.

### FlourishTextBlock

`FlourishTextBlock.Role` selects semantic typography. Available roles are `Body`, `Paragraph`, `Caption`, `Muted`, `FieldLabel`, `Subtitle`, `Description`, `CardTitle`, `SectionTitle`, `PageTitle`, `Status`, and `Icon`.

`Paragraph` provides wrapped body text with increased line spacing. `Description` provides supporting text below a heading, and `CardTitle` identifies a heading inside a card or compact content surface. Body, supporting, label, status, and icon roles use `Regular`; `CardTitle`, `SectionTitle`, and `PageTitle` use `Bold`. Roles use the active font and theme resources; an explicitly assigned text property takes precedence.

### FlourishCard and FlourishSearchBox

`FlourishCard` groups one content tree. Its appearances are `Standard`, `Subtle`, `Accent`, `Elevated`, and `Hero`. `Elevated` adds elevation, while `Hero` uses a gradient background and elevation for introductory content. Use `FlourishButton Appearance="Card"` when the complete surface must be interactive.

`FlourishSearchBox` adds a search glyph and `Placeholder` while retaining text binding, commands, selection, and `TextChanged`.

## Hover reveal and reduced motion

Participating Flourish controls use the public `HoverReveal` attached behavior. Configure it application-wide through [Motion](configure-motion.md), including the operating system reduced-motion preference:

```csharp
builder
    .ConfigureShell(shell => shell.UseMotion())
    .ConfigureMotion(motion =>
        motion
            .EnableHoverRevealAnimation(TimeSpan.FromMilliseconds(140))
            .RespectSystemReducedMotion());
```

Attached properties provide a local override:

```xml
<flourish:FlourishButton
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  Content="Preview" />
```

A custom template that participates in the behavior provides elements named `HoverChrome` and `HoverRevealScale`, sets `flourish:HoverReveal.IsParticipant="True"`, and binds `flourish:HoverReveal.IsMotionEnabled` to `{DynamicResource FlourishHoverRevealEnabled}`. `IsEnabled` and `AnimationDuration` inherit through the visual tree; `IsMotionEnabled` and `IsParticipant` do not. The behavior has no effect when either named element is absent.

Set `flourish:HoverReveal.TemplateHandlesInteraction="True"` when a replacement template defines its own static hover and pressed states. Otherwise, HoverReveal supplies those pointer states.

## Themes and semantic resources

Theme switching updates Flourish controls, generated item containers, popups, and scroll bars without recreating the page. Use [Themes](configure-themes.md) for theme mode, brand colors, and shared corner radius, and use [Typography](configure-font.md) for global and page-specific fonts.

When overriding a semantic theme resource, verify the result in both light and dark themes and preserve readable text contrast.

## Related features

- [Getting started](getting-started.md) explains runtime startup and resource loading.
- [Themes](configure-themes.md) configures theme mode, brand colors, and corner radius.
- [Typography](configure-font.md) changes global and page-specific fonts.
- [Motion](configure-motion.md) configures HoverReveal and reduced-motion behavior.
- [Themes API](xref:ArkheideSystem.Flourish.Themes) documents `FlourishThemeResources`.
