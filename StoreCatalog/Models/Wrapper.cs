using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StoreCatalogPresentation.Models;

public class Wrapper<T>(T baseObject) : INotifyPropertyChanged
    where T : class
{
    public T Base { get; } = baseObject ?? throw new ArgumentNullException(nameof(baseObject));

    public static implicit operator T(Wrapper<T> wrapper) =>
        wrapper.Base;

    protected virtual bool Set<TU>(ref TU field, TU value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TU>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected virtual bool Set<TU>(
        Func<TU> getter,
        Action<TU> setter,
        TU value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TU>.Default.Equals(getter(), value))
            return false;

        setter(value);
        OnPropertyChanged(propertyName);
        return true;
    }

    public override string ToString() => Base.ToString() ?? string.Empty;

    protected virtual void OnPropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
