namespace ArkheideSystem.Flourish.Navigation;

/// <summary>
/// Stores a navigation key and its opaque parameter in a history stack.
/// </summary>
/// <remarks>
/// The parameter is retained by reference. Navigation history does not inspect, clone,
/// serialize, activate, or otherwise interpret the value.
/// </remarks>
internal sealed record NavigationStackEntry(string NavigationKey, object? Parameter);
