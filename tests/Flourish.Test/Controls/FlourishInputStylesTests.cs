using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishInputStylesTests
{
    [Fact]
    public void ContentBodyMargin_UsesOneSharedThirtyTwoPixelGutter()
    {
        RunInSta(() =>
        {
            var resources = Assert.IsType<ResourceDictionary>(
                Application.LoadComponent(
                    new Uri(
                        "/Flourish;component/Themes/Layout.xaml",
                        UriKind.Relative
                    )
                )
            );

            Assert.Equal(
                new Thickness(32, 0, 32, 0),
                resources["FlourishContentBodyMargin"]
            );
        });
    }

    [Fact]
    public void ComboBoxItemStyle_UsesLocalAlignmentValues()
    {
        RunInSta(() =>
        {
            var resources = Assert.IsType<ResourceDictionary>(
                Application.LoadComponent(
                    new Uri(
                        "/Flourish;component/Themes/Inputs.xaml",
                        UriKind.Relative
                    )
                )
            );
            var style = Assert.IsType<Style>(resources["FlourishComboBoxItemStyle"]);
            var setters = style.Setters.OfType<Setter>().ToArray();

            Assert.Equal(typeof(ComboBoxItem), style.TargetType);
            Assert.Equal(
                HorizontalAlignment.Left,
                Assert.Single(setters, setter =>
                    setter.Property == Control.HorizontalContentAlignmentProperty
                ).Value
            );
            Assert.Equal(
                VerticalAlignment.Center,
                Assert.Single(setters, setter =>
                    setter.Property == Control.VerticalContentAlignmentProperty
                ).Value
            );
        });
    }

    private static void RunInSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                error = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }
    }
}
