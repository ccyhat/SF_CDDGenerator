using Caliburn.Micro;
using Microsoft.Win32;
using SFTemplateGenerator.Helper.Paths;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Helper.UtilityTools;
using SFTemplateGenerator.MainWindow.Interfaces;
using SFTemplateGenerator.MainWindow.Shares;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Moduels;
using System.IO;
using System.Windows;
namespace SFTemplateGenerator.MainWindow.ViewModels
{
    public class MainWindowViewModel : Conductor<object>.Collection.OneActive, IMainWindowViewModel, IHandle<GenerateTestTemplateMessage>
    {

        //MODEL
        private readonly Caliburn.Micro.IEventAggregator _eventAggregator;
        private readonly IPraseCDDProcessor _praseCDDProcessor;
        private readonly ISDLKeeper _sdlKeeper;
        private readonly IIEDFilter _IEDFilter;
        private readonly IExceptionBlocker _exceptionBlocker;
        public MainWindowViewModel(
                                   ISettingViewModel settingviewmodel,
                                   IShowCDDViewModel showCDDViewModel,
                                   INotifyExceptionOccuredProcessor notifyExceptionOccuredProcessor,
                                   IPraseCDDProcessor praseCDDProcessor,
                                   Caliburn.Micro.IEventAggregator eventAggregator,
                                   ISDLKeeper sdlKeeper,
                                   IIEDFilter iEDFilter,
                                   IExceptionBlocker exceptionBlocker
        )
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnBackgroundThread(this);
            _sdlKeeper = sdlKeeper;
            _IEDFilter = iEDFilter;
            _exceptionBlocker = exceptionBlocker;
            notifyExceptionOccuredProcessor.OnExceptionOccured += _notifyExceptionOccuredProcessor_OnExceptionOccured;//告警显示
            _praseCDDProcessor = praseCDDProcessor;
        }
        private Visibility _continueButtonVisibility = Visibility.Collapsed;
        public Visibility ContinueButtonVisibility
        {
            get { return _continueButtonVisibility; }
            set => Set(ref _continueButtonVisibility, value);
        }
        private Visibility _exceptionSectionVisibility = Visibility.Collapsed;
        public Visibility ExceptionSectionVisibility
        {
            get { return _exceptionSectionVisibility; }
            set => Set(ref _exceptionSectionVisibility, value);
        }
        private string _exceptionMessage;
        public string ExceptionMessage
        {
            get { return _exceptionMessage; }
            set => Set(ref _exceptionMessage, value);
        }
        private string _isLoading = "Hidden";
        public string IsLoading
        {
            get { return _isLoading; }
            set => Set(ref _isLoading, value);
        }
        public async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ProgramParameters.Instance.Args != null && ProgramParameters.Instance.Args.Length > 0)
            {
                var viewModel = IoC.Get<IProgramLoadingWindowViewModel>();
                await ActivateItemAsync(viewModel);
            }
        }
        protected override async void OnViewLoaded(object view)
        {
            var mainWindow = Application.Current.MainWindow;
            if (ProgramParameters.Instance.Args == null || ProgramParameters.Instance.Args.Length == 0)
            {
                var _homeviewmodel = IoC.Get<IHomeViewModel>();
                await ActivateItemAsync(_homeviewmodel);
            }
            if (mainWindow != null)
            {
                mainWindow.Closed += (source, args) =>
                {
                    Application.Current.Shutdown();
                };
            }
            await _exceptionBlocker.ResetAsync(false);
            base.OnViewLoaded(view);

        }
        public async void OpenCDDFileMenuClicked(object sender, RoutedEventArgs args)
        {
            // 获取配置实例
            var config = PathSaver.Instance.Config;
            // 使用LastFilePath或默认路径
            string initialPath = config.LastFilePath;
            if (string.IsNullOrEmpty(initialPath) || !Directory.Exists(initialPath))
            {
                // 回退到应用程序目录
                initialPath = AppDomain.CurrentDomain.BaseDirectory;
            }
            // 打开文件对话框
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CDD文件 (*.cdd) | *.cdd";
            ofd.InitialDirectory = initialPath;
            if (ofd.ShowDialog() == true)
            {
                // 更新配置中的路径
                config.LastFilePath = Path.GetDirectoryName(ofd.FileName) ?? initialPath;
                // 保存配置
                await PathSaver.Instance.SaveConfigAsync();
                // 处理选中的文件
                if (!string.IsNullOrEmpty(ofd.FileName))
                {
                    SDL sdl = await _praseCDDProcessor.Prase(ofd.FileName);
                    _sdlKeeper.SetSDL(sdl);
                    var ieds = _IEDFilter.GetIEDDevice(sdl).ToArray();
                    var _showCDDViewModel = IoC.Get<IShowCDDViewModel>();
                    _showCDDViewModel.AddDevices(ieds);
                    await ActivateItemAsync(_showCDDViewModel);
                }
            }
        }
        public async void SystemConfigurationMenuClicked(object sender, RoutedEventArgs args)
        {
            var _settingviewmodel = IoC.Get<ISettingViewModel>();
            await ActivateItemAsync(_settingviewmodel);
        }
        public void ExitMenuClicked(object sender, RoutedEventArgs args)
        {
            Application.Current.Shutdown();
        }
        public async void HomeMenuClicked(object sender, RoutedEventArgs args)
        {
            var _homeviewmodel = IoC.Get<IHomeViewModel>();
            await ActivateItemAsync(_homeviewmodel);
        }
        public void ContinueGenerateProcessingButtonClicked(object sender, RoutedEventArgs args)
        {

        }
        public async void StopGenerateProcessingButtonClicked(object sender, RoutedEventArgs args)
        {
            ExceptionSectionVisibility = Visibility.Collapsed;
            ContinueButtonVisibility = Visibility.Collapsed;
            var viewModel = IoC.Get<HomeViewModel>();
            await ActivateItemAsync(viewModel);

        }
        public Task HandleAsync(GenerateTestTemplateMessage message, CancellationToken cancellationToken)
        {
            if (message.IsLoading)
            {
                ShowLoading();
            }
            var _testTemplateManagerViewModel = IoC.Get<ITestTemplateManagerViewModel>();
            _testTemplateManagerViewModel.GenerateProcess(message.Devices.ToList());
            ActivateItemAsync(_testTemplateManagerViewModel);
            if (message.IsLoading)
            {
                HideLoading();
            }
            return Task.CompletedTask;
        }
        private async void _notifyExceptionOccuredProcessor_OnExceptionOccured(object sender, NotifyExceptionOccuredEventArgs e)
        {
            ExceptionSectionVisibility = Visibility.Visible;
            await _exceptionBlocker.ResetAsync();

            if (e.ErrorCode.HasValue)
            {
                ContinueButtonVisibility = Visibility.Visible;
            }
            else
            {
                ContinueButtonVisibility = Visibility.Collapsed;
            }

            ExceptionMessage = e.ExceptionMessage;
        }
        public void ShowLoading()
        {
            IsLoading = "Visible";
        }
        public void HideLoading()
        {
            IsLoading = "Hidden";
        }
    }
}
