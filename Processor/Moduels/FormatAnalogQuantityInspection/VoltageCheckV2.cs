using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using System.Text.RegularExpressions;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;
using static SFTemplateGenerator.Helper.Paths.PathSaver;
using static SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection.FormatAnalogQuantityInspection;

namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class VoltageCheckV2 : IVoltageCheck
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        private readonly IDeviceModelKeeper _deviceModelKeeper;
        private readonly ISwitchTest _switchTest;
        private readonly IZeroSequenceVoltageCurrentTest _zeroSequenceVoltageCurrentTest;
        private readonly ISwitchTest_dsAnAin _switchTest_DsAnAin;
        //开关量正则表达式
        private static readonly Regex REGEX_HIGHVOLTAGE = new Regex(@"(h|g|m|l|p|r)", RegexOptions.IgnoreCase);
        private static readonly List<Regex> REGEX_ACPORTS = new List<Regex> {
            new Regex(@"^(U|3U|I|3I)([abcnx0j])(\d{0,2})[\`\']?$", RegexOptions.IgnoreCase),
            new Regex(@$"^(U|3U|I|3I){REGEX_HIGHVOLTAGE}(\d{{0,2}})([abcnx0j])[\`\']?$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Uabc = new List<Regex> {
            new Regex(@"^(U)([abc])(\d{0,2})$", RegexOptions.IgnoreCase),
            new Regex(@$"^(U){REGEX_HIGHVOLTAGE}(\d{{0,2}})([abc])$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Un = new List<Regex> {
            new Regex(@"^(U)([n])(\d{0,2})$", RegexOptions.IgnoreCase),
            new Regex(@$"^(U){REGEX_HIGHVOLTAGE}(\d{{0,2}})([n])$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Uabc_hat = new List<Regex> {
            new Regex(@"^(U)([c])(\d{0,2})[\`\']$", RegexOptions.IgnoreCase),
            new Regex(@$"^(U){REGEX_HIGHVOLTAGE}(\d{{0,2}})([c])[\`\']$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Ux = new List<Regex> {
            new Regex(@"^(3U|U)([0x])(\d{0,2})[\`\']?$", RegexOptions.IgnoreCase),
            new Regex(@$"^(3U|U){REGEX_HIGHVOLTAGE}(\d{{0,2}})([0x])[\`\']?$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Iabc = new List<Regex> {
            new Regex(@"^(I)([abc])(\d{0,2})$", RegexOptions.IgnoreCase),
            new Regex(@$"^(I){REGEX_HIGHVOLTAGE}(\d{{0,2}})([abc])$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_In = new List<Regex> {
            new Regex(@"^(I)([n])(\d{0,2})$", RegexOptions.IgnoreCase),
            new Regex(@$"^(I){REGEX_HIGHVOLTAGE}(\d{{0,2}})([n])$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Iabc_hat = new List<Regex> {
            new Regex(@"^(I)([c])(\d{0,2})[\`\']$", RegexOptions.IgnoreCase),
            new Regex(@$"^(I){REGEX_HIGHVOLTAGE}(\d{{0,2}})([c])[\`\']$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_I0 = new List<Regex> {
            new Regex(@"^(3I|I)([0x])(\d{0,2})[\`\']?$", RegexOptions.IgnoreCase),
            new Regex(@$"^(3I|I){REGEX_HIGHVOLTAGE}(\d{{0,2}})([0x])[\`\']?$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_IJ = new List<Regex> {
            new Regex(@"^(3I|I)([j])(\d{0,2})[\`\']?$", RegexOptions.IgnoreCase),
            new Regex(@$"^(3I|I){REGEX_HIGHVOLTAGE}(\d{{0,2}})([j])[\`\']?$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> Regex_END = new List<Regex>()
        {
            new Regex(@"Ua\d?'"),
            new Regex(@"Ub\d?'"),
            new Regex(@"Ia\d?'"),
            new Regex(@"Ib\d?'"),

        };
        private static readonly List<Regex> Regex_MID = new List<Regex>()
        {
            new Regex(@"u1'"),
            new Regex(@"u2'"),
            new Regex(@"U1'"),
            new Regex(@"U2'"),
        };
        //guidebook正则表达式
        private static readonly Dictionary<TESTER, Regex> REGEX_TEMPLATE = new Dictionary<TESTER, Regex>() {
            {TESTER.PONOVOTester,new Regex(@"^电压检查（第一组）（博电）$")},
            {TESTER.PONOVOStandardSource,new Regex(@"^电压检查（第一组）（博电）$")},//博电标准源和博电测试仪模板一样
            {TESTER.ONLLYTester,new Regex(@"^电压检查（第一组）（昂立）$")},

        };
        public VoltageCheckV2(
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
            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => ACBORAD_REGEX.IsMatch(B.Desc)).OrderBy(B => B.Desc).ToList();        
            Dictionary<string, ACDeviceUint> dictionary = new Dictionary<string, ACDeviceUint>();
            var dic_flag = GetDescListResult(boards);
            //循环添加
            for (int i = 0; i < boards.Count(); i++)
            {
                var items = GetInfo(sdl, _targetDeviceKeeper.TargetDevice, boards[i],dic_flag);
                foreach (var item in items)
                {
                    dictionary.Add(item.Key, item.Value);
                }
            }
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
                Match match = REGEX_ACPORTS.Select(r => r.Match(p.Desc)).FirstOrDefault(m => m.Success)!;
                // 按顺序尝试匹配
                if (match != null && match.Success)
                {
                    // 提取尾号
                    string tailStr = match.Groups[3].Value;
                    string tailNumber = string.IsNullOrEmpty(tailStr) ? "0" : tailStr;
                    string tailStr1 = match.Groups[1].Value + match.Groups[2].Value;
                    if (REGEX_HIGHVOLTAGE.IsMatch(tailStr1))
                    {
                        tailNumber = tailStr1 + tailNumber;
                    }
                    if (flag.ContainsKey(tailNumber))
                    {
                        if (flag[tailNumber])
                        {
                            tailStr = match.Groups[3].Value;
                            tailNumber = string.IsNullOrEmpty(tailStr) ? "1" : (int.Parse(tailStr) + 1).ToString(); ;
                            tailStr1 = match.Groups[2].Value;
                            if (REGEX_HIGHVOLTAGE.IsMatch(tailStr1))
                            {
                                tailNumber = tailStr1 + tailNumber;
                            }
                        }
                        else
                        {
                            tailStr = match.Groups[3].Value;
                            tailNumber = string.IsNullOrEmpty(tailStr) ? "1" : tailStr;
                            tailStr1 = match.Groups[2].Value;
                            if (REGEX_HIGHVOLTAGE.IsMatch(tailStr1))
                            {
                                tailNumber = tailStr1 + tailNumber;
                            }
                        }
                    }
                    else
                    {
                        tailStr = match.Groups[3].Value;
                        tailNumber = string.IsNullOrEmpty(tailStr) ? "1" : tailStr;
                        tailStr1 = match.Groups[2].Value;
                        if (REGEX_HIGHVOLTAGE.IsMatch(tailStr1))
                        {
                            tailNumber = tailStr1 + tailNumber;
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
                    if (REGEX_Uabc.Any(R => R.IsMatch(p.Desc)))
                    {
                        u_ports.Add(p);
                    }
                    else if (REGEX_Un.Any(R => R.IsMatch(p.Desc)))
                    {
                        u_n_ports.Add(p);
                    }
                    else if (REGEX_Uabc_hat.Any(R => R.IsMatch(p.Desc))) {
                        u_n_ports.Add(p);
                    } 
                    else if (REGEX_Ux.Any(R => R.IsMatch(p.Desc))) {
                        u_x_ports.Add(p);
                    } 
                    else if (REGEX_Iabc.Any(R => R.IsMatch(p.Desc))) {
                        i_ports.Add(p);
                    } 
                    else if (REGEX_In.Any(R => R.IsMatch(p.Desc)))
                    {
                        i_n_ports.Add(p);
                    }
                    else if (REGEX_Iabc_hat.Any(R => R.IsMatch(p.Desc)) && !i_n_ports.Any())
                    {
                        i_n_ports.Add(p);
                    }
                    else if (REGEX_I0.Any(R => R.IsMatch(p.Desc))) {
                        i_0_ports.Add(p);
                    }
                    else if (REGEX_IJ.Any(R => R.IsMatch(p.Desc)))
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
                        var lines = new List<List<Core>>();
                        GetAllLines(sdl, target, board, port, new List<Core>(), lines);
                        var cores = GetTargetLine(lines);
                        if (cores != null && cores.Count > 0)
                        {
                            infoList.Add(cores);
                            var KK = GetKKName(cores);
                            var BYQ = GetBYQName(cores);
                            KK_BYQ.AddRange(KK);
                            KK_BYQ.AddRange(BYQ);
                        }
                    }
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
            if (device.Class.Equals("KK"))
            {
                if (int.TryParse(AnotherPort, out int portANumber))
                {

                    AnotherPort = (portANumber - 1).ToString();
                }
            }
            else if (device.Class.Equals("PT"))
            {

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
                var core = cores.LastOrDefault() ?? null!;
                var core2 = cores[cores.Count() - 2] ?? null!;
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
        private void GetAllLines(SDL sdl, Device target, Board board, Port port, List<Core> line, List<List<Core>> totalLines)
        {
            GetAllLines(sdl, target.Name, board.Name, port.Name, line, totalLines);
        }
        private void GetAllLines(SDL sdl, string deviceName,
        string boardName,
        string portName,
        List<Core> currentLine,
        List<List<Core>> lines)
        {
            var totalCores = sdl.Cubicle.Cores.ToList();
            var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == deviceName)!;
            List<Core> nextCores = null!;

            if (device.Class.Equals("TD"))
            {
                nextCores = totalCores.Except(currentLine).Where(c => c.Class != "接地")
                    .Where(c =>
               ((c.DeviceA == deviceName && c.BoardA == boardName) ||
               (c.DeviceB == deviceName && c.BoardB == boardName)) &&
               !(c.DeviceA == c.DeviceB && c.BoardA == c.BoardB)).ToList();//排除自己连接自己的短连片
            }
            else
            {
                nextCores = totalCores.Except(currentLine).Where(c => c.Class != "接地")
                    .Where(c =>
                (c.DeviceA == deviceName && c.BoardA == boardName && c.PortA == portName) ||
                (c.DeviceB == deviceName && c.BoardB == boardName && c.PortB == portName)).ToList();
            }
            // 封装后的判断
            if (ShouldEndSearch(currentLine, nextCores, device, boardName, portName))
            {
                if (ShouldSaveLine(currentLine, device, boardName, portName))
                {
                    lines.Add(new List<Core>(currentLine));
                }
                return;
            }

            foreach (var core in nextCores)
            {
                List<Core> newLine = new List<Core>(currentLine);
                newLine.Add(core);
                Tuple<string, string, string> anotherPort = GetAnotherPort(sdl, core, deviceName, boardName, portName);
                GetAllLines(sdl, anotherPort.Item1, anotherPort.Item2, anotherPort.Item3, newLine, lines);
            }
        }
        private bool ShouldEndSearch(List<Core> currentLine, List<Core> nextCores, Device device, string boardname, string portname)
        {

            if (nextCores.Count == 0)
            {
                return true;
            }
            if (GetKKName(currentLine).Count() > 1 && device.Class.Equals("KK"))
            {
                return true;
            }
            if (device.Class.Equals("PT"))
            {
                if (GetBYQName(currentLine).Count() > 1)
                {
                    return true;
                }                
            }
            return false;
        }
        private bool ShouldSaveLine(List<Core> currentLine, Device device, string boardname, string portname)
        {
            var portdecs = device.Boards.FirstOrDefault(b => b.Name == boardname)?.Ports.FirstOrDefault(p => p.Name == portname)?.Desc;
            if (device.Class.Equals("IED"))
            {
                if (portdecs != null)
                {
                    if (Regex_END.Any(R => R.IsMatch(portdecs)))
                    {
                        return false;
                    }
                }

            }
            return currentLine.Count > 0;
        }
        private List<string> GetKKName(List<Core> line)
        {
            var KK = line.SelectMany(C => new List<string> { C.DeviceA, C.DeviceB }).Distinct();
            return KK.Where(B => B.Contains("KK")).ToList();
        }
        private List<string> GetBYQName(List<Core> line)
        {
            var BYQ = line.SelectMany(C => new List<string> { C.DeviceA, C.DeviceB }).Distinct();
            return BYQ.Where(B => B.Contains("BYQ")).ToList();
        }
        private List<Core> GetTargetLine(List<List<Core>> lines)
        {
            foreach (var line in lines)
            {
                bool flag = false;
                foreach (var core in line)
                {
                    var deviceA = _sDLKeeper.SDL.Cubicle.Devices.FirstOrDefault(d => d.Name == core.DeviceA);
                    var deviceB = _sDLKeeper.SDL.Cubicle.Devices.FirstOrDefault(d => d.Name == core.DeviceB);
                    if (deviceA != null && deviceB != null)
                    {
                        if (deviceA.Class.Equals("PT") || deviceB.Class.Equals("PT"))
                        {
                            if (Regex_MID.Any(R => R.IsMatch(core.PortA) || R.IsMatch(core.PortB)))
                            {
                                flag = true;
                            }
                        }
                    }
                }
                if (!flag)
                {
                    return line.ToList();
                }

            }
            if (lines.FirstOrDefault() != null)
            {
                return lines.FirstOrDefault().ToList();
            }
            return null;
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
                        Match match = REGEX_ACPORTS.Select(r => r.Match(port.Desc)).FirstOrDefault(m => m.Success)!;
                        if (match != null && match.Success)
                        {
                            string tailStr = match.Groups[3].Value;
                            string tailNumber = string.IsNullOrEmpty(tailStr) ? "0" : tailStr;

                            string tailStr1 = match.Groups[1].Value + match.Groups[2].Value + tailNumber;
                            if (REGEX_HIGHVOLTAGE.IsMatch(tailStr1))
                            {
                                tailNumber = match.Groups[1].Value + match.Groups[2].Value + tailNumber;
                            }
                            else
                            {
                                tailNumber = match.Groups[1].Value + tailNumber;
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
            foreach (var decs in Desc_list)
            {
                Regex reg = new Regex(@$"(U|3U|I|3I){REGEX_HIGHVOLTAGE}?(\d{{0,2}})");
                var res = reg.Match(decs);
            }
            var categoryResult = Desc_list
                .GroupBy(elem => {
                    Regex reg = new Regex(@$"(U|3U|I|3I){REGEX_HIGHVOLTAGE}?(\d{{0,2}})");
                    var res = reg.Match(elem);
                    if (string.IsNullOrEmpty(res.Groups[2].Value))
                    {
                        return res.Groups[1].Value + res.Groups[3].Value;
                    }
                    else
                    {
                        return res.Groups[1].Value + res.Groups[2].Value + res.Groups[3].Value;
                    }
                }
                )
                .ToDictionary(
                    group => group.Key,
                    group => (
                        Elements: group.ToList(),
                        HasZero: group.Any(elem => elem.Contains("0"))
                    )
                );
            Dictionary<string, bool> result = categoryResult
                .SelectMany(cat => cat.Value.Elements.Select(elem => new { elem, cat.Value.HasZero }))
                .ToDictionary(item => item.elem, item => item.HasZero);

            return result;
        }
    }
}
