using System;

using ArkheideSystem.Flourish.Views.Page;

namespace ArkheideSystem.Flourish.Profile;

internal sealed class ProfileViewOptions
{
    public Type PageType { get; set; } = typeof(ProfilePage);
}
