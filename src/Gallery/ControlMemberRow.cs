using Localizer = Arkheide.Essential.Culture.Localizer;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArkheideSystem.Gallery.Models;

public sealed class ControlMemberRow : INotifyPropertyChanged
{
    private readonly string resourceKey;
    private string description;

    public ControlMemberRow(string name, string descriptionKey)
    {
        Name = name;
        resourceKey = descriptionKey;
        description = Localizer.Parse(resourceKey);
        Localizer.Current.Changed += Localizer_Changed;
    }

    public string Name { get; }

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

    private void Localizer_Changed(object? sender, EventArgs e)
    {
        Description = Localizer.Parse(resourceKey);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
