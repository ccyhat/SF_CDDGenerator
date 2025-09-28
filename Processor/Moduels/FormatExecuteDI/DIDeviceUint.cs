using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Moduels.FormatExecuteDI
{
    public class DIDeviceEnd
    {
        public List<Core> cores = new List<Core>();
        public ValueTuple<Device, Board, Port> StartPort;
        public DIDeviceEnd(ValueTuple<Device, Board, Port> StartPort)
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
        public Tuple<string, string, string> GetNear()
        {

            var core = cores.LastOrDefault()!;
            if (core.DeviceA == StartPort.Item1.Name && core.BoardA == StartPort.Item2.Name && core.PortA == StartPort.Item3.Name)
            {
                return new Tuple<string, string, string>(core.DeviceB, core.BoardB, core.PortB);
            }
            else
            {
                return new Tuple<string, string, string>(core.DeviceA, core.BoardA, core.PortA);
            }
        }
        public List<Device> GetDevices(SDL sdl)
        {
            var allDevices = sdl.Cubicle.Devices.ToList();
            List<Device> devices = new List<Device>();
            var reversedCores = new List<Core>(cores);

            reversedCores.Reverse();
            foreach (var core in reversedCores)
            {
                // 获取DeviceA对应的设备（注意：FirstOrDefault()!可能引发空异常，建议处理）
                var deviceA = allDevices.FirstOrDefault(d => d.Name.Equals(core.DeviceA));
                if (deviceA != null && !devices.Contains(deviceA, new DeviceNameEqualityComparer()))
                {
                    devices.Add(deviceA);
                }

                // 获取DeviceB对应的设备
                var deviceB = allDevices.FirstOrDefault(d => d.Name.Equals(core.DeviceB));
                if (deviceB != null && !devices.Contains(deviceB, new DeviceNameEqualityComparer()))
                {
                    devices.Add(deviceB);
                }
            }

            // 最终去重：使用按Name比较的自定义比较器
            devices = devices.Distinct(new DeviceNameEqualityComparer()).ToList();

            return devices;
        }
        public string GetKKName(SDL sdl, string KKClass)
        {
            var allDevices = sdl.Cubicle.Devices.ToList();
            var reversedCores = new List<Core>(cores);

            reversedCores.Reverse();
            foreach (var core in reversedCores)
            {
                var deviceA = allDevices.FirstOrDefault(d => d.Name.Equals(core.DeviceA))!;
                if (deviceA.Class.Equals(KKClass))
                {
                    return deviceA.Name;
                }
                var deviceB = allDevices.FirstOrDefault(d => d.Name.Equals(core.DeviceB))!;
                if (deviceB.Class.Equals(KKClass))
                {
                    return deviceB.Name;
                }
            }
            return String.Empty;
        }
        public bool ContainsPublicBoard(List<string> boardName)
        {
            foreach (var core in cores)
            {
                if (boardName.Any(B => B.Equals(core.DeviceA + core.BoardA) || B.Equals(core.DeviceB + core.BoardB)))
                {
                    return true;
                }

            }
            return false;
        }
    }
    public class DeviceNameEqualityComparer : IEqualityComparer<Device>
    {
        public bool Equals(Device x, Device y)
        {
            // 处理null情况：两个都为null则相等；一个为null则不等
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            // 核心逻辑：Name相同则视为重复（忽略大小写可改为StringComparison.OrdinalIgnoreCase）
            return string.Equals(x.Name, y.Name, StringComparison.Ordinal);
        }

        public int GetHashCode(Device obj)
        {
            // 与Equals逻辑保持一致：用Name的哈希码
            return obj?.Name?.GetHashCode() ?? 0;
        }
    }
}
