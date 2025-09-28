using Caliburn.Micro;
using System.Windows;

namespace SFTemplateGenerator.MainWindow.Interfaces
{
    public interface IMainWindowViewModel : IConductor
    {
        Visibility ExceptionSectionVisibility { get; set; }
        Visibility ContinueButtonVisibility { get; set; }
        void OpenCDDFileMenuClicked(object sender, RoutedEventArgs args);
        void ExitMenuClicked(object sender, RoutedEventArgs args);
        void SystemConfigurationMenuClicked(object sender, RoutedEventArgs args);
        void StopGenerateProcessingButtonClicked(object sender, RoutedEventArgs args);
        void ContinueGenerateProcessingButtonClicked(object sender, RoutedEventArgs args);

    }
}
