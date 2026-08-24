using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class RuntimeMotionPolicyTests
{
    private static readonly string RepositoryRoot = TestPaths.RepositoryRoot;
    private static readonly string FlourishRoot = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish"
    );

    [Fact]
    public void MotionService_AttachedDictionaryTracksTheRuntimePolicyWithoutAnApplication()
    {
        StaTest.Run(() =>
        {
            var duration = TimeSpan.FromMilliseconds(96);
            var options = new FlourishShellOptions();
            options.Motion.IsEnabled = true;
            options.Motion.IsHoverRevealEnabled = true;
            options.Motion.RespectSystemReducedMotion = false;
            options.Motion.HoverRevealAnimationDuration = duration;
            var resources = new ResourceDictionary();
            var sut = new FlourishMotionService(options);

            sut.Attach(Dispatcher.CurrentDispatcher, resources);

            Assert.True(Assert.IsType<bool>(resources["FlourishHoverRevealEnabled"]));
            Assert.Equal(
                duration,
                Assert.IsType<TimeSpan>(resources["FlourishHoverRevealDuration"])
            );
            Assert.Equal(2, resources.Count);

            var updatedDuration = TimeSpan.FromMilliseconds(72);
            sut.SetHoverReveal(false, updatedDuration);

            Assert.False(Assert.IsType<bool>(resources["FlourishHoverRevealEnabled"]));
            Assert.Equal(
                updatedDuration,
                Assert.IsType<TimeSpan>(resources["FlourishHoverRevealDuration"])
            );
        });
    }

    [Fact]
    public void MotionService_ApplicationAttachmentIsTheOnlySingleArgumentOwnerContract()
    {
        var attach = Assert.Single(
            typeof(FlourishMotionService)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic),
            method =>
                method.Name == "Attach" && method.GetParameters().Length == 1
        );

        Assert.Equal(typeof(Application), attach.GetParameters()[0].ParameterType);
    }

    [Fact]
    public void ShellWindow_DoesNotOwnADuplicateHoverRevealPolicyPath()
    {
        var source = File.ReadAllText(
            Path.Combine(
                FlourishRoot,
                "Views",
                "Windows",
                "FlourishShellWindow.xaml.cs"
            )
        );

        Assert.DoesNotContain("ApplyMotionResources", source, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "HoverReveal.SetIsEnabled",
            source,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "HoverReveal.SetAnimationDuration",
            source,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Runtime_AttachesMotionPolicyAfterApplicationThemeResources()
    {
        var source = File.ReadAllText(
            Path.Combine(FlourishRoot, "Internal", "Composition", "FlourishRuntime.cs")
        );
        var resourcesIndex = source.IndexOf(
            "EnsureApplicationResources(application)",
            StringComparison.Ordinal
        );
        var motionIndex = source.IndexOf(
            "GetRequiredService<FlourishMotionService>",
            StringComparison.Ordinal
        );

        Assert.True(resourcesIndex >= 0, "Application theme resources are not prepared.");
        Assert.True(
            motionIndex > resourcesIndex,
            "Motion resources must be attached after the application theme dictionary."
        );
        Assert.Contains("Attach(application)", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MotionService_DoesNotWriteInheritedHoverRevealPropertiesOrWindowResources()
    {
        var source = File.ReadAllText(
            Path.Combine(FlourishRoot, "Services", "FlourishMotionService.cs")
        );

        Assert.DoesNotContain("Window? owner", source, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "window.Resources[\"FlourishHoverReveal",
            source,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "HoverReveal.SetIsEnabled",
            source,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "HoverReveal.SetAnimationDuration",
            source,
            StringComparison.Ordinal
        );
    }
}
