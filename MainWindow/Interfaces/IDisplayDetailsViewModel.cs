using SFTemplateGenerator.Helper.Shares.TreeNode;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace SFTemplateGenerator.MainWindow.Interfaces
{
    public interface IDisplayDetailsViewModel : INotifyPropertyChanged
    {
        IDisplayITem Data { get; set; }
        ObservableCollection<Tuple<string, Type, object>> Properties { get; }
    }
}
