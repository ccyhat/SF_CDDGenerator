using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatOperationCircuitTest;
using System.Text.RegularExpressions;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;
namespace SFTemplateGenerator.Processor.Moduels.FormatOperationCircuitTest
{
    public class OperationCircuitProcess_6U : IOperationCirucuitProcess
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        private List<string> _nodename = new();

        public OperationCircuitProcess_6U(
           ITargetDeviceKeeper targetDeviceKeeper,
           ISDLKeeper sDLKeeper
           )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
            _sDLKeeper = sDLKeeper;
        }
        private Dictionary<string, List<OperationDODeviceEnd>> OperationDOObject = new Dictionary<string, List<OperationDODeviceEnd>>();

        private readonly Regex REGEX_TripDOObject = new(@"(tripDO)_(\d{1,2})");
        private readonly List<Regex> REGEX_VoiceBroadcast_Desc = new(){
            new Regex(@"控制回路断线"),
            new Regex(@"合后(\+|\-)"),
        };
        private readonly List<Regex> REGEX_VoiceBroadcast = new(){
            
            new Regex(@"合后(\+|\-)"),
        };
        // 连线终点判断正则表达式
        private readonly List<Regex> ISCONTINUE = new(){
            new Regex(@"\d{1}-\d{1,2}CD"),
            new Regex(@"\d{1,2}LD"),
        };

        public Task OperationCirucuitProcessAsync(SDL sdl, Items root, List<string> NodeName)
        {

            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => OPBORAD_REGEX.Any(R => R.IsMatch(B.Desc)) || DOBORAD_REGEX.Any(R => R.IsMatch(B.Desc))).ToList();
            Dictionary<string, List<OperationDODeviceEnd>> TripDOObject = new Dictionary<string, List<OperationDODeviceEnd>>();
            var regexMappings = new Dictionary<Regex, Dictionary<string, List<OperationDODeviceEnd>>>
            {
                { REGEX_TripDOObject, TripDOObject }
            };
            foreach (var board in boards)
            {
                // 只匹配 tripDO 类型端口
                var ports = board.Ports.Where(p => p.PortPair.Any(pair => pair.StartsWith("tripDO_")));
                foreach (var port in ports)
                {
                    Match match = null;
                    foreach (var mapping in regexMappings)
                    {
                        var regex = mapping.Key;
                        var targetDict = mapping.Value;

                        foreach (var portpair in port.PortPair)
                        {
                            match = regex.Match(portpair);
                            if (match.Success)
                            {
                                // 提取结尾的数字，默认为0
                                string name = match.Groups[2].Success ? match.Groups[2].Value : "0";

                                // 获取或创建对应的分组
                                if (!targetDict.TryGetValue(name, out var group))
                                {
                                    group = new List<OperationDODeviceEnd>();
                                    targetDict[name] = group;
                                }
                                // 添加描述到对应分组
                                group.Add(new OperationDODeviceEnd((_targetDeviceKeeper.TargetDevice, board, port)));
                            }
                        }
                    }
                }
            }
            SwitchOrder(TripDOObject);
            TripDOObject = RemoveNoConnection(TripDOObject);
            Separate_TripDO_By_Des(TripDOObject);
            CreateOperationDOObject(root);


            NodeName.AddRange(_nodename);


            return Task.CompletedTask;
        }


        private void SwitchOrder(Dictionary<string, List<OperationDODeviceEnd>> dict)
        {
            foreach (var key in dict.Keys)
            {

                dict[key].Sort((a, b) =>
                {
                    if (a.StartPort.Item3.PortPair.Count > b.StartPort.Item3.PortPair.Count)
                    {
                        return -1;
                    }
                    else
                    {
                        string a1 = a.StartPort.Item3.PortPair.FirstOrDefault().Split('_').LastOrDefault();
                        string b1 = b.StartPort.Item3.PortPair.FirstOrDefault().Split('_').LastOrDefault();
                        int a2 = int.Parse(a1);
                        int b2 = int.Parse(b1);
                        return a2.CompareTo(b2);
                    }

                });
            }

        }
        private Dictionary<string, List<OperationDODeviceEnd>> RemoveNoConnection(Dictionary<string, List<OperationDODeviceEnd>> dict)
        {
            foreach (var currentKvp in dict)
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
            bool isAPort = core.DeviceA == deviceName && core.BoardA == boardName;
            string AnotherDevice = isAPort ? core.DeviceB : core.DeviceA;
            string AnotherBoard = isAPort ? core.BoardB : core.BoardA;
            string AnotherPort = isAPort ? core.PortB : core.PortA;
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
        private void CreateOperationDOObject(Items root)
        {

            foreach (var pair in OperationDOObject)
            {

                foreach (var TripDO in pair.Value)
                {
                    var regex = REGEX_VoiceBroadcast_Desc.FirstOrDefault(R => R.IsMatch(TripDO.StartPort.Item3.Desc));
                    if (regex != null)
                    {
                        var match = regex.Match(TripDO.StartPort.Item3.Desc);
                        var trip = new OperationDO(pair.Value.FirstOrDefault(), pair.Value.LastOrDefault());
                        CreateVoiceBroadcastSelfTest(root, trip, match);
                        break;
                    }
                }
            }
            CreateSelfTest(root);
        }
        private void Separate_TripDO_By_Des(Dictionary<string, List<OperationDODeviceEnd>> TripDOObject)
        {
            var TripDORegex = new List<Regex>();
            TripDORegex = TripDORegex.Concat(REGEX_VoiceBroadcast_Desc).ToList();

            foreach (var pair in TripDOObject)
            {
                foreach (var TripDO in pair.Value)
                {
                    if (TripDORegex.Any(R => R.IsMatch(TripDO.StartPort.Item3.Desc)))
                    {
                        OperationDOObject[pair.Key] = pair.Value;
                        break;
                    }

                }

            }
            var remaining = TripDOObject
                .Where(pair => !OperationDOObject.ContainsKey(pair.Key))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        /// </summary>
        /// 创建语音播报自测节点
        /// </summary>
        private void CreateVoiceBroadcastSelfTest(Items root, OperationDO TripDOObject, Match match)
        {
            // 克隆模板节点创建语音播报自测提示
            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）"))?.Clone() as Items;
            if (item != null)
            {
                var tempString = $"{match.Value}";
                if (REGEX_VoiceBroadcast.Any(R => R.IsMatch(match.Value)))
                {
                    tempString = $"{TripDOObject.dODeviceEnd1.StartPort.Item2.Name}:{TripDOObject.dODeviceEnd1.StartPort.Item3.Name}节点是合后或手跳";
                }
                
                item.Name = $"{tempString}自测";

                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                if (safety != null)
                {

                    string speakString = $"{tempString}由调试人员自测";

                    safety.Name = speakString;
                    safety.DllCall.CData = $"SpeakString={speakString};ExpectString=是否完成;";

                    // 只保留必要的提示节点
                    item.ItemList = item.ItemList.Where(I => I.Name.Equals(speakString)).ToList();
                }

                root.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateSelfTest(Items root)
        {
            // 克隆模板节点创建自测提示
            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）"))?.Clone() as Items;
            if (item != null)
            {
                item.Name = "操作插件自测";

                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                if (safety != null)
                {
                    safety.Name = "操作插件由调试人员自测";
                    safety.DllCall.CData = "SpeakString=操作插件由调试人员自测;ExpectString=是否完成;";

                    // 只保留必要的提示节点
                    item.ItemList = item.ItemList.Where(I => I.Name.Equals("操作插件由调试人员自测")).ToList();
                }

                root.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
    }
}
