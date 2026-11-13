# Shared UI and UX integration standards

## Authority and reading order

Respect the affected project's effective design constraints, actual component source, styles, public contracts, and explicit product requirements. Read `SAF.config.json.project.ui.framework` through the shared config resolver for otherwise open design choices; its default is `Material Design 3`. This is design guidance, not automatic installation or adoption of a component library, visual token package, brand, or UI runtime. A configured preference does not override established project constraints or explicit user requirements.

Inspect existing components and host usage, supported configuration/states, interaction/accessibility contracts, and relevant requirements. Distinguish documented requirements from inferred behavior. When required evidence is unavailable, identify the gap and continue independent work.

## Component reuse and host responsibilities

Reuse established components and supported extension points when they meet the requirement. Avoid cloning controls, competing styles/token systems, or duplicating validation, state, interaction, and localization behavior without a concrete reason. Do not migrate or reskin existing components solely to impose the default design preference; dependency adoption still requires the authorization in [rules.md](rules.md#external-dependencies).

Hosts compose interfaces and own the business/backend logic and approved configuration assigned by the actual architecture. Keep behavior, permission, and session changes within scope. Follow the project's separation of concerns rather than assuming a particular framework.

When a capability is missing, explain the gap, available approaches, compatibility, and impact. Implement an approach already covered by the task; obtain direction when it expands scope or needs an unapproved dependency. Record durable project-specific constraints in `docs-ai/current/` rather than turn them into universal SAF rules.

## Verification

Inspect component usage and supported configuration, and run tests appropriate to changed behavior under [rules.md](rules.md#implementation-and-verification). Computer Use testing defaults to deny and follows effective `project.verification.computerUse`, subject to explicit applicable user exceptions and tool permissions. Honor explicit manual/static-only limits, report unavailable checks, and provide manual steps for human acceptance. Do not present unperformed interaction checks as passed or use configuration to skip required testing.
