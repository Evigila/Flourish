using ArkheideSystem.Gallery.Flourish.Blazor.Models;

internal static class AccessExampleChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        Add("access example validates empty and malformed fields without entering busy state", () =>
        {
            var state = new AccessExampleState { Email = " ", Password = " " };
            Check(!state.BeginLogin() && !state.Busy && state.Result is null, "Invalid fields must not start a result or transition.");
            Check(state.EmailError == AccessExampleFieldError.Required && state.PasswordError == AccessExampleFieldError.Required,
                "Both required errors must be visible.");
            state.Email = "not-an-email";
            state.Password = "fictitious-only";
            Check(!state.BeginLogin() && state.EmailError == AccessExampleFieldError.InvalidEmail && state.PasswordError is null,
                "Email format and password requirements must be evaluated independently.");
            state.Email = " visitor@example.invalid ";
            Check(state.BeginLogin() && state.EmailError is null && state.PasswordError is null, "Valid fictitious fields should start the UI transition.");
        });
        Add("access example busy state locks submissions and saved-account mutations", () =>
        {
            var state = Ready();
            Check(state.BeginLogin(), "The first transition should start.");
            var originalCount = state.Accounts.Count;
            Check(!state.BeginLogin() && !state.SelectAccount("mira") && !state.RemoveAccount("mira"), "Busy actions must be refused.");
            state.ClearAccounts(); state.RestoreAccounts(); state.ResetLogin();
            Check(state.Busy && state.Accounts.Count == originalCount && state.Password == "fictitious-only", "Busy actions must not reset drafts or accounts.");
            Check(state.CompleteLogin() && !state.Busy, "Completion must unlock the example.");
            Check(!state.CompleteLogin(), "Duplicate completion must have no effect.");
        });
        Add("access example remembers an email snapshot without retaining a password or checking credentials", () =>
        {
            var state = Ready();
            state.RememberEmail = true;
            var originalCount = state.Accounts.Count;
            state.BeginLogin();
            state.Email = "changed@example.invalid";
            state.OutcomeChoice = AccessExampleOutcome.Rejected;
            state.RememberEmail = false;
            state.CompleteLogin();
            Check(state.Result == AccessExampleOutcome.Accepted && state.Password == string.Empty, "The selected result snapshot should complete and clear the password.");
            Check(state.Accounts.Count == originalCount + 1 && state.Accounts[^1].Email == "visitor@example.invalid",
                "Only the email snapshot selected before busy state may be remembered.");
            state.Email = "VISITOR@example.invalid"; state.Password = "another-fictitious-value";
            state.RememberEmail = true; state.OutcomeChoice = AccessExampleOutcome.Accepted;
            state.BeginLogin(); state.CompleteLogin();
            Check(state.Accounts.Count == originalCount + 1, "Remembering the same email must not duplicate a fixture.");
        });
        Add("access example rejection and service-error views do not add saved accounts", () =>
        {
            foreach (var outcome in new[] { AccessExampleOutcome.Rejected, AccessExampleOutcome.Unavailable })
            {
                var state = Ready();
                state.OutcomeChoice = outcome; state.RememberEmail = true;
                var originalCount = state.Accounts.Count;
                Check(state.BeginLogin() && state.CompleteLogin(), "The selected failure view should execute.");
                Check(state.Result == outcome && state.Accounts.Count == originalCount && state.Password == string.Empty && !state.Busy,
                    "Failure views must clear secrets and leave saved fixtures unchanged.");
                state.Password = "retry-fictitious-value";
                Check(state.BeginLogin() && state.Result is null, "Retry must clear the previous result while busy.");
            }
        });
        Add("access example account selection only prefills an available email", () =>
        {
            var state = Ready();
            Check(!state.SelectAccount("archived") && state.SelectionRejected && state.SelectedAccountId is null,
                "Disabled fixtures must not be selectable, even when invoked directly.");
            Check(!state.SelectAccount("missing") && state.SelectionRejected, "A stale selection must fail explicitly.");
            Check(state.SelectAccount("mira") && !state.SelectionRejected && state.Email == "mira@example.invalid"
                && state.Password == string.Empty && state.Result is null && !state.Busy,
                "Selection is prefill only, never a successful login result.");
            Check(state.RemoveAccount("mira") && state.SelectedAccountId is null, "Removing the selected fixture clears its identity.");
        });
        Add("access example clear and restore expose repeatable empty and disabled states", () =>
        {
            var state = new AccessExampleState();
            Check(state.Accounts.Count == 3 && state.Accounts.Count(account => account.Disabled) == 1, "Initial fixtures should expose one disabled account.");
            state.ClearAccounts();
            Check(state.Accounts.Count == 0 && !state.SelectionRejected && state.SelectedAccountId is null, "Clear should expose a clean empty state.");
            state.RestoreAccounts(); state.RestoreAccounts();
            Check(state.Accounts.Count == 3 && state.Accounts.Select(account => account.Id).Distinct().Count() == 3,
                "Restore must be idempotent and preserve stable fixture identities.");
            Check(state.RemoveAccount("archived") && !state.RemoveAccount("archived"), "Unavailable fixtures remain removable; stale removal is inert.");
        });
        Add("access example cancellation clears pending data and rejects delayed completion", () =>
        {
            var state = Ready(); state.RememberEmail = true;
            var originalCount = state.Accounts.Count;
            state.BeginLogin(); state.CancelLogin();
            Check(!state.Busy && state.Password == string.Empty && !state.CompleteLogin() && state.Result is null
                && state.Accounts.Count == originalCount, "Abandoned transitions must not create a late result or account.");
            state.ResetLogin();
            Check(state.Email == string.Empty && state.Password == string.Empty && !state.RememberEmail
                && state.EmailError is null && state.PasswordError is null, "Reset should clear the local draft and validation state.");
        });

        void Add(string name, Action run) => tests.Add((name, () => { run(); return Task.CompletedTask; }));
    }

    private static AccessExampleState Ready() => new() { Email = "visitor@example.invalid", Password = "fictitious-only" };
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
