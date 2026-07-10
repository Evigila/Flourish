---
title: Profile
description: Configure profile identity, sign-in state, remembered credentials, and custom authentication.
---

# Profile

The profile surface provides compact account access from the title bar. It can show a default identity, collect sign-in information, remember a login for later sessions, or host an application-provided page.

Enable both the title bar and profile features before configuring the profile.

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseProfile())
    .ConfigureProfile(profile =>
        profile
            .SetNameOrder(NameOrder.FirstLast)
            .SetDefaultProfile(
                imagePath: null,
                userName: "Foo Bar"));
```

## Names and initials

The built-in sign-in form collects first and last names separately. `SetNameOrder` controls their visual order, the resulting `ProfileUser.DisplayName`, and the initials shown when no image is available.

| Value | Display name | Initials |
| --- | --- | --- |
| `NameOrder.FirstLast` | `Foo Bar` | `FB` |
| `NameOrder.LastFirst` | `Bar Foo` | `BF` |

`SetDefaultProfile()` uses `User` when no arguments are provided. A combined `userName` is split using the name order active when `SetDefaultProfile` is called, so call `SetNameOrder` first when configuring both values.

At least one name field must be non-empty. `ProfileUser.FirstName`, `LastName`, `NameOrder`, `DisplayName`, and `Initials` expose the formatted result. `ProfileUser.UserName` returns the same formatted value as `DisplayName`.

## Interaction behavior

The profile surface does not depend on window focus, so a native Windows file picker does not dismiss it. Use the profile trigger again, click outside the card, or press <kbd>Esc</kbd> to close it; selecting or cancelling an image returns to the same sign-in form.

The surface adapts to the available shell area. A custom page should fit compact content because the host does not add a scrolling region.

## Profile images

The built-in form lets the user select or replace an image with the native Windows file picker. Flourish stores the selected absolute path and does not copy the file. If the file is later moved or deleted, the profile falls back to the configured initials.

## Login state

After authentication, the sign-in form is replaced by remembered-login and sign-out actions. `IProfileService.LoginState` reports the active state.

| State | Meaning |
| --- | --- |
| `SignedOut` | No active login. |
| `SignedIn` | Active for this application session only. |
| `SignedInRemembered` | Active and marked for restoration at the next application startup. |

An unremembered login remains active until the application exits. A remembered login is restored from protected storage and authenticated again before becoming active.

## Credential persistence and security

The default service protects remembered credentials with Windows DPAPI using `DataProtectionScope.CurrentUser` before writing them to application-scoped storage. The storage scope uses the identity described in [Application data](configure-data.md). Signing out removes the remembered credential.

> [!WARNING]
> The default `IProfileAuthService` validates only that the display name and password are non-empty. Applications that require identity verification must register their own authentication service.

## Replace authentication

Register `IProfileAuthService` through [Dependency injection](configure-services.md) to replace authentication while retaining the default profile state and protected persistence.

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<IProfileAuthService, FoobarProfileAuthService>();
});
```

Register `IProfileService` instead when the application owns authentication, state, and persistence.

```csharp
services.AddSingleton<IProfileService, FoobarProfileService>();
```

Flourish supplies its default implementations only when the application has not registered those interfaces.

## Host a custom page

Use a custom page when the application needs different profile content. The shell continues to own placement and dismissal while the page supplies the content and resolves constructor dependencies from dependency injection.

```csharp
builder
    .ConfigureServices((_, services) =>
        services.AddTransient<FoobarProfilePage>())
    .ConfigureProfile(profile =>
        profile.SetProfilePage<FoobarProfilePage>());
```

## Related features

- [Shell configuration](shell-configuration.md) enables the title bar and profile surfaces.
- [Title bar](configure-title-bar.md) controls whether the profile trigger is visible.
- [Dependency injection](configure-services.md) registers custom profile services and pages.
