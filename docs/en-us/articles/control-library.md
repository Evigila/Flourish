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

The public library contains 17 controls. Each one derives from the corresponding WPF control unless the base type shown below says otherwise. It keeps that base type's properties, events, commands, data binding, validation, and automation behavior.

| Flourish control | Base type | Flourish-specific contract |
| --- | --- | --- |
| `FlourishButton` | `Button` | `Appearance` and `Variant` select semantic color and layout roles. |
| `FlourishCard` | `ContentControl` | `Appearance` selects the surface treatment. It hosts one content tree. |
| `FlourishTextBlock` | `TextBlock` | `Role` selects semantic typography. |
| `FlourishLabel` | `Label` | No additional property; keeps `Target` and native access-key support. |
| `FlourishTextBox` | `TextBox` | No additional property; supplies the Flourish input template. |
| `FlourishPasswordBox` | `Control` | Exposes password access, length and mask settings, a change event, and editor methods. |
| `FlourishSearchBox` | `FlourishTextBox` | Adds `Placeholder` and a search glyph. |
| `FlourishCheckBox` | `CheckBox` | No additional property; supports WPF two-state and three-state selection. |
| `FlourishRadioButton` | `RadioButton` | No additional property; uses WPF `GroupName` grouping. |
| `FlourishComboBox` | `ComboBox` | Generates `FlourishComboBoxItem` containers for data items. |
| `FlourishComboBoxItem` | `ComboBoxItem` | The themed item container for a combo box. |
| `FlourishListBox` | `ListBox` | `Appearance` selects a standard or navigation list; `IsCompact` collapses navigation geometry. |
| `FlourishListBoxItem` | `ListBoxItem` | Adds navigation visibility, group-heading, and command-item states. |
| `FlourishScrollViewer` | `ScrollViewer` | `IsCompact` selects compact scroll bars. |
| `FlourishScrollBar` | `ScrollBar` | The themed vertical or horizontal scroll bar. |
| `FlourishToolTip` | `ToolTip` | Enables shell-region-aware placement in its default style. |
| `FlourishGridSplitter` | `GridSplitter` | `Variant` selects a standard or navigation-pane resize affordance. |

Use ordinary WPF properties such as `Content`, `Text`, `ItemsSource`, `SelectedItem`, `Command`, `IsEnabled`, `Margin`, and `ToolTip` as documented for the base type. The tables below list the additional Flourish properties and their defaults; see the [Controls API](xref:ArkheideSystem.Flourish.Controls) for exhaustive inherited members and signatures.

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

### Buttons and content surfaces

`FlourishButton.Appearance` describes the action and defaults to `Standard`:

- `Standard` is the default action.
- `Primary` identifies the main action in a group.
- `Subtle` reduces visual emphasis.
- `Card` presents the complete button as an interactive card.
- `Danger` identifies a destructive action and uses warning feedback.

`FlourishButton.Variant` describes where the action is hosted and defaults to `Standard`:

- `Icon` is a compact square icon action.
- `Toolbar` is a compact toolbar command.
- `Action` is a dialog or form action.
- `WindowCaption` uses native caption-button geometry.
- `StatusIcon` is a small status-bar icon action.
- `MessageBox` is an action hosted by a Flourish message box.

`Appearance` and `Variant` are independent. For example, a primary dialog action uses both:

```xml
<flourish:FlourishButton
  Appearance="Primary"
  Command="{Binding SaveCommand}"
  Content="Save"
  Variant="Action" />
```

Layout containers control external placement such as `Margin`. Pointer and keyboard focus use distinct states, and keyboard focus remains visible.

`FlourishCard.Appearance` defaults to `Standard`. Other values are `Subtle`, `Accent`, `Elevated`, and `Hero`. `Elevated` adds elevation, while `Hero` uses a gradient background and elevation for introductory content. A card is not an input control; use `FlourishButton Appearance="Card"` when the complete surface must be interactive.

### Text and input

`FlourishTextBlock.Role` defaults to `Body`. Available roles are `Body`, `Paragraph`, `Caption`, `Muted`, `FieldLabel`, `Subtitle`, `Description`, `CardTitle`, `SectionTitle`, `PageTitle`, `Status`, and `Icon`.

`Paragraph` provides wrapped body text with increased line spacing. `Description` provides supporting text below a heading, and `CardTitle` identifies a heading inside a card or compact content surface. Body, supporting, label, status, and icon roles use `Regular`; `CardTitle`, `SectionTitle`, and `PageTitle` use `Bold`. Roles use the active font and theme resources; an explicitly assigned text property takes precedence.

Use `FlourishLabel` when a caption needs an access key or must focus another control through `Target`:

```xml
<flourish:FlourishLabel
  Content="_User name"
  Target="{Binding ElementName=UserNameBox}" />
<flourish:FlourishTextBox
  x:Name="UserNameBox"
  Text="{Binding UserName, UpdateSourceTrigger=PropertyChanged}" />
```

`FlourishSearchBox` adds a search glyph and `Placeholder` while retaining text binding, commands, selection, and `TextChanged`.

`FlourishPasswordBox` intentionally follows WPF password-input semantics:

