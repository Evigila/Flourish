# Organize solutions and project names

## Request and changes

Rename every Gallery to Gallery.Flourish.* and every test project to Tests.Flourish.*; group the root solution by platform with root Core, Tests and Solutions. Explain Native, Shared and public API placement.

Rename three Gallery and four test directories/project files, assembly/root/source namespaces and all references. Update WPF XAML/resource identities, linked Gallery model sources, embedded sample namespaces and friend assemblies. Preserve library/package names, all platform mappings and WPF's existing UserSecretsId.

Flourish.slnx retains 14 projects: platform folders contain libraries/Galleries, Core is direct, all tests are in Tests, and platform solution files are in Solutions. All platform solutions reference new paths. The explanatory directory map retains 894 files/132 directories; active current-document paths are synchronized and historical records remain untouched.

Native remains the Framework-only verification host. Shared remains substantive but mixes public metadata and algorithms. Registration namespaces are independent of physical assemblies; WPF currently has a same-assembly Abstract namespace, whereas Blazor has a separate Abstract assembly. The audit recommends future contract concentration and leaves dependency boundaries/registration namespaces unchanged in this task. See [full audit and manual checklist](../blazor-project-boundaries.md).

## Verification

All solution project/file paths resolve. Root and Blazor solution builds have zero warnings/errors. Core 367, WPF 901 and Blazor 102 checks pass, plus 9 palette audits. All 77 Gallery guide routes load their five sections/default columns and renamed embedded samples; Framework/Design CSS loads. Native renders controls with Framework and no Design asset.

External temporary Core/WPF outputs initially broke existing upward-only repository discovery; ignored in-repository artifacts resolved it without editing discovery helpers. WPF's stale two-package assertion was corrected to the exact six existing library packages, preserving its compatibility-artifact checks. No package addition, UI behavior change, Computer Use, Colligere edit, user-process termination or Git commit.

