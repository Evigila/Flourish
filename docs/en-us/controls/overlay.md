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

## Tooltip integration

`FlourishToolTip` uses a `Temporary` Overlay for the shared surface appearance. WPF `ToolTipService` continues to own tooltip opening, delay, popup placement, and closure, so the nested Overlay does not set `PlacementTarget`.

## Related controls

- [Card](card.md) is an in-layout information surface rather than floating content.
- [Button](button.md) provides common Overlay triggers.
- [ScrollViewer](scroll-viewer.md) contains content that can exceed the available Overlay height.
- The [Overlay API](xref:ArkheideSystem.Flourish.Controls.Overlay) and [OverlayVariant API](xref:ArkheideSystem.Flourish.Controls.OverlayVariant) list the complete member signatures.
