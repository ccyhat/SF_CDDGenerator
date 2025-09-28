using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatPowerPluginTest;
using System.Text.RegularExpressions;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;

namespace SFTemplateGenerator.Processor.Moduels.FormatPowerPluginTest
{
    public class FailAlarmTest : IFailAlarmTest
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly IDeviceModelKeeper _deviceModelKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        private static readonly List<Regex> REGEX_POWERFAIL = new()
        {
            new Regex(@"FAIL"),
            new Regex(@"装置故障\d"),
        };

        public FailAlarmTest(
            ITargetDeviceKeeper targetDeviceKeeper,
            IDeviceModelKeeper deviceModelKeeper,
            ISDLKeeper sDLKeeper
            )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
            _deviceModelKeeper = deviceModelKeeper;
            _sDLKeeper = sDLKeeper;
        }
        public async Task<bool> FailAlarmTestAsync(SDL sdl, Items root)
        {
            //获取电源插件
            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => POWERBORAD_REGEX.IsMatch(B.Desc)).ToList();

            List<FailPort> failportList = new List<FailPort>();
            List<PowerCoreInfo> powerportList = new List<PowerCoreInfo>();
            //循环添加
            for (int i = 0; i < boards.Count(); i++)
            {

                var failports = boards[i].Ports.Where(P => REGEX_POWERFAIL.Any(R => R.IsMatch(P.Desc)));
                var firstFailPortInfo = new PowerCoreInfo((_targetDeviceKeeper.TargetDevice, boards[i], failports.FirstOrDefault()));
                var lastFailPortInfo = new PowerCoreInfo((_targetDeviceKeeper.TargetDevice, boards[i], failports.LastOrDefault()));
                failportList.Add(new FailPort(firstFailPortInfo, lastFailPortInfo));
                var powerports = boards[i].Ports.Where(P => P.Desc.Contains("IN"));
                foreach (var port in powerports)
                {
                    powerportList.Add(new PowerCoreInfo((_targetDeviceKeeper.TargetDevice, boards[i], port)));
                }

            }
            if (failportList.Count() == 0)
            {
                Logger.Info("未找到电源插件的失电端口");
                return false;
            }
            foreach (var powerport in powerportList)
            {
                GetFarCore(sdl, powerport.StartPort.Item1, powerport.StartPort.Item2, powerport.StartPort.Item3, new List<Core>(), powerport.cores);
            }
            RemoveNoConnection(failportList);
            PowerPort powerPort = new PowerPort(powerportList.FirstOrDefault(), powerportList.LastOrDefault());
            await FailAlarmTestProcess(failportList, powerPort, root);
            return true;
        }
        private void RemoveNoConnection(List<FailPort> failportList)
        {
            foreach (var PORTPair in failportList)
            {
                GetFarCore(_sDLKeeper.SDL, PORTPair.PowerCoreInfo1.StartPort.Item1, PORTPair.PowerCoreInfo1.StartPort.Item2, PORTPair.PowerCoreInfo1.StartPort.Item3, new List<Core>(), PORTPair.PowerCoreInfo1.cores);
                GetFarCore(_sDLKeeper.SDL, PORTPair.PowerCoreInfo2.StartPort.Item1, PORTPair.PowerCoreInfo2.StartPort.Item2, PORTPair.PowerCoreInfo2.StartPort.Item3, new List<Core>(), PORTPair.PowerCoreInfo2.cores);
            }
            // 移除cores为空的节点
            failportList.RemoveAll(di => di.PowerCoreInfo1.cores == null || di.PowerCoreInfo1.cores.Count == 0);
            failportList.RemoveAll(di => di.PowerCoreInfo2.cores == null || di.PowerCoreInfo2.cores.Count == 0);
        }
        private void GetFarCore(SDL sdl, Device target, Board board, Port port, List<Core> fliter, List<Core> Core_list)
        {
            GetFarCore(sdl, target.Name, board.Name, port.Name, fliter, Core_list);
            if (Core_list.Count() == 0 && fliter.Count() > 0)
            {
                Core_list.Add(fliter.FirstOrDefault());
            }
        }
        private bool GetFarCore(SDL sdl, string deviceName,
        string boardName,
        string portName,
        List<Core> fliter,
        List<Core> Core_list)
        {

            var Total_cores = sdl.Cubicle.Cores.ToList();
            var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == deviceName);
            List<Core> cores = null;
            if (device == null)
            {
                return false;
            }
            if (device.Class.Equals("TD"))
            {
                cores = Total_cores.Except(fliter).Where(c =>
               (c.DeviceA == deviceName && c.BoardA == boardName) ||
               (c.DeviceB == deviceName && c.BoardB == boardName)).ToList();
            }
            else
            {
                cores = Total_cores.Except(fliter).Where(c =>
                (c.DeviceA == deviceName && c.BoardA == boardName && c.PortA == portName) ||
                (c.DeviceB == deviceName && c.BoardB == boardName && c.PortB == portName)).ToList();
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
                Tuple<string, string, string> anotherPort = GetAnotherPort(sdl, core, deviceName, boardName, portName);
                fliter.Add(core);
                if (!IsContinue(anotherPort.Item1, core))
                {
                    Core_list.Add(core);
                    return true;
                }
                bool flag = GetFarCore(sdl, anotherPort.Item1, anotherPort.Item2, anotherPort.Item3, fliter, Core_list);
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
                if (coresFrist.Class.Equals("短连片") && coresLast.Class.Equals("导线"))
                {
                    coresLast = cores.FirstOrDefault() ?? null!;
                    coresFrist = cores.LastOrDefault() ?? null!;
                }
                Tuple<string, string, string> anotherPortFirst = GetAnotherPort(sdl, coresFrist, deviceName, boardName, portName);
                fliter.Add(coresFrist);
                bool flagFirst = GetFarCore(sdl, anotherPortFirst.Item1, anotherPortFirst.Item2, anotherPortFirst.Item3, fliter, Core_list);
                if (flagFirst)
                {
                    Core_list.Add(coresFrist);
                    return true;
                }
                Tuple<string, string, string> anotherPortLast = GetAnotherPort(sdl, coresLast, deviceName, boardName, portName);
                fliter.Add(coresLast);
                bool flagLast = GetFarCore(sdl, anotherPortLast.Item1, anotherPortLast.Item2, anotherPortLast.Item3, fliter, Core_list);
                if (flagLast)
                {
                    Core_list.Add(coresLast);
                    return true;
                }

            }
            return false;
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


            Tuple<string, string, string> tuple = new Tuple<string, string, string>(AnotherDevice, AnotherBoard, AnotherPort);
            return tuple;
        }
        private bool IsCorrectEnd(string deviceName,
        string boardName,
        string portName)
        {
            if (deviceName == _targetDeviceKeeper.TargetDevice.Name)
            {

                return false;
            }
            return true;
        }
        private bool IsContinue(string deviceName, Core core)
        {
            if (deviceName.Contains("K"))
            {
                return false;
            }
            return true;
        }


        private Task FailAlarmTestProcess(List<FailPort> failPortList, PowerPort powerPort, Items rootNode)
        {
            var item = rootNode.GetItems().FirstOrDefault(p => p.Name == "失电告警检查");

            foreach (var failPort in failPortList)
            {
                var prepareItem = item.GetItems().FirstOrDefault(I => I.Name == "测试前准备").Clone();
                if (prepareItem != null)
                {
                    var safety = prepareItem.ItemList.FirstOrDefault() as Safety;
                    if (safety != null)
                    {
                        var SpeakString = $"量{failPort.PowerCoreInfo1.GetEND(_sDLKeeper.SDL).Item1}{failPort.PowerCoreInfo1.GetEND(_sDLKeeper.SDL).Item2}和" +
                            $"{failPort.PowerCoreInfo2.GetEND(_sDLKeeper.SDL).Item1}{failPort.PowerCoreInfo2.GetEND(_sDLKeeper.SDL).Item2}";
                        safety.DllCall.CData = $"SpeakString={SpeakString};ExpectString=是否完成;";
                        prepareItem.Name = $"{prepareItem.Name}:{SpeakString}";
                    }
                    item.ItemList.Add(prepareItem);
                }
            }
            foreach (var failPort in failPortList)
            {
                var testItem = item.GetItems().FirstOrDefault(I => I.Name == "断电状态检查").Clone();
                if (testItem != null)
                {

                    var safety = testItem.GetSafetys().FirstOrDefault();
                    if (safety != null)
                    {
                        var SpeakString = $"断开{powerPort.getKKName()}，量{failPort.PowerCoreInfo1.GetEND(_sDLKeeper.SDL).Item1}{failPort.PowerCoreInfo1.GetEND(_sDLKeeper.SDL).Item2}和" +
                      $"{failPort.PowerCoreInfo2.GetEND(_sDLKeeper.SDL).Item1}{failPort.PowerCoreInfo2.GetEND(_sDLKeeper.SDL).Item2}";
                        safety.DllCall.CData = $"SpeakString={SpeakString};ExpectString=是否完成;";
                        testItem.Name = $"{testItem.Name}:{SpeakString}";
                    }
                    item.ItemList.Add(testItem);
                }
            }
            item.ItemList.RemoveAll(I => I.Name == "测试前准备" || I.Name == "断电状态检查");
            return Task.CompletedTask;
        }
    }
}
