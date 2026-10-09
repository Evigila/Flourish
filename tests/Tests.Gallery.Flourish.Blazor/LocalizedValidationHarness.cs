using System.Linq.Expressions;
using ArkheideSystem.Flourish.Extensions.Culture.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Gallery.Flourish.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace ArkheideSystem.Tests.Gallery.Flourish.Blazor;

/// <summary>Exercises the production field renderers against real annotation errors in a live culture scope.</summary>
internal sealed class LocalizedValidationHarness : LocalizedComponentBase
{
    [Parameter, EditorRequired] public EditContext Context { get; set; } = default!;
    private RecordDraft Draft => (RecordDraft)Context.Model;
    private string FormatMessage(string message) => Localization.TryParse(message, [], out var text) ? text : message;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<EditForm>(0);
        builder.AddAttribute(1, nameof(EditForm.EditContext), Context);
        builder.AddAttribute(2, nameof(EditForm.ChildContent), (RenderFragment<EditContext>)(_ => RenderForm));
        builder.CloseComponent();
    }

    private void RenderForm(RenderTreeBuilder builder)
    {
        builder.OpenComponent<DataAnnotationsValidator>(0);
        builder.CloseComponent();
        builder.OpenComponent<Field>(1);
        builder.AddAttribute(2, nameof(Field.Id), "translation-test-name");
        builder.AddAttribute(3, nameof(Field.Label), Localization.Parse("Key.Record_Name"));
        builder.AddAttribute(4, nameof(Field.For), (Expression<Func<object?>>)(() => Draft.Name));
        builder.AddAttribute(5, nameof(Field.MessageFormatter), (Func<string, string>)FormatMessage);
        builder.AddAttribute(6, nameof(Field.ChildContent), (RenderFragment)RenderInput);
        builder.CloseComponent();
        builder.OpenComponent<ValidationMessages>(7);
        builder.AddAttribute(8, nameof(ValidationMessages.Id), "translation-test-email-errors");
        builder.AddAttribute(9, nameof(ValidationMessages.For), (Expression<Func<object?>>)(() => Draft.Email));
        builder.AddAttribute(10, nameof(ValidationMessages.MessageFormatter), (Func<string, string>)FormatMessage);
        builder.CloseComponent();
    }

    private void RenderInput(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TextBox>(0);
        builder.AddAttribute(1, nameof(TextBox.Id), "translation-test-name");
        builder.AddAttribute(2, nameof(TextBox.Value), Draft.Name);
        builder.AddAttribute(3, nameof(TextBox.ValueExpression), (Expression<Func<string>>)(() => Draft.Name));
        builder.CloseComponent();
    }
}
