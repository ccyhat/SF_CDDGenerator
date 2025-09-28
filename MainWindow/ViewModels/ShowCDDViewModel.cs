using Caliburn.Micro;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Helper.UtilityTools;
using SFTemplateGenerator.MainWindow.Interfaces;
using SFTemplateGenerator.MainWindow.Shares;
using System.Collections.ObjectModel;
using System.Windows;
namespace SFTemplateGenerator.MainWindow.ViewModels
{
    public class ShowCDDViewModel : Screen, IShowCDDViewModel
    {
        private readonly Caliburn.Micro.IEventAggregator _eventAggregator;

        private bool _processFlag = false;
        //private Device _firstDevice = null;
        public ShowCDDViewModel(
            Caliburn.Micro.IEventAggregator eventAggregator)
        {
            Devices = new ObservableCollection<DeviceItem>();

            _eventAggregator = eventAggregator;

        }


        public ObservableCollection<DeviceItem> Devices { get; private set; }

        private bool? _isSelectAll = false; // 初始值设为 false 或 null，取决于需求

        public bool? IsSelectAll
        {
            get => _isSelectAll;
            set => Set(ref _isSelectAll, value); // 使用 Caliburn.Micro 的 Set 方法
        }

        private bool _hasDevices;
        public bool HasDevices
        {
            get => _hasDevices;
            set => Set(ref _hasDevices, value); // Caliburn.Micro 提供的 Set 方法
        }

        private bool _enableGenerateButton;
        public bool EnableGenerateButton
        {
            get => _enableGenerateButton;
            set => Set(ref _enableGenerateButton, value); // Caliburn.Micro 提供的 Set 方法
        }

        public void AddDevices(params Device[] devices)
        {
            Devices.Clear();

            HasDevices = devices.Length > 0;

            foreach (var device in devices)
            {
                Devices.Add(new DeviceItem(device));
            }

            EnableGenerateButton = false;
        }


        public void DeviceItemChecked(object sender, object context, RoutedEventArgs e)
        {
            if (!_processFlag)
            {
                if (Devices.Any(D => D.IsSelected) && Devices.Any(D => !D.IsSelected))
                {
                    IsSelectAll = null;
                    EnableGenerateButton = true;
                }
                else if (Devices.Any(D => D.IsSelected) && !Devices.Any(D => !D.IsSelected))
                {
                    IsSelectAll = true;
                    EnableGenerateButton = true;
                }
                else if (Devices.Any(D => !D.IsSelected) && !Devices.Any(D => D.IsSelected))
                {
                    IsSelectAll = false;
                    EnableGenerateButton = false;
                }
            }
        }

        public void DeviceItemUnChecked(object sender, object context, RoutedEvent e)
        {
            if (!_processFlag)
            {
                if (Devices.Any(D => D.IsSelected) && Devices.Any(D => !D.IsSelected))
                {
                    IsSelectAll = null;
                    EnableGenerateButton = true;
                }
                else if (Devices.Any(D => D.IsSelected) && !Devices.Any(D => !D.IsSelected))
                {
                    IsSelectAll = true;
                    EnableGenerateButton = true;
                }
                else if (Devices.Any(D => !D.IsSelected) && !Devices.Any(D => D.IsSelected))
                {
                    IsSelectAll = false;
                    EnableGenerateButton = false;
                }
            }
        }

        public async void GenerateTestTemplateButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Devices.Any(D => D.IsSelected))
            {
                var devices = Devices.Where(D => D.IsSelected).Select(D => D.Device);

                var gttm = new GenerateTestTemplateMessage(devices.ToArray());

                //gttm.OperationBox = _operationBoxes.ToArray();
                // gttm.FirstDevice = _firstDevice;

                await _eventAggregator.PublishOnBackgroundThreadAsync(gttm);//发送消息到MainWindowViewModel
            }
        }

        public void SelectAllCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            _processFlag = true;

            foreach (var device in Devices)
            {
                device.IsSelected = true;
            }

            EnableGenerateButton = true;

            _processFlag = false;
        }

        public void SelectAllCheckBoxUnChecked(object sender, RoutedEventArgs e)
        {
            _processFlag = true;

            foreach (var device in Devices)
            {
                device.IsSelected = false;
            }

            EnableGenerateButton = false;

            _processFlag = false;
        }


    }
}
