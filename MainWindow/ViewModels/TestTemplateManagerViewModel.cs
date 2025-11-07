using Caliburn.Micro;
using SFTemplateGenerator.Helper.Config;
using SFTemplateGenerator.Helper.Paths;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Helper.UtilityTools;
using SFTemplateGenerator.MainWindow.Interfaces;
using SFTemplateGenerator.MainWindow.Shares;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using SFTemplateGenerator.Processor.Interfaces.FormatConnectionLineCheck;
using SFTemplateGenerator.Processor.Interfaces.FormatDirectCurrentTest;
using SFTemplateGenerator.Processor.Interfaces.FormatExecuteDI;
using SFTemplateGenerator.Processor.Interfaces.FormatExecuteDO;
using SFTemplateGenerator.Processor.Interfaces.FormatNetworkPortTest;
using SFTemplateGenerator.Processor.Interfaces.FormatOperationCircuitTest;
using SFTemplateGenerator.Processor.Interfaces.FormatPowerPluginTest;
using SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor;
using SFTemplateGenerator.Processor.Interfaces.FormatProgramVersionCheck;
using SFTemplateGenerator.Processor.Interfaces.FormatTestEndPrompt;
using SFTemplateGenerator.Processor.Interfaces.FormatTimeSynchronizationTest;
using SFTemplateGenerator.Processor.Interfaces.FormatVoltageSwitchCircuitTest;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;




namespace SFTemplateGenerator.MainWindow.ViewModels
{
    public class TestTemplateManagerViewModel : Conductor<object>.Collection.OneActive, ITestTemplateManagerViewModel
    {
        private readonly Assembly _assembly;

        private TreeView _tree = null!;

        //Model
        private readonly IConfigProcessor _configProcessor;
        private readonly IFormatAnalogQuantityInspection _analogQuantityInspection;
        private readonly IFormatPrepareTestProcessor _prepareTestProcessor;
        private readonly IFormatDirectCurrentTest _directCurrentTest;
        private readonly IFormatProgramVersionCheck _programVersionCheck;
        private readonly IFormatExecuteDOTest _executeDOTest;
        private readonly IFormatExecuteDITest _executeDITest;
        private readonly IFormatConnectionLineCheck _connectionLineCheck;
        private readonly IFormatVoltageSwitchCircuitTest _voltageSwitchCircuitTest;
        private readonly IFormatOperationCircuitTest _operationCiruitTest;
        private readonly IFormatTimeSynchronizationTest _timeSynchronizationTest;
        private readonly IFormatPowerPluginTest _powerPluginTest;
        private readonly IFormatNetworkPortTest _networkPortTest;
        private readonly IFormatTestEndPrompt _testEndPrompt;
        private readonly ISDLKeeper _sDLKeeper;
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly IDeviceModelKeeper _deviceModelKeeper;
        private readonly INotifyExceptionOccuredProcessor _notifyException;
        private readonly IGbXmlProcessor _gbxmlProcessor;
        public TestTemplateManagerViewModel(
             IDisplayDetailsViewModel displayDetailsViewModel,
             IBlankContentViewModel blankContentViewModel,
             IFormatAnalogQuantityInspection formatAnalogQuantityInspection,
             IFormatPrepareTestProcessor prepareTestProcessor,
             IFormatDirectCurrentTest directCurrentTest,
             IFormatProgramVersionCheck programVersionCheck,
             IFormatExecuteDOTest executeDOTest,
             IFormatExecuteDITest executeDITest,
             IFormatConnectionLineCheck executeConnectionLineCheck,
             IFormatVoltageSwitchCircuitTest voltageSwitchCircuitTest,
             IFormatOperationCircuitTest operationCiruitTest,
             IFormatTimeSynchronizationTest timeSynchronizationTest,
             IFormatPowerPluginTest powerPluginTest,
             IFormatNetworkPortTest networkPortTest,
             IFormatTestEndPrompt testEndPrompt,
             ISDLKeeper sDLKeeper,
             IDeviceModelKeeper deviceModelKeeper,
             ITargetDeviceKeeper targetDeviceKeeper,
             IConfigProcessor configProcessor,
             IGbXmlProcessor gbXmlProcessor,
             INotifyExceptionOccuredProcessor notifyException
            )
        {

            _analogQuantityInspection = formatAnalogQuantityInspection;
            _prepareTestProcessor = prepareTestProcessor;
            _directCurrentTest = directCurrentTest;
            _programVersionCheck = programVersionCheck;
            _executeDOTest = executeDOTest;
            _executeDITest = executeDITest;
            _connectionLineCheck = executeConnectionLineCheck;
            _voltageSwitchCircuitTest = voltageSwitchCircuitTest;
            _operationCiruitTest = operationCiruitTest;
            _timeSynchronizationTest = timeSynchronizationTest;
            _powerPluginTest = powerPluginTest;
            _networkPortTest = networkPortTest;
            _testEndPrompt = testEndPrompt;
            _assembly = GetType().Assembly;
            _sDLKeeper = sDLKeeper;
            _targetDeviceKeeper = targetDeviceKeeper;
            _deviceModelKeeper = deviceModelKeeper;
            _configProcessor = configProcessor;
            _gbxmlProcessor = gbXmlProcessor;
            _notifyException = notifyException;
            Units = new ObservableCollection<CombineUnit>();
        }

