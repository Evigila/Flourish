---
title: Control library
description: Use explicit Flourish custom controls, hover reveal, and semantic theme resources.
---

# Control library

Flourish includes a control library in addition to its shell-composition APIs. The library gives application pages, shell surfaces, dialogs, and independently hosted WPF windows the same compact geometry, small corner radii, typography, colors, and interaction states.

The visual contract is explicit: use a `Flourish*` custom control when an element should have the Flourish appearance. Loading the theme does not install implicit styles for WPF base types. A plain WPF `<Button>`, `<TextBox>`, or `<ListBox>` therefore keeps its native WPF appearance; `<flourish:FlourishButton>`, `<flourish:FlourishTextBox>`, and `<flourish:FlourishListBox>` opt in to the coordinated Flourish templates.

## Library layout

The project is divided by responsibility:

- `ArkheideSystem.Flourish.Abstract` contains runtime and builder contracts.
- `ArkheideSystem.Flourish.Controls` contains reusable visual controls and their semantic properties.
- `ArkheideSystem.Flourish.Themes` contains the canonical `FlourishThemeResources` entry point and the theme resource graph.
- `Assets` contains built-in resources. `Internal/Composition`, `Internal/Configuration`, and `Internal/Imaging` contain implementation details, while `Services` contains internal runtime services.
- `Views/Windows` and `Views/Page` contain the library's presentation surfaces without adding another public control layer.

Each public visual control keeps its implementation beside its resource dictionary: for example, `Button.xaml` and `Button.xaml.cs` own `FlourishButton`. The other public controls follow the same pairing. They are templated WPF custom controls rather than `UserControl` compositions, so consumers can replace or extend a template without inheriting a fixed visual tree.

The former physical `Styles` layer has been removed. `ArkheideSystem.Flourish.Styles.FlourishStyles` and `ArkheideSystem.Flourish.Controls.FlourishControlResources` remain only as `[Obsolete]` source-compatibility shims; neither is a canonical resource entry point.

## Theme resource graph

`Themes/Generic.xaml` is the only composition root. `FlourishThemeResources` loads it, and applications should not merge its children individually:

- `Themes/Colors/Colors.xaml` is the palette entry point; `Colors.Light.xaml` and `Colors.Dark.xaml` contain the light and dark semantic brushes.
- `Themes/Controls.xaml` collects every public control dictionary directly. It is an internal aggregation detail, not an application-level entry point.
- `Themes/Typography.xaml` defines shared font families, sizes, and typography tokens.
- `Themes/Layout.xaml` contains shell and content layout metrics shared across presentation surfaces.

This single-root graph avoids loading a control dictionary through both a style layer and a theme wrapper, and it gives runtime theme switching one authoritative palette location.

## Load the control resources

The runtime created by `FlourishBuilder` loads the complete Flourish resource graph into `Application.Resources` before it shows the shell. Applications that only create controls after `IFlourish.Show(Application)` or `Run(Application)` therefore do not need another resource declaration.

Merge `FlourishThemeResources` explicitly when controls must work in the WPF designer, before the shell starts, or without a Flourish shell:

```xml
<Application
  x:Class="MyApp.App"
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

`http://schemas.arkheide.system/flourish` is the stable XAML namespace for public controls and theme resources. Prefer it to a `clr-namespace` declaration tied to the current assembly layout. Add `FlourishThemeResources` once at application scope; do not also merge `Themes/Generic.xaml` or any child dictionary by URI.

## Public control coverage

The public control set covers the controls currently exercised by the Shell and Gallery:

| Flourish control | Purpose |
| --- | --- |
| `FlourishButton`, `FlourishCard` | Actions and grouped or interactive surfaces, including semantic appearances. |
| `FlourishTextBlock`, `FlourishLabel` | Semantic text roles and access-key-aware form labels. |
| `FlourishTextBox`, `FlourishPasswordBox`, `FlourishSearchBox` | Text, password, and search input with coordinated field states. |
| `FlourishCheckBox`, `FlourishRadioButton` | Independent and mutually exclusive choices. |
| `FlourishComboBox`, `FlourishComboBoxItem` | Drop-down selection; generated item containers are Flourish controls too. |
| `FlourishListBox`, `FlourishListBoxItem` | List selection and generated item containers, including the navigation foundation. |
| `FlourishScrollViewer`, `FlourishScrollBar` | Coordinated scrolling surfaces and scroll chrome. |
| `FlourishToolTip`, `FlourishGridSplitter` | Themed tips and a compact layout-resize target. |

Control templates may use WPF primitives such as `ToggleButton`, `RepeatButton`, and `Thumb` as private template parts with local keyed resources. Flourish does not register broad implicit styles for those primitives—or for any other native WPF control—so unrelated controls are never restyled as a side effect of loading the theme.

## Use the controls

Reference Flourish controls explicitly in page XAML:

```xml
<StackPanel
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:flourish="http://schemas.arkheide.system/flourish"
  Margin="24">
  <flourish:FlourishTextBlock
    Role="PageTitle"
    Text="Account" />

  <flourish:FlourishTextBlock
    Role="Subtitle"
    Text="Manage the current profile and sign-in state." />

  <flourish:FlourishCard Margin="0,16,0,0">
    <StackPanel>
      <flourish:FlourishTextBlock
        Role="FieldLabel"
        Text="Display name" />
      <flourish:FlourishTextBox Text="Ada Lovelace" />
      <flourish:FlourishSearchBox
        Margin="0,12,0,0"
        Placeholder="Search accounts" />
      <StackPanel
        Margin="0,16,0,0"
        HorizontalAlignment="Right"
        Orientation="Horizontal">
        <flourish:FlourishButton
          Appearance="Subtle"
          Content="Cancel" />
        <flourish:FlourishButton
          Margin="8,0,0,0"
          Appearance="Primary"
          Content="Save" />
      </StackPanel>
    </StackPanel>
  </flourish:FlourishCard>
</StackPanel>
```

