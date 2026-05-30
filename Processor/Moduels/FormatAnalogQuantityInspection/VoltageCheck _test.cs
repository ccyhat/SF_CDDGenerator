using Autofac.Core;
using SFTemplateGenerator.Helper.Container;
using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Shapes;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;
using static SFTemplateGenerator.Helper.Paths.PathSaver;

namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class VoltageCheck_Test : IVoltageCheck
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        private readonly IDeviceModelKeeper _deviceModelKeeper;
        private readonly ISwitchTest _switchTest;
        private readonly IZeroSequenceVoltageCurrentTest _zeroSequenceVoltageCurrentTest;
        private readonly ISwitchTest_dsAnAin _switchTest_DsAnAin;

        private static readonly List<Regex> REGEX_U = new List<Regex> {
            new Regex(@"^(U)(a|b|c)(\d{0,2})$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Un = new List<Regex> {
            new Regex(@"^(U)(n)(\d{0,2})$", RegexOptions.IgnoreCase),
            new Regex(@"^(U)(c)(\d{0,2})(\')$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Ux = new List<Regex> {//Ux
            new Regex(@"^(U)(0|x)(\d{0,2})$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Uxn = new List<Regex> {
            new Regex(@"^(U)(0|x)(\d{0,2})(n|\')$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_I = new List<Regex> {
            new Regex(@"^(I)(a|b|c)(\d{0,2})$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_In = new List<Regex> {
            new Regex(@"^(I)(n)(\d{0,2})$", RegexOptions.IgnoreCase),
            new Regex(@"^(I)(c)(\d{0,2})[\']$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_I0 = new List<Regex> {//I0
            new Regex(@"^(I)(0|x)(\d{0,2})$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_I0n = new List<Regex> {//I0回路
            new Regex(@"^(I)(0|x)(\d{0,2})(\')$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Ic = new List<Regex> {//测量电流
            new Regex(@"^(Ic)(a|b|c)(\d{0,2})$", RegexOptions.IgnoreCase),
        };
        private static readonly List<Regex> REGEX_Icn = new List<Regex> {
            new Regex(@"^(Ic)(c)(\d{0,2})(\')$", RegexOptions.IgnoreCase),
        };
        private static readonly Dictionary<string, List<Regex>> DIC_ACPORTS = new() { 
            //就两种，流入和回路
            {"U",REGEX_U},//电压回路
            {"Un",REGEX_Un},
            {"Ux",REGEX_Ux},//同期电压
            {"Uxn",REGEX_Uxn},
            {"I",REGEX_I},//电流回路
            {"In",REGEX_In},
            {"I0",REGEX_I0},//零序电流
            {"I0n",REGEX_I0n},
            {"Ic",REGEX_Ic},//测量电流回路
            {"Icn",REGEX_Icn},
        };
        

        //guidebook正则表达式
        private static readonly Dictionary<TESTER, Regex> REGEX_TEMPLATE = new Dictionary<TESTER, Regex>() {
            {TESTER.PONOVOTester,new Regex(@"^电压检查（第一组）（博电）$")},
            {TESTER.PONOVOStandardSource,new Regex(@"^电压检查（第一组）（博电）$")},//博电标准源和博电测试仪模板一样
            {TESTER.ONLLYTester,new Regex(@"^电压检查（第一组）（昂立）$")},
        };
        public VoltageCheck_Test(
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
            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => ACBORAD_REGEX.Any(R => R.IsMatch(B.Desc))).OrderBy(B => B.Desc).ToList();
            MakeGroups(boards);
            
            return Task.CompletedTask;
        }
      
        private Dictionary<string, ACPortsUint> MakeGroups(List<Board> boards)
        {
            //找到所有匹配的端口
           Dictionary<ValueTuple<Board, Port>, string> acports = new();
           foreach (var board in boards)
           {
                foreach(var port in board.Ports)
                {
                    var  Keys = DIC_ACPORTS.Where(kv => kv.Value.Any(regex => regex.IsMatch(port.Desc)))
                        .Select(kv => kv.Key)
                        .ToList();
                    if(Keys.Any())
                    {
                        //匹配数字
                        var number = new Regex( @"\d+");
                        var match=number.Match(port.Desc.Replace("I0", ""));
                        if (match.Success)
                        {
                            acports.Add((board,port), $"{Keys.FirstOrDefault()!}");
                        }
                        else
                        {
                            acports.Add((board, port), $"{Keys.FirstOrDefault()!}-@");//空的就这么放
                        }
                    }
                }
            }
            //先组成unit
            Dictionary<string, ACPortsUint> Units = new();
            foreach(var port in acports)
            {
                var key = port.Value;
                if (Units.ContainsKey(key))
                {
                    var lines = new List<List<Core>>();
                    LineHelper.GetAllLines(_sDLKeeper.SDL, _targetDeviceKeeper.TargetDevice, port.Key.Item1, port.Key.Item2, lines);
                    var acport = new ACPorts(LineHelper.GetTargetLine(_sDLKeeper.SDL, lines));
                    Units[key].Add(acport);
                }
                else
                {
                    Units.Add(key, new ACPortsUint (key));
                    var lines=new List<List<Core>>();
                    LineHelper.GetAllLines(_sDLKeeper.SDL,_targetDeviceKeeper.TargetDevice,port.Key.Item1,port.Key.Item2, lines);
                    var acport = new ACPorts(LineHelper.GetTargetLine(_sDLKeeper.SDL, lines));
                    Units[key].Add(acport);
                }
            }

            Dictionary<string, ACPortsUint> res = new();
            return res;
        }

    }
}
