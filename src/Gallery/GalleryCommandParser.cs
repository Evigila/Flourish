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
}
