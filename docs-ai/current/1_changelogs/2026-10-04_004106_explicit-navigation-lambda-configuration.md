# Explicit navigation lambda configuration

The user clarified that Gallery should demonstrate the same nested configure-lambda construction style as AddNav rather than registering navigation through foreach.

Program now explicitly chains eight secondary AddSubNav callbacks and all 73 third-level AddSubNav calls. Every label, icon and route is editable directly in its configuration lambda. The existing library callback overload already supports this syntax; its leaf and positional-Boolean compatibility remain intact. Removed the unused Gallery category-icon helper and Models import. Current documentation and the manual checklist describe explicit configuration. This supersedes the catalog-driven registration described in the previous change record without altering that append-only history.

Verification: Gallery builds with zero warnings/errors. Independent read-only review confirmed all 73 unique leaf labels and routes against SampleFor registrations and category mappings, all eight category lambdas, and no navigation foreach or catalog registration in Program. An owned temporary host starts successfully; three representative independent pages return 200, each with one guide and all 73 third-level entries. The host was stopped. git diff --check passed.

Manual check: rebuild/restart Gallery, change one explicit third-level label/icon in Program, and confirm the changed entry while category expansion and page navigation remain coherent. Existing tree visual/keyboard acceptance stays in blazor-manual-tests.md. No Computer Use, dependency change, Colligere edit or Git commit occurred.
