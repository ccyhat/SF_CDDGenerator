using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.MainWindow.Shares;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace SFTemplateGenerator.MainWindow.Interfaces
{
    public interface IShowCDDViewModel : INotifyPropertyChanged
    {
        ObservableCollection<DeviceItem> Devices { get; }
        bool? IsSelectAll { get; set; }
        bool HasDevices { get; set; }
        bool EnableGenerateButton { get; set; }
        void AddDevices(params Device[] devices);
        void SelectAllCheckBoxChecked(object sender, RoutedEventArgs e);
        void SelectAllCheckBoxUnChecked(object sender, RoutedEventArgs e);
        void DeviceItemChecked(object sender, object context, RoutedEventArgs e);
        void DeviceItemUnChecked(object sender, object context, RoutedEvent e);
        void GenerateTestTemplateButtonClicked(object sender, RoutedEventArgs e);


    }
}
