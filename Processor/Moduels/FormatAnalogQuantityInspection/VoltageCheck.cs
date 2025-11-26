using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;
using static SFTemplateGenerator.Helper.Paths.PathSaver;
using static SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection.FormatAnalogQuantityInspection;

namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class VoltageCheck : IVoltageCheck
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        private readonly IDeviceModelKeeper _deviceModelKeeper;
        private readonly ISwitchTest _switchTest;
        private readonly IZeroSequenceVoltageCurrentTest _zeroSequenceVoltageCurrentTest;
        private readonly ISwitchTest_dsAnAin _switchTest_DsAnAin;
        //开关量正则表达式
        private static readonly List<Regex> REGEX_ACPORTS = new List<Regex> { 
            new Regex(@"^(U|3U|I|3I)(h|l)?(a|b|c|n|x|0|j|l)(\d{0,2})[\`\']?$", RegexOptions.IgnoreCase),
            new Regex(@"^(U|3U|I|3I)(h|g|m|l|p|r|z|k)(\d{0,2})(a|b|c|n|x|0|j)[\`\']?$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Uabc = new List<Regex> { 
            new Regex(@"^(U)(a|b|c)(\d{0,2})$", RegexOptions.IgnoreCase),
            new Regex(@"^(U)(h|g|m|l|p|r|z|k)(\d{0,2})(a|b|c)$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Un = new List<Regex> { 
            new Regex(@"^(U)(n)(\d{0,2})$", RegexOptions.IgnoreCase),
            new Regex(@"^(U)(h|g|m|l|p|r|z|k)(\d{0,2})(n)$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Uabc_hat = new List<Regex> { 
            new Regex(@"^(U)(c)(\d{0,2})[\`\']$", RegexOptions.IgnoreCase),
            new Regex(@"^(U)(h|g|m|l|p|r|z|k)(\d{0,2})(c)[\`\']$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Ux = new List<Regex> { 
            new Regex(@"^(3U|U)(0|x|l)(\d{0,2})[\`\']?$", RegexOptions.IgnoreCase),
            new Regex(@"^(3U|U)(h|g|m|l|p|r|z|k)(\d{0,2})(0|x)[\`\']?$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Iabc = new List<Regex> {
            new Regex(@"^(I)(h|l)?(a|b|c)(\d{0,2})$", RegexOptions.IgnoreCase),
            new Regex(@"^(I)(h|g|m|l|p|r|z|k)(\d{0,2})(a|b|c)$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_In = new List<Regex> { 
            new Regex(@"^(I)(n)(\d{0,2})$", RegexOptions.IgnoreCase),
            new Regex(@"^(I)(h|g|m|l|p|r|z|k)(\d{0,2})(n)$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Iabc_hat = new List<Regex> {
            new Regex(@"^(I)(h|l)?(c)(\d{0,2})[\`\']$", RegexOptions.IgnoreCase),
            new Regex(@"^(I)(h|g|m|l|p|r|z|k)(\d{0,2})(c)[\`\']$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_I0 = new List<Regex> { 
            new Regex(@"^(3I|I)(0|x)(\d{0,2})[\`\']?$", RegexOptions.IgnoreCase),
            new Regex(@"^(3I|I)(h|g|m|l|p|r|z|k)(\d{0,2})(0|x)[\`\']?$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Ij = new List<Regex> {
            new Regex(@"^(3I|I)(j)(\d{0,2})[\`\']?$", RegexOptions.IgnoreCase),
            new Regex(@"^(3I|I)(h|g|m|l|p|r|z|k)(\d{0,2})(j)[\`\']?$", RegexOptions.IgnoreCase),
        };
        //guidebook正则表达式
        private static readonly Dictionary<TESTER, Regex> REGEX_TEMPLATE = new Dictionary<TESTER, Regex>() {
            {TESTER.PONOVOTester,new Regex(@"^电压检查（第一组）（博电）$")},
            {TESTER.PONOVOStandardSource,new Regex(@"^电压检查（第一组）（博电）$")},//博电标准源和博电测试仪模板一样
            {TESTER.ONLLYTester,new Regex(@"^电压检查（第一组）（昂立）$")},
        };
        public VoltageCheck(
            ITargetDeviceKeeper targetDeviceKeeper,
            ISDLKeeper sdlKeeper,
            IDeviceModelKeeper deviceModelKeeper,
            ISwitchTest switchTest,
            IZeroSequenceVoltageCurrentTest zeroSequenceVoltageCurrentTest,
            ISwitchTest_dsAnAin switchTest_DsAnAin
            )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
            _sDLKeeper = sdlKeeper;
            _deviceModelKeeper = deviceModelKeeper;
            _switchTest = switchTest;
            _switchTest_DsAnAin = switchTest_DsAnAin;
            _zeroSequenceVoltageCurrentTest = zeroSequenceVoltageCurrentTest;
        }
        public Task VoltageCheckProcess(SDL sdl, Items root, List<string> _nodename)
        {
            //获取交流插件
            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => ACBORAD_REGEX.Any(R=>R.IsMatch(B.Desc))).OrderBy(B => B.Desc).ToList();
            Dictionary<string, ACDeviceUint> dictionary = new Dictionary<string, ACDeviceUint>();
            var dic_flag=GetDescListResult(boards);
            //循环添加
            for (int i = 0; i < boards.Count(); i++)
            {
                var items = GetInfo(sdl, _targetDeviceKeeper.TargetDevice, boards[i], dic_flag);
                foreach (var item in items)
                {
                    if(dictionary.ContainsKey(item.Key))
                    {

                        dictionary[item.Key].Add(item.Value);
                    }
                    else
                    {
                        dictionary.Add(item.Key, item.Value);
                    }                   
                }
            }
            // 用更紧凑的方式按“前缀首次出现顺序 + 数字升序”排序字典
            var keyRegex = new Regex(@"^([A-Za-z]*)(\d+)$");
            var prefixOrder = new List<string>();

            var parsed = dictionary.Keys.Select(k =>
            {
                var m = keyRegex.Match(k);
                var prefix = m.Success ? m.Groups[1].Value : "";
                var num = m.Success && int.TryParse(m.Groups[2].Value, out var n) ? n : int.MaxValue;
                if (!prefixOrder.Contains(prefix)) prefixOrder.Add(prefix); // 记录首次出现顺序
                return new { Key = k, Prefix = prefix, Number = num };
            }).ToList();

            var sortedKeys = parsed
                .OrderBy(p => prefixOrder.IndexOf(p.Prefix))
                .ThenBy(p => p.Number)
                .Select(p => p.Key)
                .ToList();

            dictionary = sortedKeys.ToDictionary(k => k, k => dictionary[k]);

            if (dictionary.Count() == 0)
            {
                Logger.Info($"没有交流线，跳过电压检查");
            }
            else
            {
                bool isdsAin = HasDsAin();
                if (isdsAin)
                {
                    int loopCount = 1;
                    foreach (var dic in dictionary)
                    {
                        var item = root.GetItems().FirstOrDefault(I => REGEX_TEMPLATE[Instance.Config.Tester].IsMatch(I.Name))!.Clone();//获取电压检查模板
                        item.Name = $"电压检查（第{loopCount}组交流）";
                        item.ItemList = item.ItemList.Where(M => !M.Name.Equals("零序电压测试") && !M.Name.Equals("测量电流测试")).ToList();
                        _switchTest.SwitchTestProcess(sdl, item, dic, Instance.Config.Tester);//开关测试
                        AlternatingCurrentLine(item, dic.Value, sdl, loopCount);
                        root.ItemList.Add(item);
                        _nodename.Add(item.Name);
                        loopCount++;
                    }
                }
                else
                {
                    int loopCount = 1;
                    foreach (var dic in dictionary)
                    {
                        var item = root.GetItems().FirstOrDefault(I => REGEX_TEMPLATE[Instance.Config.Tester].IsMatch(I.Name))!.Clone();//获取电压检查模板
                        item.Name = $"电压检查（第{loopCount}组交流）";
                        item.ItemList = item.ItemList.Where(M => !M.Name.Equals("测量电流测试")).ToList();
                        _switchTest_DsAnAin.SwitchTest_dsAnAinProcess(sdl, item, dic, Instance.Config.Tester);//开关测试
                        _zeroSequenceVoltageCurrentTest.ZeroSequenceVoltageCurrentProcess(sdl, item, dic, Instance.Config.Tester);//零序电流测试
                        AlternatingCurrentLine(item, dic.Value, sdl, loopCount);
                        root.ItemList.Add(item);
                        _nodename.Add(item.Name);
                        loopCount++;
                    }
                }
            }
            return Task.CompletedTask;
        }
        private Dictionary<string, ACDeviceUint> GetInfo(SDL sdl, Device target, Board board, Dictionary<string, bool> flag)
        {
            // 预先缓存 Ports，避免多次遍历
            var ports = board.Ports;
            var dictionary = new Dictionary<string, ACDeviceUint>();
            var groupedByTail = new Dictionary<string, List<Port>>();
            foreach (var p in ports)
            {
                var (match, index) = REGEX_ACPORTS
                             .Select((regex, i) => (regex.Match(p.Desc), i))  // 打包：(匹配结果, 索引)
                             .FirstOrDefault(tuple => tuple.Item1.Success);      // 筛选成功的匹配
                // 按顺序尝试匹配
                if (match != null && match.Success)
                {
                    string tailNumber = string.Empty;
                    // 提取尾号
                    if (index == 1)
                    {
                        string tailStr = match.Groups[3].Value;
                        tailNumber = string.IsNullOrEmpty(tailStr) ? "0" : tailStr;
                        tailNumber = match.Groups[1].Value + match.Groups[2].Value + tailNumber;
                    }
                    else
                    {
                        string tailStr = match.Groups[4].Value;
                        tailNumber = string.IsNullOrEmpty(tailStr) ? "0" : tailStr;
                        tailNumber = match.Groups[1].Value + match.Groups[2].Value + tailNumber;
                    }
                             
                    
                    if (flag.ContainsKey(tailNumber))
                    {
                        if (flag[tailNumber])
                        {
                            if (index == 1)
                            {
                                string tailStr = match.Groups[3].Value;
                                tailNumber = string.IsNullOrEmpty(tailStr) ? "1" : (int.Parse(tailStr) + 1).ToString(); 
                                tailNumber =  match.Groups[2].Value + tailNumber;
                            }
                            else
                            {
                                string tailStr = match.Groups[4].Value;
                                tailNumber = string.IsNullOrEmpty(tailStr) ? "1" : (int.Parse(tailStr) + 1).ToString(); 
                                tailNumber =  match.Groups[2].Value + tailNumber;
                            }
                        }
                        else
                        {
                            if (index == 1)
                            {
                                string tailStr = match.Groups[3].Value;
                                tailNumber = string.IsNullOrEmpty(tailStr) ? "1" : tailStr;
                                tailNumber = match.Groups[2].Value + tailNumber;
                            }
                            else
                            {
                                string tailStr = match.Groups[4].Value;
                                tailNumber = string.IsNullOrEmpty(tailStr) ? "1" : tailStr;
                                tailNumber =  match.Groups[2].Value + tailNumber;
                            }
                            
                        }
                    }
                    else
                    {
                        if (index == 1)
                        {
                            string tailStr = match.Groups[3].Value;
                            tailNumber = string.IsNullOrEmpty(tailStr) ? "1" : tailStr;
                            tailNumber = match.Groups[2].Value + tailNumber;
                        }
                        else
                        {
                            string tailStr = match.Groups[4].Value;
                            tailNumber = string.IsNullOrEmpty(tailStr) ? "1" : tailStr;
                            tailNumber = match.Groups[2].Value + tailNumber;
                        }
                        
                    }
                    // 检查字典中是否已存在该尾号的键
                    if (!groupedByTail.ContainsKey(tailNumber))
                    {
                        // 如果不存在，则动态创建新的列表并添加到字典
                        groupedByTail[tailNumber] = new List<Port>();
                    }
                    // 将端口添加到对应的分组
                    groupedByTail[tailNumber].Add(p);
                }
            }
            // 循环结束后去掉空的groupedByTail项目
            var emptyKeys = groupedByTail.Where(kv => kv.Value.Count == 0).Select(kv => kv.Key).ToList();
            foreach (var key in emptyKeys)
            {
                groupedByTail.Remove(key);
            }
            foreach (var group in groupedByTail)
            {
                var u_ports = new List<Port>();
                var u_n_ports = new List<Port>();
                var u_x_ports = new List<Port>();
                var i_ports = new List<Port>();
                var i_n_ports = new List<Port>();
                var i_0_ports = new List<Port>();
                var i_j_ports = new List<Port>();
                foreach (var p in group.Value)
                {
                    if (REGEX_Uabc.Any(R => R.IsMatch(p.Desc))){
                        u_ports.Add(p);
                    }
                    else if (REGEX_Un.Any(R => R.IsMatch(p.Desc))){
                        u_n_ports.Add(p);
                    }
                    else if (REGEX_Uabc_hat.Any(R => R.IsMatch(p.Desc))){
                        u_n_ports.Add(p);
                    }
                    else if (REGEX_Ux.Any(R => R.IsMatch(p.Desc))){
                        u_x_ports.Add(p);
                    }
                    else if (REGEX_Iabc.Any(R => R.IsMatch(p.Desc))){
                        i_ports.Add(p);
                    }
                    else if (REGEX_In.Any(R => R.IsMatch(p.Desc))){
                        i_n_ports.Add(p);
                    }
                    else if (REGEX_Iabc_hat.Any(R => R.IsMatch(p.Desc)) && !i_n_ports.Any()){
                        i_n_ports.Add(p);
                    }
                    else if (REGEX_I0.Any(R => R.IsMatch(p.Desc))){
                        i_0_ports.Add(p);
                    }
                    else if (REGEX_Ij.Any(R => R.IsMatch(p.Desc)))
                    {
                        i_j_ports.Add(p);
                    }
                }
                var info = new ACDeviceUint();
                void AddCores(IEnumerable<Port> portList, List<List<Core>> infoList, List<string> KK_BYQ)
                {
                    foreach (var port in portList)
                    {
                        var KK_BYQ_list = new List<string>();
                        var cores = new List<Core>();
                        GetFarCore(sdl, target, board, port, new List<Core>(), KK_BYQ_list, cores);
                        if (cores.Count > 0)
                        {
                            infoList.Add(cores);
                        }
                        KK_BYQ.AddRange(KK_BYQ_list);
                    }
                    // 直接对原List去重
                    var distinct = KK_BYQ.Distinct().ToList();
                    KK_BYQ.Clear();
                    KK_BYQ.AddRange(distinct);
                }
                AddCores(u_ports, info.Group1, info.KK_BYQ_List1);
                AddCores(u_n_ports, info.Group1, info.KK_BYQ_List1);
                AddCores(u_x_ports, info.Group2, info.KK_BYQ_List2);
                AddCores(i_ports, info.Group3, info.KK_BYQ_List3);
                AddCores(i_n_ports, info.Group3, info.KK_BYQ_List3);
                AddCores(i_0_ports, info.Group4, info.KK_BYQ_List4);
                AddCores(i_j_ports, info.Group5, info.KK_BYQ_List5);
                if (info.Group1.Count == 0 && info.Group2.Count == 0 && info.Group3.Count == 0 && info.Group4.Count == 0 && info.Group5.Count == 0)
                {
                    continue;
                }
                dictionary.Add(group.Key, info);
            }
            return dictionary;
        }
        private void GetFarCore(SDL sdl, Device target, Board board, Port port, List<Core> fliter, List<string> KK_BYQ_list, List<Core> Core_list)
        {
            GetFarCore(sdl, target.Name, board.Name, port.Name, fliter, KK_BYQ_list, Core_list);
            if (Core_list.Count() == 0 && fliter.Count() > 0)
            {
                Core_list.Add(fliter.FirstOrDefault()!);
            }
        }
        private bool GetFarCore(SDL sdl, string deviceName,
        string boardName,
        string portName,
        List<Core> fliter,
        List<string> KK_BYQ_list,
        List<Core> Core_list)
        {
            var Total_cores = sdl.Cubicle.Cores.ToList();
            var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == deviceName)!;
            List<Core> cores = null!;
            if (device.Class.Equals("TD"))
            {
                cores = Total_cores.Except(fliter).Where(c=>c.Class!="接地")
                    .Where(c =>
               (c.DeviceA == deviceName && c.BoardA == boardName) ||
               (c.DeviceB == deviceName && c.BoardB == boardName) &&
               !(c.DeviceA == c.DeviceB && c.BoardA == c.BoardB)).ToList();//排除自己连接自己的短连片
            }
            else
            {
                cores = Total_cores.Except(fliter).Where(c => c.Class!="接地")
                    .Where(c =>
                (c.DeviceA == deviceName && c.BoardA == boardName && c.PortA == portName) ||
                (c.DeviceB == deviceName && c.BoardB == boardName && c.PortB == portName)).ToList();
            }
            if (Check_KK_BYQ_List(KK_BYQ_list))//检查开关和变压器数量
            {
                return true;
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

                Tuple<string, string, string> anotherPort = GetAnotherPort(sdl, core, deviceName, boardName, portName, KK_BYQ_list);
                fliter.Add(core);
                bool flag = GetFarCore(sdl, anotherPort.Item1, anotherPort.Item2, anotherPort.Item3, fliter, KK_BYQ_list, Core_list);
                if (flag)
                {
                    Core_list.Add(core);
                    return true;
                }
            }
            else
            {
                var coresFrist = cores.FirstOrDefault() ?? null!;
                var coresLast = cores.LastOrDefault() ?? null!;
                if(coresFrist.Class== "短连片" && coresLast.Class == "导线")
                {
                    coresFrist= cores.LastOrDefault() ?? null!;
                    coresLast= cores.FirstOrDefault() ?? null!;
                }
                Tuple<string, string, string> anotherPortFirst = GetAnotherPort(sdl, coresFrist, deviceName, boardName, portName, KK_BYQ_list);
                fliter.Add(coresFrist);
                bool flagFirst = GetFarCore(sdl, anotherPortFirst.Item1, anotherPortFirst.Item2, anotherPortFirst.Item3, fliter, KK_BYQ_list, Core_list);
                if (flagFirst)
                {
                    Core_list.Add(coresFrist);
                    return true;
                }
                Tuple<string, string, string> anotherPortLast = GetAnotherPort(sdl, coresLast, deviceName, boardName, portName, KK_BYQ_list);
                fliter.Add(coresLast);
                bool flagLast = GetFarCore(sdl, anotherPortLast.Item1, anotherPortLast.Item2, anotherPortLast.Item3, fliter, KK_BYQ_list, Core_list);
                if (flagLast)
                {
                    Core_list.Add(coresLast);
                    return true;
                }
            }
            return false;
        }
        private Tuple<string, string, string> GetAnotherPort(SDL sdl, Core core, string deviceName, string boardName, string portName, List<string> KK_BYQ_list)
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
            if (device.Class.Equals("KK"))
            {
                if (int.TryParse(AnotherPort, out int portANumber))
                {
                    KK_BYQ_list.Add(AnotherDevice);
                    AnotherPort = (portANumber - 1).ToString();
                }
            }
            else if (device.Class.Equals("PT"))
            {
                KK_BYQ_list.Add(AnotherDevice);
                AnotherPort = AnotherPort.ToUpper();

            }
            else if (device.Class.Equals("IED"))
            {
                if (!device.Name.Equals(_targetDeviceKeeper.TargetDevice.Name))
                {
                    if (int.TryParse(AnotherPort, out int portANumber))
                    {
                        AnotherPort = (portANumber + 1).ToString();
                    }
                }
            }
            Tuple<string, string, string> tuple = new Tuple<string, string, string>(AnotherDevice, AnotherBoard, AnotherPort);
            return tuple;
        }
        private bool IsCorrectEnd(string deviceName, string boardName, string portName)
        {
            // 1. 目标设备自身
            if (deviceName == _targetDeviceKeeper.TargetDevice.Name)
            {
                var board = _targetDeviceKeeper.TargetDevice.Boards.FirstOrDefault(B => B.Name.Equals(boardName));
                if (board != null && ACBORAD_REGEX.Any(R=>R.IsMatch(board.Desc)))
                {
                    var port = board.Ports.FirstOrDefault(P => P.Name.Equals(portName));
                    return port != null && REGEX_Ux.Any(R => R.IsMatch(port.Desc));
                }
                // 目标设备不是交流板，直接视为终端
                return true;
            }
            // 2. 变压器设备直接返回false
            if (deviceName.Contains("BYQ"))
            {
                return false;
            }
            // 3. 其他情况默认true
            return true;
        }
        private bool Check_KK_BYQ_List(List<string> KK_BYQ_list)
        {
            if (KK_BYQ_list.Count(L => L.Contains("KK")) >= 2)
            {
                // 使用LINQ获取所有KK的索引
                var index = KK_BYQ_list
                     .Select((value, index) => new { value, index })  // 同时获取值和索引
                     .Where(item => item.value.Contains("KK"))        // 筛选出值为2的项
                     .Select(item => item.index).ToList();            // 提取索引
                KK_BYQ_list.RemoveAt(index.LastOrDefault());
                return true;
            }
            return false;
        }
        private void AlternatingCurrentLine(Items root, ACDeviceUint info, SDL sdl, int loopcount)
        {
            var safety = root.GetSafetys().FirstOrDefault(S => S.Name.StartsWith("接入交流线"));
            if (safety != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("SpeakString=接交流线：");
                if (info.Group1 != null && info.Group1.Count() > 0)
                {
                    sb.Append(" 电压ABCN接");
                    string deviceName = "";
                    for (int i = 0; i < info.Group1.Count(); i++)
                    {
                        List<Core> cores = info.Group1[i];
                        Tuple<string, string> end_device_board = GetEND(cores);
                        if (deviceName != end_device_board.Item1)
                        {
                            sb.Append($"{end_device_board.Item1}{end_device_board.Item2}");
                            deviceName = end_device_board.Item1;
                        }
                        else
                        {
                            sb.Append($"{end_device_board.Item2}");
                        }
                        if (i < info.Group1.Count() - 1)
                        {
                            sb.Append("、 ");
                        }
                    }
                    sb.Append(".");
                }
                if (info.Group2 != null && info.Group2.Count() > 0)
                {
                    var core = info.Group2.FirstOrDefault().FirstOrDefault() ?? null!;
                    if (core.StartDesc.Contains("0"))
                    {
                        sb.Append("  U0接");
                    }
                    else
                    {
                        sb.Append("  Ux接");
                    }

                    string deviceName = "";
                    for (int i = 0; i < info.Group2.Count(); i++)
                    {
                        List<Core> cores = info.Group2[i];
                        Tuple<string, string> end_device_board = GetEND(cores);
                        if (deviceName != end_device_board.Item1)
                        {
                            sb.Append($"{end_device_board.Item1}{end_device_board.Item2}");
                            deviceName = end_device_board.Item1;
                        }
                        else
                        {
                            sb.Append($"{end_device_board.Item2}");
                        }
                        if (i < info.Group2.Count() - 1)
                        {
                            sb.Append("、 ");
                        }
                    }
                    sb.Append(".");
                }
                if (info.Group3 != null && info.Group3.Count() > 0)
                {
                    sb.Append("  电流ABCN接");
                    string deviceName = "";
                    for (int i = 0; i < info.Group3.Count(); i++)
                    {
                        List<Core> cores = info.Group3[i];
                        Tuple<string, string> end_device_board = GetEND(cores);
                        if (deviceName != end_device_board.Item1)
                        {
                            sb.Append($"{end_device_board.Item1}{end_device_board.Item2}");
                            deviceName = end_device_board.Item1;
                        }
                        else
                        {
                            sb.Append($"{end_device_board.Item2}");
                        }
                        if (i < info.Group3.Count() - 1)
                        {
                            sb.Append("、 ");
                        }
                    }
                    sb.Append(".");
                }
                if (info.Group4 != null && info.Group4.Count() > 0)
                {
                    sb.Append("  I0接");
                    string deviceName = "";
                    for (int i = 0; i < info.Group4.Count(); i++)
                    {
                        List<Core> cores = info.Group4[i];
                        Tuple<string, string> end_device_board = GetEND(cores);
                        if (deviceName != end_device_board.Item1)
                        {
                            sb.Append($"{end_device_board.Item1}{end_device_board.Item2}");
                            deviceName = end_device_board.Item1;
                        }
                        else
                        {
                            sb.Append($"{end_device_board.Item2}");
                        }
                        if (i < info.Group4.Count() - 1)
                        {
                            sb.Append("、 ");
                        }
                    }
                    sb.Append(".");
                }
                if (info.Group5 != null && info.Group5.Count() > 0)
                {
                    sb.Append("  Ij接");
                    string deviceName = "";
                    for (int i = 0; i < info.Group5.Count(); i++)
                    {
                        List<Core> cores = info.Group5[i];
                        Tuple<string, string> end_device_board = GetEND(cores);
                        if (deviceName != end_device_board.Item1)
                        {
                            sb.Append($"{end_device_board.Item1}{end_device_board.Item2}");
                            deviceName = end_device_board.Item1;
                        }
                        else
                        {
                            sb.Append($"{end_device_board.Item2}");
                        }
                        if (i < info.Group5.Count() - 1)
                        {
                            sb.Append("、 ");
                        }
                    }
                    sb.Append(".");
                }
                sb.Append("  ;ExpectString=是否完成;");
                safety.DllCall.CData = sb.ToString();
                safety.Name = $"接入交流线(第{loopcount}组交流)";
            }
        }
        private Tuple<string, string> GetEND(List<Core> cores)
        {
            var Devices = _sDLKeeper.SDL.Cubicle.Devices.ToList();
            string DeviceName = "";
            string BoardName = "";
            if (cores.Count() == 1)
            {
                var core = cores.FirstOrDefault() ?? null!;
                DeviceName = core.DeviceB;
                BoardName = core.BoardB;
                return new Tuple<string, string>(DeviceName, BoardName);
            }
            else if (cores.Count() >= 2)
            {
                var core = cores.FirstOrDefault() ?? null!;
                var core2 = cores[1] ?? null!;
                if ((core.DeviceB == core2.DeviceB && core.BoardB == core2.BoardB) || (core.DeviceB == core2.DeviceA && core.BoardB == core2.BoardA))
                {
                    DeviceName = core.DeviceA;
                    BoardName = core.BoardA;
                    var device = Devices.Where(D => D.Name.Equals(DeviceName)).FirstOrDefault()!;
                    if (!device.Class.Equals("TD"))
                    {
                        DeviceName = core.DeviceB;
                        BoardName = core.BoardB;
                        return new Tuple<string, string>(DeviceName, BoardName);
                    }
                    return new Tuple<string, string>(DeviceName, BoardName);
                }
                else if (core.DeviceA == core2.DeviceA && core.BoardA == core2.BoardA || core.DeviceA == core2.DeviceB && core.BoardA == core2.BoardB)
                {
                    DeviceName = core.DeviceB;
                    BoardName = core.BoardB;
                    var device = Devices.Where(D => D.Name.Equals(DeviceName)).FirstOrDefault()!;
                    if (!device.Class.Equals("TD"))
                    {
                        DeviceName = core.DeviceA;
                        BoardName = core.BoardA;
                        return new Tuple<string, string>(DeviceName, BoardName);
                    }
                    return new Tuple<string, string>(DeviceName, BoardName);
                }
                else
                {
                    return new Tuple<string, string>("", "");
                }
            }
            else
            {
                return new Tuple<string, string>("", "");
            }
        }
        private bool HasDsAin()
        {
            var ldevice = _deviceModelKeeper.TargetDeviceModel.LDevices.FirstOrDefault(L => L.Name == "装置信息");
            if (ldevice == null)
            {
                return false;
            }
            var datset = ldevice.Datasets.FirstOrDefault(D => D.Name == "测量量");
            return datset != null && datset.Datas.Count > 0;
        }
        private Dictionary<string, bool> GetDescListResult(List<Board> boards)
        {
            var Desc_list = new List<string>();
            foreach (var board in boards)
            {
                foreach (var port in board.Ports)
                {                  
                    var regexs = REGEX_Uabc.Concat(REGEX_Iabc).Concat(REGEX_Uabc_hat).Concat(REGEX_Iabc_hat);
                    if (regexs.Any(R => R.IsMatch(port.Desc)))
                    {
                        // 从REGEX_ACPORTS中筛选第一个成功匹配的项，返回 (Match, 索引) 元组
                        var (match, index) = REGEX_ACPORTS
                            .Select((regex, i) => (regex.Match(port.Desc), i))  // 打包：(匹配结果, 索引)
                            .FirstOrDefault(tuple => tuple.Item1.Success);      // 筛选成功的匹配
                        if (match != null && match.Success)
                        {
                            string tailNumber=string.Empty;
                            if (index == 1)
                            {
                                string tailStr = match.Groups[3].Value;
                                tailNumber = string.IsNullOrEmpty(tailStr) ? "0" : tailStr;
                                tailNumber = match.Groups[1].Value + match.Groups[2].Value + tailNumber;
                            }
                            else
                            {
                                string tailStr = match.Groups[4].Value;
                                tailNumber = string.IsNullOrEmpty(tailStr) ? "0" : tailStr;
                                tailNumber = match.Groups[1].Value + match.Groups[2].Value + tailNumber;
                            }
                          
                            if (Desc_list.Contains(tailNumber))
                            {
                                continue;
                            }
                            else
                            {
                                Desc_list.Add(tailNumber);
                            }
                        }
                    }
                }
            }

          var categoryResult = Desc_list
                .GroupBy(elem =>
                {
                    Regex reg = new Regex(@"(U|3U|I|3I)(h|g|m|l|p|r|z|k)?(\d{0,2})");
                    var res = reg.Match(elem);
                    return res.Groups[1].Value + res.Groups[2].Value;
                }
                ).SelectMany(group => group.Select(item => new
                {
                    item,
                    hasZero = group.Any(elem => elem.Contains("0"))
                }))
             .ToDictionary(x => x.item, x => x.hasZero);
            return categoryResult;
        }

    }
}
