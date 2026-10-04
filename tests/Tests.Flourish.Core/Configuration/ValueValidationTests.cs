using System;

using ArkheideSystem.Flourish.Configuration;

using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Configuration;

public sealed class ValueValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotBlank_RejectsMissingValues(string? value)
    {
        var error = Assert.Throws<ArgumentException>(() =>
            ValueValidation.NotBlank(value!, "value")
        );

        Assert.Equal("value", error.ParamName);
    }

    [Fact]
    public void NotBlank_PreservesAcceptedValue()
    {
        Assert.Equal(" value ", ValueValidation.NotBlank(" value ", "value"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void PositiveFinite_RejectsNonPositiveOrNonFiniteValues(double value)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ValueValidation.PositiveFinite(value, "value")
        );

        Assert.Equal("value", error.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonNegativeFinite_RejectsNegativeOrNonFiniteValues(double value)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ValueValidation.NonNegativeFinite(value, "value")
        );

        Assert.Equal("value", error.ParamName);
        ValueValidation.NonNegativeFinite(0, "value");
    }

    [Fact]
    public void Enum_RejectsUndefinedValue()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ValueValidation.Enum((DayOfWeek)int.MaxValue, "value")
        );

        Assert.Equal("value", error.ParamName);
        ValueValidation.Enum(DayOfWeek.Monday, "value");
    }
}
