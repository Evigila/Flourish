using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;

[assembly: InternalsVisibleTo("Tests.Flourish.WPF")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: XmlnsDefinition(
    "http://schemas.arkheide.system/flourish",
    "ArkheideSystem.Flourish.Controls"
)]
[assembly: XmlnsDefinition(
    "http://schemas.arkheide.system/flourish",
    "ArkheideSystem.Flourish.Themes"
)]
[assembly: XmlnsPrefix("http://schemas.arkheide.system/flourish", "flourish")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