        private CombineUnit _selectedUnit = null!;
        public CombineUnit SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (_selectedUnit != value)
                {
                    _selectedUnit = value;
                    NotifyOfPropertyChange(() => SelectedUnit);
                }
            }
        }

        public ObservableCollection<CombineUnit> Units { get; private set; }
        public bool? SelectAll { get; set; }
        public string deviceName { get; set; } = null!;
        // 私有字段（实际存储）
        private Local_DB _localDB;

        // 属性（通过字段实现）
        private Local_DB local_DB
        {
            get => _localDB;
            set => _localDB = value;
        }
        public async void GenerateProcess(List<Device> devices)
        {
            try
            {
                Units.Clear();
                _targetDeviceKeeper.SetDevice(devices.FirstOrDefault()!);
                var modelname = _targetDeviceKeeper.TargetDevice.EAIs.FirstOrDefault(E => E.Name.Equals("serNum"));
                var id_unnormal = GetID_Unormal();
                var result = _configProcessor.GetDeviceModel(id_unnormal, ref _localDB);
                if (result.Equals(null) || result.Entity == null)
                {
                    _notifyException.RaiseException(this, $"{result.ErrorMsg}");
                    return;
                }
                _deviceModelKeeper.SetDeviceModel(result.Entity);
                var modelCache = new DeviceModelCache();
                foreach (var LDevice in result.Entity.LDevices)
                {
                    foreach (var DataSet in LDevice.Datasets)
                    {
                        foreach (var data in DataSet.Datas)
                        {
                            modelCache.CreateDes(data.Position, data.Name);
                            modelCache.CreateId(data.Position, data.Name, DataSet.Id);
                        }
                    }
                }
                if (modelname==null)
                {
                    _notifyException.RaiseException(this, $"cdd 模板错误");
                    return;
                }
                var result_dio = _configProcessor.GetDIODefine(modelname.Val);
                if (result_dio.Equals(null) || result_dio.Entity == null)
                {
                    _notifyException.RaiseException(this, $"{result_dio.ErrorMsg}");
                    return;
                }
                foreach (var DIOPort in result_dio.Entity.Ports)
                {
                    foreach (var board in devices.FirstOrDefault().Boards)
                    {
                        foreach (var port in board.Ports)
                        {
                            if (board.Name == DIOPort.board && port.Name == DIOPort.port)
                            {
                                port.PortPair = DIOPort.CollectionId;
                            }
                        }
                    }
                }
                using (Stream stream = _assembly.GetManifestResourceStream("SFTemplateGenerator.MainWindow.Template.gbxml"))
                {
                    GuideBook guideBook = XmlHelper.Deserialize<GuideBook>(stream);
                    guideBook.Device.DeviceModel = _deviceModelKeeper.TargetDeviceModel;
                    deviceName = _sDLKeeper.SDL.Cubicle.Desc + _sDLKeeper.SDL.Cubicle.Type;
                    //修改guideBook
                    await _prepareTestProcessor.FormatPrepareTestAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _analogQuantityInspection.FormatAnalogQuantityInspectionAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _directCurrentTest.FormatDirectCurrentTestAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _programVersionCheck.FormatProgramVersionCheckAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _executeDOTest.FormatExecuteDOAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _executeDITest.FormatExecuteDIAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _connectionLineCheck.FormatConnectionLineCheckAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _voltageSwitchCircuitTest.FormatVoltageSwitchCircuitTestAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _operationCiruitTest.FormatOperationCircuitTestAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _timeSynchronizationTest.FormatTimeSynchronizationTestAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _powerPluginTest.FormatPowerPluginTestAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _networkPortTest.FormatNetworkPortTestAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    await _testEndPrompt.FormatTestEndPromptAsync(_targetDeviceKeeper.TargetDevice, _sDLKeeper.SDL, guideBook);
                    guideBook.Device.SortRecuresely();
                    guideBook.Device.RefreshId();
                    var @uint = new CombineUnit(_targetDeviceKeeper.TargetDevice, guideBook);
                    Units.Add(@uint);
                    SelectAll = false;
                }
            }
            catch (Exception ex)
            {
                _notifyException.RaiseException(this, "发生未知异常.请联系工艺研发部接口人协助处理.");
            }
        }

        public void DeviceCheckBoxChecked(object sender, RoutedEventArgs e)
        {

        }

        public void DeviceCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {

        }

        public void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
        }

        public void SelectAllCheckBoxCheced(object sender, RoutedEventArgs e)
        {

        }

        public void SelectAllCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {

        }

        public async void SelectedTreeNodeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = sender as TreeView;
            CTreeItem item = tree.SelectedItem as CTreeItem;

            if (item != null)
            {
                var _displayDetailsViewModel = IoC.Get<IDisplayDetailsViewModel>();
                _displayDetailsViewModel.Data = item.HostItem;
                await ActivateItemAsync(_displayDetailsViewModel);
            }
        }

        public void TableSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_tree != null)
            {

            }
        }

        public async void SaveTestTemplateButtonClicked(object sender, RoutedEventArgs e)
        {

            if (Units.Any(U => U.Selected))
            {
                var config = PathSaver.Instance.Config;
                if (!Directory.Exists(config.GBxmlFileSavePath))
                {
                    config.GBxmlFileSavePath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
                }
                if (!string.IsNullOrEmpty(config.GBxmlFileSavePath))
                {
                    var devices = from d in Units where d.Selected select d;
                    var file_path = "";
                    foreach (var device in devices)
                    {
                        string filename = $"{deviceName}_{device.Device.Name}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.gbxml";
                        file_path = Path.Combine(config.GBxmlFileSavePath, filename);
                        await _gbxmlProcessor.SaveGbXmlAsync(GetID_Unormal(), local_DB, file_path, device.GuideBook);
                    }
                    System.Windows.Forms.MessageBox.Show($"保存成功！保存路径：\r\n{file_path}", "提示");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("请使用合法的文件路径！", "提示");
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("请选择至少一个设置！", "提示");
            }
        }
        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            var _blankContentViewModel = IoC.Get<IBlankContentViewModel>();
            await ActivateItemAsync(_blankContentViewModel);
            FrameworkElement frameworkElement = view as FrameworkElement;
            if (frameworkElement != null)
            {
                _tree = frameworkElement.FindName("treeView_1") as TreeView;
            }
        }
        private Tuple<string, string> GetID_Unormal()
        {
            var modelid = _targetDeviceKeeper.TargetDevice.Model;
            var unnormalEAI = _targetDeviceKeeper.TargetDevice.EAIs.FirstOrDefault(p => p.Name == "unnormal");
            var unnormal = unnormalEAI == null ? "" : unnormalEAI.Val ?? "";
            return Tuple.Create(modelid, unnormal);
        }
    }
}
