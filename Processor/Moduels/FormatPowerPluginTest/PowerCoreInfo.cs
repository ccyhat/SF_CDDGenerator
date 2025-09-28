using SFTemplateGenerator.Helper.Shares.SDL;


namespace SFTemplateGenerator.Processor.Moduels.FormatPowerPluginTest
{
    public class PowerCoreInfo
    {
        public List<Core> cores = new List<Core>();
        public ValueTuple<Device, Board, Port> StartPort;
        public PowerCoreInfo(ValueTuple<Device, Board, Port> StartPort)
        {
            this.StartPort = StartPort;
        }
        public Tuple<string, string, string> GetEND(SDL sdl)
        {

            string DeviceName = "";
            string BoardName = "";
            string PortName = "";
            var Devices = sdl.Cubicle.Devices.ToList();
            if (cores.Count() == 1)
            {
                var core = cores.FirstOrDefault() ?? null!;
                if (StartPort.Item1.Name == core.DeviceB)
                {
                    DeviceName = core.DeviceA;
                    BoardName = core.BoardA;
                    PortName = core.PortA;
                }
                else
                {
                    DeviceName = core.DeviceB;
                    BoardName = core.BoardB;
                    PortName = core.PortB;
                }

                return new Tuple<string, string, string>(DeviceName, BoardName, PortName);
            }
            else if (cores.Count() >= 2)
            {
                var core = cores.FirstOrDefault() ?? null!;
                var core2 = cores[1] ?? null!;
                if ((core.DeviceB == core2.DeviceB && core.BoardB == core2.BoardB) || (core.DeviceB == core2.DeviceA && core.BoardB == core2.BoardA))
                {
                    DeviceName = core.DeviceA;
                    BoardName = core.BoardA;
                    PortName = core.PortA;
                    var device = Devices.Where(D => D.Name.Equals(DeviceName)).FirstOrDefault()!;
                    if (!device.Class.Equals("TD"))
                    {
                        DeviceName = core.DeviceB;
                        BoardName = core.BoardB;
                        PortName = core.PortB;
                        return new Tuple<string, string, string>(DeviceName, BoardName, PortName);
                    }

                    return new Tuple<string, string, string>(DeviceName, BoardName, PortName);
                }
                else if (core.DeviceA == core2.DeviceA && core.BoardA == core2.BoardA || core.DeviceA == core2.DeviceB && core.BoardA == core2.BoardB)
                {
                    DeviceName = core.DeviceB;
                    BoardName = core.BoardB;
                    PortName = core.PortB;
                    var device = Devices.Where(D => D.Name.Equals(DeviceName)).FirstOrDefault()!;
                    if (!device.Class.Equals("TD"))
                    {
                        DeviceName = core.DeviceA;
                        BoardName = core.BoardA;
                        PortName = core.PortA;
                        return new Tuple<string, string, string>(DeviceName, BoardName, PortName);
                    }

                    return new Tuple<string, string, string>(DeviceName, BoardName, PortName);
                }
                else
                {
                    return new Tuple<string, string, string>("", "", "");
                }
            }
            else
            {
                return new Tuple<string, string, string>("", "", "");
            }
        }
    }
    public class FailPort
    {
        public PowerCoreInfo PowerCoreInfo1;
        public PowerCoreInfo PowerCoreInfo2;
        public FailPort(PowerCoreInfo PowerCoreInfo1, PowerCoreInfo PowerCoreInfo2)
        {
            this.PowerCoreInfo1 = PowerCoreInfo1;
            this.PowerCoreInfo2 = PowerCoreInfo2;
        }
    }
    public class PowerPort
    {
        public PowerCoreInfo PowerCoreInfo1;
        public PowerCoreInfo PowerCoreInfo2;
        public PowerPort(PowerCoreInfo PowerCoreInfo1, PowerCoreInfo PowerCoreInfo2)
        {
            this.PowerCoreInfo1 = PowerCoreInfo1;
            this.PowerCoreInfo2 = PowerCoreInfo2;
        }
        public string getKKName()
        {
            foreach (var core in PowerCoreInfo1.cores.Concat(PowerCoreInfo2.cores))
            {
                if (core.DeviceA.Contains("K"))
                {
                    return core.DeviceA;
                }
                if (core.DeviceB.Contains("K"))
                {
                    return core.DeviceB;
                }
            }
            return string.Empty;
        }
    }
}
