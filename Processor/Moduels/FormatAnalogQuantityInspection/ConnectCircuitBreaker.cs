using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{

    public class ConnectCircuitBreaker : IConnectCircuitBreaker
    {
        private static readonly Regex OPBOX_REGEX = new Regex(@"^(\d-)?4n$");
        private static readonly Regex REGEX_OPBOX_Dese = new Regex(@"^组合操作箱$");
        /// <summary>
        /// 跳闸
        /// </summary>       
        private static readonly List<Regex> TRIP_REGEX = new List<Regex>() {
             new Regex (@"^至跳闸线圈$"),
             new Regex (@"^(\d)?TQ[abc]$"),
        };
     
        /// <summary>
        /// 跳闸监视
        /// </summary>
        private static readonly List<Regex> TRIP_MONITORING_REGEX = new List<Regex>() {
            new Regex (@"^断路器跳位监视$"),
            new Regex (@"^(\d)?TW[abc]$"),

        };
        /// <summary>
        /// 合闸
        /// </summary>
        private static readonly List<Regex> CLOSE_REGEX = new List<Regex>() {
            new Regex (@"^至合闸线圈 \(内部防跳\)$"),
            new Regex (@"^(\d)?HQ[abc]$"),

        };
        /// <summary>
        /// 合闸监视
        /// </summary>
        private static readonly List<Regex> CLOSE_MONITORING_REGEX = new List<Regex>() {
             new Regex (@"^断路器合位监视$"),
             new Regex (@"^(\d)?HW[abc]$"),
        };
        /// <summary>
        /// 跳闸
        /// </summary>       
        private static readonly List<Regex> SEPARATED_TRIP_REGEX = new List<Regex>() {
             new Regex (@"^(\d)?TQ$"),
        };

        /// <summary>
        /// 跳闸监视
        /// </summary>
        private static readonly List<Regex> SEPARATED_TRIP_MONITORING_REGEX = new List<Regex>() {
            new Regex (@"^(\d)?TW-$"),

        };
        /// <summary>
        /// 合闸
        /// </summary>
        private static readonly List<Regex> SEPARATED_CLOSE_REGEX = new List<Regex>() {
            new Regex (@"^(\d)?HQ$"),

        };
        /// <summary>
        /// 合闸监视
        /// </summary>
        private static readonly List<Regex> SEPARATED_CLOSE_MONITORING_REGEX = new List<Regex>() {
             new Regex (@"^(\d)?HW-$"),
        };
        /// <summary>
        /// 4Q*D正规
        /// </summary>
        private static readonly List<Regex> QBDREGEX = new List<Regex>() {
             new Regex(@"^(?:\d*-)?4Q(\d*)D"),
        };
        /// <summary>
        /// 连公共端的端子判断正规
        /// </summary>
        private static readonly List<Regex> PUB_REGEX = new List<Regex>() {
             new Regex (@"^第一组操作电源-$"),
             new Regex (@"^-KM(\d)?$"),
             new Regex (@"^电源-$"),
             new Regex (@"^控制电源-$"),
        };
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        public ConnectCircuitBreaker(
            ITargetDeviceKeeper targetDeviceKeeper,
            ISDLKeeper sDLKeeper
            )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
            _sDLKeeper = sDLKeeper;
        }
        public Task ConnectCircuitBreakerAsync(SDL sdl, Items root)
        {
            StringBuilder sb = new StringBuilder();
            var opbox = sdl.Cubicle.Devices.FirstOrDefault(D => OPBOX_REGEX.IsMatch(D.Name));
            if (opbox != null && REGEX_OPBOX_Dese.IsMatch(opbox.Desc))
            {
                //跳闸
                AppendConnectionInfo(opbox, SEPARATED_TRIP_REGEX, SEPARATED_CLOSE_MONITORING_REGEX, sdl, sb);
                //合闸
                AppendConnectionInfo(opbox, SEPARATED_CLOSE_REGEX, SEPARATED_TRIP_MONITORING_REGEX, sdl, sb);
            }
            else
            {
                //跳闸
                AppendConnectionInfo( TRIP_REGEX, CLOSE_MONITORING_REGEX, sdl, sb);
                //合闸
                AppendConnectionInfo( CLOSE_REGEX, TRIP_MONITORING_REGEX, sdl, sb);
            }
           
            //公共端
            var devices = sdl.Cubicle.Devices.Where(D => QBDREGEX.Any(R => R.IsMatch(D.Name))).ToList();
            foreach (var device in devices)
            {
                var board = device.Boards.FirstOrDefault(B => PUB_REGEX.Any(R => R.IsMatch(B.Desc)));
                if (board != null)
                {
                    List<List<Core>> lines = new List<List<Core>>();
                    GetAllLines(_sDLKeeper.SDL, device, board, new Port(), new List<Core>(), lines);
                    List<Core> line = FindKKLine(lines);
                    if (line != null && line.Count > 0)
                    {

                        sb.Append($"{line.LastOrDefault().DeviceA}{line.LastOrDefault().BoardA}接共公端");

                    }
                    else
                    {
                        sb.Append($"{device.Name}{board.Name}接共公端");

                    }
                    sb.Append(".  ");
                    break;
                }
            }
            if (sb.ToString().Length > 0)
            {
                var Speaking = $"SpeakString={sb.ToString()};ExpectString=是否完成;";
                var current = root.ItemList.FirstOrDefault(I => I.Name.StartsWith("接入断路器")) as Safety;
                current.DllCall.CData = Speaking;
                current.Name = "接入断路器";
                var additional = current.Clone();
                additional.Name = "请调试员根据图纸核实防跳逻辑";
                additional.DllCall.CData = $"SpeakString={additional.Name};ExpectString=是否完成;";
                root.ItemList.Add(additional);
            }
            else
            {
                root.ItemList = root.ItemList.Where(I => !I.Name.StartsWith("接入断路器")).ToList();
            }
            return Task.CompletedTask;
        }

        private void AppendConnectionInfo(List<Regex> mainRegex, List<Regex> monitorRegex, SDL sdl, StringBuilder sb)
        {
            var opbox = sdl.Cubicle.Devices.FirstOrDefault(D => OPBOX_REGEX.IsMatch(D.Name));
            if (opbox != null)
            {
                foreach (var board in opbox.Boards.ToList())
                {
                    var portPairs = GetPortPairs(board, mainRegex, monitorRegex);
                    foreach (var (main_port, monitor_port) in portPairs)
                    {
                        ProcessPortPair(sdl, opbox, board, main_port, monitor_port, sb);
                    }
                }
            }
          
            foreach (var board in _targetDeviceKeeper.TargetDevice.Boards.ToList())
            {
                var portPairs = GetPortPairs(board, mainRegex, monitorRegex);
                foreach (var (main_port, monitor_port) in portPairs)
                {
                    ProcessPortPair(sdl, _targetDeviceKeeper.TargetDevice, board, main_port, monitor_port, sb);
                }
            }
        }
        private void AppendConnectionInfo(Device opbox, List<Regex> mainRegex, List<Regex> monitorRegex, SDL sdl, StringBuilder sb)
        {
            List<string> Phase= new List<string>() { "a", "b", "c" };
            var boards = opbox.Boards.ToList();
            List<(Port,Port,Board)> portMessage = new();
            foreach (var board in boards)
            {
                var portPairs=GetPortPairs(board, mainRegex, monitorRegex);
                if (portPairs.Any())
                {
                    portMessage.AddRange(portPairs.Select(p => (p.main_port, p.monitor_port, board)));
                }          
            }
            for(int i = 0; i < portMessage.Count; i++)
            {
                ProcessPortPair(sdl, opbox, portMessage[i].Item3, portMessage[i].Item1, portMessage[i].Item2, sb, Phase[i]);
            }          
        }

        private List<(Port main_port, Port monitor_port)> GetPortPairs(Board board, List<Regex> mainRegex, List<Regex> monitorRegex)
        {
            var main_ports = board.Ports.Where(P => mainRegex.Any(R => R.IsMatch(P.Desc))).ToList();
            var monitor_ports = board.Ports.Where(P => monitorRegex.Any(R => R.IsMatch(P.Desc))).ToList();
            int pairCount = Math.Min(main_ports.Count, monitor_ports.Count);
            var pairs = new List<(Port, Port)>();
            for (int i = 0; i < pairCount; i++)
            {
                pairs.Add((main_ports[i], monitor_ports[i]));
            }
            return pairs;
        }

        private void ProcessPortPair(SDL sdl, Device device, Board board, Port main_port, Port monitor_port, StringBuilder sb)
        {
            var main_totalLines = new List<List<Core>>();
            GetAllLines(sdl, device, board, main_port, new List<Core>(), main_totalLines);
            var monitor_totalLines = new List<List<Core>>();
            GetAllLines(sdl, device, board, monitor_port, new List<Core>(), monitor_totalLines);
            List<Core> main_line = new List<Core>();
            GetTargetLine(main_totalLines, main_line);
            List<Core> monitor_line = new List<Core>();
            GetTargetLine(monitor_totalLines, monitor_line);
            if (main_line.Count == 0 || monitor_line.Count == 0)
            {
                return;
            }
            if (IsConnectedToIED(main_line) && IsConnectedToIED(monitor_line))
            {
                var main = FindNearestPort(main_line);
                var Decs = BreakerMatcher.GetShortCircuitedDesc(main_port.Desc);
                sb.Append($"{main.Item1}{main.Item2}接{Decs}");
                sb.Append(".  ");
            }
            else
            {
                var main = FindNearestPort(main_line);
                var monitor = FindNearestPort(monitor_line);
                var Decs = BreakerMatcher.GetNotShortCircuitedDesc(main_port.Desc);
                sb.Append($"{main.Item1}{main.Item2}和{monitor.Item2}短连接{Decs}");
                sb.Append(".  ");
            }
        }
        private void ProcessPortPair(SDL sdl, Device device, Board board, Port main_port, Port monitor_port, StringBuilder sb,string Phase)
        {
            var main_totalLines = new List<List<Core>>();
            GetAllLines(sdl, device, board, main_port, new List<Core>(), main_totalLines);
            var monitor_totalLines = new List<List<Core>>();
            GetAllLines(sdl, device, board, monitor_port, new List<Core>(), monitor_totalLines);
            List<Core> main_line = new List<Core>();
            GetTargetLine(main_totalLines, main_line);
            List<Core> monitor_line = new List<Core>();
            GetTargetLine(monitor_totalLines, monitor_line);
            if (main_line.Count == 0 || monitor_line.Count == 0)
            {
                return;
            }
            if (IsConnectedToIED(main_line) && IsConnectedToIED(monitor_line))
            {
                var main = FindNearestPort(main_line);
                var Decs = BreakerMatcher.GetShortCircuitedDesc(main_port.Desc + Phase);
                sb.Append($"{main.Item1}{main.Item2}接{Decs}");
                sb.Append(".  ");
            }
            else
            {
                var main = FindNearestPort(main_line);
                var monitor = FindNearestPort(monitor_line);
                var Decs = BreakerMatcher.GetNotShortCircuitedDesc(main_port.Desc+ Phase);
                sb.Append($"{main.Item1}{main.Item2}和{monitor.Item2}短连接{Decs}");
                sb.Append(".  ");
            }
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
                nextCores = totalCores.Except(currentLine).Where(c =>
               ((c.DeviceA == deviceName && c.BoardA == boardName) ||
               (c.DeviceB == deviceName && c.BoardB == boardName)) &&
               !(c.DeviceA == c.DeviceB && c.BoardA == c.BoardB)).ToList();//会有重名的短链片，如2个1CD1
            }
            else
            {
                nextCores = totalCores.Except(currentLine).Where(c =>
                (c.DeviceA == deviceName && c.BoardA == boardName && c.PortA == portName) ||
                (c.DeviceB == deviceName && c.BoardB == boardName && c.PortB == portName)).ToList();
            }
            if (nextCores.Count == 0 || device.Class.Equals("IED"))
            {
                if (currentLine.Count > 0)
                {
                    lines.Add(new List<Core>(currentLine));
                    return;
                }
            }
            foreach (var core in nextCores)
            {
                List<Core> newLine = new List<Core>(currentLine);
                newLine.Add(core);
                Tuple<string, string, string> anotherPort = GetAnotherPort(sdl, core, deviceName, boardName, portName);
                GetAllLines(sdl, anotherPort.Item1, anotherPort.Item2, anotherPort.Item3, newLine, lines);
            }
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
                    // 核心逻辑：单数（奇数）减1，双数（偶数）加1
                    if (portANumber % 2 == 1) // 单数（奇数：1,3,5...）
                    {
                        AnotherPort = (portANumber + 1).ToString();
                    }
                    else // 双数（偶数：2,4,6...）
                    {
                        AnotherPort = (portANumber - 1).ToString();
                    }
                }
            }
            Tuple<string, string, string> tuple = new Tuple<string, string, string>(AnotherDevice, AnotherBoard, AnotherPort);
            return tuple;
        }
        private void GetTargetLine(List<List<Core>> lines, List<Core> line)
        {
            if (lines.FirstOrDefault() != null)
            {
                line.AddRange(lines.FirstOrDefault());
            }
        }
        private Tuple<string, string> FindNearestPort(List<Core> Line)
        {
            var regex = new Regex(@"^(\d-)?(\d*)n$");
            var core = Line.FirstOrDefault() ?? null!;
            if (core != null)
            {
                if (regex.IsMatch(core.DeviceA))
                {
                    return new Tuple<string, string>(core.DeviceB, core.BoardB);
                }
                else
                {
                    return new Tuple<string, string>(core.DeviceA, core.BoardA);
                }

            }
            return new Tuple<string, string>("", "");
        }
        private bool IsConnectedToIED(List<Core> Line)
        {
            var regex = new Regex(@"^(\d-)?(\d*)n$");
            if (Line.Count >= 2)
            {
                var last = Line.LastOrDefault();
                if (last != null)
                {
                    if (regex.IsMatch(last.DeviceA) || regex.IsMatch(last.DeviceB))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private List<Core> FindKKLine(List<List<Core>> Lines)
        {
            foreach (var line in Lines)
            {
                foreach (var core in line)
                {
                    if (core.DeviceA.Contains("LP") || core.DeviceB.Contains("LP"))
                    {
                        return line;
                    }
                }
            }
            return new List<Core>();
        }
    }
}
