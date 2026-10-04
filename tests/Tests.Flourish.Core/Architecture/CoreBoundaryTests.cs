using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using ArkheideSystem.Flourish.Abstract;
using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Architecture;

public sealed class CoreBoundaryTests
{
    private static readonly IReadOnlySet<string> ForbiddenAssemblyReferences =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PresentationCore",
            "PresentationFramework",
            "System.Windows.Forms",
            "WindowsBase",
            "Microsoft.UI.Xaml",
            "Microsoft.WindowsAppSDK",
            "Microsoft.Windows.SDK.NET",
            "WinRT.Runtime",
        };

    private static readonly string[] ForbiddenNamespacePrefixes =
    [
        "System.Windows",
        "System.Windows.Forms",
        "Microsoft.UI",
        "Windows.UI",
        "WinRT",
    ];

    [Fact]
    public void CoreAssembly_DoesNotReferenceUiFrameworks()
    {
        var coreAssembly = typeof(IRegistration).Assembly;
        var forbiddenReferences = coreAssembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null && ForbiddenAssemblyReferences.Contains(name))
            .ToArray();

        Assert.Equal("Flourish.Core", coreAssembly.GetName().Name);
        Assert.Empty(forbiddenReferences);
    }

    [Fact]
    public void CoreAssembly_DoesNotTargetWindows()
    {
        Assembly coreAssembly = typeof(IRegistration).Assembly;

        Assert.DoesNotContain(
            coreAssembly.GetCustomAttributesData(),
            attribute =>
                string.Equals(
                    attribute.AttributeType.FullName,
                    "System.Runtime.Versioning.TargetPlatformAttribute",
                    StringComparison.Ordinal
                )
        );
    }

    [Fact]
    public void CorePublicApi_DoesNotExposePlatformTypes()
    {
        Assembly coreAssembly = typeof(IRegistration).Assembly;
        string[] violations = coreAssembly
            .GetExportedTypes()
            .SelectMany(GetPublicApiTypes)
            .Where(IsForbiddenPlatformType)
            .Select(type => type.FullName ?? type.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void CoreProject_DoesNotEnableAUiSdkOrWindowsTargetFramework()
    {
        string projectPath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Flourish.Core",
            "Flourish.Core.csproj"
        );
        XDocument project = XDocument.Load(projectPath);
        string[] properties = project
            .Descendants()
            .Where(element =>
                element.Name.LocalName is "TargetFramework"
                    or "UseWPF"
                    or "UseWindowsForms"
                    or "UseWinUI"
                    or "FrameworkReference"
            )
            .Select(element => $"{element.Name.LocalName}={element.Value}")
            .ToArray();

        Assert.Contains("TargetFramework=net10.0", properties);
        Assert.DoesNotContain(properties, value => value.StartsWith("Use", StringComparison.Ordinal));
        Assert.DoesNotContain(
            properties,
            value => value.Contains("WindowsDesktop", StringComparison.OrdinalIgnoreCase)
        );
    }

    private static IEnumerable<Type> GetPublicApiTypes(Type declaringType)
    {
        yield return declaringType;

        if (declaringType.BaseType is { } baseType)
        {
            yield return baseType;
        }

        foreach (Type interfaceType in declaringType.GetInterfaces())
        {
            yield return interfaceType;
        }

        foreach (
            MemberInfo member in declaringType.GetMembers(
                BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly
            )
        )
        {
            foreach (Type referencedType in GetMemberTypes(member))
            {
                yield return referencedType;
            }
        }
    }

    private static IEnumerable<Type> GetMemberTypes(MemberInfo member)
    {
        switch (member)
        {
            case MethodInfo method:
                yield return method.ReturnType;
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    yield return parameter.ParameterType;
                }
                break;
            case ConstructorInfo constructor:
                foreach (ParameterInfo parameter in constructor.GetParameters())
                {
                    yield return parameter.ParameterType;
                }
                break;
            case PropertyInfo property:
                yield return property.PropertyType;
                break;
            case EventInfo eventInfo when eventInfo.EventHandlerType is { } eventType:
                yield return eventType;
                break;
            case FieldInfo field:
                yield return field.FieldType;
                break;
        }
    }

    private static bool IsForbiddenPlatformType(Type type)
    {
        if (type.HasElementType)
        {
            return IsForbiddenPlatformType(type.GetElementType()!);
        }

        if (type.IsGenericType && type.GetGenericArguments().Any(IsForbiddenPlatformType))
        {
            return true;
        }

        string typeNamespace = type.Namespace ?? string.Empty;
        return ForbiddenNamespacePrefixes.Any(prefix =>
            typeNamespace.Equals(prefix, StringComparison.Ordinal)
            || typeNamespace.StartsWith($"{prefix}.", StringComparison.Ordinal)
        );
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Flourish.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("The Flourish repository root was not found.");
    }
}
