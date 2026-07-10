---
title: ConfigureProfile
description: Configure the Flourish profile flyout, login state, and replaceable authentication services.
---

# ConfigureProfile

`ConfigureProfile` configures the compact profile page hosted below the title bar. The profile is a fixed-size, non-scrolling container whose content is a WPF `Page`. It appears only when [`ConfigureShell`](configure-shell.md) enables both `UseTitleBar()` and `UseProfile()`.

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseProfile())
    .ConfigureProfile(profile =>
        profile.SetDefaultProfile(
            imagePath: null,
            userName: "Cristian Ronaldo"));
```

`SetDefaultProfile()` uses `User` when called without arguments. If no image can be loaded, Flourish displays initials: `User` becomes `U`, and `Cristian Ronaldo` becomes `CR`. `imagePath` may be a local file path or a pack URI; a missing or invalid image falls back to initials.

## Default page

The built-in page displays the image and user name. While signed out, **Sign in** opens fields for a user name, image, and password. The image button uses the native Windows file picker.

After authentication, the sign-in form is replaced by **Remember login** and **Sign out**. `IProfileService.LoginState` reports one of these states:

| State | Meaning |
| --- | --- |
| `SignedOut` | No active login. |
| `SignedIn` | Active for this application session only. |
| `SignedInRemembered` | Restored from encrypted storage at the next startup. |

An unremembered login remains active for the current session. On the next startup Flourish checks the persisted flag first, removes those credentials, and starts signed out. A remembered login is decrypted and authenticated again before it becomes active.

## Credential storage

The default service writes one profile credential value through the .NET User Secrets storage location. Before writing, Flourish serializes the user name, image path, password, and remember flag and encrypts the complete payload with Windows DPAPI using `DataProtectionScope.CurrentUser`.

User Secrets alone is not an encrypted vault. The DPAPI step is what protects the Flourish payload and binds it to the current Windows user. Signing out removes the stored profile value.

## Replace authentication

The built-in `IProfileAuthService` is intentionally simple: it accepts any non-empty user name and password. Register an application implementation in `ConfigureServices` to replace it while keeping the default profile state and encrypted storage behavior.

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<IProfileAuthService, CompanyProfileAuthService>();
});
```

For complete ownership of authentication, state, and persistence, replace `IProfileService` instead. Flourish registers both defaults only when the application has not already registered those interfaces.

```csharp
services.AddSingleton<IProfileService, CompanyProfileService>();
```

## Host a custom page

The flyout remains the shell-owned container. A custom page can replace only its content and can resolve constructor dependencies from DI.

```csharp
builder
    .ConfigureServices((_, services) =>
        services.AddTransient<AccountProfilePage>())
    .ConfigureProfile(profile =>
        profile.SetProfilePage<AccountProfilePage>());
```

The profile `Frame` disables both horizontal and vertical scrolling, so custom pages should fit the compact surface.

## Related APIs

- [`ConfigureShell`](configure-shell.md) owns the `UseProfile` switch.
- [`ConfigureTitleBar`](configure-title-bar.md) can explicitly hide the title bar profile trigger with `ShowProfile(false)`.
- [`ConfigureServices`](configure-services.md) is where custom profile services and pages are registered.
