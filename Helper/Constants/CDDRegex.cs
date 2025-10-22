using System.Text.RegularExpressions;

namespace SFTemplateGenerator.Helper.Constants
{
    public static partial class CDDRegex
    {
        public static readonly Regex ACBORAD_REGEX = new Regex(@"交流(板|插件)\d?");
        public static readonly Regex DCBORAD_REGEX = new Regex(@"直流(板|插件)\d?");
        public static readonly Regex DOBORAD_REGEX = new Regex(@"开出(板|插件)\d?");
        public static readonly Regex SIGNALBORAD_REGEX = new Regex(@"信号(板|插件)\d?");
        public static readonly Regex DIBORAD_REGEX = new Regex(@"开入(板|插件)\d?");
        public static readonly Regex EXBORAD_REGEX = new Regex(@"扩展(板|插件)\d?");
        public static readonly Regex POWERBORAD_REGEX = new Regex(@"电源(板|插件)\d?");
        public static readonly Regex OPBORAD_REGEX = new Regex(@"操作(板|插件)\d?");
        public static readonly List<Regex> SWITCHBORAD_REGEX = new() {
            new Regex(@"切换(板|插件)\d?"),
            new Regex(@"^电压切换\d?$"),
        };
        public static readonly List<Regex> MANAGEBORAD_REGEX = new(){
            new Regex(@"管理(板|插件)\d?"),
            new Regex(@"CPU(板|插件)\d?"),
        };
        public static readonly Regex DEVICE_REGEX = new Regex(@"^(\d-)?(\d{1,2})n$");
        public static readonly Regex CEKONG_DEVICE_REGEX = new Regex(@"200F");
    }
}
