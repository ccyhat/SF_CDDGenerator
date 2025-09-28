using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static SFTemplateGenerator.MainWindow.Views.Controls.PropertyList;
namespace SFTemplateGenerator.MainWindow.Views.Controls
{
    /// <summary>
    /// ValueControl.xaml 的交互逻辑
    /// </summary>
   	public partial class ValueControl : UserControl
    {
        private static readonly Type[] NUMBERTYPES = new Type[]
        {
            typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(float), typeof(double), typeof(decimal), typeof(long), typeof(ulong), typeof(short?),
            typeof(ushort?), typeof(int?), typeof(uint?), typeof(float?), typeof(double?), typeof(decimal?), typeof(long?), typeof(ulong?)
        };

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type),
            typeof(Type),
            typeof(ValueControl),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(object),
            typeof(ValueControl),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ValueControl()
        {
            InitializeComponent();
        }

        public Type Type
        {
            get
            {
                return (Type)GetValue(TypeProperty);
            }

            set
            {
                SetValue(TypeProperty, value);
            }
        }

        public object Value
        {
            get
            {
                return GetValue(ValueProperty);
            }

            set
            {
                SetValue(ValueProperty, value);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PropertyItem context = DataContext as PropertyItem;

            if (Type != null)
            {

                if (NUMBERTYPES.Contains(Type))
                {
                    TextBox textBox = new TextBox()
                    {
                        BorderThickness = new Thickness(0),
                        Text = $"{Value}",
                    };

                    textBox.TextChanged += (obj, args) =>
                    {
                        if (context != null && context.ValueChangedCommand.CanExecute(textBox))
                        {
                            context.ValueChangedCommand.Execute(textBox);
                        }
                    };

                    Content = textBox;
                }
                else if (Type == typeof(bool) || Type == typeof(bool?))
                {
                    CheckBox checkBox = new CheckBox()
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        IsChecked = (bool?)Value,
                    };

                    checkBox.Checked += (obj, arg) =>
                    {
                        if (context != null && context.ValueChangedCommand.CanExecute(checkBox))
                        {
                            context.ValueChangedCommand.Execute(checkBox);
                        }
                    };

                    checkBox.Unchecked += (obj, arg) =>
                    {
                        if (context != null && context.ValueChangedCommand.CanExecute(checkBox))
                        {
                            context.ValueChangedCommand.Execute(checkBox);
                        }
                    };

                    Content = checkBox;
                }
                else
                {
                    TextBox textBox = new TextBox()
                    {
                        BorderThickness = new Thickness(0),
                        Text = $"{Value}",
                    };

                    textBox.TextChanged += (obj, args) =>
                    {
                        if (context != null && context.ValueChangedCommand.CanExecute(textBox))
                        {
                            context.ValueChangedCommand.Execute(textBox);
                        }
                    };

                    Content = textBox;
                }
            }
        }
    }

    public class ValueControlTypeConverter : IValueConverter
    {
        private static readonly Type[] NUMBERTYPES = new Type[]
        {
            typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(float), typeof(double), typeof(decimal), typeof(long), typeof(ulong), typeof(short?),
            typeof(ushort?), typeof(int?), typeof(uint?), typeof(float?), typeof(double?), typeof(decimal?), typeof(long?), typeof(ulong?)
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = value as Type;

            if (type != null)
            {
                if (NUMBERTYPES.Contains(type))
                {
                    return 2;
                }
                else if (type == typeof(bool) || type == typeof(bool?))
                {
                    return 3;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
