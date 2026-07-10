---
title: ConfigureProfile
description: Configure the compact Flourish profile surface, name order, authentication, and encrypted persistence.
---

# ConfigureProfile

`ConfigureProfile` configures the compact profile card opened from the title bar. It appears only when [`ConfigureShell`](configure-shell.md) enables both `UseTitleBar()` and `UseProfile()`.

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseProfile())
    .ConfigureProfile(profile =>
        profile
            .SetNameOrder(NameOrder.FirstLast)
            .SetDefaultProfile(
                imagePath: null,
                userName: "Cristian Ronaldo"));
```

`SetDefaultProfile()` uses `User` when called without arguments. Its `userName` parameter is retained for source compatibility and is split according to the name order that is active when the method is called. When chaining both methods, call `SetNameOrder()` first.

## Name order and initials

The built-in sign-in form has separate **First Name** and **Last Name** fields. `SetNameOrder()` controls their visual order as well as `ProfileUser.DisplayName` and the initials shown when no image is available:

| Value | Display name | Initials |
| --- | --- | --- |
| `NameOrder.FirstLast` | `Cristian Ronaldo` | `CR` |
| `NameOrder.LastFirst` | `Ronaldo Cristian` | `RC` |

At least one name field must be non-empty. `ProfileUser.FirstName`, `LastName`, `NameOrder`, and `DisplayName` expose the structured result. `ProfileUser.UserName` remains as a compatibility alias for `DisplayName`.

The original `ProfileUser(string userName, ...)` and `ProfileSignInRequest(string userName, ...)` constructors also remain available and interpret their combined name using `FirstLast`. New code that needs explicit ordering should use the overloads with separate first and last names. Version 1 stored credentials containing only `UserName` are read with the configured name order and upgraded to the structured format after a remembered login is restored.

## Compact in-window surface

The profile card is a Shell-owned in-window overlay rather than a separate WPF `Popup`. Its normal width is 304 pixels, its height follows its content, and it can shrink to the available window width and height. Flourish centers it immediately below the profile button and clamps it inside the Shell with a 5-pixel safe margin on every edge.

The overlay stays open while the native Windows file picker owns focus, so selecting or cancelling an image returns to the same sign-in form. Clicking outside the card or pressing <kbd>Esc</kbd> closes it. The hosted `Frame` disables horizontal and vertical scrolling; custom pages should therefore fit the compact, adaptive surface.

The built-in form uses the same Flourish text box, password box, check box, and action button styles as the rest of the Shell.

## Upload an image

The built-in form presents one full-width **Upload image** button. Clicking it opens the native Windows file picker. After a valid image is chosen, the same button displays an image preview and **Image selected**; clicking it again can replace the selection. If no image is selected, the button continues to show **Upload image**.

Flourish does not copy an uploaded image. The file remains at the absolute path returned by the Windows picker. A successful sign-in stores only that path; moving or deleting the source file later makes the UI fall back to the ordered initials.

## Login state

After authentication, the sign-in form is replaced by **Remember login** and **Sign out**. `IProfileService.LoginState` reports one of these states:

| State | Meaning |
| --- | --- |
| `SignedOut` | No active login. |
| `SignedIn` | Active for this application session only. |
| `SignedInRemembered` | Restored from encrypted storage at the next startup. |

An unremembered login remains active for the current session. On the next startup Flourish removes its persisted credentials and starts signed out. A remembered login is decrypted and authenticated again before it becomes active.

## Storage and debugging

After a successful sign-in, the default service serializes the schema version, first name, last name, password, absolute image path, and remember flag. It encrypts the complete payload with Windows DPAPI using `DataProtectionScope.CurrentUser`, then stores the Base64 value under the User Secrets key `Flourish.Profile.Credential`.

On Windows the file is located at:

```text
%APPDATA%\Microsoft\UserSecrets\<secretId>\secrets.json
```

The secret ID is `ArkheideSystem.Flourish.Profile.` followed by the first 24 uppercase hexadecimal characters of the SHA-256 hash of:

```text
<companyName>|<appName>|<entryAssemblyName>
```

With the current Gallery settings, the exact values are:

```text
secretId: ArkheideSystem.Flourish.Profile.7523BCEB80CE0A555E66754B
file: %APPDATA%\Microsoft\UserSecrets\ArkheideSystem.Flourish.Profile.7523BCEB80CE0A555E66754B\secrets.json
key: Flourish.Profile.Credential
image: the original file selected by the user; Flourish creates no copy
```

User Secrets alone is not an encrypted vault. DPAPI protects the Flourish payload and binds it to the current Windows user, so `secrets.json` contains encrypted Base64 rather than readable profile fields. Signing out removes the Profile key and deletes the file when it is otherwise empty. An unremembered credential is likewise removed on the next startup.

## Replace authentication

The built-in `IProfileAuthService` intentionally accepts any request whose display name and password are non-empty. Register an application implementation in `ConfigureServices` to replace authentication while retaining the default profile state and encrypted storage.

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

The Shell continues to own the overlay. A custom page replaces only its content and can resolve constructor dependencies from DI.

```csharp
builder
    .ConfigureServices((_, services) =>
        services.AddTransient<AccountProfilePage>())
    .ConfigureProfile(profile =>
        profile.SetProfilePage<AccountProfilePage>());
```

## Related APIs

- [`ConfigureShell`](configure-shell.md) owns the `UseProfile` switch.
- [`ConfigureTitleBar`](configure-title-bar.md) can explicitly hide the title bar profile trigger with `ShowProfile(false)`.
- [`ConfigureServices`](configure-services.md) is where custom profile services and pages are registered.
