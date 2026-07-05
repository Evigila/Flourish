using System.Diagnostics;
using System.Windows;
using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Gallery;

internal sealed class GalleryCommandParser : ICommandParser
{
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
            case "home.open":
                MessageBox.Show("Hello, World!");
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

    private static void ShowCommandOutput(string text)
    {
        Debug.WriteLine(text);
        MessageBox.Show(text);
    }
}
