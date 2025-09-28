using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using System.Text;
using static SFTemplateGenerator.Helper.CodingStr.CodingStr;
using static SFTemplateGenerator.Helper.Paths.PathSaver;
using static SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection.FormatAnalogQuantityInspection;

namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class ZeroSequenceVoltageCurrentTest : IZeroSequenceVoltageCurrentTest
    {
        private enum IS_CONTINUE
        {
            Stop = 0,
            Continue = 1
        }
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly IDeviceModelKeeper _deviceModelKeeper;
        public ZeroSequenceVoltageCurrentTest(
           ITargetDeviceKeeper targetDeviceKeeper,

           IDeviceModelKeeper deviceModelKeeper

           )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
            _deviceModelKeeper = deviceModelKeeper;
        }
        // private List<ItemBase> newitems = new List<ItemBase>();
        public Task ZeroSequenceVoltageCurrentProcess(SDL sdl, Items root, KeyValuePair<string, CoreInfo> info, TESTER mode)
        {
            if (mode == TESTER.PONOVOTester || mode == TESTER.PONOVOStandardSource)
            {
                PONOVOZeroSequenceVoltageCurrentProcess(sdl, root, info);
            }
            else if (mode == TESTER.ONLLYTester)
            {
                OnllyZeroSequenceVoltageCurrentProcess(sdl, root, info);
            }
            else
            {
                throw new NotSupportedException("不支持的模式");
            }

            return Task.CompletedTask;
        }
        private void OnllyZeroSequenceVoltageCurrentProcess(SDL sdl, Items root, KeyValuePair<string, CoreInfo> info)
        {
            bool has3I0 = _deviceModelKeeper.deviceModelCache.Has3I0();
            bool has3U0 = _deviceModelKeeper.deviceModelCache.Has3U0();
            if (has3I0 || has3U0)
            {
                var target = root.GetItems().FirstOrDefault(MT => MT.Name.Equals("零序电压测试"));
                InitSafe_Command(target, info);
                var res = NoKK(target, info);

            }
            else
            {
                root.ItemList = root.ItemList.Where(I => !I.Name.Equals("零序电压测试")).ToList();
            }
        }
        private void PONOVOZeroSequenceVoltageCurrentProcess(SDL sdl, Items root, KeyValuePair<string, CoreInfo> info)
        {
            bool has3I0 = _deviceModelKeeper.deviceModelCache.Has3I0();
            bool has3U0 = _deviceModelKeeper.deviceModelCache.Has3U0();
            if (has3I0 || has3U0)
            {
                var target = root.GetMacroTests().FirstOrDefault(MT => MT.Name.Equals("零序电压测试"));
                InitSafe_Command(target, info);
                var res = NoKK(target, info);

            }
            else
            {
                root.ItemList = root.ItemList.Where(I => !I.Name.Equals("零序电压测试")).ToList();
            }
        }
        private void InitSafe_Command(MacroTest target, KeyValuePair<string, CoreInfo> info)
        {
            List<ItemBase> newitems = new List<ItemBase>();
            int cpu_count = _deviceModelKeeper.TargetDeviceModel.LDevices.Count(LD => LD.Name.StartsWith("CPU"));
            if (cpu_count == 1)
            {
                var commands = target.GetCommCMDs();
                var command1 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU1(输出AB，C无电压)")).Clone();
                newitems.Add(command1);
            }
            if (cpu_count == 2)
            {
                var commands = target.GetCommCMDs();
                var command1 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU1(输出AB，C无电压)")).Clone();
                var command2 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU2(输出AB，C无电压)")).Clone();
                newitems.Add(command1);
                newitems.Add(command2);
            }
            target.Safety_CommCMD_List = newitems.ToList();
        }
        private IS_CONTINUE NoKK(MacroTest target, KeyValuePair<string, CoreInfo> info)
        {
            bool has3I0 = _deviceModelKeeper.deviceModelCache.Has3I0();
            bool has3U0 = _deviceModelKeeper.deviceModelCache.Has3U0();
            var commands = target.GetCommCMDs();
            for (int i = 0; i < commands.Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                {
                    int count = 0;
                    if (has3U0 && info.Value.Group1 != null && info.Value.Group1.Count > 0)
                    {
                        var ua = info.Value.Group1[0];
                        var ub = info.Value.Group1[1];
                        if (ua != null)
                        {
                            var source = GetBoardPort(ua);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$mag$f\", v_Ua, -1, vg_MRUErrorRel); ");
                            count++;
                        }
                        if (ub != null)
                        {
                            var source = GetBoardPort(info.Value.Group1[1]);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$mag$f\", v_Ub, -1, vg_MRUErrorRel); ");
                            count++;
                        }
                        sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{_deviceModelKeeper.deviceModelCache.Get3U0($"3U0{info.Key}")}$cVal$mag$f\", v_Ua, -1, vg_MRUErrorRel); ");
                        count++;
                        if (ua != null)
                        {
                            var source = GetBoardPort(ua);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$ang$f\", 0, vg_MRUAngErrorAbs, -1);  ");
                            count++;
                        }
                        if (ub != null)
                        {
                            var source = GetBoardPort(ub);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$ang$f\", -120, vg_MRUAngErrorAbs, -1);");
                            count++;
                        }
                        sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{_deviceModelKeeper.deviceModelCache.Get3U0($"3U0{info.Key}")}$cVal$ang$f\", -60, vg_MRUAngErrorAbs, -1);");
                        count++;
                    }

                    if (has3I0 && info.Value.Group3 != null && info.Value.Group3.Count > 0)
                    {
                        var ia = info.Value.Group3[0];
                        var ib = info.Value.Group3[1];
                        if (ia != null)
                        {
                            var source = GetBoardPort(ia);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$mag$f\", v_Ia, -1, vg_MRIErrorRel); ");
                            count++;
                        }
                        if (ib != null)
                        {
                            var source = GetBoardPort(ib);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$mag$f\", v_Ib, -1, vg_MRIErrorRel);");
                            count++;
                        }
                        sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{_deviceModelKeeper.deviceModelCache.Get3I0($"3I0{info.Key}")}$cVal$mag$f\", v_Ia, -1, vg_MRIErrorRel);");
                        count++;
                        if (ia != null)
                        {
                            var source = GetBoardPort(ia);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$ang$f\", 0, vg_MRIAngErrorAbs, -1); ");
                            count++;
                        }
                        if (ib != null)
                        {
                            var source = GetBoardPort(ib);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$ang$f\", -120, vg_MRIAngErrorAbs, -1);");
                            count++;
                        }
                        sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{_deviceModelKeeper.deviceModelCache.Get3I0($"3I0{info.Key}")}$cVal$ang$f\", -60, vg_MRIAngErrorAbs, -1);");
                        count++;

                    }

                    sb.AppendLine();
                    sb.Append($@"if(nRsltJdg=={count}) then");
                }
                commands[i].CMD.DataSetPath = $"CPU{i + 1}$dsRelayAin";
                commands[i].ScriptResult.CData = $"{SCRIPT_4_HEAD}\n{sb.ToString()}\n{SCRIPT_4_END}";
            }

            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() == 0 && info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() == 0)
            {
                return IS_CONTINUE.Stop;
            }
            else
            {
                return IS_CONTINUE.Continue;
            }
        }
        private void InitSafe_Command(Items target, KeyValuePair<string, CoreInfo> info)
        {
            List<ItemBase> newitems = new List<ItemBase>();
            int cpu_count = _deviceModelKeeper.TargetDeviceModel.LDevices.Count(LD => LD.Name.StartsWith("CPU"));
            if (cpu_count == 1)
            {
                var commands = target.GetCommCMDs();
                var command1 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU1(输出AB，C无电压)")).Clone();
                newitems.Add(command1);
            }
            if (cpu_count == 2)
            {
                var commands = target.GetCommCMDs();
                var command1 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU1(输出AB，C无电压)")).Clone();
                var command2 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU2(输出AB，C无电压)")).Clone();
                newitems.Add(command1);
                newitems.Add(command2);
            }
            target.ItemList = newitems.ToList();
        }
        private IS_CONTINUE NoKK(Items target, KeyValuePair<string, CoreInfo> info)
        {
            bool has3I0 = _deviceModelKeeper.deviceModelCache.Has3I0();
            bool has3U0 = _deviceModelKeeper.deviceModelCache.Has3U0();
            var commands = target.GetCommCMDs();
            for (int i = 0; i < commands.Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                {
                    int count = 0;
                    if (has3U0 && info.Value.Group1 != null && info.Value.Group1.Count > 0)
                    {
                        var ua = info.Value.Group1[0];
                        var ub = info.Value.Group1[1];
                        if (ua != null)
                        {
                            var source = GetBoardPort(ua);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$mag$f\", v_Ua, -1, vg_MRUErrorRel); ");
                            count++;
                        }
                        if (ub != null)
                        {
                            var source = GetBoardPort(info.Value.Group1[1]);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$mag$f\", v_Ub, -1, vg_MRUErrorRel); ");
                            count++;
                        }
                        sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{_deviceModelKeeper.deviceModelCache.Get3U0($"3U0{info.Key}")}$cVal$mag$f\", v_Ua, -1, vg_MRUErrorRel); ");
                        count++;
                        if (ua != null)
                        {
                            var source = GetBoardPort(ua);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$ang$f\", 0, vg_MRUAngErrorAbs, -1);  ");
                            count++;
                        }
                        if (ub != null)
                        {
                            var source = GetBoardPort(ub);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$ang$f\", -120, vg_MRUAngErrorAbs, -1);");
                            count++;
                        }
                        sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{_deviceModelKeeper.deviceModelCache.Get3U0($"3U0{info.Key}")}$cVal$ang$f\", -60, vg_MRUAngErrorAbs, -1);");
                        count++;
                    }

                    if (has3I0 && info.Value.Group3 != null && info.Value.Group3.Count > 0)
                    {
                        var ia = info.Value.Group3[0];
                        var ib = info.Value.Group3[1];
                        if (ia != null)
                        {
                            var source = GetBoardPort(ia);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$mag$f\", v_Ia, -1, vg_MRIErrorRel); ");
                            count++;
                        }
                        if (ib != null)
                        {
                            var source = GetBoardPort(ib);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$mag$f\", v_Ib, -1, vg_MRIErrorRel);");
                            count++;
                        }
                        sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{_deviceModelKeeper.deviceModelCache.Get3I0($"3I0{info.Key}")}$cVal$mag$f\", v_Ia, -1, vg_MRIErrorRel);");
                        count++;
                        if (ia != null)
                        {
                            var source = GetBoardPort(ia);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$ang$f\", 0, vg_MRIAngErrorAbs, -1); ");
                            count++;
                        }
                        if (ib != null)
                        {
                            var source = GetBoardPort(ib);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{para}$cVal$ang$f\", -120, vg_MRIAngErrorAbs, -1);");
                            count++;
                        }
                        sb.AppendLine($"nRsltJdg = nRsltJdg + CalAinError(\"{_deviceModelKeeper.deviceModelCache.Get3I0($"3I0{info.Key}")}$cVal$ang$f\", -60, vg_MRIAngErrorAbs, -1);");
                        count++;

                    }

                    sb.AppendLine();
                    sb.Append($@"if(nRsltJdg=={count}) then");
                }
                commands[i].CMD.DataSetPath = $"CPU{i + 1}$dsRelayAin";
                commands[i].ScriptResult.CData = $"{SCRIPT_4_HEAD}\n{sb.ToString()}\n{SCRIPT_4_END}";
            }

            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() == 0 && info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() == 0)
            {
                return IS_CONTINUE.Stop;
            }
            else
            {
                return IS_CONTINUE.Continue;
            }
        }
        private Tuple<string, string, string> GetBoardPort(List<Core> cores)
        {
            var core = cores.LastOrDefault();
            if (core.DeviceA == _targetDeviceKeeper.TargetDevice.Name)
            {
                return new Tuple<string, string, string>(core.BoardA, core.PortA, core.Desc);
            }
            else
            {
                return new Tuple<string, string, string>(core.BoardB, core.PortB, core.Desc);
            }
        }
    }
}
