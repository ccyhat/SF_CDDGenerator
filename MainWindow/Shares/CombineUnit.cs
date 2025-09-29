using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace SFTemplateGenerator.MainWindow.Shares
{
    public class CombineUnit : INotifyPropertyChanged
    {
        public CombineUnit(Device device, GuideBook guideBook)
        {
            Device = device;
            GuideBook = guideBook;
            var tree = new CTreeItem
            {
                Name = "测试模板",
                HostItem = guideBook.Device,
                Children = new ObservableCollection<CTreeItem>(),
                IsExpand = true
            };
            Tree = new ObservableCollection<CTreeItem>();
            Tree.Add(tree);
            SelectedNode = tree;
            BuildTreeByDevice(tree, guideBook.Device);
        }
        public bool Selected { get; set; }
        public Device Device { get; private set; }
        public GuideBook GuideBook { get; private set; }
        public ObservableCollection<CTreeItem> Tree { get; set; }
        public CTreeItem SelectedNode { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void BuildTreeByDevice(CTreeItem root, TestDevice device)
        {
            foreach (var item in device.Items)
            {
                CTreeItem childTree = new CTreeItem
                {
                    Parent = root,
                    HostItem = item,
                    Name = item.Name,
                    Children = new ObservableCollection<CTreeItem>(),
                    IsExpand = true,
                };
                BuildCTreeByDevice(childTree, item);
                root.Children.Add(childTree);
            }
        }
        private void BuildCTreeByDevice(CTreeItem root, Items item)
        {
            foreach (var data in item.ItemList)
            {
                var type = data.GetType();
                if (type.Name == "CommCMD")
                {
                    var commCMD = (CommCMD)data;
                    root.Children.Add(new CTreeItem
                    {
                        Parent = root,
                        HostItem = commCMD,
                        Name = commCMD.Name,
                        Children = new ObservableCollection<CTreeItem>()
                    });
                }
                if (type.Name == "MacroTest")
                {
                    var macroTest = (MacroTest)data;
                    var child = new CTreeItem
                    {
                        Parent = root,
                        HostItem = macroTest,
                        Name = macroTest.Name,
                        Children = new ObservableCollection<CTreeItem>()
                    };
                    BuildCTreeByMacroTests(child, macroTest);
                    root.Children.Add(child);
                }
                if (type.Name == "Safety")
                {
                    var safety = (Safety)data;
                    root.Children.Add(new CTreeItem
                    {
                        Parent = root,
                        HostItem = safety,
                        Name = safety.Name,
                        Children = new ObservableCollection<CTreeItem>()
                    });
                }
                if (type.Name == "Items")
                {
                    var citem = (Items)data;
                    var child = new CTreeItem
                    {
                        Parent = root,
                        HostItem = citem,
                        Name = citem.Name,
                        Children = new ObservableCollection<CTreeItem>()
                    };
                    //root.Children.Add(child);
                    BuildCTreeByDevice(child, citem);
                    root.Children.Add(child);
                }
            }
        }
        private void BuildCTreeByCommCMDs(CTreeItem root, List<CommCMD> commCMDs)
        {
            if (commCMDs != null)
            {
                //var detailsSort = OutSort.GetOutDetailsSort();
                foreach (var commCMD in commCMDs)
                {
                    // var sort = //detailsSort.FirstOrDefault(p => p.name.Contains() );
                    root.Children.Add(new CTreeItem
                    {
                        Parent = root,
                        HostItem = commCMD,
                        Name = commCMD.Name,
                        Children = new ObservableCollection<CTreeItem>(),
                    }); ;
                }
            }
        }
        private void BuildCTreeBySafeties(CTreeItem root, List<Safety> safeties)
        {
            if (safeties != null)
            {
                //var detailsSort = OutSort.GetOutDetailsSort();
                foreach (var safety in safeties)
                {
                    //var sort = detailsSort.FirstOrDefault(p => p.name == safety.Name);
                    root.Children.Add(new CTreeItem
                    {
                        Parent = root,
                        HostItem = safety,
                        Name = safety.Name,
                        Children = new ObservableCollection<CTreeItem>(),
                    });
                }
            }
        }
        private void BuildCTreeByMacroTests(CTreeItem root, MacroTest item)
        {
            foreach (var data in item.Safety_CommCMD_List)
            {
                var type = data.GetType();
                if (type.Name == "CommCMD")
                {
                    var commCMD = (CommCMD)data;
                    root.Children.Add(new CTreeItem
                    {
                        Parent = root,
                        HostItem = commCMD,
                        Name = commCMD.Name,
                        Children = new ObservableCollection<CTreeItem>()
                    });
                }
                if (type.Name == "Safety")
                {
                    var safety = (Safety)data;
                    root.Children.Add(new CTreeItem
                    {
                        Parent = root,
                        HostItem = safety,
                        Name = safety.Name,
                        Children = new ObservableCollection<CTreeItem>()
                    });
                }
            }
        }
    }
}
