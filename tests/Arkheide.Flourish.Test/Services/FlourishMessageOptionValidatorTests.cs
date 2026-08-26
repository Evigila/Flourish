using System;
using System.Collections.Generic;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Messaging;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FlourishMessageOptionValidatorTests
{
    [Fact]
    public void Validate_WithNullChoices_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            FlourishMessageOptionValidator.Validate(null!)
        );

        Assert.Equal("choices", exception.ParamName);
    }

    [Fact]
    public void Validate_WithNoChoices_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            FlourishMessageOptionValidator.Validate([])
        );

        Assert.Equal("choices", exception.ParamName);
        Assert.Contains("At least one", exception.Message);
    }

    [Fact]
    public void Validate_WithNullChoice_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            FlourishMessageOptionValidator.Validate([null!])
        );

        Assert.Equal("choices", exception.ParamName);
        Assert.Contains("cannot contain null", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingId_ThrowsArgumentException(string? id)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            FlourishMessageOptionValidator.Validate([new FlourishMessageOption(id!, "Continue")])
        );

        Assert.Equal("choices", exception.ParamName);
        Assert.Contains("ids cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingText_ThrowsArgumentException(string? optionText)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            FlourishMessageOptionValidator.Validate([
                new FlourishMessageOption("continue", optionText!),
            ])
        );

        Assert.Equal("choices", exception.ParamName);
        Assert.Contains("text cannot be empty", exception.Message);
    }

    [Fact]
    public void Validate_WithDuplicateIds_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            FlourishMessageOptionValidator.Validate([
                new FlourishMessageOption("continue", "Continue"),
                new FlourishMessageOption("continue", "Proceed"),
            ])
        );

        Assert.Equal("choices", exception.ParamName);
        Assert.Contains("Duplicate id: 'continue'", exception.Message);
    }

    [Fact]
    public void Validate_WithMultipleDefaultChoices_ThrowsArgumentException()
    {
        AssertRoleConflict(
            new FlourishMessageOption("first", "First") { IsDefault = true },
            new FlourishMessageOption("second", "Second") { IsDefault = true },
            "default"
        );
    }

    [Fact]
    public void Validate_WithMultipleCancelChoices_ThrowsArgumentException()
    {
        AssertRoleConflict(
            new FlourishMessageOption("first", "First") { IsCancel = true },
            new FlourishMessageOption("second", "Second") { IsCancel = true },
            "cancel"
        );
    }

    [Fact]
    public void Validate_WithMultiplePrimaryChoices_ThrowsArgumentException()
    {
        AssertRoleConflict(
            new FlourishMessageOption("first", "First") { IsPrimary = true },
            new FlourishMessageOption("second", "Second") { IsPrimary = true },
            "primary"
        );
    }

    [Fact]
    public void Validate_WithValidChoices_ReturnsMaterializedChoicesInOriginalOrder()
    {
        var cancel = new FlourishMessageOption("cancel", "Cancel") { IsCancel = true };
        var later = new FlourishMessageOption("later", "Later");
        var continueOption = new FlourishMessageOption("continue", "Continue")
        {
            IsDefault = true,
            IsPrimary = true,
        };
        var choices = new List<FlourishMessageOption> { cancel, later, continueOption };

        var result = FlourishMessageOptionValidator.Validate(choices);
        choices.Reverse();

        Assert.IsType<FlourishMessageOption[]>(result);
        Assert.Equal(new[] { cancel, later, continueOption }, result);
    }

    private static void AssertRoleConflict(
        FlourishMessageOption first,
        FlourishMessageOption second,
        string role
    )
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            FlourishMessageOptionValidator.Validate([first, second])
        );

        Assert.Equal("choices", exception.ParamName);
        Assert.Contains(
            $"one message option can be marked as the {role} option",
            exception.Message
        );
    }
}
