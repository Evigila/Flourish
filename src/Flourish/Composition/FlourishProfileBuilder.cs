using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Composition;

internal sealed class FlourishProfileBuilder(FlourishProfileOptions options)
    : IFlourishProfileBuilder
{
    public IFlourishProfileBuilder SetDefaultProfile(
        string? imagePath = null,
        string userName = "User"
    )
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name cannot be empty.", nameof(userName));
        }

        var name = ProfileUser.ParseDisplayName(userName, options.NameOrder);
        options.DefaultFirstName = name.FirstName;
        options.DefaultLastName = name.LastName;
        options.DefaultImagePath = string.IsNullOrWhiteSpace(imagePath)
            ? null
            : imagePath.Trim();
        return this;
    }

    public IFlourishProfileBuilder SetNameOrder(NameOrder nameOrder)
    {
        if (!Enum.IsDefined(nameOrder))
        {
            throw new ArgumentOutOfRangeException(nameof(nameOrder), nameOrder, null);
        }

        options.NameOrder = nameOrder;
        return this;
    }

    public IFlourishProfileBuilder SetProfilePage<TPage>()
        where TPage : Page
    {
        return SetProfilePage(typeof(TPage));
    }

    public IFlourishProfileBuilder SetProfilePage(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException(
                $"Profile page type {pageType.FullName} must derive from {typeof(Page).FullName}.",
                nameof(pageType)
            );
        }

        options.PageType = pageType;
        return this;
    }
}
