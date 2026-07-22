---
title: CodeSpace
description: Use CodeSpace to present exact code text with a fixed code style and a built-in copy action.
---

# CodeSpace

`CodeSpace` presents an exact text snippet in a transparent, rounded, lightly outlined surface and provides a copy button in its upper-right corner. Use it for source code or command text that readers may copy. Use [Document](document.md) for ordinary multi-paragraph prose and [OutputCard](output-card.md) for runtime output or log history.

## Basic usage

Assign the complete snippet through `Text`. `CodeSpace` is not a content container and does not accept child controls.

```xml
<flourish:Chunk Title="Example">
  <flourish:CodeSpace Text="{Binding ExampleCode}" />
</flourish:Chunk>
```

`Text` is displayed and copied without inserting indentation or changing newline characters. Prefer a binding, resource, or property value that makes the intended whitespace explicit. Long lines remain unwrapped and use the built-in horizontal scrolling behavior.

## Code presentation

The code presentation uses the Large typography tier, Normal font style, Bold weight, Consolas family, and an adaptive blue foreground. The size follows global or page-level Large changes. `CodeSpace` does not parse a language or color individual tokens; syntax-aware highlighting is outside this contract.

The surface shares Document's transparent background, rounded thin low-contrast border, padding, and small outer top margin. Preserve the outer margin when the control follows a Chunk title or optional content.

## Copy action

The upper-right icon button invokes `ApplicationCommands.Copy` for the `CodeSpace`. It copies the complete `Text` value, including leading spaces and line endings, to the system clipboard. The command is disabled when `Text` is empty. Its tooltip uses the shared Tip typography with Normal style and Regular weight rather than inheriting the Bold code presentation. Do not add a second copy button around the control.

## Related content

- [Document](document.md) presents several Paragraph elements with automatic spacing and first-line indentation.
- [OutputCard](output-card.md) presents append-only output and logs in a scrolling viewport.
- [Chunk](chunk.md) defines the section that contains CodeSpace.
- The [CodeSpace API](xref:ArkheideSystem.Flourish.Controls.CodeSpace) lists all inherited and declared members.
