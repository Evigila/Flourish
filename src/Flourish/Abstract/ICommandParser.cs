namespace AcksheedSys.Flourish.Abstract;

public interface ICommandParser
{
    bool TryParse(string commandKey);
}
