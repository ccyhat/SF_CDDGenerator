using Caliburn.Micro;
using SFTemplateGenerator.Helper.Shares.TreeNode;
using SFTemplateGenerator.MainWindow.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace SFTemplateGenerator.MainWindow.ViewModels
{
    public class DisplayDetailsViewModel : Screen, IDisplayDetailsViewModel, INotifyPropertyChanged
    {
        public DisplayDetailsViewModel()
        {
            Properties = new ObservableCollection<Tuple<string, Type, object>>();
            Data = null; // 初始化 Data 为 null
        }
        public IDisplayITem Data { get; set; }
        public ObservableCollection<Tuple<string, Type, object>> Properties { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
