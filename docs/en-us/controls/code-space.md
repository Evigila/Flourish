---
title: CodeSpace
description: Use CodeSpace to present exact code text with a fixed code style and a built-in copy action.
---

# CodeSpace

`CodeSpace` presents an exact text snippet in a transparent, rounded, lightly outlined surface. It starts as a compact 72 DIP “View code” surface and reveals its code and actions when expanded. Use it for source code or command text that readers may copy. Use [Document](document.md) for ordinary multi-paragraph prose and [OutputCard](output-card.md) for runtime output or log history.

## Basic usage

Assign the complete snippet through `Text`. `CodeSpace` is not a content container and does not accept child controls.

```xml
<flourish:Chunk Title="Example">
  <flourish:CodeSpace Text="{Binding ExampleCode}" />
</flourish:Chunk>
```

`Text` is displayed and copied without inserting indentation or changing newline characters. Prefer a binding, resource, or property value that makes the intended whitespace explicit. Long lines remain unwrapped and use the built-in horizontal scrolling behavior.

## Expansion behavior

`IsExpanded` is `false` by default and supports two-way binding. Clicking anywhere on the collapsed surface, or pressing Enter or Space while the surface has keyboard focus, expands the complete code presentation. Automation clients receive the standard ExpandCollapse pattern. `ExpandCommand` and `CollapseCommand` expose the same state transition for application commands and custom templates.

When expanded, a collapse button appears immediately to the left of the copy button. It returns the control to the 72 DIP collapsed surface without invoking Copy or reopening the surface. Set `IsExpanded="True"` when a CodeSpace should be open initially, including when it is intended to fill a Presenter presentation region.

`CanCollapse` is `true` by default. Set it to `false` for code that must remain open: the collapse button is removed and CollapseCommand, keyboard input, and UI Automation cannot collapse the surface. `CanCollapse` governs user interaction only; an external binding or local assignment remains authoritative and may still set `IsExpanded` to either state. Gallery Usage presentations use `IsExpanded="True"` with `CanCollapse="False"` so their guidance remains visible.

## Code presentation

The code presentation uses the Large typography tier, Normal font style, Bold weight, Consolas family, and an adaptive blue foreground. The size follows global or page-level Large changes. `CodeSpace` does not parse a language or color individual tokens; syntax-aware highlighting is outside this contract.

The surface shares Document's transparent background, rounded thin low-contrast border, and padding. `CodeSpace` does not add an outer margin; its parent layout owns spacing between sections so the control can fill a Presenter presentation region without leaving an inset.

## Copy action

The expanded surface's upper-right icon button invokes `ApplicationCommands.Copy` for the `CodeSpace`. It uses the Elevated variant so the action remains distinct above the blue code text. It copies the complete `Text` value, including leading spaces and line endings, to the system clipboard. After a successful copy, its 16 DIP icon briefly changes to a check and then restores the copy glyph. The command is disabled when `Text` is empty. Its tooltip uses the shared Tip typography with Normal style and Regular weight rather than inheriting the Bold code presentation. Do not add a second copy button around the control.

## Related content

- [Document](document.md) presents several Paragraph elements with automatic spacing and first-line indentation.
- [OutputCard](output-card.md) presents append-only output and logs in a scrolling viewport.
- [Chunk](chunk.md) defines the section that contains CodeSpace.
- The [CodeSpace API](xref:ArkheideSystem.Flourish.Controls.CodeSpace) lists all inherited and declared members.
