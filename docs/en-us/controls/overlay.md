---
title: Overlay
description: Present themed floating content with explicit dismissal behavior.
---

# Overlay

`Overlay` is a themed floating surface. Its `Variant` defines pointer-driven or deliberate dismissal.

`Overlay` supplies the surface chrome and lifetime contract; place it in the popup, canvas, or other host that owns positioning and open state.

## Variants

| Variant | Behavior | Typical use |
| --- | --- | --- |
| `Temporary` | When `PlacementTarget` is set, leaving both the target and overlay raises `DismissRequested` after a short transition allowance. | Tooltips, Logo details, and device information. |
| `Strong` | Pointer movement does not request dismissal. The host closes the surface through an outside click, <kbd>Esc</kbd>, its trigger, or another explicit action. | Profile and interactive task views. |

The transition allowance lets the pointer cross the gap between an anchor and its overlay without closing the surface. Moving back over either element cancels the pending request.

```xml
<flourish:Overlay
  x:Name="DetailsOverlay"
  PlacementTarget="{Binding ElementName=DetailsButton}"
  Variant="Temporary"
  DismissRequested="DetailsOverlay_DismissRequested">
  <TextBlock Text="Workspace details" />
</flourish:Overlay>
```

Handle `DismissRequested` by updating the open state owned by the surrounding popup or shell host. A `Strong` overlay does not raise this event in response to pointer movement.

## Host an Overlay in application UI

An application page can place `Overlay` inside a WPF `Popup`. Set both placement targets to the interactive trigger, let the Popup own `IsOpen`, and close it when the Overlay requests dismissal:

```xml
<Button x:Name="DetailsButton" Content="Show details" Click="DetailsButton_Click" />
<Popup
  x:Name="DetailsPopup"
  AllowsTransparency="True"
  Placement="Bottom"
  StaysOpen="True">
  <flourish:Overlay
    x:Name="DetailsOverlay"
    Variant="Temporary"
    DismissRequested="DetailsOverlay_DismissRequested">
    <TextBlock Text="Workspace details" />
  </flourish:Overlay>
</Popup>
```

```csharp
DetailsPopup.PlacementTarget = DetailsButton;
DetailsOverlay.PlacementTarget = DetailsButton;

private void DetailsButton_Click(object sender, RoutedEventArgs e) =>
    DetailsPopup.IsOpen = true;

private void DetailsOverlay_DismissRequested(object sender, RoutedEventArgs e) =>
    DetailsPopup.IsOpen = false;
```

For a `Strong` Overlay, the host must also provide deliberate dismissal, such as an action button, <kbd>Esc</kbd>, and outside-click handling. A Popup can supply outside-click behavior with `StaysOpen="False"`.

## Compose Overlay content

`Overlay` is a content container, so its child may be any one WPF content tree. Use a vertical [ActionCard](card.md#actioncard) for the common floating-card structure: icon first, copy below it, and one action at the bottom.

```xml
<flourish:Overlay Variant="Strong">
  <flourish:ActionCard
    Variant="Vertical"
    Icon="&#xE77B;"
    Title="Profile"
    Content="Manage the active account and preferences.">
    <flourish:Button
      Variant="Filled"
      Command="{Binding OpenProfileCommand}"
      Content="Open profile" />
  </flourish:ActionCard>
</flourish:Overlay>
```

Use a custom layout when `ActionCard` does not fit. The host still owns position, open state, and dismissal.

## Shell integration

Shell Overlays use a window-bounded layer that owns position, visibility, outside clicks, <kbd>Esc</kbd>, and `DismissRequested`. Shell features invoke their integration point rather than opening `Overlay` directly.

Use an interactive control such as [Button](button.md) or `CardButton` as the trigger. These controls provide click or command activation, keyboard focus, and automation semantics. `Card` and `ActionCard` are presentation surfaces; do not attach pointer handlers to them to imitate a trigger.

## Tooltip integration

When Flourish tooltip presentation is enabled through `ConfigureToolTips` or `IToolTipService`, Flourish controls present their own hints with a `ToolTip` template containing one `Temporary` Overlay. WPF `ToolTipService` continues to own opening, delay, popup placement, and closure, so the nested Overlay does not set `PlacementTarget`.

Without `ConfigureToolTips`, or after `IToolTipService.SetEnabled(false)`, Flourish hints use native WPF presentation. Native and third-party tooltips remain unchanged.

## Related controls

- [Card](card.md)
- [ActionCard](card.md#actioncard)
- [Button](button.md)
- [ScrollViewer](scroll-viewer.md)
- [Overlay API](xref:ArkheideSystem.Flourish.Controls.Overlay) and [OverlayVariant API](xref:ArkheideSystem.Flourish.Controls.OverlayVariant)
