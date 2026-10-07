@{
    VersionProps = 'Directory.Build.props'
    Solution = 'src/Flourish.Blazor/Flourish.Blazor.slnx'
    DemoSolution = ''
    ConsoleTests = @('tests/Tests.Flourish.Blazor', 'tests/Tests.Flourish.Extensions.Culture.Blazor', 'tests/Tests.Gallery.Flourish.Blazor')
    JavaScriptTests = @('tests/Tests.Flourish.Blazor/tests.js', 'tests/Tests.Flourish.Blazor/measurement-lifecycle.test.mjs', 'tests/Tests.Flourish.Blazor/palette-checks.mjs', 'tests/Tests.Flourish.Blazor/controls-dom.mjs', 'tests/Tests.Flourish.Blazor/split-menu-checks.mjs', 'tests/Tests.Flourish.Blazor/clipboard-dom.mjs', 'tests/Tests.Flourish.Blazor/heading-dom.mjs', 'tests/Tests.Flourish.Blazor/interaction-origin-dom.mjs', 'tests/Tests.Flourish.Blazor/action-menu-dom.mjs', 'tests/Tests.Flourish.Blazor/section-navigator-dom.mjs', 'tests/Tests.Flourish.Blazor/presentation-offers-dom.mjs', 'tests/Tests.Flourish.Blazor/surface-form-layout.test.mjs', 'tests/Tests.Flourish.Blazor/page-body-actions.test.mjs', 'tests/Tests.Flourish.Blazor/organization-access-presentation.test.mjs', 'tests/Tests.Flourish.Blazor/inline-toolbar-layout.test.mjs', 'tests/Tests.Flourish.Blazor/access-form-spacing-dom.mjs')
    CheckScripts = @('build/Test-CssBundle.ps1', 'build/Test-CssAssets.ps1', 'scripts/Test-StartProject.ps1', 'build/Test-CultureCatalogs.ps1')
    PackagePrefix = 'Arkheide.Flourish.'
    EssentialVersion = '1.3.0'
    Packages = @(
        @{ Id = 'Arkheide.Flourish.Core'; Project = 'src/Flourish.Core/Flourish.Core.csproj'; Dependencies = @(); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Blazor.Abstract'; Project = 'src/Flourish.Blazor/Flourish.Blazor.Abstract/Flourish.Blazor.Abstract.csproj'; Dependencies = @('Arkheide.Flourish.Core'); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Extensions.Culture.Blazor'; Project = 'src/Flourish.Extensions/Flourish.Extensions.Culture.Blazor/Flourish.Extensions.Culture.Blazor.csproj'; Dependencies = @('Arkheide.Flourish.Blazor.Abstract', 'Arkheide.Essential.Culture.Blazor'); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Blazor.Framework'; Project = 'src/Flourish.Blazor/Flourish.Blazor.Framework/Flourish.Blazor.Framework.csproj'; Dependencies = @('Arkheide.Flourish.Blazor.Abstract'); Assets = @('staticwebassets/framework.css', 'staticwebassets/shell.js', 'staticwebassets/controls.js', 'staticwebassets/multi-select-box.js', 'staticwebassets/presentation/offers.js', 'staticwebassets/icons/material/MaterialSymbolsOutlined.woff2', 'staticwebassets/icons/material/LICENSE.txt'); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Blazor.Design'; Project = 'src/Flourish.Blazor/Flourish.Blazor.Design/Flourish.Blazor.Design.csproj'; Dependencies = @('Arkheide.Flourish.Blazor.Framework', 'Arkheide.Flourish.Blazor.Abstract'); Assets = @('staticwebassets/design.css'); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Blazor'; Project = 'src/Flourish.Blazor/Flourish.Blazor/Flourish.Blazor.csproj'; Dependencies = @('Arkheide.Flourish.Blazor.Abstract', 'Arkheide.Flourish.Blazor.Framework', 'Arkheide.Flourish.Blazor.Design', 'Arkheide.Flourish.Extensions.Culture.Blazor'); Assets = @(); Managed = $false }
    )
}
