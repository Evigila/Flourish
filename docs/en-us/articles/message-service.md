---
title: Message service
description: Show Flourish-styled modal messages with standard or custom options.
---

# Message service

Flourish registers `IMessageService` in the application service provider. Inject it into view models, command parsers, or application services when you need a modal message that follows the Flourish window style.

## Standard messages

The standard overloads mirror WPF `MessageBox` button, icon, option, and result enums.

```csharp
if (messages.Show(
        "Close the current workspace?",
        "Close",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question,
        MessageBoxResult.No) == MessageBoxResult.Yes)
{
    CloseWorkspace();
}
```

`MessageBoxButton.YesNo` is displayed as `No | Yes`, so the affirmative action appears on the right. `MessageBoxButton.YesNoCancel` follows the same rule and is displayed as `Cancel | No | Yes`. When no default result is provided, `Yes` remains the default result for these standard button sets.

## Custom options

Use the custom option overload when the result is not one of the standard `MessageBoxResult` values. The method returns the selected `FlourishMessageOption`, or `null` if the dialog is dismissed and no cancel option was configured.

```csharp
var selected = messages.Show(
    "The import target already contains matching files.",
    "Import",
    [
        new FlourishMessageOption("skip", "Skip") { IsCancel = true },
        new FlourishMessageOption("replace", "Replace")
        {
            IsDefault = true,
            IsPrimary = true,
        },
    ],
    MessageBoxImage.Question);

if (selected?.Id == "replace")
{
    ReplaceFiles();
}
```

Options are displayed in the order provided. The last option appears on the right side of the dialog footer. `IsDefault` controls the Enter key, `IsCancel` controls Escape and the title bar close button, and `IsPrimary` applies the accent button style. Each custom option must have a unique non-empty `Id` and non-empty `Text`.

## Owner window

Both standard and custom overloads have an owner-aware form. Use it when the active window is not the desired dialog owner.

```csharp
var selected = messages.Show(
    owner,
    "Apply changes to every open item?",
    "Apply",
    [
        new FlourishMessageOption("current", "Current only") { IsCancel = true },
        new FlourishMessageOption("all", "All items")
        {
            IsDefault = true,
            IsPrimary = true,
        },
    ]);
```
