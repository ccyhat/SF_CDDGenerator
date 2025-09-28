using System.Windows;
using System.Windows.Controls;


namespace SFTemplateGenerator.MainWindow.Views.Controls
{
    /// <summary>
    /// TextShowCode.xaml 的交互逻辑
    /// </summary>
    public partial class TextShowCode : UserControl
    {
        public TextShowCode()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty TextValueProperty = DependencyProperty.Register(
           "TextValue",
           typeof(string),
           typeof(TextShowCode),
           new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string TextValue
        {
            get { return (string)GetValue(TextValueProperty); }
            set { SetValue(TextValueProperty, value); }
        }
    }
}
