using System.ComponentModel;
using System.Runtime.CompilerServices;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Models;

public sealed class ControlMemberRow(string name, string description) : INotifyPropertyChanged
{
    private readonly string sourceDescription = description;
    private string description = description;

    public string Name { get; } = name;

    public string Description
    {
        get => description;
        private set
        {
            if (description == value)
            {
                return;
            }

            description = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    internal void Apply(IGalleryLocalization localization)
    {
        Description = localization.Get(sourceDescription);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
