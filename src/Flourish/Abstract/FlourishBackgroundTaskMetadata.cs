using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a background task before it is submitted for execution.
/// </summary>
public sealed class FlourishBackgroundTaskMetadata
{
    /// <summary>
    /// Initializes background task metadata.
    /// </summary>
    /// <param name="name">The non-empty task name displayed to the user.</param>
    /// <param name="description">An optional task description.</param>
    /// <param name="iconGlyph">An optional glyph displayed for the task.</param>
    public FlourishBackgroundTaskMetadata(
        string name,
        string? description = null,
        string? iconGlyph = null
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Background task name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
        Description = NormalizeOptionalValue(description);
        IconGlyph = NormalizeOptionalValue(iconGlyph);
    }

    /// <summary>
    /// Gets the task name displayed to the user.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional task description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the optional task icon glyph.
    /// </summary>
    public string? IconGlyph { get; }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
