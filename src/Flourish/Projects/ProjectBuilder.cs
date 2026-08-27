using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Projects;

internal sealed class ProjectBuilder(ProjectOptions options)
    : BuilderMutationGuard,
        IProjectBuilder
{
    public IProjectBuilder SetMultiProjectEnabled(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsMultiProjectEnabled = enabled;
        return this;
    }
}
