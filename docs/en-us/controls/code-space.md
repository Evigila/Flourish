---
title: CodeSpace
description: Use CodeSpace to present exact code text with a fixed code style and a built-in copy action.
---

# CodeSpace

`CodeSpace` presents copyable source or command text, starting as a 72 DIP “View code” surface that expands to reveal text and actions. Use [Document](document.md) for prose and [OutputCard](output-card.md) for runtime history.

## Basic usage

Assign the complete snippet through `Text`. `CodeSpace` is not a content container and does not accept child controls.

```xml
<flourish:Chunk Title="Example">
  <flourish:CodeSpace Text="{Binding ExampleCode}" />
</flourish:Chunk>
```

`Text` is displayed and copied without inserting indentation or changing newline characters. Prefer a binding, resource, or property value that makes the intended whitespace explicit. Long lines remain unwrapped and use the built-in horizontal scrolling behavior.

## Expansion behavior

`IsExpanded` is `false` by default and supports two-way binding. Click the collapsed surface or press Enter or Space to expand it; automation uses ExpandCollapse. `ExpandCommand` and `CollapseCommand` expose the same transition.

When expanded, a collapse button appears immediately to the left of the copy button. It returns the control to the 72 DIP collapsed surface without invoking Copy or reopening the surface. Set `IsExpanded="True"` when a CodeSpace should be open initially, including when it is intended to fill a Presenter presentation region.

`CanCollapse` is `true` by default. When `false`, users and automation cannot collapse the surface, but bindings may still set `IsExpanded`.

## Code presentation

The code presentation uses the Large typography tier, Normal font style, Bold weight, Consolas family, and an adaptive blue foreground. The size follows global or page-level Large changes. `CodeSpace` does not parse a language or color individual tokens; syntax-aware highlighting is outside this contract.

The surface shares Document's transparent background, rounded thin low-contrast border, and padding. `CodeSpace` does not add an outer margin; its parent layout owns spacing between sections so the control can fill a Presenter presentation region without leaving an inset.

## Copy action

The upper-right button invokes `ApplicationCommands.Copy` and copies the complete `Text`, including whitespace and line endings. It is disabled for empty text and briefly confirms success. Do not add another copy button.

## Related content

- [Document](document.md)
- [OutputCard](output-card.md)
- [Chunk](chunk.md)
- [CodeSpace API](xref:ArkheideSystem.Flourish.Controls.CodeSpace)
