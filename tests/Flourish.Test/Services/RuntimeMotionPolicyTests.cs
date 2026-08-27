using System;
using Xunit;
using ArkheideSystem.Flourish.Motion;
using ArkheideSystem.Flourish.Test.Infrastructure;

using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

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
            var options = new MotionOptions();
            options.IsEnabled = true;
            options.IsHoverRevealEnabled = true;
            options.RespectSystemReducedMotion = false;
            options.HoverRevealAnimationDuration = duration;
            var resources = new ResourceDictionary();
            var sut = new MotionService(options);

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
            typeof(MotionService)
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
                "ShellWindow.xaml.cs"
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
            Path.Combine(FlourishRoot, "Hosting", "HostedApplicationRuntime.cs")
        );
        var resourcesIndex = source.IndexOf(
            "EnsureApplicationResources(application)",
            StringComparison.Ordinal
        );
        var motionIndex = source.IndexOf(
            "GetRequiredService<MotionService>",
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
            Path.Combine(FlourishRoot, "Motion", "MotionService.cs")
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
