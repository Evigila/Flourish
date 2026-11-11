# Fix Avatar crop streaming and spacing before Section content

Recorded 2026-11-09 15:06:35 America/Sao_Paulo (UTC-03).

The crop encoder created a JavaScript stream reference before returning to a .NET call expecting IJSStreamReference. Blazor then tried to wrap that reference as if it were the original binary data, and rejected the result. The encoder returns the JPEG Blob directly, allowing Blazor to create the stream reference. The existing 256 KiB output limit and JPEG checks still apply. [ASP.NET Core interop implementation](https://github.com/dotnet/aspnetcore/blob/v10.0.11/src/JSInterop/Microsoft.JSInterop.JS/src/src/Microsoft.JSInterop.ts#L716-L724)

A direct Avatar in a Section now reserves 36px below the portrait when content follows. The earlier selector only matched editable avatars with file inputs. Standalone layouts and last-child avatars keep their spacing.

The ten Node checks passed. The encoding check expects a JPEG Blob and exercises size reduction and cancellation. For manual verification, choose a JPEG, PNG or static WebP, adjust and save, then reload to confirm the photo persisted. Repeat with a read-only portrait above Section fields to verify the gap.
