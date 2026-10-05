using System.ComponentModel.DataAnnotations;

namespace ArkheideSystem.Gallery.Flourish.Blazor.Models;

public enum AccessExampleOutcome { Accepted, Rejected, Unavailable }
public enum AccessExampleFieldError { Required, InvalidEmail }
public sealed record AccessExampleAccount(string Id, string Name, string Email, bool Disabled = false);

/// <summary>Deterministic, component-local UI fixtures. Never authenticates, stores credentials or calls a service.</summary>
public sealed class AccessExampleState
{
    private readonly List<AccessExampleAccount> accounts = [];
    private string? pendingEmail;
    private bool pendingRemember;
    private AccessExampleOutcome pendingOutcome;
    private int nextAccountId;

    public AccessExampleState()
    {
        Accounts = accounts.AsReadOnly();
        RestoreAccounts();
    }

    public string? Email { get; set; } = "visitor@example.invalid";
    public string? Password { get; set; }
    public bool RememberEmail { get; set; }
    public AccessExampleOutcome OutcomeChoice { get; set; } = AccessExampleOutcome.Accepted;
    public IReadOnlyList<AccessExampleAccount> Accounts { get; }
    public bool Busy { get; private set; }
    public AccessExampleFieldError? EmailError { get; private set; }
    public AccessExampleFieldError? PasswordError { get; private set; }
    public AccessExampleOutcome? Result { get; private set; }
    public bool SelectionRejected { get; private set; }
    public string? SelectedAccountId { get; private set; }

    public bool BeginLogin()
    {
        if (Busy) return false;
        EmailError = string.IsNullOrWhiteSpace(Email) ? AccessExampleFieldError.Required
            : new EmailAddressAttribute().IsValid(Email.Trim()) ? null : AccessExampleFieldError.InvalidEmail;
        PasswordError = string.IsNullOrWhiteSpace(Password) ? AccessExampleFieldError.Required : null;
        Result = null;
        if (EmailError is not null || PasswordError is not null) return false;
        pendingEmail = Email!.Trim();
        pendingRemember = RememberEmail;
        pendingOutcome = OutcomeChoice;
        Busy = true;
        return true;
    }

    public bool CompleteLogin()
    {
        if (!Busy) return false;
        Busy = false;
        Password = string.Empty;
        Result = pendingOutcome;
        // Only an email fixture is retained in this component instance, never the supplied password.
        if (pendingOutcome == AccessExampleOutcome.Accepted && pendingRemember && pendingEmail is not null
            && !accounts.Any(account => string.Equals(account.Email, pendingEmail, StringComparison.OrdinalIgnoreCase)))
            accounts.Add(new("remembered-" + ++nextAccountId, "Visitor", pendingEmail));
        pendingEmail = null;
        pendingRemember = false;
        return true;
    }

    public void CancelLogin()
    {
        Busy = false;
        pendingEmail = null;
        pendingRemember = false;
        Password = string.Empty;
    }

    public bool SelectAccount(string id)
    {
        if (Busy) return false;
        var account = accounts.Find(candidate => candidate.Id == id);
        SelectionRejected = account is null || account.Disabled;
        if (SelectionRejected) return false;
        SelectedAccountId = account!.Id;
        Email = account.Email;
        Password = string.Empty;
        EmailError = PasswordError = null;
        Result = null;
        return true;
    }

    public bool RemoveAccount(string id)
    {
        if (Busy) return false;
        var removed = accounts.RemoveAll(account => account.Id == id) > 0;
        if (SelectedAccountId == id) SelectedAccountId = null;
        return removed;
    }

    public void ClearAccounts()
    {
        if (Busy) return;
        accounts.Clear();
        SelectedAccountId = null;
        SelectionRejected = false;
    }

    public void RestoreAccounts()
    {
        if (Busy) return;
        accounts.Clear();
        accounts.AddRange([
            new("mira", "Mira", "mira@example.invalid"),
            new("theo", "Theo", "theo@example.invalid"),
            new("archived", "Archived", "archived@example.invalid", true)]);
        SelectedAccountId = null;
        SelectionRejected = false;
    }

    public void ResetLogin()
    {
        if (Busy) return;
        Email = Password = string.Empty;
        RememberEmail = false;
        EmailError = PasswordError = null;
        Result = null;
        SelectedAccountId = null;
        SelectionRejected = false;
    }
}
