using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Owns the lifetime of a runtime Flourish registration.</summary>
public interface IRegistration : IDisposable
{
    /// <summary>Gets whether the registration is still active.</summary>
    bool IsRegistered { get; }
}
