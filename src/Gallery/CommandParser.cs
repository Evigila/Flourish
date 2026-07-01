using System.Diagnostics;
using System.Windows;
using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Gallery;

internal sealed class CommandParser : ICommandParser
{
    public bool TryParse(string commandKey)
    {
        switch (commandKey)
        {
            case "home.open":
                MessageBox.Show("Hello, World!");
                return true;
            case "home.save":
            case "gallery.open":
            case "gallery.save":
            case "gallery.import":
                Debug.WriteLine($"Gallery command executed: {commandKey}");
                return true;

            default:
                return false;
        }
    }
}
