using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class CommandParserTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_WithMissingCommandKey_ReturnsFalseWithoutCallingParsers(string? commandKey)
    {
        var parser = new StubCommandParser(_ => true);
        var sut = new CommandParser([parser]);

        var handled = sut.Parse(commandKey);

        Assert.False(handled);
        Assert.Empty(parser.ReceivedKeys);
    }

    [Fact]
    public void Parse_WhenFirstParserHandlesCommand_StopsAtFirstHandler()
    {
        var firstParser = new StubCommandParser(_ => true);
        var secondParser = new StubCommandParser(_ => throw new InvalidOperationException());
        var sut = new CommandParser([firstParser, secondParser]);

        var handled = sut.Parse("gallery.open");

        Assert.True(handled);
        Assert.Equal(["gallery.open"], firstParser.ReceivedKeys);
        Assert.Empty(secondParser.ReceivedKeys);
    }

    [Fact]
    public void Parse_WhenNoParserHandlesCommand_CallsEveryParserInOrder()
    {
        var calls = new List<string>();
        var firstParser = new StubCommandParser(key =>
        {
            calls.Add($"first:{key}");
            return false;
        });
        var secondParser = new StubCommandParser(key =>
        {
            calls.Add($"second:{key}");
            return false;
        });
        var sut = new CommandParser([firstParser, secondParser]);

        var handled = sut.Parse("gallery.unknown");

        Assert.False(handled);
        Assert.Equal(
            ["first:gallery.unknown", "second:gallery.unknown"],
            calls
        );
    }

    [Fact]
    public void Constructor_MaterializesParserCollection()
    {
        var initialParser = new StubCommandParser(_ => false);
        var laterParser = new StubCommandParser(_ => true);
        var parsers = new List<ICommandParser> { initialParser };
        var sut = new CommandParser(parsers);
        parsers.Add(laterParser);

        var handled = sut.Parse("gallery.open");

        Assert.False(handled);
        Assert.Equal(["gallery.open"], initialParser.ReceivedKeys);
        Assert.Empty(laterParser.ReceivedKeys);
    }

    private sealed class StubCommandParser(Func<string, bool> parse) : ICommandParser
    {
        public List<string> ReceivedKeys { get; } = [];

        public bool TryParse(string commandKey)
        {
            ReceivedKeys.Add(commandKey);
            return parse(commandKey);
        }
    }
}
