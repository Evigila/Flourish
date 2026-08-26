using System;
using System.Linq;
using Xunit;
using ArkheideSystem.Flourish.Abstract;

using System.Reflection;

namespace ArkheideSystem.Flourish.Test.Abstract;

public sealed class CommandKeyApiContractTests
{
    [Fact]
    public void PublicApis_PlaceIconGlyphImmediatelyBeforeCommandKey()
    {
        MethodBase[] members =
        [
            Assert.Single(typeof(FlourishToolbarItem).GetConstructors()),
            GetMethod<ICustomContentBuilder>(
                nameof(ICustomContentBuilder.AddTitleBarAction)
            ),
            GetMethod<ICustomContentBuilder>(
                nameof(ICustomContentBuilder.AddFooterCommand)
            ),
            GetMethod<INavigationBuilder>(
                nameof(INavigationBuilder.AddFixedNavigableItem)
            ),
            GetMethod<INavigationGroupBuilder>(
                nameof(INavigationGroupBuilder.AddNavigableItem)
            ),
        ];

        foreach (var member in members)
        {
            var parameterNames = member
                .GetParameters()
                .Select(parameter => parameter.Name)
                .ToArray();
            var iconIndex = Array.IndexOf(parameterNames, "iconGlyph");
            var commandIndex = Array.IndexOf(parameterNames, "commandKey");

            Assert.True(
                iconIndex >= 0,
                $"{member.DeclaringType?.Name}.{member.Name} has no iconGlyph parameter."
            );
            Assert.Equal(iconIndex + 1, commandIndex);
        }
    }

    private static MethodInfo GetMethod<T>(string methodName)
    {
        return Assert.Single(typeof(T).GetMethods(), method => method.Name == methodName);
    }
}
