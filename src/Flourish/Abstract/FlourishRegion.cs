namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies a Flourish shell region that can host application-provided WPF content.
/// </summary>
/// <remarks>
/// Region content is configured during application composition with
/// <see cref="IFlourishBuilder.ConfigureCustomHandler(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishCustomHandlerBuilder})" />.
/// </remarks>
public enum FlourishRegion
{
    /// <summary>
    /// A title bar area after breadcrumb and navigation controls, before the built-in brand block.
    /// </summary>
    TitlebarStart,

    /// <summary>
    /// The center area of the title bar.
    /// </summary>
    TitlebarCenter,

    /// <summary>
    /// A title bar area after built-in profile or theme controls and before the window caption buttons.
    /// </summary>
    TitlebarEnd,

    /// <summary>
    /// The title bar profile area. Custom content replaces the built-in profile content.
    /// </summary>
    TitlebarProfile,

    /// <summary>
    /// The top area of the navigation panel, above scrollable navigation items.
    /// </summary>
    NavigationHeader,

    /// <summary>
    /// The bottom area of the navigation panel, below fixed navigation items.
    /// </summary>
    NavigationFooter,

    /// <summary>
    /// The top area of the content surface, above toolbar and breadcrumb surfaces.
    /// </summary>
    ContentHeader,

    /// <summary>
    /// The bottom area of the content surface, below the current page.
    /// </summary>
    ContentFooter,

    /// <summary>
    /// An overlay area displayed above the current page content.
    /// </summary>
    ContentOverlay,

    /// <summary>
    /// The start of the dynamic toolbar surface, before built-in toolbar items.
    /// </summary>
    ToolbarStart,

    /// <summary>
    /// The end of the dynamic toolbar surface, after built-in toolbar items.
    /// </summary>
    ToolbarEnd,

    /// <summary>
    /// The start of the footer, after active background-task indicators.
    /// </summary>
    FooterStart,

    /// <summary>
    /// The end of the footer, after custom and built-in status indicators.
    /// </summary>
    FooterEnd,

    /// <summary>
    /// The application-provided body displayed below identity metadata in the logo information surface.
    /// </summary>
    TitlebarApplicationInfo,
}