| Member | Type and default | Use |
| --- | --- | --- |
| `Password` | `string`, empty; not a dependency property | Gets or sets the current password. Because it is not a dependency property, do not use it as a binding target. |
| `SecurePassword` | Read-only `SecureString` | Reads the password without requesting a managed string from the control. |
| `PasswordChar` | `char`, inherited from WPF | Changes the masking character. |
| `MaxLength` | `int`, `0` | Limits input length; `0` means no control-imposed limit. |
| `PasswordChanged` | Bubbling routed event | Responds when the user or code changes the password. |
| `Clear()`, `SelectAll()` | Methods | Clears or selects the complete password. |
| `FocusEditor()` | Method returning `bool` | Applies the template and moves keyboard focus to the inner editor. |

```xml
<flourish:FlourishPasswordBox
  MaxLength="128"
  PasswordChanged="PasswordBox_PasswordChanged" />
<flourish:FlourishSearchBox
  Placeholder="Search reports"
  Text="{Binding Query, UpdateSourceTrigger=PropertyChanged}" />
```

### Selection controls

`FlourishCheckBox` uses the standard `IsChecked` and `IsThreeState` properties. `FlourishRadioButton` uses `IsChecked` and `GroupName`. `FlourishComboBox` keeps selection, editable-text, item-template, and `ItemsSource` behavior and automatically wraps data items in `FlourishComboBoxItem`.

```xml
<StackPanel>
  <flourish:FlourishCheckBox
    Content="Include archived reports"
    IsChecked="{Binding IncludeArchived}" />
  <flourish:FlourishRadioButton
    Content="Summary"
    GroupName="ReportMode"
    IsChecked="True" />
  <flourish:FlourishRadioButton
    Content="Detailed"
    GroupName="ReportMode" />
  <flourish:FlourishComboBox
    ItemsSource="{Binding Formats}"
    SelectedItem="{Binding SelectedFormat}" />
</StackPanel>
```

`FlourishListBox.Appearance` defaults to `Standard`; `Navigation` removes the general-purpose border and enables navigation presentation. `IsCompact` defaults to `false` and affects collapsed navigation geometry. In a navigation list, data items are expected to expose `IsVisible`, `IsGroupHeader`, `IsCommandItem`, `IsEnabled`, and `Label`; Flourish binds those values to generated containers and uses `Label` for the tooltip. Directly supplied `FlourishListBoxItem` instances keep their own local values and bindings.

`FlourishListBoxItem` adds the following properties:

| Property | Default | Effect in a navigation list |
| --- | --- | --- |
| `IsItemVisible` | `true` | Shows or hides the item. |
| `IsGroupHeader` | `false` | Presents the item as a group heading. |
| `IsCommandItem` | `false` | Presents the item as a command rather than a page destination; it does not add an `ICommand`. |

For an ordinary selectable list, only `ItemsSource`, `SelectedItem`, and an optional `ItemTemplate` are needed.

### Scrolling, tooltips, and resizing

`FlourishScrollViewer` retains all WPF scrolling properties and commands. Set `IsCompact="True"` to use compact scroll bars; the default is `false`. `FlourishScrollBar` is normally created by the viewer template, but can also be used directly with standard `Orientation`, `Minimum`, `Maximum`, `Value`, and `ViewportSize` properties.

```xml
<flourish:FlourishScrollViewer
  HorizontalScrollBarVisibility="Disabled"
  IsCompact="True"
  VerticalScrollBarVisibility="Auto">
  <StackPanel />
</flourish:FlourishScrollViewer>
```

`FlourishToolTip` keeps WPF tooltip content, timing, and placement properties. Its default style enables `FlourishToolTipPlacement.IsEnabled`, which chooses a placement that remains inside the Shell and accounts for title bar, toolbar, breadcrumb, navigation, status-bar, and content regions. Set the attached property to `False` on the tooltip to use normal WPF placement.

```xml
<flourish:FlourishButton Content="Refresh">
  <flourish:FlourishButton.ToolTip>
    <flourish:FlourishToolTip Content="Reload the current report" />
  </flourish:FlourishButton.ToolTip>
</flourish:FlourishButton>
```

`FlourishGridSplitter.Variant` defaults to `Standard`. Use `NavigationPane` only for the resize edge of a navigation pane. Standard `ResizeDirection`, `ResizeBehavior`, keyboard movement, and alignment rules still apply.

## Hover reveal and reduced motion

`FlourishButton`, `FlourishComboBoxItem`, and `FlourishListBoxItem` participate in the public `HoverReveal` attached behavior. Configure it application-wide through [Motion](configure-motion.md), including the operating system reduced-motion preference:

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

| Attached property | Default | Inherits | Purpose |
| --- | --- | --- | --- |
| `HoverReveal.IsEnabled` | `true` | Yes | Enables reveal behavior; the effective value is also disabled when `IsMotionEnabled` is `false`. |
| `HoverReveal.AnimationDuration` | 140 ms | Yes | Sets the reveal animation duration. |
| `HoverReveal.IsMotionEnabled` | `true` | No | Supplies the runtime motion policy for one participant. |
| `HoverReveal.IsParticipant` | `false` | No | Opts a custom control template into the behavior. |
| `HoverReveal.TemplateHandlesInteraction` | `false` | No | Declares that a custom template supplies its own static hover and pressed states. |

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
