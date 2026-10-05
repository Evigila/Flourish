@{
    VersionProps = 'Directory.Build.props'
    Solution = 'Flourish.slnx'
    DemoSolution = ''
    ConsoleTests = @('tests/Tests.Flourish.Blazor', 'tests/Tests.Flourish.Extensions.Culture.Blazor')
    JavaScriptTests = @('tests/Tests.Flourish.Blazor/tests.js', 'tests/Tests.Flourish.Blazor/measurement-lifecycle.test.mjs', 'tests/Tests.Flourish.Blazor/palette-checks.mjs', 'tests/Tests.Flourish.Blazor/controls-dom.mjs', 'tests/Tests.Flourish.Blazor/clipboard-dom.mjs', 'tests/Tests.Flourish.Blazor/heading-dom.mjs', 'tests/Tests.Flourish.Blazor/interaction-origin-dom.mjs', 'tests/Tests.Flourish.Blazor/row-action-menu-dom.mjs', 'tests/Tests.Flourish.Blazor/section-navigator-dom.mjs')
    CheckScripts = @('build/Test-CssBundle.ps1', 'build/Test-CssAssets.ps1')
    PackagePrefix = 'Arkheide.Flourish.'
    EssentialVersion = '1.3.0'
    Packages = @(
        @{ Id = 'Arkheide.Flourish.Core'; Project = 'src/Flourish.Core/Flourish.Core.csproj'; Dependencies = @(); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Flourish.WPF'; Project = 'src/Flourish.WPF/Flourish.WPF.csproj'; Dependencies = @('Arkheide.Flourish.Core'); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Blazor.Shared'; Project = 'src/Flourish.Blazor/Flourish.Blazor.Shared/Flourish.Blazor.Shared.csproj'; Dependencies = @('Arkheide.Flourish.Core'); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Blazor.Abstract'; Project = 'src/Flourish.Blazor/Flourish.Blazor.Abstract/Flourish.Blazor.Abstract.csproj'; Dependencies = @('Arkheide.Flourish.Blazor.Shared'); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Blazor.Framework'; Project = 'src/Flourish.Blazor/Flourish.Blazor.Framework/Flourish.Blazor.Framework.csproj'; Dependencies = @('Arkheide.Flourish.Blazor.Abstract', 'Arkheide.Flourish.Blazor.Shared'); Assets = @('staticwebassets/framework.css', 'staticwebassets/shell.js', 'staticwebassets/controls.js', 'staticwebassets/icons/material/MaterialSymbolsOutlined.woff2', 'staticwebassets/icons/material/LICENSE.txt'); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Blazor.Design'; Project = 'src/Flourish.Blazor/Flourish.Blazor.Design/Flourish.Blazor.Design.csproj'; Dependencies = @('Arkheide.Flourish.Blazor.Framework', 'Arkheide.Flourish.Blazor.Abstract', 'Arkheide.Flourish.Blazor.Shared'); Assets = @('staticwebassets/design.css'); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Extensions.Culture.WPF'; Project = 'src/Flourish.Extensions/Flourish.Extensions.Culture.WPF/Flourish.Extensions.Culture.WPF.csproj'; Dependencies = @('Arkheide.Flourish.WPF', 'Arkheide.Essential.Culture.Wpf'); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Flourish.Extensions.Culture.Blazor'; Project = 'src/Flourish.Extensions/Flourish.Extensions.Culture.Blazor/Flourish.Extensions.Culture.Blazor.csproj'; Dependencies = @('Arkheide.Flourish.Blazor.Abstract', 'Arkheide.Essential.Culture.Blazor'); Assets = @(); Managed = $true }
    )
}
