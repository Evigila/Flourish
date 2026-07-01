using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Services;

internal sealed class CommandParser(IEnumerable<ICommandParser> parsers)
{
    private readonly IReadOnlyList<ICommandParser> parsers = parsers.ToArray();

    public bool Parse(string? commandKey)
    {
        if (string.IsNullOrWhiteSpace(commandKey))
        {
            return false;
        }

        foreach (var parser in parsers)
        {
            if (parser.TryParse(commandKey))
            {
                return true;
            }
        }

        return false;
    }
}
