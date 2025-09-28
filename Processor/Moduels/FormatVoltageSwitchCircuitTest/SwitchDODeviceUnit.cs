using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Moduels.FormatVoltageSwitchCircuitTest
{
    internal class SwtichDODeviceEnd
    {
        public List<Core> cores = new List<Core>();
        public ValueTuple<Device, Board, Port> StartPort;
        public SwtichDODeviceEnd(ValueTuple<Device, Board, Port> StartPort)
        {
            this.StartPort = StartPort;
#if DEBUG

            ObservationPoint = StartPort.Item1.Name + StartPort.Item2.Name + StartPort.Item3.Name;
#endif

        }
#if DEBUG
        private string ObservationPoint { get; set; }
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
        public string GetYBName()
        {
            // 合并所有DODeviceEnd的cores，查找DeviceA或DeviceB包含"LP"的第一个
            foreach (var core in cores)
            {
                if (core.DeviceA != null && core.DeviceA.Contains("LP"))
                    return core.DeviceA;
                if (core.DeviceB != null && core.DeviceB.Contains("LP"))
                    return core.DeviceB;
            }
            return "";
        }

    }
}
