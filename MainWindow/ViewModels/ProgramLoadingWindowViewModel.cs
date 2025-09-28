using Caliburn.Micro;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Helper.UtilityTools;
using SFTemplateGenerator.MainWindow.Interfaces;
using SFTemplateGenerator.MainWindow.Shares;
using SFTemplateGenerator.Processor.Interfaces;
using System.Text.RegularExpressions;


namespace SFTemplateGenerator.MainWindow.ViewModels
{

    public class ProgramLoadingWindowViewModel : Screen, IProgramLoadingWindowViewModel
    {
        private static readonly Regex MDR = new Regex(@"^-Md=(\d+)$", RegexOptions.IgnoreCase);
        private static readonly Regex DR = new Regex(@"-D=(\S+)$", RegexOptions.IgnoreCase);
        private readonly IPraseCDDProcessor _praseCDDProcessor;
        private readonly ISDLKeeper _sdlKeeper;
        private readonly Caliburn.Micro.IEventAggregator _eventAggregator;

        public ProgramLoadingWindowViewModel(
            IPraseCDDProcessor praseCDDProcessor,
            ISDLKeeper sdlKeeper,

             Caliburn.Micro.IEventAggregator eventAggregator
            )
        {
            _praseCDDProcessor = praseCDDProcessor;
            _sdlKeeper = sdlKeeper;
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnBackgroundThread(this);
            GenerateFromXML();
        }
        public async Task GenerateFromXML()
        {
            string[] selected_devices = new string[0];
            Dictionary<string, dynamic> args_map = new Dictionary<string, dynamic>();
            if (ProgramParameters.Instance.Args.Length > 1)
            {

                /**
                 * 新增多参数适配，
                 * -Md  测试仪、标准源适配
                 * -D   要生成Gbxml的Device适配device名称以英语","分隔device名称间不能有空格
                 * 示例：
                 * -MD=1 -D=1-21n,1-31n
                 */
                List<string> args_temp_list = new List<string>();
                for (int i = 1; i < ProgramParameters.Instance.Args.Length; i++)
                {
                    args_temp_list.Add(ProgramParameters.Instance.Args[i]);
                }
                foreach (var arg in args_temp_list)
                {
                    if (MDR.IsMatch(arg))
                    {
                        int mod = int.Parse(MDR.Match(arg).Groups[1].Value);

                        if (args_map.ContainsKey("MD"))
                        {
                            args_map["MD"] = mod;
                        }
                        else
                        {
                            args_map.Add("MD", mod);
                        }
                    }

                    if (DR.IsMatch(arg))
                    {
                        string str = DR.Match(arg).Groups[1].Value;
                        string[] devices = str.Split(',').Select(S => S.Trim()).ToArray();

                        if (args_map.ContainsKey("D"))
                        {
                            args_map["D"] = devices;
                        }
                        else
                        {
                            args_map.Add("D", devices);
                        }
                    }
                }
            }
            if (args_map.Count > 0 && args_map.ContainsKey("MD"))
            {
                //int mode = args_map["MD"];
                //PathSaver.Instance.Config.TesterMode = mode;
            }
            else
            {
                //PathSaver.Instance.Config.TesterMode = 0;
            }
            SDL sdl = await _praseCDDProcessor.Prase(ProgramParameters.Instance.Args[0]);
            _sdlKeeper.SetSDL(sdl);
            GenerateTestTemplateMessage msg = null!;
            if (args_map.Count > 0 && args_map.ContainsKey("D"))
            {
                selected_devices = args_map["D"];
            }
            if (selected_devices.Length > 0)
            {
                List<Device> device_list = new List<Device>();

                foreach (var device in selected_devices)
                {
                    var d = sdl.Cubicle.Devices.Where(D => D.Name == device).FirstOrDefault();

                    if (d != null)
                    {
                        if (!device_list.Any(D => D.Name == device))
                        {
                            device_list.Add(d);
                        }
                    }
                }
                if (device_list.Count > 0)
                {
                    msg = new GenerateTestTemplateMessage(device_list.ToArray());
                }
            }
            msg.IsLoading = true;
            await _eventAggregator.PublishOnBackgroundThreadAsync(msg);
        }
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }
    }
}
