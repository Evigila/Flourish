# Remove Elevated grid button hover outline

Local timestamp: 2026-10-06 12:48:28, America/Sao_Paulo.

## Cause and implementation

UniformGridButton's shared Design hover rule forced the Border color for every appearance. Elevated therefore acquired a visible outline on hover even though its idle border was transparent. The CSS never increased border thickness; the existing transparent 1px border became visible.

The library Design stylesheet now resolves hover border color from the cell appearance. Default and Elevated cells keep a transparent hover border. Filled, Outlined, Danger and FormActions explicitly retain the existing Border color, including child variants overriding their parent grid. This applies to both standalone buttons and inherited grid appearances without a Gallery skin or new component/API.

The existing transparent border reservation stays constant to avoid shifting contents or changing dimensions on hover. Background hover feedback, shadows, pressed-state paint, disabled handling and keyboard focus outlines retain their behavior.

## Verification and manual acceptance

Existing palette/style checks pass 18/18. Gallery Release builds to artifacts/elevated-hover-verification with zero warnings and zero errors. The changed-file whitespace check passes. These checks do not certify physical browser geometry.

Restart Gallery and inspect UniformGridButton in light and dark themes. Hover default/inherited and explicit Elevated cells: no visible border should appear and the cell contents should not shift. Check an explicit Elevated child in a non-Elevated grid and a non-Elevated child in an Elevated grid. Verify the other appearances, disabled buttons, click feedback and Tab focus indicators still behave as expected.

No Computer Use, dependency/API update, package publication, process termination, Git commit or push occurred. Existing uncommitted changes and prior records were preserved.