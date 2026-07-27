using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Internal.Validation;

internal static class FlourishMessageOptionValidator
{
    public static IReadOnlyList<FlourishMessageOption> Validate(
        IReadOnlyList<FlourishMessageOption> choices
    )
    {
        ArgumentNullException.ThrowIfNull(choices);

        if (choices.Count == 0)
        {
            throw new ArgumentException(
                "At least one message option must be provided.",
                nameof(choices)
            );
        }

        var normalizedChoices = choices.ToArray();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var defaultCount = 0;
        var cancelCount = 0;
        var primaryCount = 0;

        foreach (var choice in normalizedChoices)
        {
            if (choice is null)
            {
                throw new ArgumentException("Message options cannot contain null.", nameof(choices));
            }

            if (string.IsNullOrWhiteSpace(choice.Id))
            {
                throw new ArgumentException("Message option ids cannot be empty.", nameof(choices));
            }

            if (string.IsNullOrWhiteSpace(choice.Text))
            {
                throw new ArgumentException(
                    "Message option text cannot be empty.",
                    nameof(choices)
                );
            }

            if (!ids.Add(choice.Id))
            {
                throw new ArgumentException(
                    $"Message option ids must be unique. Duplicate id: '{choice.Id}'.",
                    nameof(choices)
                );
            }

            if (choice.IsDefault)
            {
                defaultCount++;
            }

            if (choice.IsCancel)
            {
                cancelCount++;
            }

            if (choice.IsPrimary)
            {
                primaryCount++;
            }
        }

        if (defaultCount > 1)
        {
            throw new ArgumentException(
                "Only one message option can be marked as the default option.",
                nameof(choices)
            );
        }

        if (cancelCount > 1)
        {
            throw new ArgumentException(
                "Only one message option can be marked as the cancel option.",
                nameof(choices)
            );
        }

        if (primaryCount > 1)
        {
            throw new ArgumentException(
                "Only one message option can be marked as the primary option.",
                nameof(choices)
            );
        }

        return normalizedChoices;
    }
}
