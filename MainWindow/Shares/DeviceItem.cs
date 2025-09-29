using Caliburn.Micro;
using SFTemplateGenerator.Helper.Shares.SDL;
using System.ComponentModel;
namespace SFTemplateGenerator.MainWindow.Shares
{
    public class DeviceItem : PropertyChangedBase
    {
        public DeviceItem(Device device)
        {
            Device = device;
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }
        public Device Device { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaiseNotifyPropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
