using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.TreeNode;
using System.Windows;
using System.Windows.Controls;

namespace SFTemplateGenerator.MainWindow.Views.Controls
{
    /// <summary>
    /// CMDList.xaml 的交互逻辑
    /// </summary>
    public partial class CMDList : UserControl
    {
        public static readonly DependencyProperty DisplayItemProperty = DependencyProperty.Register(
     nameof(DisplayItem),
     typeof(IDisplayITem),
     typeof(CMDList),
     new FrameworkPropertyMetadata(null,
         FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        private List<CMDShow> CMDShows { get; set; }
        private DataGrid _CMDList;
        public CMDList()
        {
            InitializeComponent();
            Loaded += CMDList_Loaded;
            // CMDShows = new List<CMDShow>();
        }

        public override void OnApplyTemplate()
        {
            _CMDList = Template.FindName("_CMDList", this) as DataGrid;
        }

        private void CMDList_Loaded(object sender, RoutedEventArgs e)
        {
            if (_CMDList != null)
            {

                if (DisplayItem != null)
                {
                    Type type = DisplayItem.GetType();

                    if (type.Name == "CommCMD")
                    {
                        CMDShows = new List<CMDShow>();
                        var commCMD = (CommCMD)DisplayItem;
                        if (commCMD.CMD != null)
                        {
                            foreach (var item in commCMD.CMD.Value)
                            {
                                CMDShows.Add(new CMDShow()
                                {
                                    ID = item.Id,
                                    Name = item.Value
                                });
                            }
                        }
                        if (commCMD.DsvScript != null)
                        {
                            foreach (var item in commCMD.DsvScript.Elements)
                            {
                                CMDShows.Add(new CMDShow()
                                {
                                    ID = item.Id,
                                    Name = item.Name
                                });
                            }

                        }
                    }

                }
                _CMDList.ItemsSource = CMDShows;
            }
        }


        public IDisplayITem DisplayItem
        {
            get
            {
                return GetValue(DisplayItemProperty) as IDisplayITem;
            }

            set
            {
                SetValue(DisplayItemProperty, value);
            }
        }
        public class CMDShow
        {
            public string ID { get; set; }

            public string Name { get; set; }
        }
    }
}
