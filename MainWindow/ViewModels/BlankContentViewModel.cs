using SFTemplateGenerator.MainWindow.Interfaces;
using System.ComponentModel;

namespace SFTemplateGenerator.MainWindow.ViewModels
{
    public class BlankContentViewModel : IBlankContentViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
