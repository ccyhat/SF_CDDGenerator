using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatOperationCircuitTest;
using SFTemplateGenerator.Processor.Moduels.FormatVoltageSwitchCircuitTest;
using System.Text.RegularExpressions;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;
using static SFTemplateGenerator.Helper.UtilityTools.RegexProcess;

namespace SFTemplateGenerator.Processor.Moduels.FormatOperationCircuitTest
{
    public class OperationCirucuitProcess : IOperationCirucuitProcess
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        private List<string> _nodename = new();
        private readonly Regex REGEX_TripDOObject = new(@"(tripDO)_(\d{1,2})");
        /// <summary>
        /// 跳位继电器
        /// </summary>
        private readonly List<Regex> REGEX_TripDOBreaker = new(){
            new Regex(@"TWJ"),
        };
        /// <summary>
        /// 合位继电器
        /// </summary>
        private readonly List<Regex> REGEX_CloseDOBreaker = new(){
            new Regex(@"HWJ"),
        };
        /// <summary>
        /// 跳闸位置开出
        /// </summary>
        private readonly List<Regex> REGEX_TripPositionDO = new(){
            new Regex(@"跳闸位置开出"),
        };
        /// <summary>
        /// 合闸位置开出
        /// </summary>
        private readonly List<Regex> REGEX_ClosePositionDO = new(){
            new Regex(@"合闸位置开出"),
        };
        /// <summary>
        /// 跳闸
        /// </summary>
        private readonly List<Regex> REGEX_TripDO = new(){
            new Regex(@"保护1跳闸"),
        };
        /// <summary>
        /// 电源消失或断线
        /// </summary>
        private readonly List<Regex> REGEX_PowerFailDO = new(){
            new Regex(@"电源消失或断线"),
        };
        /// <summary>
        ///手跳
        /// </summary>
        private readonly List<Regex> REGEX_STJHHJDO = new(){
            new Regex(@"STJ/HHJ"),
        };
        /// <summary>
        ///中间继电器
        /// </summary>
        private readonly List<Regex> REGEX_ZJDO = new(){
            new Regex(@"ZJ-(\d{1,2})"),
        };
        /// <summary>
        ///位置不匹配或事故音响
        /// </summary>
        private readonly List<Regex> REGEX_PosNotMatchDO = new(){
            new Regex(@"事故音响"),
        };
        /// <summary>
        /// 触发操作箱控制开入点
        /// </summary>
        private readonly List<Regex> REGEX_CloseDI = new(){
            new Regex(@"^合闸开入$"),
        };
        private readonly List<Regex> REGEX_TripDI = new(){
            new Regex(@"手动跳闸开入"),
        };
        private readonly List<Regex> REGEX_Permanent_TripDI = new(){
            new Regex(@"永跳开入"),
        };
        private readonly List<Regex> REGEX_ZJ_CloseDI = new(){
            new Regex(@"ZJ\+"),
        };
        // 连线终点判断正则表达式
        private readonly List<Regex> ISCONTINUE = new(){
            new Regex(@"\d{1}-\d{1,2}CD"),
            new Regex(@"\d{1,2}LD"),
        };
        //直流电源回路
        private static readonly List<Regex> REGEX_4QD = new(){
            new Regex(@"^(?:\d*-)?4Q(\d*)D"),
        };
        private static readonly List<Regex> REGEX_POSITIVE = new(){
            new Regex(@"\+KM"),
        };
        public OperationCirucuitProcess(
            ITargetDeviceKeeper targetDeviceKeeper,
            ISDLKeeper sDLKeeper
            )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
            _sDLKeeper = sDLKeeper;
        }
        private Dictionary<string, List<OperationDODeviceEnd>> OperationDOObject = new Dictionary<string, List<OperationDODeviceEnd>>();
        public Task OperationCirucuitProcessAsync(SDL sdl, Items root, List<string> NodeName)
        {
            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => OPBORAD_REGEX.IsMatch(B.Desc) || DOBORAD_REGEX.IsMatch(B.Desc)).ToList();
            Dictionary<string, List<OperationDODeviceEnd>> TripDOObject = new Dictionary<string, List<OperationDODeviceEnd>>();
            var regexMappings = new Dictionary<Regex, Dictionary<string, List<OperationDODeviceEnd>>>
            {
                { REGEX_TripDOObject, TripDOObject }
            };
            foreach (var board in boards)
            {
                var ports = board.Ports.Where(p => p.PortPair.Any(pair => pair.Contains("tripDO")));
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
        //确保1对多的公共端会被排到前面
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
        private void Separate_TripDO_By_Des(Dictionary<string, List<OperationDODeviceEnd>> TripDOObject)
        {
            var TripDORegex = new List<Regex>();
            TripDORegex = TripDORegex.Concat(REGEX_TripDOBreaker).Concat(REGEX_CloseDOBreaker).ToList();
            TripDORegex = TripDORegex.Concat(REGEX_TripPositionDO).Concat(REGEX_ClosePositionDO).ToList();
            TripDORegex = TripDORegex.Concat(REGEX_TripDO).Concat(REGEX_PowerFailDO).ToList();
            TripDORegex = TripDORegex.Concat(REGEX_STJHHJDO).ToList();
            TripDORegex = TripDORegex.Concat(REGEX_ZJDO).ToList();
            TripDORegex = TripDORegex.Concat(REGEX_PosNotMatchDO).ToList();
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
        private Tuple<string, string> FindPositivePort()
        {
            var devices = _sDLKeeper.SDL.Cubicle.Devices.Where(D => REGEX_4QD.Any(R => R.IsMatch(D.Name))).ToList();
            foreach (var device in devices)
            {
                var board = device.Boards.FirstOrDefault(B => REGEX_POSITIVE.Any(R => R.IsMatch(B.Desc)));
                return new Tuple<string, string>(device.Name, board.Name);
            }
            return new Tuple<string, string>("", "");
        }
        private Tuple<string, string> FindTripDIPort(List<Regex> DIRegex)
        {
            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => OPBORAD_REGEX.IsMatch(B.Desc) || DOBORAD_REGEX.IsMatch(B.Desc)).ToList();
            foreach (var board in boards)
            {
                var port = board.Ports.FirstOrDefault(P => DIRegex.Any(R => R.IsMatch(P.Desc)));
                if (port != null)
                {
                    var cores1 = _sDLKeeper.SDL.Cubicle.Cores.Where(c => c.DeviceA == _targetDeviceKeeper.TargetDevice.Name && c.BoardA == board.Name && c.PortA == port.Name).ToList();
                    var cores2 = _sDLKeeper.SDL.Cubicle.Cores.Where(c => c.DeviceB == _targetDeviceKeeper.TargetDevice.Name && c.BoardB == board.Name && c.PortB == port.Name).ToList();
                    var cores = cores1.Concat(cores2).ToList();
                    if (cores.Count() > 0)
                    {
                        var core = cores.FirstOrDefault() ?? null!;
                        var anotherPort = GetAnotherPort(_sDLKeeper.SDL, core, _targetDeviceKeeper.TargetDevice.Name, board.Name, port.Name);
                        return new Tuple<string, string>(anotherPort.Item1, anotherPort.Item2);
                    }
                }
            }
            return new Tuple<string, string>("", "");
        }
        private void CreateBreaker_HWJTest(Items root, OperationDO CloseDOObject)
        {

            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）")).Clone() as Items;
            var PositiveBoard = FindPositivePort();
            item.Name = "断路器合闸";
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                var tuple1 = CloseDOObject.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                var tuple2 = CloseDOObject.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                var TripDI = FindTripDIPort(REGEX_TripDI);
                var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{TripDI.Item1}{TripDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                var CloseDI = FindTripDIPort(REGEX_CloseDI);
                var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{CloseDI.Item1}{CloseDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            root.ItemList.Add(item);
            _nodename.Add(item.Name);
        }
        private void CreateBreaker_TWJTest(Items root, OperationDO TripDOObject)
        {

            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）")).Clone() as Items;
            var PositiveBoard = FindPositivePort();
            item.Name = "断路器跳闸";
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                var tuple1 = TripDOObject.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                var tuple2 = TripDOObject.dODeviceEnd2.GetEND(_sDLKeeper.SDL);

                var CloseDI = FindTripDIPort(REGEX_CloseDI);
                var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{CloseDI.Item1}{CloseDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                var TripDI = FindTripDIPort(REGEX_TripDI);
                var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{TripDI.Item1}{TripDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            root.ItemList.Add(item);
            _nodename.Add(item.Name);
        }
        private void CreateOperationDOObject(Items root)
        {

            foreach (var pair in OperationDOObject)
            {

                foreach (var TripDO in pair.Value)
                {
                    if (REGEX_CloseDOBreaker.Any(R => R.IsMatch(TripDO.StartPort.Item3.Desc)))
                    {
                        var close = new OperationDO(pair.Value.FirstOrDefault(), pair.Value.LastOrDefault());
                        CreateBreaker_HWJTest(root, close);
                        break;
                    }
                    if (REGEX_TripDOBreaker.Any(R => R.IsMatch(TripDO.StartPort.Item3.Desc)))
                    {
                        var trip = new OperationDO(pair.Value.FirstOrDefault(), pair.Value.LastOrDefault());
                        CreateBreaker_TWJTest(root, trip);
                        break;
                    }

                    if (REGEX_ClosePositionDO.Any(R => R.IsMatch(TripDO.StartPort.Item3.Desc)))
                    {
                        var close = new OperationDO(pair.Value.FirstOrDefault(), pair.Value.LastOrDefault());
                        CreatePosition_HWJTest(root, close);
                        break;
                    }
                    if (REGEX_TripPositionDO.Any(R => R.IsMatch(TripDO.StartPort.Item3.Desc)))
                    {
                        var trip = new OperationDO(pair.Value.FirstOrDefault(), pair.Value.LastOrDefault());
                        CreatePosition_TWJTest(root, trip);
                        break;
                    }
                    if (REGEX_TripDO.Any(R => R.IsMatch(TripDO.StartPort.Item3.Desc)))
                    {
                        var trip = new OperationDO(pair.Value.FirstOrDefault(), pair.Value.LastOrDefault());
                        CreateTripTest(root, trip);
                        break;
                    }
                    if (REGEX_PowerFailDO.Any(R => R.IsMatch(TripDO.StartPort.Item3.Desc)))
                    {
                        var DOobject = new OperationDO(pair.Value.FirstOrDefault(), pair.Value.LastOrDefault());
                        CreatePowerFailTest(root, DOobject);
                        break;
                    }
                    if (REGEX_STJHHJDO.Any(R => R.IsMatch(TripDO.StartPort.Item3.Desc)))
                    {
                        var DOobject = new OperationDO(pair.Value.FirstOrDefault(), pair.Value.LastOrDefault());
                        CreateSTJHHJTest(root, DOobject);
                        break;
                    }
                    if (REGEX_ZJDO.Any(R => R.IsMatch(TripDO.StartPort.Item3.Desc)))
                    {
                        var DOobject = new OperationDO(pair.Value.FirstOrDefault(), pair.Value.LastOrDefault());
                        CreateZJTest(root, DOobject);
                        break;
                    }
                    if (REGEX_PosNotMatchDO.Any(R => R.IsMatch(TripDO.StartPort.Item3.Desc)))
                    {
                        var DOobject = new OperationDO(pair.Value.FirstOrDefault(), pair.Value.LastOrDefault());
                        CreatePosNotMatchTest(root, DOobject);
                        break;
                    }

                }
            }

        }
        private void CreatePosition_HWJTest(Items root, OperationDO CloseDOObject)
        {

            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）")).Clone() as Items;
            var PositiveBoard = FindPositivePort();
            item.Name = "合位";
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                var tuple1 = CloseDOObject.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                var tuple2 = CloseDOObject.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                var TripDI = FindTripDIPort(REGEX_TripDI);
                var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{TripDI.Item1}{TripDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                var CloseDI = FindTripDIPort(REGEX_CloseDI);
                var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{CloseDI.Item1}{CloseDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            root.ItemList.Add(item);
            _nodename.Add(item.Name);
        }
        private void CreatePosition_TWJTest(Items root, OperationDO TripDOObject)
        {
            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）")).Clone() as Items;
            var PositiveBoard = FindPositivePort();
            item.Name = "跳位";
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                var tuple1 = TripDOObject.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                var tuple2 = TripDOObject.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                var CloseDI = FindTripDIPort(REGEX_CloseDI);
                var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{CloseDI.Item1}{CloseDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                var TripDI = FindTripDIPort(REGEX_TripDI);
                var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{TripDI.Item1}{TripDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            root.ItemList.Add(item);
            _nodename.Add(item.Name);
        }
        private void CreateTripTest(Items root, OperationDO TripDOObject)
        {
            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）")).Clone() as Items;
            item.Name = "跳闸";
            var PositiveBoard = FindPositivePort();
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                var tuple1 = TripDOObject.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                var tuple2 = TripDOObject.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                var CloseDI = FindTripDIPort(REGEX_CloseDI);
                var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{CloseDI.Item1}{CloseDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                var TripDI = FindTripDIPort(REGEX_Permanent_TripDI);
                var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{TripDI.Item1}{TripDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            root.ItemList.Add(item);
            _nodename.Add(item.Name);
        }
        private void CreatePowerFailTest(Items root, OperationDO TripDOObject)
        {
            {
                var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）")).Clone() as Items;
                item.Name = "回路断线";
                {
                    var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                    var tuple1 = TripDOObject.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                    var tuple2 = TripDOObject.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                    var PositiveBoard = FindPositivePort();
                    var CloseDI = FindTripDIPort(REGEX_CloseDI);
                    var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{CloseDI.Item1}{CloseDI.Item2}";
                    safety.Name = speakStrings;
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                    var speakStrings = $"断开跳闸回路接线";
                    safety.Name = speakStrings;
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                root.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
            {
                var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）")).Clone() as Items;
                item.Name = "电源消失";
                {
                    var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                    var tuple1 = TripDOObject.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                    var tuple2 = TripDOObject.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                    var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";
                    safety.Name = speakStrings;
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                    List<string> kkbase = new List<string>() { "4K" };//电源消失的空开基本都是4K
                    var kkname = GetModifiedRegexList(_targetDeviceKeeper.TargetDevice.Name, kkbase);
                    var speakStrings = $"断开{kkname.FirstOrDefault()}空开";
                    safety.Name = speakStrings;
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                root.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateSTJHHJTest(Items root, OperationDO TripDOObject)
        {
            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("手跳（点）")).Clone() as Items;
            item.Name = "STJ/HHJ测试";
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                var tuple1 = TripDOObject.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                var tuple2 = TripDOObject.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                var PositiveBoard = FindPositivePort();
                var TripDI = FindTripDIPort(REGEX_TripDI);
                var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{TripDI.Item1}{TripDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                var speakStrings = $"请调试员根据图纸核实节点是否为手跳";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            root.ItemList.Add(item);
            _nodename.Add(item.Name);
        }
        private void CreateZJTest(Items root, OperationDO TripDOObject)
        {
            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("手跳（点）")).Clone() as Items;
            var decs = TripDOObject.dODeviceEnd1.StartPort.Item3.Desc.ToString();
            item.Name = $"{decs}测试";
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                var tuple1 = TripDOObject.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                var tuple2 = TripDOObject.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                var PositiveBoard = FindPositivePort();
                var TripDI = FindTripDIPort(REGEX_ZJ_CloseDI);
                var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{TripDI.Item1}{TripDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            item.ItemList = item.ItemList.Where(I => !I.Name.Equals("提示接入表笔1")).ToList();
            root.ItemList.Add(item);
            _nodename.Add(item.Name);
        }
        private void CreatePosNotMatchTest(Items root, OperationDO TripDOObject)
        {
            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("位置不对应")).Clone() as Items;
            var PositiveBoard = FindPositivePort();
            item.Name = "事故音响(位置不对应)";
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                var tuple1 = TripDOObject.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                var tuple2 = TripDOObject.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                var CloseDI = FindTripDIPort(REGEX_CloseDI);
                var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2},用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{CloseDI.Item1}{CloseDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔1")) as Safety;
                var PermanentTripDI = FindTripDIPort(REGEX_Permanent_TripDI);
                var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{PermanentTripDI.Item1}{PermanentTripDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            {
                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔2")) as Safety;
                var TripDI = FindTripDIPort(REGEX_TripDI);
                var speakStrings = $"用{PositiveBoard.Item1}{PositiveBoard.Item2}点击{TripDI.Item1}{TripDI.Item2}";
                safety.Name = speakStrings;
                safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
            }
            root.ItemList.Add(item);
            _nodename.Add(item.Name);
        }
    }
}
