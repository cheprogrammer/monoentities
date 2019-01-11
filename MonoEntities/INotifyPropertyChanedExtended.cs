using System.ComponentModel;

namespace MonoEntities
{
    public interface INotifyPropertyChangedExtended
    {
        event PropertyChangedExtendedEventHandler PropertyChanged;
    }

    public class PropertyChangedExtendedEventArgs : PropertyChangedEventArgs
    {
        public object OldValue { get; }
        public object NewValue { get; }

        public PropertyChangedExtendedEventArgs(object oldValue, object newValue, string propertyName = null)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public delegate void PropertyChangedExtendedEventHandler(object sender, PropertyChangedExtendedEventArgs e);
}