using System.Windows;
namespace SFTemplateGenerator.Helper.Controls
{
    public class NotifyPropertyListValueChangedEventArgs : RoutedEventArgs
    {
        public NotifyPropertyListValueChangedEventArgs(RoutedEvent @event, object source)
            : base(@event, source)
        {
        }

        public string PropertyName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}
