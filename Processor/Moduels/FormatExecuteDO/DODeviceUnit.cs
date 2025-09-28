using SFTemplateGenerator.Helper.Shares.SDL;
using System.Text.RegularExpressions;

namespace SFTemplateGenerator.Processor.Moduels.FormatExecuteDO
{

    internal class DODeviceEnd
    {
        private readonly Regex REGEX_4QD = new Regex(@"4Q(\d)?D");
        public List<Core> cores = new List<Core>();
        public ValueTuple<Device, Board, Port> StartPort;
        public DODeviceEnd(ValueTuple<Device, Board, Port> StartPort)
        {
            this.StartPort = StartPort;
#if DEBUG

            ObservationPoint = StartPort.Item1.Name + StartPort.Item2.Name + StartPort.Item3.Name;
#endif

        }
#if DEBUG
        public string ObservationPoint { get; set; }
#endif
        public Tuple<string, string> GetEND(SDL sdl)
        {

            var Devices = sdl.Cubicle.Devices.ToList();
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
        public string GetYBName()
        {
            foreach (var core in cores)
            {
                if (core.DeviceA != null && core.DeviceA.Contains("LP"))
                    return core.DeviceA;
                if (core.DeviceB != null && core.DeviceB.Contains("LP"))
                    return core.DeviceB;
            }
            return "";
        }
        public string Get4QDName()
        {
            foreach (var core in cores)
            {
                if (core.DeviceA != null && REGEX_4QD.IsMatch(core.DeviceA))
                    return core.DeviceA;
                if (core.DeviceB != null && REGEX_4QD.IsMatch(core.DeviceB))
                    return core.DeviceB;
            }
            return "";
        }
        public string GetOtherHighVoltage_QDName()
        {
            foreach (var core in cores)
            {
                if (core.DeviceA != null && core.DeviceA.Contains("QD") && !core.DeviceA.Contains("4QD"))
                    return core.DeviceA;
                if (core.DeviceB != null && core.DeviceB.Contains("QD") && !core.DeviceB.Contains("4QD"))
                    return core.DeviceB;
            }
            return "";
        }

    }
    internal class DOPortPair
    {
        public DODeviceEnd dODeviceEnd1;
        public DODeviceEnd dODeviceEnd2;
        public DOPortPair(DODeviceEnd dODeviceEnd1, DODeviceEnd dODeviceEnd2)
        {
            this.dODeviceEnd1 = dODeviceEnd1;
            this.dODeviceEnd2 = dODeviceEnd2;
        }
        public string GetYBName()
        {
            // 合并所有DODeviceEnd的cores，查找DeviceA或DeviceB包含"LP"的第一个
            foreach (var core in dODeviceEnd1.cores.Concat(dODeviceEnd2.cores))
            {
                if (core.DeviceA != null && core.DeviceA.Contains("LP"))
                    return core.DeviceA;
                if (core.DeviceB != null && core.DeviceB.Contains("LP"))
                    return core.DeviceB;
            }
            return "";
        }
        public string Get4QDName()
        {
            // 合并所有DODeviceEnd的cores，查找DeviceA或DeviceB包含"LP"的第一个
            foreach (var core in dODeviceEnd1.cores.Concat(dODeviceEnd2.cores))
            {
                if (core.DeviceA != null && core.DeviceA.Contains("QD"))
                    return core.DeviceA;
                if (core.DeviceB != null && core.DeviceB.Contains("QD"))
                    return core.DeviceB;
            }
            return "";
        }
        public string GetXDName()
        {
            // 合并所有DODeviceEnd的cores，查找DeviceA或DeviceB包含"LP"的第一个
            foreach (var core in dODeviceEnd1.cores.Concat(dODeviceEnd2.cores))
            {
                if (core.DeviceA != null && core.DeviceA.Contains("XD"))
                    return core.DeviceA;
                if (core.DeviceB != null && core.DeviceB.Contains("XD"))
                    return core.DeviceB;
            }
            return "";
        }
    }


}
