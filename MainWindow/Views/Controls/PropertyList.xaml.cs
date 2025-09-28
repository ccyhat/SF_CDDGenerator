using SFTemplateGenerator.Helper.Controls;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.TreeNode;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SFTemplateGenerator.MainWindow.Views.Controls
{
    /// <summary>
    /// PropertyList.xaml 的交互逻辑
    /// </summary>
    public partial class PropertyList : UserControl
    {
        public static readonly DependencyProperty DisplayItemProperty = DependencyProperty.Register(
            nameof(DisplayItem),
            typeof(IDisplayITem),
            typeof(PropertyList),
            new FrameworkPropertyMetadata(
                ShowSampleData(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(
            nameof(NameColumnWidth),
            typeof(int),
            typeof(PropertyList),
            new FrameworkPropertyMetadata(
                255,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly RoutedEvent NotifyPropertyListValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(NotifyPropertyListValueChanged),
            RoutingStrategy.Bubble,
            typeof(NotifyPropertyListValueChangedDelegate),
            typeof(PropertyList));


        private ItemsControl _itemsContainer;
        private readonly ICommand _valueChangedCommand;

        public event NotifyPropertyListValueChangedDelegate NotifyPropertyListValueChanged
        {
            add
            {
                AddHandler(NotifyPropertyListValueChangedEvent, value);
            }

            remove
            {
                RemoveHandler(NotifyPropertyListValueChangedEvent, value);
            }
        }

        public PropertyList()
        {
            InitializeComponent();
            Loaded += PropertyList_Loaded;
            _valueChangedCommand = new NotifyPropertyListValueChangedCommand(HandleValueChanged);
        }

        public int NameColumnWidth
        {
            get
            {
                return (int)GetValue(NameColumnWidthProperty);
            }

            set
            {
                SetValue(NameColumnWidthProperty, value);
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

        public override void OnApplyTemplate()
        {
            _itemsContainer = Template.FindName("_ItemsContainer", this) as ItemsControl;
        }

        private void PropertyList_Loaded(object sender, RoutedEventArgs e)
        {
            if (_itemsContainer != null)
            {
                if (DisplayItem != null)
                {
                    Type type = DisplayItem.GetType();
                    PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(p => p.PropertyType == typeof(int) || p.PropertyType == typeof(string)).ToArray();
                    List<PropertyItem> list = new List<PropertyItem>();
                    if (properties.Length > 0)
                    {
                        foreach (var property in properties)
                        {
                            PropertyItem item = new PropertyItem(NameColumnWidth, property.Name, property.PropertyType, property.GetValue(DisplayItem), DisplayItem, _valueChangedCommand, property, Foreground);

                            list.Add(item);
                        }
                    }
                    _itemsContainer.ItemsSource = list;
                }
            }
        }

        private void HandleValueChanged(object parameter)
        {
            if (parameter is TextBox textBox)
            {
                NotifyPropertyListValueChangedEventArgs args = new NotifyPropertyListValueChangedEventArgs(NotifyPropertyListValueChangedEvent, this);
                var item = textBox.DataContext as PropertyItem;

                if (item != null)
                {
                    args.OldValue = item.Value;
                    args.NewValue = Convert.ChangeType(textBox.Text, item.Type);
                    item.Property.SetValue(item.Host, args.NewValue);

                    RaiseEvent(args);
                }
            }
            else if (parameter is CheckBox checkBox)
            {
                NotifyPropertyListValueChangedEventArgs args = new NotifyPropertyListValueChangedEventArgs(NotifyPropertyListValueChangedEvent, this);
                var item = checkBox.DataContext as PropertyItem;

                if (item != null)
                {
                    args.OldValue = item.Value;
                    args.NewValue = checkBox.IsChecked;
                    item.Property.SetValue(item.Host, args.NewValue);

                    RaiseEvent(args);
                }
            }
        }

        private static IDisplayITem ShowSampleData()
        {
            return new SampleItem
            {
                Value1 = true,
                Value2 = 100,
                Value3 = 101.1f,
                Value4 = 127.6,
                Value5 = null,
                Value6 = 5,
                Value7 = 3.14f,
                Value8 = 1.414,
                Value9 = 15,
                Value10 = 123.45m,
                Name = "这是个字符串"
            };
        }

        internal class PropertyItem
        {
            public PropertyItem(long width, string name, Type type, object value, object host, ICommand valuechangedCommand, PropertyInfo pi, Brush foreground)
            {
                Host = host;
                Column1Width = width;
                Name = name;
                Type = type;
                Value = value;
                ValueChangedCommand = valuechangedCommand;
                Property = pi;
                Foreground = foreground;
            }
            public PropertyInfo Property { get; private set; }
            public object Host { get; private set; }
            public long Column1Width { get; set; }
            public string Name { get; set; }
            public Type Type { get; set; }
            public object Value { get; set; }
            public ICommand ValueChangedCommand { get; set; }
            public Brush Foreground { get; set; }
        }

        private class SampleItem : IDisplayITem
        {

            public bool Value1 { get; set; }

            public int Value2 { get; set; }

            public float Value3 { get; set; }

            public double Value4 { get; set; }

            public bool? Value5 { get; set; }

            public int? Value6 { get; set; }

            public float? Value7 { get; set; }

            public double? Value8 { get; set; }

            public decimal Value9 { get; set; }

            public decimal? Value10 { get; set; }


            public string Name { get; set; }
            public ScriptInit ScriptInit { get; set; }
            public ScriptName ScriptName { get; set; }
            public ScriptResult ScriptResult { get; set; }
            public RptMap RptMap { get; set; }
            public DllCall DllCall { get; set; }
        }
    }
}
