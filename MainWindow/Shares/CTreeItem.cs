using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace SFTemplateGenerator.MainWindow.Shares
{
    public class CTreeItem : PropertyChangedBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public dynamic HostItem { get; set; } = null!;
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;


        public CTreeItem Parent { get; set; } = null!;

        public ObservableCollection<CTreeItem> Children { get; set; } = null!;

        public bool IsExpand { get; set; }


        protected virtual void RaisePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
