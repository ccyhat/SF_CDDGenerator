using Caliburn.Micro;
using Microsoft.Win32;
using SFTemplateGenerator.Helper.Paths;
using SFTemplateGenerator.MainWindow.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using static SFTemplateGenerator.Helper.Paths.PathSaver;

namespace SFTemplateGenerator.MainWindow.ViewModels
{

    public class SettingViewModel : Screen, ISettingViewModel
    {
        public SettingViewModel()
        {
            var config = PathSaver.Instance.Config;
            GBxmlFileSavePath = config.GBxmlFileSavePath;
            ConfigPath = config.ConfigPath;
            DIODefinePath = config.DIODefinePath;
            DeviceModelPath = config.DeviceModelPath;
            Tester = config.Tester;
            TesterModeOptions = new ObservableCollection<string>(
                Enum.GetValues(typeof(TESTER))
                .OfType<TESTER>()
                .Select(mode => GetEnumDescription(mode))
             );
        }
        private string _gBxmlFileSavePath = null!;
        public string GBxmlFileSavePath
        {
            get => _gBxmlFileSavePath;
            set => Set(ref _gBxmlFileSavePath, value);
        }
        private string _configPath = null!;
        public string ConfigPath
        {
            get => _configPath;
            set => Set(ref _configPath, value);
        }
        private string _dIODefinePath = null!;
        public string DIODefinePath
        {
            get => _dIODefinePath;
            set => Set(ref _dIODefinePath, value);
        }
        private string _deviceModelPath = null!;
        public string DeviceModelPath
        {
            get => _deviceModelPath;
            set => Set(ref _deviceModelPath, value);
        }
        private TESTER _tester;
        public TESTER Tester
        {
            get => _tester;
            set
            {
                _tester = value;
                NotifyOfPropertyChange(() => Tester);
                // 同步更新选中的文本（从枚举的Description中获取）
                SelectedTesterMode = GetEnumDescription(value);
            }
        }
        private string GetEnumDescription(Enum enumValue)
        {
            // 获取枚举成员的字段信息
            FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null) return enumValue.ToString();

            // 获取Description特性
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? enumValue.ToString(); // 没有特性则返回枚举名
        }
        public ObservableCollection<string> TesterModeOptions { get; set; }
        public string SelectedTesterMode { get; set; } // 用于绑定到ComboBox的SelectedItem 
        public async void SetGBxmlFileSavePath(object sender, RoutedEventArgs args)
        {
            OpenFolderDialog ofd = new OpenFolderDialog();

            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;


            if (ofd.ShowDialog() == true)
            {
                GBxmlFileSavePath = ofd.FolderName ?? AppDomain.CurrentDomain.BaseDirectory;
                PathSaver.Instance.Config.GBxmlFileSavePath = GBxmlFileSavePath;
                // 保存配置
                await PathSaver.Instance.SaveConfigAsync();
            }
        }
        public async void SetConfigPath(object sender, RoutedEventArgs args)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // 设置文件筛选器，只显示指定名称的XML文件
            ofd.Filter = "SF Template Local DB|sf-template-local-db.xml";
            // 只允许选择一个文件
            ofd.Multiselect = false;
            // 检查文件是否存在
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == true)
            {
                ConfigPath = ofd.FileName ?? AppDomain.CurrentDomain.BaseDirectory;
                PathSaver.Instance.Config.ConfigPath = ConfigPath;
                // 保存配置
                await PathSaver.Instance.SaveConfigAsync();
            }
        }
        public async void SetDeviceModelPath(object sender, RoutedEventArgs args)
        {
            OpenFolderDialog ofd = new OpenFolderDialog();

            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == true)
            {
                DeviceModelPath = ofd.FolderName ?? AppDomain.CurrentDomain.BaseDirectory;
                PathSaver.Instance.Config.DeviceModelPath = DeviceModelPath;
                // 保存配置
                await PathSaver.Instance.SaveConfigAsync();
            }
        }
        public async void SetDIODefinePath(object sender, RoutedEventArgs args)
        {
            OpenFolderDialog ofd = new OpenFolderDialog();

            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == true)
            {
                DIODefinePath = ofd.FolderName ?? AppDomain.CurrentDomain.BaseDirectory;
                PathSaver.Instance.Config.DIODefinePath = DIODefinePath;
                // 保存配置
                await PathSaver.Instance.SaveConfigAsync();
            }
        }
        public async void FilledComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 获取选中的项  
            var selectedItem = ((ComboBox)e.Source).SelectedItem;

            //// 执行相应的操作  
            if (selectedItem is string selectedString)
            {
                // 根据选中的文本找到对应的枚举值（核心逻辑：文本→枚举）
                TESTER? selectedMode = Enum.GetValues(typeof(TESTER))
                    .Cast<TESTER>()
                    .FirstOrDefault(mode => GetEnumDescription(mode) == selectedString);

                if (selectedMode.HasValue)
                {
                    // 更新视图模型的枚举属性
                    Tester = selectedMode.Value;
                    // 更新配置（直接使用枚举值）
                    PathSaver.Instance.Config.Tester = selectedMode.Value;
                    // 保存配置
                    await PathSaver.Instance.SaveConfigAsync();
                }
            }
        }
    }
}
