---
title: Profile
description: Configure profile identity, sign-in state, remembered credentials, and custom authentication.
---

# Profile

The profile surface provides account access from the title bar. Call `SetProfile` to display the profile trigger and use the built-in profile page.

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetProfile(NameOrder.FirstLast));
```

Calling `SetProfile()` without an argument uses `NameOrder.FirstLast`. Before sign-in, the built-in page displays the localized `Profile.DefaultName` value.

## Names and initials

The built-in sign-in form collects first and last names separately. `NameOrder` controls the input order, `ProfileUser.DisplayName`, and the initials shown when no image is available.

| Value | Display name | Initials |
| --- | --- | --- |
| `NameOrder.FirstLast` | `Foo Bar` | `FB` |
| `NameOrder.LastFirst` | `Bar Foo` | `BF` |

At least one name field must be non-empty. `ProfileUser.FirstName`, `LastName`, `NameOrder`, `DisplayName`, and `Initials` expose the formatted result.

Labels, status text, file-picker filters, and validation messages on the built-in page follow the locale selected through [Application data](configure-data.md). An application-provided profile page manages its own text.

## Interaction behavior

Profile uses a strong [Overlay](../controls/overlay.md), so pointer movement does not dismiss it. Use the profile trigger, click outside the profile card, or press <kbd>Esc</kbd> to close it. Opening the native Windows file picker does not close the profile surface; selecting or cancelling an image returns to the same sign-in form.

The host does not provide a scrolling region. If custom content can exceed the available height, include a `ScrollViewer` or another scrolling region in the custom page.

## Profile images

The built-in form lets the user select or replace an image with the native Windows file picker. Flourish stores the selected absolute path and does not copy the file. If the file cannot be loaded later, the profile displays the configured initials.

## Login state

After authentication, the sign-in form is replaced by remembered-login and sign-out actions. `IProfileService.LoginState` reports the current state.

| State | Meaning |
| --- | --- |
| `SignedOut` | No active login. |
| `SignedIn` | Active for this application session only. |
| `SignedInRemembered` | Active and marked for restoration at the next application startup. |

An unremembered login remains active until the application exits. A remembered login is authenticated again before it becomes active at the next startup.

## Remembered credentials

The default profile service keeps an ordinary login in memory. When remembered login is enabled, Flourish protects the credential for the current Windows user and stores it through the application's User Secrets configuration. Signing out or disabling remembered login removes the stored credential.

Give the application project a stable User Secrets identity:

```xml
<PropertyGroup>
  <UserSecretsId>Foobar.Desktop</UserSecretsId>
</PropertyGroup>
```

Without a User Secrets provider, ordinary sign-in remains available, but enabling remembered login throws `InvalidOperationException`. Do not place profile credentials in `appsettings.json`.

> [!WARNING]
> The default `IProfileAuthService` only requires a non-empty display name and password. Register an application authentication service when credentials must be verified.

## Replace authentication

Register `IProfileAuthService` through [Dependency injection](configure-services.md) to replace authentication while retaining the built-in profile state and remembered-login behavior.

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<IProfileAuthService, FoobarProfileAuthService>();
});
```

Register `IProfileService` when the application owns authentication, state, and persistence.

```csharp
services.AddSingleton<IProfileService, FoobarProfileService>();
```

Flourish supplies its default implementations only when the application has not registered those interfaces.

## Host a custom page

Use `ConfigureProfile` to replace the content hosted by the profile surface. The custom page is resolved from dependency injection; the title bar still requires `SetProfile` to display the trigger.

```csharp
builder
    .ConfigureServices((_, services) =>
        services.AddTransient<FoobarProfilePage>())
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetProfile(NameOrder.FirstLast))
    .ConfigureProfile(profile =>
        profile.SetProfilePage<FoobarProfilePage>());
```

When `ConfigureProfile` is omitted, `SetProfile` uses the built-in page.

## Related features

- [Shell configuration](shell-configuration.md) enables the title bar.
- [Title bar](configure-title-bar.md) displays the profile trigger and selects name order.
- [Dependency injection](configure-services.md) registers custom profile services and pages.