### FlourishButton

`FlourishButton.Appearance` expresses intent without exposing a template key:

- `Standard` is the neutral default action.
- `Primary` is the prominent action in a group.
- `Subtle` is a low-emphasis action for quiet surfaces.
- `Card` turns the whole button into an interactive card surface.
- `Danger` identifies a destructive action and uses red interaction feedback.

Prefer one Primary action per local decision area. Layout containers still own external Margin; an Appearance controls the button itself, not its placement.

Pointer activation does not leave a focus outline on the button. Keyboard navigation still displays a theme-aware focus indicator, so focus remains visible without being confused with hover or pressed feedback.

### FlourishTextBlock

`FlourishTextBlock.Role` selects semantic typography. Available roles are `Body`, `Caption`, `Muted`, `FieldLabel`, `Subtitle`, `SectionTitle`, `PageTitle`, `Status`, and `Icon`.

Roles use the active font and theme resources, so prefer them to repeated `FontSize`, `FontWeight`, and `Foreground` values in a page. Explicit properties still take precedence when content has a genuine one-off requirement.

### FlourishCard and FlourishSearchBox

`FlourishCard` groups one content tree on a themed surface. Its appearances are `Standard`, `Subtle`, and `Accent`. Put a panel inside the card when it contains multiple children.

`FlourishSearchBox` is a TextBox with search chrome and a `Placeholder`. It keeps normal TextBox features such as `Text`, binding, commands, selection, and `TextChanged`, while avoiding every page rebuilding a search icon, border, and placeholder layer.

## Hover reveal and reduced motion

Participating Flourish templates use the public `HoverReveal` attached behavior. Configure it application-wide through [Motion](configure-motion.md), so `RespectSystemReducedMotion()` can suppress animation when Windows requests reduced motion:

```csharp
builder.ConfigureMotion(motion =>
    motion
        .EnableHoverRevealAnimation(TimeSpan.FromMilliseconds(140))
        .RespectSystemReducedMotion());
```

The attached properties can provide a local override or support a custom template:

```xml
<flourish:FlourishButton
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  Content="Preview" />
```

A custom participating template supplies elements named `HoverChrome` and `HoverRevealScale`, and opts its control in with `flourish:HoverReveal.IsParticipant="True"`. `IsParticipant` is intentionally not inherited, while `IsEnabled` and `AnimationDuration` are inherited policy values. This lets a container disable or retime reveal without attaching behavior to every visual descendant. If either template part is absent, HoverReveal safely does nothing.

HoverReveal can manage the pointer interaction itself, or a participating template can provide its own static hover and pressed states. Set `flourish:HoverReveal.TemplateHandlesInteraction="True"` only when the template supplies a non-animated hover fallback for disabled reveal motion and its own pressed state. A locally assigned `Template` uses behavior-managed interaction unless `TemplateHandlesInteraction` is also assigned locally. When a custom `Style` replaces the template, set this property explicitly to match the replacement template. Text-entry controls use pointer and focus strokes instead of HoverReveal.

Standard buttons, interactive card buttons, list items, and navigation items use the same vivid, theme-aware blue reveal in both light and dark themes. Selection remains visible beneath that feedback, while pressed feedback uses its own state. `Danger` is the semantic exception: destructive buttons use red hover and pressed feedback instead of the blue reveal.

## Themes and semantic tokens

Control templates consume theme and typography resources through `DynamicResource`. Theme switching therefore updates controls, generated item containers, ComboBox popups, and scrollbars without recreating the page. The main semantic groups are:

- primary and muted text;
- accent and on-accent content;
- control backgrounds and borders;
- hover, pressed, selected, focused, and disabled states;
- card and popup surfaces;
- global text and icon fonts and typography sizes.

Use [Themes](configure-themes.md) and [Typography](configure-font.md) for application-wide changes. If an application overrides an individual brush or typography resource, override the semantic resource rather than copying a ControlTemplate; this keeps all controls and states coordinated. Check both light and dark themes and preserve readable contrast when applying brand colors.

## Why other control families are deferred

The current Shell and Gallery do not use `TreeView`, `DataGrid`, `ListView`, or `GridView`. Flourish therefore does not claim unified templates for them yet. These families require additional contracts for hierarchy, virtualization, editing, sorting, headers, columns, selection, and keyboard navigation. Implementing them without a real product surface would create an untested visual promise.

Standard WPF versions of those controls remain available, but they are outside the current Flourish visual contract. They can be added when an application or Gallery page exercises their complete state model.

## Related features

- [Getting started](getting-started.md) explains runtime startup and resource loading.
- [Themes](configure-themes.md) selects the active light, dark, or system palette.
- [Typography](configure-font.md) changes the global and per-page fonts.
- [Motion](configure-motion.md) configures HoverReveal and reduced-motion behavior.
- [Controls API](xref:ArkheideSystem.Flourish.Controls) lists the public controls, enums, and attached properties.
- [Themes API](xref:ArkheideSystem.Flourish.Themes) lists the canonical resource entry point.
