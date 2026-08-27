---
title: Profile
description: Configure profile identity, sign-in state, remembered credentials, and custom authentication.
---

# Profile

Call `SetProfile` to show account access in the title bar.

```csharp
builder
    .ConfigureTitleBar(titleBar =>
        titleBar
            .SetEnabled()
            .SetProfile(nameOrder: NameOrder.FirstLast));
```

Calling `SetProfile()` without an argument uses `NameOrder.FirstLast`. Before sign-in, the built-in page displays the localized `Profile.DefaultName` value.

## Names and initials

The built-in sign-in form collects first and last names separately. `NameOrder` controls the input order, `ProfileUser.DisplayName`, and the initials shown when no image is available.

| Value | Display name | Initials |
| --- | --- | --- |
| `NameOrder.FirstLast` | `Foo Bar` | `FB` |
| `NameOrder.LastFirst` | `Bar Foo` | `BF` |

At least one name field must be non-empty. `ProfileUser.FirstName`, `LastName`, `NameOrder`, `DisplayName`, and `Initials` expose the formatted result.

`SetProfile` defines the startup order. Change the global order at runtime through
`IProfileService`:

```csharp
await profile.SetNameOrderAsync(NameOrder.LastFirst);
var state = profile.Current;
var currentOrder = state.NameOrder;
```

The service refreshes the profile and raises `Changed` without changing login state, credentials, names, or image path.

The user's last name-order choice takes precedence and later changes are written back by default. Pass `usePersistedPreference: false` when the configured order must always win. This persists only `NameOrder`; credentials remain in User Secrets.

Labels, status text, file-picker filters, and validation messages on the built-in page follow the locale selected through [Application data](configure-data.md). An application-provided profile page manages its own text.

## Interaction behavior

Profile uses a strong [Overlay](../controls/overlay.md). Close it with the trigger, an outside click, or <kbd>Esc</kbd>; the native image picker does not dismiss it.

The host does not provide a scrolling region. If custom content can exceed the available height, include a `ScrollViewer` or another scrolling region in the custom page.

## Profile images

The built-in form lets the user select or replace an image with the native Windows file picker. Flourish stores the selected absolute path and does not copy the file. If the file cannot be loaded later, the profile displays the configured initials.

## Login state

After authentication, the sign-in form is replaced by remembered-login and sign-out actions. `IProfileService.Current.LoginState` reports the current state.

| State | Meaning |
| --- | --- |
| `SignedOut` | No active login. |
| `SignedIn` | Active for this application session only. |
| `SignedInRemembered` | Active and marked for restoration at the next application startup. |

An unremembered login remains active until the application exits. A remembered login is authenticated again before it becomes active at the next startup.

## Remembered credentials

Ordinary login stays in memory. Remembered credentials are protected for the current Windows user and stored through User Secrets; sign-out or disabling remembrance removes them.

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

Use `SetProfilePage` in the title bar configuration to replace the content hosted by the profile surface. The custom page is resolved from dependency injection; call `SetProfile` in the same configuration to display the trigger.

```csharp
builder
    .ConfigureServices((_, services) =>
        services.AddTransient<FoobarProfilePage>())
    .ConfigureTitleBar(titleBar =>
        titleBar
            .SetEnabled()
            .SetProfile(nameOrder: NameOrder.FirstLast)
            .SetProfilePage<FoobarProfilePage>());
```

When `SetProfilePage` is omitted, `SetProfile` uses the built-in page.

## Related features

- [Shell configuration](shell-configuration.md)
- [Title bar](configure-title-bar.md)
- [Dependency injection](configure-services.md)
