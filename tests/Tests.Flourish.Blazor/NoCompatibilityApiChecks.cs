using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;

internal static class NoCompatibilityApiChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("library styles contain no retired table selector adapters", () =>
        {
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var obsolete = new Regex(@"\.(?:table-column-selector|table-advanced-selector|table-popup-panel|table-popup-item|table-action|danger-action|table-scroll|standard-table-scroll|column-option|row-action-menu(?:-[A-Za-z0-9_-]+)?)(?![A-Za-z0-9_-])");
            foreach (var project in new[] { "Flourish.Blazor.Framework", "Flourish.Blazor.Design" })
                foreach (var path in Directory.EnumerateFiles(Path.Combine(root, "src/Flourish.Blazor", project, "wwwroot"), "*.css", SearchOption.AllDirectories))
                {
                    var css = File.ReadAllText(path);
                    Require(!obsolete.IsMatch(css), "A retired table selector survives in " + path);
                    Require(!css.Contains("-ms-overflow-style", StringComparison.Ordinal), "An obsolete IE-only style adapter survives in " + path);
                }
            return Task.CompletedTask;
        }));
        tests.Add(("Flourish Blazor exports current contracts without obsolete compatibility members", () =>
        {
            foreach (var assembly in new[] { typeof(Button).Assembly, typeof(IFrameworkBuilder).Assembly, typeof(AppearancePalette).Assembly })
                foreach (var type in assembly.GetExportedTypes())
                {
                    Require(type.GetCustomAttribute<ObsoleteAttribute>() is null, "An obsolete exported type survives: " + type.FullName);
                    foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                        Require(member.GetCustomAttribute<ObsoleteAttribute>() is null, "An obsolete public member survives: " + type.FullName + "." + member.Name);
                }
            var contract = typeof(IFrameworkBuilder).Assembly;
            foreach (var retired in new[] { "IApplicationBuilder", "ITitleBarBuilder", "INavigationGroupBuilder" })
                Require(contract.GetType("ArkheideSystem.Flourish.Blazor.Abstract." + retired) is null, "An old shell builder survives: " + retired);
            Require(typeof(ServiceCollectionExtensions).GetMethods().Where(method => method.IsPublic).All(method => method.Name != "AddFlourish"), "The registration alias survives.");
            return Task.CompletedTask;
        }));
        tests.Add(("appearance and button APIs expose no old palette or variant aliases", () =>
        {
            Require(typeof(AppearancePalette).Assembly.GetType("ArkheideSystem.Flourish.Blazor.ThemePalette") is null, "The old palette facade survives.");
            Require(Enum.GetNames<ButtonVariant>().SequenceEqual(new[] { "Primary", "Secondary", "Danger", "Quiet", "Underline", "Elevated" }), "Button variants contain aliases or an alternate family.");
            Require(typeof(UniformGrid).GetProperty("Filled") is null && !Enum.GetNames<UniformGridVariant>().Contains("Outline"), "The old grid appearance API survives.");
            return Task.CompletedTask;
        }));
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
