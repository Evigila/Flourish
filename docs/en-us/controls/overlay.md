---
title: Overlay
description: Present temporary hover details or strong floating content with a shared themed surface and explicit dismissal semantics.
---

# Overlay

`Overlay` is a themed content surface for floating information anchored to another element. Its `Variant` communicates whether pointer movement dismisses the surface or whether the host must keep it open until a deliberate close action.

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

The ActionCard pattern is a recommendation, not an Overlay restriction. Use a custom `Grid`, panel, or purpose-built view when the floating surface needs a different composition. For example, a profile view may arrange identity information, account actions, and status content without placing them in an ActionCard. The host still owns positioning, open state, and dismissal.

## Shell integration

Flourish Shell features host their Overlays in a window-bounded layer instead of an application Popup. The Shell calculates the anchored position, changes the host visibility when a feature is invoked, and handles `DismissRequested`, outside clicks, and <kbd>Esc</kbd>. Code that adds a Shell feature therefore invokes the feature's Shell integration point; it does not ask `Overlay` to open itself.

Use an interactive control such as [Button](button.md) or `CardButton` as the trigger. These controls provide click or command activation, keyboard focus, and automation semantics. `Card` and `ActionCard` are presentation surfaces; do not attach pointer handlers to them to imitate a trigger.

## Tooltip integration

When `UseTips` is active, Flourish controls present their own hints with a `FlourishToolTip` template containing one `Temporary` Overlay. WPF `ToolTipService` continues to own opening, delay, popup placement, and closure, so the nested Overlay does not set `PlacementTarget`.

When `UseTips` is omitted or the `ToolTips` feature is disabled at runtime, Flourish controls present the same hint content with the native WPF tooltip appearance and default behavior. Tooltips attached to native WPF controls and tooltips owned by third-party controls always keep their own templates and default behavior; Flourish does not globally re-template them.

## Related controls

- [Card](card.md) is an in-layout information surface rather than floating content.
- [ActionCard](card.md#actioncard) supplies the standard vertical floating-card composition.
- [Button](button.md) provides common Overlay triggers.
- [ScrollViewer](scroll-viewer.md) contains content that can exceed the available Overlay height.
- The [Overlay API](xref:ArkheideSystem.Flourish.Controls.Overlay) and [OverlayVariant API](xref:ArkheideSystem.Flourish.Controls.OverlayVariant) list the complete member signatures.
