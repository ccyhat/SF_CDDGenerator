using SFTemplateGenerator.MainWindow.Interfaces;
using System.Windows.Controls;

namespace SFTemplateGenerator.MainWindow.Views
{
    /// <summary>
    /// LoadingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingWindowView : UserControl, ILoadingWindowView
    {
        public LoadingWindowView()
        {
            InitializeComponent();
        }
    }
}
