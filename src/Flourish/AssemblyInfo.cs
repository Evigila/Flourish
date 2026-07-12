using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;

[assembly: InternalsVisibleTo("Flourish.Test")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: XmlnsDefinition(
    "http://schemas.arkheide.system/flourish",
    "ArkheideSystem.Flourish.Controls"
)]
[assembly: XmlnsDefinition(
    "http://schemas.arkheide.system/flourish",
    "ArkheideSystem.Flourish.Styles"
)]
[assembly: XmlnsDefinition(
    "http://schemas.arkheide.system/flourish",
    "ArkheideSystem.Flourish.Themes"
)]
[assembly: XmlnsDefinition(
    "http://schemas.arkheide.system/flourish",
    "ArkheideSystem.Flourish.Abstract"
)]
[assembly: XmlnsPrefix("http://schemas.arkheide.system/flourish", "flourish")]
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]
