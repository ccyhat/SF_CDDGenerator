using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.MainWindow.Shares;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SFTemplateGenerator.MainWindow.Interfaces
{
    public interface ITestTemplateManagerViewModel
    {
        ObservableCollection<CombineUnit> Units { get; }
        CombineUnit SelectedUnit { get; set; }
        bool? SelectAll { get; set; }
        void GenerateProcess(List<Device> devices);
        void SelectedTreeNodeChanged(object sender, RoutedPropertyChangedEventArgs<object> e);
        void TableSelectionChanged(object sender, SelectionChangedEventArgs e);
        void SaveButtonClicked(object sender, RoutedEventArgs e);
        void SelectAllCheckBoxCheced(object sender, RoutedEventArgs e);
        void SelectAllCheckBoxUnchecked(object sender, RoutedEventArgs e);
        void DeviceCheckBoxChecked(object sender, RoutedEventArgs e);
        void DeviceCheckBoxUnchecked(object sender, RoutedEventArgs e);
        void SaveTestTemplateButtonClicked(object sender, RoutedEventArgs e);


    }
}
