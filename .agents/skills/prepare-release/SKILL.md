---
name: prepare-release
description: Select a suffixed version, prepare release copy for approval, and verify authorized Git tags or package publication under SAF.
---

# Prepare a release

1. Read the [release rules](../../../docs-ai/common/rules.md#release-versioning-and-approvals), project version declarations, relevant Git tags, and the project's release workflow. Establish the current version from those sources.
2. Ask the user to select patch, minor, or major, with patch as the default. Ask the user to select the alpha, beta, or release channel. Restate and confirm choices already specified for this release. Wait for the answers, then derive the full version and check for collisions.
3. Prepare and verify the local release candidate. Compare the project version, intended tag, package identities, and actual changes. Prepare the public release title and body, or use the user's wording verbatim if provided.
4. Show the version, title, and full body to the user and wait for confirmation before creating a release commit, tag, push, or published release. If the user supplies replacement wording, use it verbatim.
5. After approval, perform only the confirmed release actions. Verify the resulting tag and any distribution or release entry against the approved version and copy. Report any failed or pending publication separately from local preparation.
