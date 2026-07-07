using System.Windows;
using AckSS.Flourish.Abstract;

namespace AckSS.Gallery;

internal sealed class GalleryCommandParser(IMessageService messages) : ICommandParser
{
    private readonly IMessageService messages = messages;

    public bool TryParse(string commandKey)
    {
        switch (commandKey)
        {
            case "demo.hello":
                ShowCommandOutput("Hello");
                return true;
            case "demo.world":
                ShowCommandOutput("World");
                return true;
            case "tree.button1":
                ShowCommandOutput("Button1");
                return true;
            case "tree.button2":
                ShowCommandOutput("Button2");
                return true;
            case "app.about":
                ShowCommandOutput("关于");
                return true;
            case "titlebar.trace":
                ShowCommandOutput("Titlebar command invoked");
                return true;
            case "footer.trace":
                ShowCommandOutput("Footer command invoked");
                return true;
            case "home.open":
                messages.Show(
                    "Hello, World!",
                    "Gallery",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return true;
            case "home.save":
                return true;
            case "gallery.open":
                return true;
            case "gallery.save":
                return true;
            case "gallery.import":
                return true;

            default:
                return false;
        }
    }

    private void ShowCommandOutput(string text)
    {
        messages.Show(text, "Gallery", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
