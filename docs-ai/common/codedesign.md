# Shared code standards

## Existing conventions

Use the affected repository's architecture, language conventions, formatter/compiler settings, dependency management, and established patterns. Read affected source before choosing an implementation.

Keep responsibilities clear and changes within the task. Prefer existing abstractions and direct implementations. Introduce an abstraction, pattern, or broad refactoring only for a concrete requirement.

Name types, members, and files by their responsibilities. Follow the project's naming and namespace conventions; SAF imposes no organization prefix, product name, language, package ecosystem, or runtime dependency. Apply configured protected paths and the fixed testing/commit requirements under [rules.md](rules.md). Configured commit styles affect authorized new commit messages, not public identifiers, task scope, or existing history.

Comments explain intent, constraints, and non-obvious behavior. Keep them accurate. Task history, approval narratives, and change ledgers belong in project records, not source comments.

## Names and public identities

Keep solution/project names, file basenames, namespaces, assembly identities, package IDs, and published API identifiers distinct. Renaming one does not authorize changing the others.

For a requested rename, inspect references, consumers, serialization/configuration, build/publication identity, and migration implications. Preserve third-party ownership and naming. Report conflicting project standards rather than silently renaming existing code to match a new preference.

## Behavior and compatibility

Preserve established identifiers, public contracts, access control, domain behavior, and session behavior unless modification is authorized. A UI request does not authorize unrelated business-rule changes.

Assess affected callers and persistence before compatibility-sensitive changes. Follow existing async, cancellation, resource-lifetime, and error-handling conventions. Keep user-facing behavior consistent with the product contract and report conflicts rather than inventing a replacement contract.

## Supporting rules

Apply [rules.md](rules.md) for dependency authorization, verification, records, and delivery. For UI work, apply [UIUX.md](UIUX.md) and the project's actual design system and components.
