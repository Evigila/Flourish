# Local release verification on 2026-10-05

This records the final verification reported by the coordinating implementation task. Flourish 1.1.0 packages are prepared locally. This is not a NuGet.org publication record.

## Final preparation

The full root publish-helper.bat -Mode Prepare completed with exit code 0 after the final source changes.

| Check | Result |
|---|---|
| Core automated tests | 367 passed |
| WPF automated tests | 901 passed |
| WPF Culture bridge tests | 5 passed |
| Blazor framework checks | 133/133 passed |
| Blazor Culture bridge checks | 12 passed |
| Node behavior tests | 37 passed, with the existing mock DOM checks passing |
| CSS bundle verification | 21 checks passed |
| SDK asset verification | 194 checks passed |
| Package verifier | All eight configured 1.1.0 packages passed |

The package IDs, dependency order and commands are maintained in [NuGet release and integration](nuget-release-integration.md). Essential's prior full preparation passed 89 tests, built all five demos and verified all six 1.3.0 packages. Its source was unchanged afterward.

## Final generic framework fixes

- Bound native inputs emit their SSR form names while preserving supplied native attributes.
- Field.ControlId supports a control ID distinct from the field/error container ID; labels and validation descriptions remain associated.
- aria-describedby combines existing descriptions with the field error ID without duplicates.
- Button preserves explicit link role and tabindex semantics, including its disabled link behavior.
- RecordPageHeading now uses the scoped text base and Heading_BackTo for its return label: English Back to {0}, Chinese 返回{0}, Portuguese Voltar para {0}.
- These changes remain generic framework behavior. Application pages, identities, permissions and business vocabulary remain in consuming hosts.

## Published local application assets

A standalone NuGet consumer using Framework without Design was built with dotnet publish. Its 18 tested static resources returned HTTP 200 from the published local application.

Colligere's locally published NuGet consumer returned HTTP 200 for all 23 tested static resources.

Here, published means local dotnet publish output served for HTTP verification. It does not mean that NuGet packages, a production application or a public website were released. This task did not use Computer Use for verification.

## GitHub and NuGet configuration

The coordinating task successfully created Evigila/Flourish's nuget environment. Its returned configuration had protection_rules=[] and branch_policy=null. NUGET_USER remains unconfigured; the requested NuGet profile username is still pending with the user. No API key was requested.

Essential already has the nuget environment and NUGET_USER secret name. Its environment likewise has no configured protection rules or branch policy. Secret values were not retrieved, and the NuGet.org account-side trust policies were not verified.

A complete read-only query of the 14 IDs in both ReleaseSettings files found no public target versions: all six Essential 1.3.0 and eight Flourish 1.1.0 versions remain unpublished. Five existing Essential platform packages have latest version 1.2.0; Culture.Blazor and all eight Flourish IDs returned 404. The required sequence is Essential's six 1.3.0 packages, then Flourish's eight 1.1.0 packages. The pending username, account policy/package scope and separately authorized Publish step remain release prerequisites. Environment creation alone does not establish publishing readiness or add an approval requirement.

## Approval review and Git boundary

Automatic approval review rejected execution of a Publish-mode negative test because that helper can fetch, create tags and push. A read-only PowerShell AST guard inspection was used instead; the Publish helper was not executed for the negative test.

No commit, release tag, push or NuGet publication was performed. The final Prepare and HTTP checks are local verification only. See the release guide for the clean master/origin/master and exact-tag confirmation requirements before any future Publish action.
