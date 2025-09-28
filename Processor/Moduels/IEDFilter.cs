using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Shares;
using System.Text.RegularExpressions;

namespace SFTemplateGenerator.Processor.Moduels
{
    public class IEDFilter : IIEDFilter
    {
        public const string SDL = "SDL instance";
        public const string DEVICES = "IED devices";
        private readonly static Regex DEVICE_NAME_REGEX = new Regex(@"^(?:(\d+)-)?(\d+)n$");
        private static readonly List<Regex> DEVICE_Regex = new() {
            new Regex(@"^CSD"),
            new Regex(@"^CSC"),
            new Regex(@"^CSI"),
            new Regex(@"^CSN"),
        };//IED 智能电子设备（Intelligent Electronic Device）
        private static readonly Regex JFZ_Regex = new Regex(@"^JFZ-");//操作箱
        public Device GetFirstDevice(List<Device> devices)
        {
            var device_infos = devices.Select(D => OrgDeviceInfo(D)).Distinct().OrderBy(T => T.Item1).ThenBy(T => T.Item2).ToList();
            return device_infos.FirstOrDefault()!.Item3;
        }
        public IEnumerable<Device> GetIEDDevice(SDL sdl)
        {
            var ieds = sdl.Cubicle.Devices.Where(D => !string.IsNullOrEmpty(D.Class) && DEVICE_Regex.Any(R => R.IsMatch(D.Model))).ToList();
            return ieds;
        }
        public IEnumerable<Device> GetOperationBoxes(SDL sdl)
        {
            var obx = sdl.Cubicle.Devices.Where(D => !string.IsNullOrEmpty(D.Class) && JFZ_Regex.IsMatch(D.Model)).ToArray();
            return obx;
        }
        private Tuple<int, int, Device> OrgDeviceInfo(Device device)
        {
            if (!string.IsNullOrEmpty(device.Name))
            {
                if (DEVICE_NAME_REGEX.IsMatch(device.Name))
                {
                    Match match = DEVICE_NAME_REGEX.Match(device.Name);
                    string first = match.Groups[1].Value;
                    string second = match.Groups[2].Value;
                    int first_value = -1, second_value = -1;
                    if (!string.IsNullOrEmpty(first))
                    {
                        int.TryParse(first, out first_value);
                    }
                    if (!string.IsNullOrEmpty(second))
                    {
                        int.TryParse(second, out second_value);
                    }
                    return Tuple.Create<int, int, Device>(first_value, second_value, device);
                }
                else
                {
                    return Tuple.Create<int, int, Device>(999, 999, device);
                }
            }
            else
            {
                return Tuple.Create<int, int, Device>(999, 999, device);
            }
        }
    }
    public static class IEDFilterContextAssister
    {
        public static Context CreateGetFirstDeviceContext(params Device[] devices)
        {
            return Context.Create(Map(
                new
                {
                    Key = IEDFilter.DEVICES,
                    Value = devices
                }));
        }
        private static IReadOnlyDictionary<string, object> Map(params dynamic[] args)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();
            foreach (var arg in args)
            {
                if (!map.ContainsKey(arg.Key))
                {
                    map.Add(arg.Key, arg.Value);
                }
                else
                {
                    map[arg.Key] = arg.Value;
                }
            }
            return map;
        }
    }
}

