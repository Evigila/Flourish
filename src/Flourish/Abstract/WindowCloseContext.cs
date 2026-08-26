using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a shell close request passed to runtime close guards.
/// </summary>
/// <param name="reason">The source of the close request.</param>
/// <param name="services">The application's Flourish service provider.</param>
public sealed class WindowCloseContext(
    WindowCloseRequestReason reason,
    IServiceProvider services
)
{
    /// <summary>
    /// Gets the source of the close request.
    /// </summary>
    public WindowCloseRequestReason Reason { get; } = reason;

    /// <summary>
    /// Gets the application service provider for resolving guard dependencies.
    /// </summary>
    public IServiceProvider Services { get; } = services;
}
