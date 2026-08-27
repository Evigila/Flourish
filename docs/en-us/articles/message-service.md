---
title: Message service
description: Show Flourish-styled modal messages with standard or custom options.
---

# Message service

Inject `IMessageService` to show modal messages with Flourish window styling.

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

`YesNo` orders negative then affirmative; `YesNoCancel` orders cancel, negative, then affirmative. Labels follow [Application data](configure-data.md), and `Yes` is the default when none is supplied.

## Custom options

Use custom options for results outside `MessageBoxResult`. The method returns the selection, or `null` when dismissed without a cancel option.

```csharp
var selected = messages.Show(
    "The import target already contains matching files.",
    "Import",
    [
        new MessageDialogOption("skip", "Skip") { IsCancel = true },
        new MessageDialogOption("replace", "Replace")
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

Options appear in supplied order, with the last on the right. `IsDefault` handles Enter, `IsCancel` handles Escape and close, and `IsPrimary` applies accent styling. Every option needs unique non-empty `Id` and `Text`.

Message text, captions, and `MessageDialogOption.Text` values are supplied by the application and are not translated automatically.

## Owner window

Both standard and custom overloads have an owner-aware form. Use it when the active window is not the desired dialog owner.

```csharp
var selected = messages.Show(
    owner,
    "Apply changes to every open item?",
    "Apply",
    [
        new MessageDialogOption("current", "Current only") { IsCancel = true },
        new MessageDialogOption("all", "All items")
        {
            IsDefault = true,
            IsPrimary = true,
        },
    ]);
```
