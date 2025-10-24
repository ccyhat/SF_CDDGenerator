using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatVoltageSwitchCircuitTest;
using System.Text.RegularExpressions;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;
using static SFTemplateGenerator.Helper.UtilityTools.RegexProcess;
namespace SFTemplateGenerator.Processor.Moduels.FormatVoltageSwitchCircuitTest
{
    public class SwitchCirucuitProcess : ISwitchCirucuitProcess
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        private static readonly List<Regex> REGEX_VOLTAGESWITCH_7Q = new()
        {
            new Regex(@"7Q\d?D"),
        };
        private static readonly List<Regex> REGEX_VOLTAGESWITCH_7Y = new()
        {
            new Regex(@"7Y\d?D"),
        };
        private static readonly List<Regex> REGEX_POSITIVE = new()
        {
            new Regex(@"^装置电源\+$"),
        };
        private readonly List<Regex> ISCONTINUE = new() {
            new Regex(@"\d{1}-\d{1,2}CD"),
            new Regex(@"\d{1,2}LD"),
            new Regex(@"KK"),
        };
        private List<string> _nodename = new();
        public SwitchCirucuitProcess(
            ITargetDeviceKeeper targetDeviceKeeper,
            ISDLKeeper sDLKeeper
        )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
            _sDLKeeper = sDLKeeper;
        }
        List<Regex> REGEX_IBusbarNO = new(){
            new Regex(@"^I母刀闸常开$"),
            new Regex(@"^启动第一组继电器$"),
        };
        List<Regex> REGEX_IIBusbarNO = new(){
            new Regex(@"^II母刀闸常开$"),
            new Regex(@"^启动第二组继电器$"),
        };
        Dictionary<string, List<SwtichDODeviceEnd>> swtichDOObject = new();
        List<Tuple<string, string>> action_pairs = new();
        List<Tuple<string, string>> disapear_pairs = new();
        public Task SwitchCirucuitProcessAsync(SDL sdl, Items root, List<string> NodeName)
        {
            GetSwitchPair(_targetDeviceKeeper.TargetDevice);
            swtichDOObject = RemoveNoConnection(swtichDOObject);
            CreateSwitchPair(root);
            Get7YDPair(root);
            Create7YDNode(root);
            NodeName.AddRange(_nodename);
            return Task.CompletedTask;
        }
        private Tuple<string, string> FindDIPort(List<Regex> BusbarNORegex)
        {
            List<Regex> modifiedRegexList = GetModifiedRegexList(_targetDeviceKeeper.TargetDevice.Name, REGEX_VOLTAGESWITCH_7Q);
            var devices = _sDLKeeper.SDL.Cubicle.Devices.Where(D => modifiedRegexList.Any(R => R.IsMatch(D.Name)));
            foreach (var device in devices)
            {
                foreach (var board in device.Boards)
                {
                    if (BusbarNORegex.Any(R => R.IsMatch(board.Desc)))
                    {
                        return new Tuple<string, string>(device.Name, board.Name);
                    }
                }
            }
            return new Tuple<string, string>("", "");
        }
        private void GetSwitchPair(Device device)
        {
            Regex comm = new Regex(@"^[UV]\w$");
            Dictionary<string, List<SwtichDODeviceEnd>> SwtichComm = new Dictionary<string, List<SwtichDODeviceEnd>>();
            var boards = device.Boards.Where(B => SWITCHBORAD_REGEX.Any(R=>R.IsMatch(B.Desc)));
            foreach (var board in boards)
            {
                foreach (var port in board.Ports)
                {
                    var res = comm.Match(port.Desc);
                    if (res.Success)
                    {
                        if (!SwtichComm.ContainsKey(res.Groups[0].Value))
                        {
                            SwtichDODeviceEnd temp = new SwtichDODeviceEnd(new ValueTuple<Device, Board, Port>(device, board, port));
                            SwtichComm[res.Groups[0].Value] = new List<SwtichDODeviceEnd>() { temp };
                        }
                    }
                }
            }
            Regex Input = new Regex(@"^([UV]\w)\d$");
            foreach (var board in boards)
            {
                foreach (var port in board.Ports)
                {
                    var res = Input.Match(port.Desc);
                    if (res.Success)
                    {
                        if (SwtichComm.ContainsKey(res.Groups[1].Value))
                        {
                            swtichDOObject[res.Groups[0].Value] = new List<SwtichDODeviceEnd>();
                            swtichDOObject[res.Groups[0].Value].Add(new SwtichDODeviceEnd((device, board, port)));
                            swtichDOObject[res.Groups[0].Value].Add(SwtichComm[res.Groups[1].Value].FirstOrDefault());
                        }
                    }
                }
            }
        }
        private void CreateSwitchPair(Items root)
        {
            foreach (var dic in swtichDOObject)
            {
                var tuple = new Tuple<string, List<SwtichDODeviceEnd>>(dic.Key, dic.Value);
                if (dic.Key.Contains("1"))
                {
                    var IBusbarNO = FindDIPort(REGEX_IBusbarNO);
                    CreateNode(root, tuple, IBusbarNO);
                }
                if (dic.Key.Contains("2"))
                {
                    var IIBusbarNO = FindDIPort(REGEX_IIBusbarNO);
                    CreateNode(root, tuple, IIBusbarNO);
                }
            }
        }
        private void Get7YDPair(Items root)
        {
            Regex comm = new Regex(@"^公共端$");
            Regex action = new Regex(@"^切换继电器同时动作$");
            Regex disapear = new Regex(@"^切换继电器直流消失$");
            List<Regex> modifiedRegexList = GetModifiedRegexList(_targetDeviceKeeper.TargetDevice.Name, REGEX_VOLTAGESWITCH_7Y);
            var devices = _sDLKeeper.SDL.Cubicle.Devices.Where(D => modifiedRegexList.Any(R => R.IsMatch(D.Name)));
            foreach (var device in devices)
            {
                foreach (var board in device.Boards)
                {
                    if (comm.IsMatch(board.Desc))
                    {
                        var tuple = new Tuple<string, string>(device.Name, board.Name);
                        action_pairs.Add(tuple);
                        disapear_pairs.Add(tuple);
                    }
                    if (action.IsMatch(board.Desc))
                    {
                        var tuple = new Tuple<string, string>(device.Name, board.Name);
                        action_pairs.Add(tuple);
                    }
                    if (disapear.IsMatch(board.Desc))
                    {
                        var tuple = new Tuple<string, string>(device.Name, board.Name);
                        disapear_pairs.Add(tuple);
                    }
                }
            }
        }
        private void Create7YDNode(Items root)
        {
            Tuple<string, string> IBusbarNO = FindDIPort(REGEX_IBusbarNO);
            Tuple<string, string> IIBusbarNO = FindDIPort(REGEX_IIBusbarNO);
            var item_action = root.GetItems().FirstOrDefault(I => I.Name.Equals("切换器同时动作（合格）")).Clone();
            if (item_action != null&& action_pairs.Any())
            {
                item_action.Name = $"切换器同时动作";
                {
                    var safety = item_action.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                    var tuple1 = action_pairs.FirstOrDefault();
                    var tuple2 = action_pairs.LastOrDefault();
                    var PositiveBoard = FindPositivePort();
                    var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点{IBusbarNO.Item1}{IBusbarNO.Item2}";
                    safety.Name = $"提示:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var safety = item_action.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                    var PositiveBoard = FindPositivePort();
                    var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}点{IIBusbarNO.Item1}{IIBusbarNO.Item2}";
                    safety.Name = $"提示:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var safety = item_action.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔2")) as Safety;
                    var PositiveBoard = FindPositivePort();
                    var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}同时点{IBusbarNO.Item1}{IBusbarNO.Item2}和{IIBusbarNO.Item1}{IIBusbarNO.Item2}";
                    safety.Name = $"提示:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                root.ItemList.Add(item_action);
                _nodename.Add(item_action.Name);
            }
            var item_disapear = root.GetItems().FirstOrDefault(I => I.Name.Equals("切换器直流消失（合格）")).Clone();
            if (item_disapear != null&& disapear_pairs.Any())
            {
                item_disapear.Name = $"切换器直流消失";
                {
                    var safety = item_disapear.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                    var tuple1 = disapear_pairs.FirstOrDefault();
                    var tuple2 = disapear_pairs.LastOrDefault();
                    var PositiveBoard = FindPositivePort();
                    var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";
                    safety.Name = $"提示:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var safety = item_disapear.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                    var PositiveBoard = FindPositivePort();
                    var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}点{IBusbarNO.Item1}{IBusbarNO.Item2}";
                    safety.Name = $"提示:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var safety = item_disapear.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔2")) as Safety;
                    var PositiveBoard = FindPositivePort();
                    var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}点{IIBusbarNO.Item1}{IIBusbarNO.Item2}";
                    safety.Name = $"提示:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                root.ItemList.Add(item_disapear);
                _nodename.Add(item_disapear.Name);
            }
        }
        
       
        private Dictionary<string, List<SwtichDODeviceEnd>> RemoveNoConnection(Dictionary<string, List<SwtichDODeviceEnd>> dict)
        {
            foreach(var currentKvp in dict)
            {
                foreach (var deviceEnd in currentKvp.Value)
                {
                    GetFarCore(_sDLKeeper.SDL, deviceEnd.StartPort.Item1, deviceEnd.StartPort.Item2, deviceEnd.StartPort.Item3, new List<Core>(), deviceEnd.cores);                   
                }
            }
            // 筛选出符合条件的键值对：值列表中cores有效（不为null且数量>0）的元素刚好有2个
            var newDict = dict.Where(kvp =>
                kvp.Value.Count(deviceEnd =>
                    deviceEnd.cores != null && deviceEnd.cores.Count > 0
                ) == 2
            ).ToDictionary();
            return newDict;
        }

        private void CreateNode(Items root, Tuple<string, List<SwtichDODeviceEnd>> SwitchPair, Tuple<string, string> DIBoard)
        {
            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("UAI（合格）")).Clone() as Items;
            if (item != null)
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                var tuple1 = SwitchPair.Item2.FirstOrDefault().GetEND(_sDLKeeper.SDL);
                var tuple2 = SwitchPair.Item2.LastOrDefault().GetEND(_sDLKeeper.SDL);
                var PositiveBoard = FindPositivePort();
                var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点{DIBoard.Item1}{DIBoard.Item2}";
                safety.Name = $"提示:{speakStrings}";
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                item.Name = $"{SwitchPair.Item1}:量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";
                root.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void GetFarCore(SDL sdl, Device target, Board board, Helper.Shares.SDL.Port port, List<Core> fliter, List<Core> Core_list)
        {
            GetFarCore(sdl, target.Name, board.Name, port.Name, fliter, Core_list);
            if (Core_list.Count() == 0 && fliter.Count() > 0)
            {
                Core_list.Add(fliter.FirstOrDefault()!);
            }
        }
        private bool GetFarCore(SDL sdl, string deviceName,
        string boardName,
        string portName,
        List<Core> fliter,
        List<Core> Core_list)
        {
            var Total_cores = sdl.Cubicle.Cores.ToList();
            var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == deviceName)!;
            List<Core> cores = null!;
            if (device.Class.Equals("TD"))
            {
                cores = Total_cores.Except(fliter).Where(c =>
               (c.DeviceA == deviceName && c.BoardA == boardName) ||
               (c.DeviceB == deviceName && c.BoardB == boardName)).ToList();
            }
            else
            {
                cores = Total_cores.Except(fliter).Where(c =>
                (c.DeviceA == deviceName && c.BoardA == boardName && c.PortA == portName) ||
                (c.DeviceB == deviceName && c.BoardB == boardName && c.PortB == portName)).ToList();
            }

            if (cores.Count() == 0)//是否是终端
            {
                if (IsCorrectEnd(deviceName, boardName, portName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (cores.Count() == 1)
            {
                var core = cores.FirstOrDefault() ?? null!;
                Tuple<string, string, string> anotherPort = GetAnotherPort(sdl, core, deviceName, boardName, portName);
                fliter.Add(core);
                if (!IsContinue(anotherPort.Item1, core))
                {
                    return true;
                }
                bool flag = GetFarCore(sdl, anotherPort.Item1, anotherPort.Item2, anotherPort.Item3, fliter, Core_list);
                if (flag)
                {
                    Core_list.Add(core);
                    return true;
                }
            }
            else
            {
                //暂时不考虑分叉情况，如需考虑，参考模拟量检测中的处理方式，使用递归处理
                return true;
            }
            return false;
        }
        private Tuple<string, string, string> GetAnotherPort(SDL sdl, Core core, string deviceName, string boardName, string portName)
        {
            string AnotherDevice = "";
            string AnotherBoard = "";
            string AnotherPort = "";
            if (core.DeviceA == deviceName && core.BoardA == boardName)
            {
                AnotherDevice = core.DeviceB;
                AnotherBoard = core.BoardB;
                AnotherPort = core.PortB;
            }
            else
            {
                AnotherDevice = core.DeviceA;
                AnotherBoard = core.BoardA;
                AnotherPort = core.PortA;
            }
            var cores = sdl.Cubicle.Cores.ToList();
            //检查下一个是不是短连片
            var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == AnotherDevice)!;
            if (device.Class.Equals("YB"))
            {
                if (int.TryParse(AnotherPort, out int portANumber))
                {
                    AnotherPort = (portANumber - 1).ToString();
                }
            }
            Tuple<string, string, string> tuple = new Tuple<string, string, string>(AnotherDevice, AnotherBoard, AnotherPort);
            return tuple;
        }
        private bool IsCorrectEnd(string deviceName,
        string boardName,
        string portName)
        {
            return true;
        }
        private bool IsContinue(string deviceName, Core core)
        {
            if (ISCONTINUE.Any(I => I.IsMatch(deviceName)))
            {
                return false;
            }
            return true;
        }
        private Tuple<string, string> FindPositivePort()
        {
            List<Regex> modifiedRegexList = GetModifiedRegexList(_targetDeviceKeeper.TargetDevice.Name, REGEX_VOLTAGESWITCH_7Q);
            var devices = _sDLKeeper.SDL.Cubicle.Devices.Where(D => modifiedRegexList.Any(R => R.IsMatch(D.Name))).ToList();
            foreach (var device in devices)
            {
                var board = device.Boards.FirstOrDefault(B => REGEX_POSITIVE.Any(R => R.IsMatch(B.Desc)));
                return new Tuple<string, string>(device.Name, board.Name);
            }
            return new Tuple<string, string>("", "");
        }
    }
}