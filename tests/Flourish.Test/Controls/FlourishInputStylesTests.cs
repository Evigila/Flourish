using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishInputStylesTests
{
    private const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";

    [Fact]
    public void ContentBodyMargin_UsesOneSharedThirtyTwoPixelGutter()
    {
        RunInSta(() =>
        {
            var resources = LoadResourceDictionary(
                "/Flourish;component/Themes/Layout.xaml"
            );

            Assert.Equal(
                new Thickness(32, 0, 32, 0),
                resources["FlourishContentBodyMargin"]
            );
        });
    }

    [Fact]
    public void ComboBoxItem_DefaultStyleUsesStableAlignmentWithoutAncestorBindings()
    {
        RunInSta(() =>
        {
            var item = new FlourishComboBoxItem { Content = "Choice" };
            var comboBox = new FlourishComboBox { Items = { item } };
            var window = CreateWindow(comboBox);

            try
            {
                window.Show();
                comboBox.IsDropDownOpen = true;
                window.UpdateLayout();

                Assert.Equal(HorizontalAlignment.Left, item.HorizontalContentAlignment);
                Assert.Equal(VerticalAlignment.Center, item.VerticalContentAlignment);
                Assert.False(
                    BindingOperations.IsDataBound(
                        item,
                        Control.HorizontalContentAlignmentProperty
                    )
                );
                Assert.False(
                    BindingOperations.IsDataBound(
                        item,
                        Control.VerticalContentAlignmentProperty
                    )
                );
            }
            finally
            {
                comboBox.IsDropDownOpen = false;
                window.Close();
            }
        });
    }

    private static Window CreateWindow(UIElement content)
    {
        var window = new Window
        {
            Width = 320,
            Height = 240,
            Left = -10000,
            Top = -10000,
            ShowActivated = false,
            ShowInTaskbar = false,
            Content = content,
        };
        window.Resources.MergedDictionaries.Add(
            LoadResourceDictionary(GenericThemeSource)
        );
        return window;
    }

    private static ResourceDictionary LoadResourceDictionary(string source)
    {
        return Assert.IsType<ResourceDictionary>(
            Application.LoadComponent(new Uri(source, UriKind.Relative))
        );
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
