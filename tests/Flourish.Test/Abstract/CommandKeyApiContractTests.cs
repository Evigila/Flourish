using System.Reflection;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Test.Abstract;

public sealed class CommandKeyApiContractTests
{
    [Fact]
    public void PublicApis_PlaceIconGlyphImmediatelyBeforeCommandKey()
    {
        MethodBase[] members =
        [
            Assert.Single(typeof(FlourishToolbarItem).GetConstructors()),
            GetMethod<IFlourishCustomHandlerBuilder>(
                nameof(IFlourishCustomHandlerBuilder.AddTitlebarAction)
            ),
            GetMethod<IFlourishCustomHandlerBuilder>(
                nameof(IFlourishCustomHandlerBuilder.AddFooterCommand)
            ),
            GetMethod<IFlourishNavigationBuilder>(
                nameof(IFlourishNavigationBuilder.AddFixedNavigableItem)
            ),
            GetMethod<IFlourishNavigationGroupBuilder>(
                nameof(IFlourishNavigationGroupBuilder.AddNavigableItem)
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

            Assert.True(iconIndex >= 0, $"{member.DeclaringType?.Name}.{member.Name} has no iconGlyph parameter.");
            Assert.Equal(iconIndex + 1, commandIndex);
        }
    }

    private static MethodInfo GetMethod<T>(string methodName)
    {
        return Assert.Single(
            typeof(T).GetMethods(),
            method => method.Name == methodName
        );
    }
}
