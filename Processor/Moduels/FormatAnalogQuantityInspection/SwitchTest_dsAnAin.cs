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
    public class SwitchTest_dsAnAin : ISwitchTest_dsAnAin
    {
        private enum IS_CONTINUE
        {
            Stop = 0,
            Continue = 1
        }
        private readonly string[] U_param = { "v_Ua", "v_Ub", "v_Uc" };
        private readonly string[] I_param = { "v_Ia", "v_Ib", "v_Ic" };
        private readonly string[] Angle = { "0", "-120", "120" };
        private readonly IDeviceModelKeeper _deviceModelKeeper;
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private List<ItemBase> newitems = new List<ItemBase>();
        public SwitchTest_dsAnAin(
            IDeviceModelKeeper deviceModelKeeper,
            ITargetDeviceKeeper targetDeviceKeeper
            )
        {
            _deviceModelKeeper = deviceModelKeeper;
            _targetDeviceKeeper = targetDeviceKeeper;
        }

        public Task SwitchTest_dsAnAinProcess(SDL sdl, Items root, KeyValuePair<(string, string), ACDeviceUint> info, TESTER tester)
        {
            newitems.Clear();
            if (tester == TESTER.PONOVOTester || tester == TESTER.PONOVOStandardSource)
            {
                PONOVOSwitchTest_dsAnAinProcess(sdl, root, info);
            }
            else if (tester == TESTER.ONLLYTester)
            {
                ONLLYSwitchTest_dsAnAinProcess(sdl, root, info);
            }
            else
            {
                throw new NotSupportedException("不支持的模式");
            }

            return Task.CompletedTask;
        }
        private void ONLLYSwitchTest_dsAnAinProcess(SDL sdl, Items root, KeyValuePair<(string, string), ACDeviceUint> info)
        {
            var target = root.GetItems().Where(MT => MT.Name.Equals("开关测试")).FirstOrDefault();
            var output = target.GetSafetys().FirstOrDefault(I => I.Name.Contains("加量")).Clone();
            var stop = target.GetSafetys().FirstOrDefault(I => I.Name.Contains("停止输出")).Clone();
            InitSafe_Command(target, info);
            var res = NoKK(target, info);
            //Un上有开关就生成2个command和2个kk
            if (res == IS_CONTINUE.Continue)
            {
                res = ONEKK(target, info);
            }
            //Un和Ux上都有开关就生成3个command和2个kk
            if (res == IS_CONTINUE.Continue)
            {
                res = TWOKK(target, info);
            }
            int cpuCount = _deviceModelKeeper.TargetDeviceModel.LDevices
                .Count(LD => LD.Name.StartsWith("CPU")); // 简化了CPU计数逻辑
            int order = 0;
            var commands = target.GetCommCMDs();
            var safeties = target.GetSafetys();
            for (int k = 0; k < cpuCount; k++)
            {
                commands[k].OrderNum = order++;
            }

            for (int i = 0; i < safeties.Count(); i++)
            {
                safeties[i].OrderNum = order++;

                // 计算基础索引，避免重复计算cpu_count + k
                int baseIndex = cpuCount * (i + 1);

                for (int k = 0; k < cpuCount; k++)
                {
                    commands[baseIndex + k].OrderNum = order++;
                }
            }
            output.OrderNum = -1;
            newitems.Add(output);

            stop.OrderNum = 99;
            newitems.Add(stop);
        }
        private void PONOVOSwitchTest_dsAnAinProcess(SDL sdl, Items root, KeyValuePair<(string, string), ACDeviceUint> info)
        {
            var target = root.GetMacroTests().Where(MT => MT.Name.Equals("开关测试")).FirstOrDefault();
            InitSafe_Command(target, info);
            var res = NoKK(target, info);
            //Un上有开关就生成2个command和2个kk
            if (res == IS_CONTINUE.Continue)
            {
                res = ONEKK(target, info);
            }
            //Un和Ux上都有开关就生成3个command和2个kk
            if (res == IS_CONTINUE.Continue)
            {
                res = TWOKK(target, info);
            }
            int cpuCount = _deviceModelKeeper.TargetDeviceModel.LDevices
                .Count(LD => LD.Name.StartsWith("CPU")); // 简化了CPU计数逻辑
            int order = 0;
            var commands = target.GetCommCMDs();
            var safeties = target.GetSafetys();
            for (int k = 0; k < cpuCount; k++)
            {
                commands[k].OrderNum = order++;
            }
            for (int i = 0; i < safeties.Count(); i++)
            {
                safeties[i].OrderNum = order++;
                // 计算基础索引，避免重复计算cpu_count + k
                int baseIndex = cpuCount * (i + 1);
                for (int k = 0; k < cpuCount; k++)
                {
                    commands[baseIndex + k].OrderNum = order++;
                }
            }
        }
        private void InitSafe_Command(MacroTest target, KeyValuePair<(string, string), ACDeviceUint> info)
        {
            int cpu_count = _deviceModelKeeper.TargetDeviceModel.LDevices.Count(LD => LD.Name.StartsWith("CPU"));
            if (cpu_count == 1)
            {
                var commands = target.GetCommCMDs();
                var command1 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU1(输出ABC,电压ABC/X/0/无电压)")).Clone();
                command1.CMD.DataSetPath = $"CPU1$dsRelayAin";
                newitems.Add(command1);
            }
            if (cpu_count == 2)
            {
                var commands = target.GetCommCMDs();
                var command1 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU1(输出ABC,电压ABC/X/0/无电压)")).Clone();
                var command2 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU2(输出ABC,电压ABC/X/0/无电压)")).Clone();
                command1.CMD.DataSetPath = $"CPU1$dsRelayAin";
                command2.CMD.DataSetPath = $"CPU2$dsRelayAin";
                newitems.Add(command1);
                newitems.Add(command2);
            }
            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() == 0 && info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() == 0)
            {
                //没有空开，不用切换
            }
            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() != 0 && info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() == 0)
            {
                var safetys = target.GetSafetys();
                var safe1 = safetys.FirstOrDefault(S => S.Name.Contains("提示闭合1ZKK1")).Clone();
                string KKname = info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).FirstOrDefault();
                safe1.Name = $"提示闭合{KKname}";
                safe1.DllCall.CData = $"SpeakString=闭合{KKname};ExpectString=是否完成;";
                newitems.Add(safe1);
            }
            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() != 0 && info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() != 0)
            {
                var safetys = target.GetSafetys();
                var safe1 = safetys.FirstOrDefault(S => S.Name.Contains("提示闭合1ZKK1")).Clone();
                var safe2 = safetys.FirstOrDefault(S => S.Name.Contains("提示闭合1ZKK2")).Clone();
                string KKname1 = info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).FirstOrDefault();
                safe1.Name = $"提示闭合{KKname1}";
                safe1.DllCall.CData = $"SpeakString=闭合{KKname1};ExpectString=是否完成;";
                string KKname2 = info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).FirstOrDefault();
                safe2.Name = $"提示闭合{KKname2}";
                safe2.DllCall.CData = $"SpeakString=闭合{KKname2};ExpectString=是否完成;";
                newitems.Add(safe1);
                newitems.Add(safe2);
            }
            target.Safety_CommCMD_List = newitems.ToList();
        }
        private IS_CONTINUE NoKK(MacroTest target, KeyValuePair<(string, string), ACDeviceUint> info)
        {
            var commands = target.GetCommCMDs();
            foreach (var command in commands)
            {
                StringBuilder sb = new StringBuilder();
                {
                    int count = 0;
                    if (info.Value.Group1 != null && info.Value.Group1.Count > 0)
                    {
                       
                        for (int i = 0; i < Math.Min(3, info.Value.Group1.Count); i++)
                        {
                            var item = info.Value.Group1[i];
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];

                            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() == 0)
                            {
                                if (info.Value.KK_BYQ_List1.Where(L => L.Contains("BYQ")).Count() != 0)
                                {
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", 15.19, -1, vg_MRUErrorRel); ");
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRUAngErrorAbs, -1);");
                                    count += 2;
                                }
                                else
                                {
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {U_param[i]}, -1, vg_MRUErrorRel); ");
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRUAngErrorAbs, -1);");

                                    count += 2;
                                }

                            }
                            else
                            {
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", 0, vg_U1VErrorAbs, -1); ");
                                count++;
                            }
                        }
                    }
                    if (info.Value.Group2 != null && info.Value.Group2.Count > 0)
                    {
                        foreach (var item in info.Value.Group2.GetRange(0, 1))
                        {
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            if (info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() == 0)
                            {
                                //如果没有UABC，Ux没法分析角度，因此Ux测试后移到，Uabc的KK闭合后。
                            }
                            else
                            {
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", 0, vg_U1VErrorAbs, -1); ");
                                count++;
                            }

                        }
                    }
                    if (info.Value.Group1 != null && info.Value.Group1.Count == 0)//如果没有电压通道，电流测试需要前置
                    {
                        if (info.Value.Group3 != null && info.Value.Group3.Count > 0)
                        {
                            for (int i = 0; i < Math.Min(3, info.Value.Group3.Count); i++)                          
                            {
                                var item = info.Value.Group3[i];
                                var source = GetBoardPort(item);
                                var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {I_param[i]}, -1, vg_MRIErrorRel); ");
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRIAngErrorAbs, -1); ");
                                count += 2;
                            }
                        }
                    }
                    sb.AppendLine();
                    sb.Append($@"if(nRsltJdg=={count}) then");
                }
                command.ScriptResult.CData = $"{SCRIPT_1}\n{sb.ToString()}\n{SCRIPT_1_END}";
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
        private IS_CONTINUE ONEKK(MacroTest target, KeyValuePair<(string, string), ACDeviceUint> info)
        {
            var commCmds = target.GetCommCMDs();
            var commCmdsDeepCopy = commCmds.Select(cmd => cmd.Clone()).ToList();

            foreach (var command in commCmdsDeepCopy)
            {
                int count = 0;
                StringBuilder sb = new StringBuilder();
                {
                    sb.AppendLine();
                    if (info.Value.KK_BYQ_List1.Where(L => L.Contains("BYQ")).Count() != 0)
                    {
                        sb.Append("vPsum = vPsum/3.8;");
                    }

                    sb.AppendLine();
                    if (info.Value.Group1 != null && info.Value.Group1.Count > 0)
                    {                     
                        for (int i = 0; i < Math.Min(3, info.Value.Group1.Count); i ++)              
                        {
                            var item = info.Value.Group1[i];
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() != 0)
                            {
                                if (info.Value.KK_BYQ_List1.Where(L => L.Contains("BYQ")).Count() != 0)
                                {
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", 15.19, -1, vg_MRUErrorRel); ");
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRUAngErrorAbs, -1);");
                                    count += 2;
                                }
                                else
                                {
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {U_param[i]}, -1, vg_MRUErrorRel); ");
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRUAngErrorAbs, -1);");
                                    count += 2;
                                }
                            }
                        }
                    }
                    if (info.Value.Group2 != null && info.Value.Group2.Count > 0)
                    {
                        foreach (var item in info.Value.Group2.GetRange(0, 1))
                        {
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            if (info.Value.KK_BYQ_List1.Any(L => L.Contains("KK")) ^ info.Value.KK_BYQ_List2.Any(L => L.Contains("KK")))//异或关系
                            {
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {U_param[0]}, -1, vg_MRUErrorRel); ");
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[0]}, vg_MRUAngErrorAbs, -1);");
                                count += 2;
                            }

                        }
                    }
                    sb.AppendLine("strshow = string.format(\"nRsltJdg=%d\", nRsltJdg);");
                    sb.AppendLine("ShowMsg(strshow);");
                    sb.AppendLine();
                    if (info.Value.Group3 != null && info.Value.Group3.Count > 0)
                    {
                        for (int i = 0; i < Math.Min(3, info.Value.Group3.Count); i++)
                        {
                            var item = info.Value.Group3[i];
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {I_param[i]}, -1, vg_MRIErrorRel); ");
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRIAngErrorAbs, -1); ");             
                            count += 2;
                        }
                    }
                    if (info.Value.Group4 != null && info.Value.Group4.Count > 0)
                    {
                        foreach (var item in info.Value.Group3.GetRange(0, 1))
                        {
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {I_param[0]}, -1, vg_MRIErrorRel); ");
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[0]}, vg_MRIAngErrorAbs, -1); ");
                            count += 2;
                        }
                    }
                    if ((info.Value.Group3 != null && info.Value.Group3.Count > 0) || (info.Value.Group4 != null && info.Value.Group4.Count > 0))
                    {
                        sb.AppendLine(@"strshow = string.format(""nRsltJdg=%d"", nRsltJdg);");
                        sb.AppendLine(@"ShowMsg(strshow);");
                    }
                    sb.AppendLine();
                }
                command.ScriptResult.CData = $"{SCRIPT_2}\n{sb.ToString()}\n{SCRIPT_2_END}\n\nif(nRsltJdg=={count}) then\n{SCRIPT_2_TAIL}";
            }
            target.Safety_CommCMD_List.AddRange(commCmdsDeepCopy);
            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() == 0 || info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() == 0)
            {
                return IS_CONTINUE.Stop;
            }
            else
            {
                return IS_CONTINUE.Continue;
            }
        }
        private IS_CONTINUE TWOKK(MacroTest target, KeyValuePair<(string, string), ACDeviceUint> info)
        {
            var commCmds = target.GetCommCMDs();
            var commCmdsDeepCopy = commCmds.Select(cmd => cmd.Clone()).ToList();
            commCmdsDeepCopy = commCmdsDeepCopy.GetRange(0, commCmdsDeepCopy.Count() / 2);
            foreach (var command in commCmdsDeepCopy)
            {
                int count = 0;
                StringBuilder sb = new StringBuilder();
                {
                    if (info.Value.Group2 != null && info.Value.Group2.Count > 0)
                    {
                        foreach (var item in info.Value.Group2.GetRange(0, 1))
                        {
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {U_param[0]}, -1, vg_MRUErrorRel); ");
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[0]}, vg_MRUAngErrorAbs, -1);");
                            count += 2;
                        }
                        sb.AppendLine();
                    }
                    sb.AppendLine("strshow = string.format(\"nRsltJdg=%d\", nRsltJdg);");
                    sb.AppendLine("ShowMsg(strshow);");
                    sb.AppendLine();
                    if (info.Value.Group3 != null && info.Value.Group3.Count > 0)
                    {
                        for (int i = 0; i < Math.Min(3, info.Value.Group3.Count); i++)
                        {
                            var item = info.Value.Group3[i];
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {I_param[i]}, -1, vg_MRIErrorRel); ");
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRIAngErrorAbs, -1); ");
                            count += 2;
                        }
                    }
                    if (info.Value.Group4 != null && info.Value.Group4.Count > 0)
                    {
                        foreach (var item in info.Value.Group3.GetRange(0, 1))
                        {
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {I_param[0]}, -1, vg_MRIErrorRel); ");
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[0]}, vg_MRIAngErrorAbs, -1); ");
                            count++;
                        }
                    }
                    if ((info.Value.Group3 != null && info.Value.Group3.Count > 0) || (info.Value.Group4 != null && info.Value.Group4.Count > 0))
                    {
                        sb.AppendLine(@"strshow = string.format(""nRsltJdg=%d"", nRsltJdg);");
                        sb.AppendLine(@"ShowMsg(strshow);");
                    }
                    sb.AppendLine();
                }

                command.ScriptResult.CData = $"{SCRIPT_2}\n{sb.ToString()}\n{SCRIPT_2_END}\n\nif(nRsltJdg=={count}) then\n{SCRIPT_2_TAIL}";
            }
            target.Safety_CommCMD_List.AddRange(commCmdsDeepCopy);
            return IS_CONTINUE.Stop;
        }
        private void InitSafe_Command(Items target, KeyValuePair<(string, string), ACDeviceUint> info)
        {
            int cpu_count = _deviceModelKeeper.TargetDeviceModel.LDevices.Count(LD => LD.Name.StartsWith("CPU"));
            if (cpu_count == 1)
            {
                var commands = target.GetCommCMDs();
                var command1 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU1(输出ABC,电压ABC/X/0/无电压)")).Clone();
                command1.CMD.DataSetPath = $"CPU1$dsRelayAin";
                newitems.Add(command1);
            }
            if (cpu_count == 2)
            {
                var commands = target.GetCommCMDs();
                var command1 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU1(输出ABC,电压ABC/X/0/无电压)")).Clone();
                var command2 = commands.FirstOrDefault(C => C.Name.Contains("读遥测CPU2(输出ABC,电压ABC/X/0/无电压)")).Clone();
                command1.CMD.DataSetPath = $"CPU1$dsRelayAin";
                command2.CMD.DataSetPath = $"CPU2$dsRelayAin";
                newitems.Add(command1);
                newitems.Add(command2);
            }
            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() == 0 && info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() == 0)
            {
                //没有空开，不用切换
            }
            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() != 0 && info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() == 0)
            {
                var safetys = target.GetSafetys();
                var safe1 = safetys.FirstOrDefault(S => S.Name.Contains("提示闭合1ZKK1")).Clone();
                string KKname = info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).FirstOrDefault();
                safe1.Name = $"提示闭合{KKname}";
                safe1.DllCall.CData = $"SpeakString=闭合{KKname};ExpectString=是否完成;";
                newitems.Add(safe1);
            }
            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() != 0 && info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() != 0)
            {
                var safetys = target.GetSafetys();
                var safe1 = safetys.FirstOrDefault(S => S.Name.Contains("提示闭合1ZKK1")).Clone();
                var safe2 = safetys.FirstOrDefault(S => S.Name.Contains("提示闭合1ZKK2")).Clone();
                string KKname1 = info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).FirstOrDefault();
                safe1.Name = $"提示闭合{KKname1}";
                safe1.DllCall.CData = $"SpeakString=闭合{KKname1};ExpectString=是否完成;";
                string KKname2 = info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).FirstOrDefault();
                safe2.Name = $"提示闭合{KKname2}";
                safe2.DllCall.CData = $"SpeakString=闭合{KKname2};ExpectString=是否完成;";
                newitems.Add(safe1);
                newitems.Add(safe2);
            }
            target.ItemList = newitems.ToList();
        }
        private IS_CONTINUE NoKK(Items target, KeyValuePair<(string, string), ACDeviceUint> info)
        {
            var commands = target.GetCommCMDs();
            foreach (var command in commands)
            {
                StringBuilder sb = new StringBuilder();
                {
                    int count = 0;
                    if (info.Value.Group1 != null && info.Value.Group1.Count > 0)
                    {
                        for (int i = 0; i < Math.Min(3, info.Value.Group1.Count); i++)
                        {
                            var item = info.Value.Group1[i];
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];

                            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() == 0)
                            {
                                if (info.Value.KK_BYQ_List1.Where(L => L.Contains("BYQ")).Count() != 0)
                                {
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", 15.19, -1, vg_MRUErrorRel); ");
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRUAngErrorAbs, -1);");
                                    count += 2;
                                }
                                else
                                {
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {U_param[i]}, -1, vg_MRUErrorRel); ");
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRUAngErrorAbs, -1);");
                                    count += 2;
                                }
                            }
                            else
                            {
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", 0, vg_U1VErrorAbs, -1); ");
                                count++;
                            }
                        }
                    }
                    if (info.Value.Group2 != null && info.Value.Group2.Count > 0)
                    {
                        foreach (var item in info.Value.Group2.GetRange(0, 1))
                        {
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            if (info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() == 0)
                            {

                            }
                            else
                            {
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", 0, vg_U1VErrorAbs, -1); ");
                                count++;
                            }
                        }
                    }
                    if (info.Value.Group1 != null && info.Value.Group1.Count == 0)//如果没有电压通道，电流测试需要前置
                    {
                        if (info.Value.Group3 != null && info.Value.Group3.Count > 0)
                        {                          
                            for (int i = 0; i < Math.Min(3, info.Value.Group1.Count); i++)
                            {
                                var item = info.Value.Group1[i];
                                var source = GetBoardPort(item);
                                var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {I_param[i]}, -1, vg_MRIErrorRel); ");
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRIAngErrorAbs, -1); ");
                                count += 2;
                            }
                        }
                    }
                    sb.AppendLine();
                    sb.Append($@"if(nRsltJdg=={count}) then");
                }
                command.ScriptResult.CData = $"{SCRIPT_1}\n{sb.ToString()}\n{SCRIPT_1_END}";
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
        private IS_CONTINUE ONEKK(Items target, KeyValuePair<(string, string), ACDeviceUint> info)
        {
            var commCmds = target.GetCommCMDs();
            var commCmdsDeepCopy = commCmds.Select(cmd => cmd.Clone()).ToList();

            foreach (var command in commCmdsDeepCopy)
            {
                int count = 0;
                StringBuilder sb = new StringBuilder();
                {
                    sb.AppendLine();
                    if (info.Value.KK_BYQ_List1.Where(L => L.Contains("BYQ")).Count() != 0)
                    {
                        sb.Append("vPsum = vPsum/3.8;");
                    }

                    sb.AppendLine();
                    if (info.Value.Group1 != null && info.Value.Group1.Count > 0)
                    {
                      
                        for (int i = 0; i < Math.Min(3, info.Value.Group1.Count); i++)
                        {
                            var item = info.Value.Group1[i];
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() != 0)
                            {
                                if (info.Value.KK_BYQ_List1.Where(L => L.Contains("BYQ")).Count() != 0)
                                {
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", 15.19, -1, vg_MRUErrorRel); ");
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRUAngErrorAbs, -1);");
                                    count += 2;
                                }
                                else
                                {
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {U_param[i]}, -1, vg_MRUErrorRel); ");
                                    sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRUAngErrorAbs, -1);");
                                    count += 2;

                                }
                            }
                        }
                    }
                    if (info.Value.Group2 != null && info.Value.Group2.Count > 0)
                    {
                        foreach (var item in info.Value.Group2.GetRange(0, 1))
                        {
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            if (info.Value.KK_BYQ_List1.Any(L => L.Contains("KK")) ^ info.Value.KK_BYQ_List2.Any(L => L.Contains("KK")))//异或
                            {
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {U_param[0]}, -1, vg_MRUErrorRel); ");
                                sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[0]}, vg_MRUAngErrorAbs, -1);");
                                count += 2;
                            }

                        }
                    }
                    sb.AppendLine("strshow = string.format(\"nRsltJdg=%d\", nRsltJdg);");
                    sb.AppendLine("ShowMsg(strshow);");
                    sb.AppendLine();
                    if (info.Value.Group3 != null && info.Value.Group3.Count > 0)
                    {                       
                       for (int i = 0; i < Math.Min(3, info.Value.Group3.Count); i++)
                       {
                            var item = info.Value.Group3[i];
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {I_param[i]}, -1, vg_MRIErrorRel); ");
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRIAngErrorAbs, -1); ");
                            count += 2;
                       }
                    }
                    if (info.Value.Group4 != null && info.Value.Group4.Count > 0)
                    {
                        foreach (var item in info.Value.Group3.GetRange(0, 1))
                        {
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {I_param[0]}, -1, vg_MRIErrorRel); ");
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[0]}, vg_MRIAngErrorAbs, -1); ");

                            count += 2;
                        }
                    }
                    if ((info.Value.Group3 != null && info.Value.Group3.Count > 0) || (info.Value.Group4 != null && info.Value.Group4.Count > 0))
                    {
                        sb.AppendLine(@"strshow = string.format(""nRsltJdg=%d"", nRsltJdg);");
                        sb.AppendLine(@"ShowMsg(strshow);");
                    }
                    sb.AppendLine();
                }
                command.ScriptResult.CData = $"{SCRIPT_2}\n{sb.ToString()}\n{SCRIPT_2_END}\n\nif(nRsltJdg=={count}) then\n{SCRIPT_2_TAIL}";
            }
            target.ItemList.AddRange(commCmdsDeepCopy);
            if (info.Value.KK_BYQ_List1.Where(L => L.Contains("KK")).Count() == 0 || info.Value.KK_BYQ_List2.Where(L => L.Contains("KK")).Count() == 0)
            {
                return IS_CONTINUE.Stop;
            }
            else
            {
                return IS_CONTINUE.Continue;
            }
        }
        private IS_CONTINUE TWOKK(Items target, KeyValuePair<(string, string), ACDeviceUint> info)
        {
            var commCmds = target.GetCommCMDs();
            var commCmdsDeepCopy = commCmds.Select(cmd => cmd.Clone()).ToList();
            commCmdsDeepCopy = commCmdsDeepCopy.GetRange(0, commCmdsDeepCopy.Count() / 2);
            foreach (var command in commCmdsDeepCopy)
            {
                int count = 0;
                StringBuilder sb = new StringBuilder();
                {
                    if (info.Value.Group2 != null && info.Value.Group2.Count > 0)
                    {
                        foreach (var item in info.Value.Group2.GetRange(0, 1))
                        {
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {U_param[0]}, -1, vg_MRUErrorRel); ");
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[0]}, vg_MRUAngErrorAbs, -1);");
                            count += 2;
                        }
                        sb.AppendLine();
                    }
                    sb.AppendLine("strshow = string.format(\"nRsltJdg=%d\", nRsltJdg);");
                    sb.AppendLine("ShowMsg(strshow);");
                    sb.AppendLine();
                    if (info.Value.Group3 != null && info.Value.Group3.Count > 0)
                    {

                        for (int i = 0; i < Math.Min(3, info.Value.Group3.Count); i++)               
                        {
                            var item = info.Value.Group3[i];
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {I_param[i]}, -1, vg_MRIErrorRel); ");
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[i]}, vg_MRIAngErrorAbs, -1); ");
                            count += 2;
                        }
                    }
                    if (info.Value.Group4 != null && info.Value.Group4.Count > 0)
                    {
                        foreach (var item in info.Value.Group3.GetRange(0, 1))
                        {
                            var source = GetBoardPort(item);
                            var para = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$mag$f"", {I_param[0]}, -1, vg_MRIErrorRel); ");
                            sb.AppendLine($@"nRsltJdg = nRsltJdg + CalAinError(""{para}$cVal$ang$f"", {Angle[0]}, vg_MRIAngErrorAbs, -1); ");
                            count++;
                        }
                    }
                    if ((info.Value.Group3 != null && info.Value.Group3.Count > 0) || (info.Value.Group4 != null && info.Value.Group4.Count > 0))
                    {
                        sb.AppendLine(@"strshow = string.format(""nRsltJdg=%d"", nRsltJdg);");
                        sb.AppendLine(@"ShowMsg(strshow);");
                    }
                    sb.AppendLine();
                }
                command.ScriptResult.CData = $"{SCRIPT_2}\n{sb.ToString()}\n{SCRIPT_2_END}\n\nif(nRsltJdg=={count}) then\n{SCRIPT_2_TAIL}";
            }
            target.ItemList.AddRange(commCmdsDeepCopy);
            return IS_CONTINUE.Stop;
        }
        private Tuple<string, string, string> GetBoardPort(List<Core> cores)
        {
            var core = cores.LastOrDefault();
            if (core.DeviceA == _targetDeviceKeeper.TargetDevice.Name)
            {
                var board = _targetDeviceKeeper.TargetDevice.Boards.FirstOrDefault(B => B.Name == core.BoardA);
                var port = board.Ports.FirstOrDefault(P => P.Name == core.PortA);
                return new Tuple<string, string, string>(board.Name, port.Name, port.Desc);
            }
            else
            {
                var board = _targetDeviceKeeper.TargetDevice.Boards.FirstOrDefault(B => B.Name == core.BoardB);
                var port = board.Ports.FirstOrDefault(P => P.Name == core.PortB);
                return new Tuple<string, string, string>(board.Name, port.Name, port.Desc);
            }
        }

    }
}
