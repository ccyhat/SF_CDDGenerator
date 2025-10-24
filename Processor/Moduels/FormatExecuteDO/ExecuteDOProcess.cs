
using Castle.Components.DictionaryAdapter.Xml;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatExecuteDO;
using SFTemplateGenerator.Processor.Moduels.FormatVoltageSwitchCircuitTest;
using System.Text.RegularExpressions;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;

namespace SFTemplateGenerator.Processor.Moduels.FormatExecuteDO
{
    public class ExecuteDOProcess : IExecuteDOProcess
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        private readonly IDeviceModelKeeper _deviceModelKeeper;
        private readonly Regex REGEX_DOObject = new Regex("(DO)_(\\d{1,2})");
        private readonly List<Regex> REGEX_DOOthers = new List<Regex> {
            new Regex(@"手合同期"),
            new Regex(@"^通道故障(\d)?$"),
            new Regex(@"^通道一告警(\d)?$"),
            new Regex(@"^通道一告警(\d)?\(常闭\)$"),
            new Regex(@"^通道二告警(\d)?$"),
            new Regex(@"^通道二告警(\d)?\(常闭\)$"),
            new Regex(@"^闭锁调压\(常闭\)$"),
        };
        private readonly List<Regex> REGEX_Air_Switch_Auxiliary_Contact = new List<Regex> {
            new Regex(@"^空开辅助接点"),
        };
        private readonly List<Regex> REGEX_DOUBLE_YB = new List<Regex> {
            new Regex(@"^智能切换片"),
            new Regex(@"^连接片_XH76W4T-DKZ_红色_常州洛阳华泰$"),
        };
        private readonly Regex REGEX_4QD = new Regex(@"4Q(\d)?D");
        private readonly Regex REGEX_4YD = new Regex(@"4Y(\d)?D");
        private List<string> _nodename = new List<string>();//存储节点名称
        public ExecuteDOProcess(
            ITargetDeviceKeeper targetDeviceKeeper,
            ISDLKeeper sDLKeeper,
            IDeviceModelKeeper deviceModelKeeper
        )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
            _sDLKeeper = sDLKeeper;
            _deviceModelKeeper = deviceModelKeeper;
        }
        public Task ExecuteDOProcessAsync(SDL sdl, Items rootItem, List<string> NodeName)
        {
            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => DOBORAD_REGEX.IsMatch(B.Desc)|| SIGNALBORAD_REGEX.IsMatch(B.Desc) || POWERBORAD_REGEX.IsMatch(B.Desc)).ToList();//开出插件
            Dictionary<string, List<DODeviceEnd>> DOObjectOperation = new Dictionary<string, List<DODeviceEnd>>();
            var regexMappings = new Dictionary<Regex, Dictionary<string, List<DODeviceEnd>>>
            {
                { REGEX_DOObject, DOObjectOperation }
            };
            foreach (var board in boards)
            {
                var ports = board.Ports.Where(p => p.PortPair.Any(pair => pair.Contains("DO") && !pair.Contains("tripDO")));
                foreach (var port in ports)
                {
                    Match match = null;
                    foreach (var mapping in regexMappings)
                    {
                        var regex = mapping.Key;
                        var targetDict = mapping.Value;
                        foreach (var portpair in port.PortPair)
                        {
                            match = regex.Match(portpair);
                            if (match.Success)
                            {
                                // 提取结尾的数字，默认为0
                                string name = match.Groups[2].Success ? match.Groups[2].Value : "0";
                                // 获取或创建对应的分组
                                if (!targetDict.TryGetValue(name, out var group))
                                {
                                    group = new List<DODeviceEnd>();
                                    targetDict[name] = group;
                                }
                                // 添加描述到对应分组
                                group.Add(new DODeviceEnd((_targetDeviceKeeper.TargetDevice, board, port)));
                            }
                        }
                    }
                }
            }
            SwitchOrder(DOObjectOperation);
            DOObjectOperation = RemoveNoConnection(DOObjectOperation);
            CreateDOObject(DOObjectOperation, rootItem);
            CreateYBD(rootItem);
            CreateAuxiliaryContact(rootItem);
            NodeName.AddRange(this._nodename);
            return Task.CompletedTask;
        }
        private void CreateDOObject(Dictionary<string, List<DODeviceEnd>> DOObjectOperation, Items rootItem)
        {
            foreach (var dic in DOObjectOperation)
            {
                SeperateDOObject(dic.Value, rootItem);
            }
        }
        private void SeperateDOObject(List<DODeviceEnd> doDeviceEnds, Items rootItem)
        {
            foreach (var dODeviceEnd in doDeviceEnds)
            {
                if (REGEX_DOOthers.Any(R => R.IsMatch(dODeviceEnd.StartPort.Item3.Desc)))
                {
                    Create_DO_By_Des(doDeviceEnds, rootItem);
                    return;
                }
                if (dODeviceEnd.GetYBName() != string.Empty)
                {
                    if (dODeviceEnd.GetOtherHighVoltage_QDName() != string.Empty)
                    {
                        Create_DO_By_HighVoltage_QD(doDeviceEnds, rootItem);
                    }
                    else if (dODeviceEnd.Get4QDName() != string.Empty)
                    {
                        Create_DO_By_4QD(doDeviceEnds, rootItem);
                    }
                    else
                    {
                        Create_DO_By_YB(doDeviceEnds, rootItem);
                    }
                    return;
                }
            }
            CreateRest(doDeviceEnds, rootItem);

        }
        private void Create_DO_By_Des(List<DODeviceEnd> DOObject, Items rootItem)
        {
            foreach (var dODeviceEnd in DOObject)
            {
                var dOObjectPair = new DOPortPair(DOObject.FirstOrDefault(), DOObject.LastOrDefault()!);
                if (dODeviceEnd.StartPort.Item3.Desc.Contains("手合同期"))
                {
                    CreateShouHeTongQi(dOObjectPair, rootItem);
                    break;
                }
                if (dODeviceEnd.StartPort.Item3.Desc.Contains("告警"))
                {
                    CreateAlarm(dOObjectPair, rootItem);
                    break;
                }
                if (dODeviceEnd.StartPort.Item3.Desc.Contains("故障"))
                {
                    CreateFault(dOObjectPair, rootItem);
                    break;
                }
                if (dODeviceEnd.StartPort.Item3.Desc.Contains("闭锁调压(常闭)"))
                {
                    CreateLockingVoltageRegulationNC(dOObjectPair, rootItem);
                    break;
                }
            }
        }
        private void CreateShouHeTongQi(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"调试人员自测").Clone();
            item.OrderNum = 3;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);
                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                if (safety != null)
                {
                    var speakStrings = $"调试人员自测{source.Item3}把手测试";
                    item.Name = $"调试人员自测：把手测试{source.Item3}";
                    safety.DllCall.CData = $"SpeakString={source.Item3};ExpectString=是否合格;";
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateAlarm(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"通道告警").Clone();
            item.OrderNum = 3;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                {
                    var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                    if (safety != null)
                    {
                        var tuple1 = DOObjectPair.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                        var tuple2 = DOObjectPair.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                        var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";
                        safety.Name = $"提示接入表笔:{speakStrings}";
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                        item.Name = $"{PortReallDesc}:{speakStrings}";
                    }
                }
                {
                    var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔1");
                    if (safety != null)
                    {
                        var speakStrings = $"断开{PortReallDesc}光纤";
                        safety.Name = $"{speakStrings}";
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                    }
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateFault(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"通道故障").Clone();
            item.OrderNum = 3;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                if (safety != null)
                {
                    var tuple1 = DOObjectPair.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                    var tuple2 = DOObjectPair.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                    var speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";
                    safety.Name = $"提示接入表笔:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                    item.Name = $"{PortReallDesc}:{speakStrings}";
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateLockingVoltageRegulationNC(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"闭锁调压传动测试").Clone();
            item.OrderNum = 2;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                if (safety != null)
                {
                    var speakStrings = "";
                    var tuple1 = DOObjectPair.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                    var tuple2 = DOObjectPair.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                    speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";
                    item.Name = $"{PortReallDesc.Replace("ProtDO:", "")}传动测试：{speakStrings}";
                    safety.Name = $"提示接入表笔:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为1");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                        CommCMD.CMD.Value.FirstOrDefault().Value = "1";
                    }
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为0");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                        CommCMD.CMD.Value.FirstOrDefault().Value = "0";
                    }
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void Create_DO_By_YB(List<DODeviceEnd> DOObject, Items rootItem)
        {
            var dOObjectPair = new DOPortPair(DOObject.FirstOrDefault(), DOObject.LastOrDefault()!);
            CreateYBOpen(dOObjectPair, rootItem);
            CreateYBClose(dOObjectPair, rootItem);
        }
        private void Create_DO_By_HighVoltage_QD(List<DODeviceEnd> DOObject, Items rootItem)
        {
            var dOObjectPair = new DOPortPair(DOObject.FirstOrDefault(), DOObject.LastOrDefault()!);
            CreateSelfTest(dOObjectPair, rootItem);
        }
        private void Create_DO_By_4QD(List<DODeviceEnd> DOObject, Items rootItem)
        {
            var dOObjectPair = new DOPortPair(DOObject.FirstOrDefault(), DOObject.LastOrDefault()!);
            CreateOperationBoxOpen(dOObjectPair, rootItem);
            CreateOperationBoxClose(dOObjectPair, rootItem);
        }
        private void CreateYBOpen(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == "开出1经过压板不闭合测试").Clone();
            item.OrderNum = 1;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];

                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                if (safety != null)
                {
                    var tuple1 = DOObjectPair.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                    var tuple2 = DOObjectPair.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                    var speakStrings =
                        $@"打开{DOObjectPair.GetYBName()}，量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";
                    var ReallyDesc = PortReallDesc.Replace("ProtDO:", "");
                    item.Name = $"{ReallyDesc}{"经过压板不闭合测试"}：{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为1");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                        CommCMD.CMD.Value.FirstOrDefault().Value = "1";
                    }
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为0");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                        CommCMD.CMD.Value.FirstOrDefault().Value = "0";
                    }
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateYBClose(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == "开出1经过压板闭合测试").Clone();
            item.OrderNum = 1;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                if (safety != null)
                {
                    var tuple1 = DOObjectPair.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                    var tuple2 = DOObjectPair.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                    var speakStrings =
                       $@"关闭{DOObjectPair.GetYBName()}，量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";

                    var ReallyDesc = PortReallDesc.Replace("ProtDO:", "");
                    item.Name = $"{ReallyDesc}{"经过压板闭合测试"}：{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为1");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                    }
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为0");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                    }
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateOperationBoxOpen(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == "开出1接操作箱不闭合测试").Clone();
            item.OrderNum = 1;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                {
                    var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                    if (safety != null)
                    {
                        var speakStrings = "";
                        if (rootItem.ItemList.Any(I => I.Name.Contains("操作箱置合位")))
                        {
                            speakStrings = $@"打开{DOObjectPair.GetYBName()}";
                        }
                        else
                        {
                            speakStrings = $@"操作箱置合位,打开{DOObjectPair.GetYBName()}";
                        }
                        safety.Name = speakStrings;
                        var ReallyDesc = PortReallDesc.Replace("ProtDO:", "");
                        item.Name = $"{ReallyDesc}{"操作箱测试"}：{speakStrings}";
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                    }
                }
                {
                    var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔1");
                    if (safety != null)
                    {
                        var speakStrings = $@"操作箱不动作";
                        safety.Name = speakStrings;
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                    }
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为1");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                        CommCMD.CMD.Value.FirstOrDefault().Value = "1";
                    }
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为0");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                        CommCMD.CMD.Value.FirstOrDefault().Value = "0";
                    }
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateOperationBoxClose(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == "开出1接操作箱闭合测试").Clone();
            item.OrderNum = 1;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                {
                    var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                    if (safety != null)
                    {

                        var speakStrings = $@"闭合{DOObjectPair.GetYBName()}";
                        safety.Name = speakStrings;
                        var ReallyDesc = PortReallDesc.Replace("ProtDO:", "");
                        item.Name = $"{ReallyDesc}{"操作箱测试"}：{speakStrings}";
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                    }
                }
                {
                    var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔1");
                    if (safety != null)
                    {
                        var speakStrings = $@"操作箱动作";
                        safety.Name = speakStrings;
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                    }
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为1");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                    }
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为0");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                    }
                }

                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateRest(List<DODeviceEnd> DOObject, Items rootItem)
        {
            var dOObjectPair = new DOPortPair(DOObject.FirstOrDefault(), DOObject.LastOrDefault()!);
            if (dOObjectPair.GetXDName() != string.Empty)
            {
                CreateDOTransmissionXD(dOObjectPair, rootItem);
            }
            else
            {
                CreateDOTransmissionYD(dOObjectPair, rootItem);
            }
        }
        private void CreateDOTransmissionYD(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"开出非保持型传动测试").Clone();
            item.OrderNum = 2;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];


                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                if (safety != null)
                {
                    var speakStrings = "";
                    var tuple1 = DOObjectPair.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                    var tuple2 = DOObjectPair.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                    speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";
                    item.Name = $"{PortReallDesc.Replace("ProtDO:", "")}传动测试：{speakStrings}";
                    safety.Name = $"提示接入表笔:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为1");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                        CommCMD.CMD.Value.FirstOrDefault().Value = "1";
                    }
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为0");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                        CommCMD.CMD.Value.FirstOrDefault().Value = "0";
                    }
                }

                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateDOTransmissionXD(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"开出保持型传动测试").Clone();
            item.OrderNum = 2;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                if (safety != null)
                {
                    var speakStrings = "";
                    var tuple1 = DOObjectPair.dODeviceEnd1.GetEND(_sDLKeeper.SDL);
                    var tuple2 = DOObjectPair.dODeviceEnd2.GetEND(_sDLKeeper.SDL);
                    speakStrings = $"量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";
                    item.Name = $"{PortReallDesc.Replace("ProtDO:", "")}传动测试：{speakStrings}";
                    safety.Name = $"提示接入表笔:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为1");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                        CommCMD.CMD.Value.FirstOrDefault().Value = "1";
                    }
                }
                {
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "将装置信号变为0");
                    if (CommCMD != null)
                    {
                        CommCMD.CMD.Value.FirstOrDefault().Id = $"{PortReallDesc}";
                        CommCMD.CMD.Value.FirstOrDefault().Value = "0";
                    }
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateSelfTest(DOPortPair DOObjectPair, Items rootItem)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"调试人员自测").Clone();
            item.OrderNum = 4;
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DOObjectPair.dODeviceEnd2);

                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                if (safety != null)
                {

                    var speakStrings = $"调试人员自测{source.Item3}";
                    item.Name = $"调试人员自测：{source.Item3}";
                    safety.Name = $"{source.Item3}测试";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否合格;";
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateYBD(Items rootItem)
        {
            var sdl = _sDLKeeper.SDL;
            var double_ybs=_sDLKeeper.SDL.Cubicle.Devices.Where(D => REGEX_DOUBLE_YB.Any(R=>R.IsMatch(D.Desc)));
            foreach(var double_yb in double_ybs)
            {
                var tuple1 = FindNearestPort(sdl, double_yb.Name, "", "3", new List<Core>());
                var tuple2 = FindNearestPort(sdl, double_yb.Name, "", "4", new List<Core>());
                var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"双层压板测试").Clone();
                if (item != null)
                {
                    item.Name = $"{double_yb.Name}双层压板测试";
                    item.OrderNum = 3;
                    {
                        var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔");
                        if (safety != null)
                        {
                            var speakStrings = $"打开{double_yb.Name}, 量{tuple1.Item1}{tuple1.Item2}和{tuple2.Item1}{tuple2.Item2}";
                            safety.Name = $"{speakStrings}";
                            safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                        }
                    }
                    {
                        var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入表笔1");
                        if (safety != null)
                        {
                            var speakStrings = $"闭合{double_yb.Name}";
                            safety.Name = $"{speakStrings}";
                            safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                        }
                    }
                    rootItem.ItemList.Add(item);
                    _nodename.Add(item.Name);
                }
            }
        }
        private void CreateAuxiliaryContact(Items rootItem)
        {
            var sdl = _sDLKeeper.SDL;
            var AuxiliaryContact = sdl.Cubicle.Devices.Where(D => REGEX_Air_Switch_Auxiliary_Contact.Any(R => R.IsMatch(D.Desc)));
            foreach (var KKdevice in AuxiliaryContact)
            {
                var tuple1 = FindNearestPort(sdl, KKdevice.Name, "", "11",new List<Core>());
                var tuple2 = FindNearestPort(sdl, KKdevice.Name, "", "12", new List<Core>());
                var item = rootItem.GetItems().FirstOrDefault(I => I.Name == @"空开辅助接点测试").Clone();
                item.OrderNum = 3;
                if (item != null)
                {
                    item.Name = $"辅助空开{KKdevice.Name}测试";
                    {
                        var safety = item.GetSafetys().FirstOrDefault(I => I.Name == @"提示接入表笔");
                        var speakStrings = $"闭合{KKdevice.Name},量{tuple1.Item1 + tuple1.Item2}和{tuple2.Item1 + tuple2.Item2}";
                        safety.Name = $"提示：{speakStrings}";
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                    }
                    {
                        var safety = item.GetSafetys().FirstOrDefault(I => I.Name == @"提示接入表笔1");
                        var speakStrings = $"断开{KKdevice.Name}";
                        safety.Name = $"提示：{speakStrings}";
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                    }

                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
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
            var cores = sdl.Cubicle.Cores.ToList();
            //检查下一个是不是短连片
            var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == AnotherDevice)!;
            if (device.Class.Equals("YB"))
            {
                if (REGEX_DOUBLE_YB.Any(R => R.IsMatch(device.Desc)))
                {
                    if (int.TryParse(AnotherPort, out int portANumber))
                    {
                        AnotherPort = (portANumber + 2).ToString();
                    }
                }
                else
                {
                    if (int.TryParse(AnotherPort, out int portANumber))
                    {
                        AnotherPort = (portANumber - 1).ToString();
                    }
                }

            }
            Tuple<string, string, string> tuple = new Tuple<string, string, string>(AnotherDevice, AnotherBoard, AnotherPort);
            return tuple;
        }
        private void GetAllLines(SDL sdl, Device target, Board board, Port port, List<Core> line, List<List<Core>> totalLines)
        {
            GetAllLines(sdl, target.Name, board.Name, port.Name, line, totalLines);
        }
        private void GetAllLines(SDL sdl, string deviceName,
        string boardName,
        string portName,
        List<Core> currentLine,
        List<List<Core>> lines)
        {
            var totalCores = sdl.Cubicle.Cores.ToList();
            var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == deviceName)!;
            List<Core> nextCores = null!;

            if (device.Class.Equals("TD"))
            {
                nextCores = totalCores.Except(currentLine).Where(c =>
               ((c.DeviceA == deviceName && c.BoardA == boardName) ||
               (c.DeviceB == deviceName && c.BoardB == boardName)) &&
               !(c.DeviceA == c.DeviceB && c.BoardA == c.BoardB)).ToList();//排除自己连接自己的短连片
            }
            else
            {
                nextCores = totalCores.Except(currentLine).Where(c =>
                (c.DeviceA == deviceName && c.BoardA == boardName && c.PortA == portName) ||
                (c.DeviceB == deviceName && c.BoardB == boardName && c.PortB == portName)).ToList();
            }
            if (nextCores.Count == 0 || device.Class.Equals("IED"))
            {
                if (currentLine.Count > 0)
                {
                    lines.Add(new List<Core>(currentLine));
                    return;
                }
            }
            foreach (var core in nextCores)
            {
                List<Core> newLine = new List<Core>(currentLine);
                newLine.Add(core);
                Tuple<string, string, string> anotherPort = GetAnotherPort(sdl, core, deviceName, boardName, portName);
                GetAllLines(sdl, anotherPort.Item1, anotherPort.Item2, anotherPort.Item3, newLine, lines);
            }
        }
        private Tuple<string, string, string> GetBoard_Port_PortDesc(DODeviceEnd DeviceEnd)
        {
            return new Tuple<string, string, string>(DeviceEnd.StartPort.Item2.Name, DeviceEnd.StartPort.Item3.Name, DeviceEnd.StartPort.Item3.Desc);
        }
        //确保1对多的公共端会被排到前面,目标是把公共端放到前面
        private void SwitchOrder(Dictionary<string, List<DODeviceEnd>> dict)
        {
            foreach (var key in dict.Keys)
            {
                dict[key].Sort((a, b) =>
                {
                    if (_deviceModelKeeper.deviceModelCache.ContainsDescName(a.StartPort.Item2.Name, a.StartPort.Item3.Name))
                    {
                        return 1;//b在前
                    }
                    if (_deviceModelKeeper.deviceModelCache.ContainsDescName(b.StartPort.Item2.Name, b.StartPort.Item3.Name))
                    {
                        return -1;//a在前
                    }

                    if (a.StartPort.Item3.PortPair.Count > b.StartPort.Item3.PortPair.Count)
                    {
                        return -1;
                    }
                    else
                    {
                        string a1 = a.StartPort.Item3.PortPair.FirstOrDefault().Split('_').LastOrDefault();
                        string b1 = b.StartPort.Item3.PortPair.FirstOrDefault().Split('_').LastOrDefault();
                        int a2 = int.Parse(a1);
                        int b2 = int.Parse(b1);
                        return a2.CompareTo(b2);
                    }
                });
            }
        }     
        private Dictionary<string, List<DODeviceEnd>> RemoveNoConnection(Dictionary<string, List<DODeviceEnd>> dict)
        {           
            foreach (var currentKvp in dict)
            {
                foreach (var deviceEnd in currentKvp.Value)
                {
                    List<List<Core>> lines = new List<List<Core>>();
                    GetAllLines(_sDLKeeper.SDL,
                               deviceEnd.StartPort.Item1,
                               deviceEnd.StartPort.Item2,
                               deviceEnd.StartPort.Item3,
                               new List<Core>(),
                               lines);
                    deviceEnd.cores = GetTargetLine(lines);                  
                }
            }
            // 筛选出符合条件的键值对：值列表中cores有效（不为null且数量>0）的元素刚好有2个
            var newDict = dict.Where(kvp =>
                kvp.Value.Count(deviceEnd =>
                    deviceEnd.cores != null && deviceEnd.cores.Count > 0
                ) == 2
            ).ToDictionary();
            return newDict;
        }
        private List<Core> GetTargetLine(List<List<Core>> lines)
        {
            foreach (var line in lines)
            {
           
                if(line.Any(C=>REGEX_4QD.IsMatch(C.DeviceA)|| REGEX_4QD.IsMatch(C.DeviceB)|| REGEX_4YD.IsMatch(C.DeviceA) || REGEX_4YD.IsMatch(C.DeviceB)) )
                {
                    return line;
                }

            }
            foreach (var line in lines)
            {
                
                foreach (var core in line)
                {
                    var deviceA = _sDLKeeper.SDL.Cubicle.Devices.FirstOrDefault(d => d.Name == core.DeviceA);
                    var deviceB = _sDLKeeper.SDL.Cubicle.Devices.FirstOrDefault(d => d.Name == core.DeviceB);
                    if (deviceA != null && deviceB != null)
                    {
                        if (deviceA.Class.Equals("YB") || deviceB.Class.Equals("YB"))
                        {
                            return line;
                        }
                    }
                }
            }
            if (lines.FirstOrDefault() != null)
            {
                return lines.FirstOrDefault().ToList();
            }
            return null;
        }
        private Tuple<string, string, string> FindNearestPort(SDL sdl, Device device, Board board, Port port)
        {
            return FindNearestPort(sdl, device.Name, board.Name, port.Name);
        }
        private Tuple<string, string, string> FindNearestPort(SDL sdl, string device, string board, string port,List<Core> fliter)
        {
            var cores = sdl.Cubicle.Cores.Except(fliter).Where(C =>
               (C.DeviceA == device && C.BoardA == board && C.PortA == port) ||
               (C.DeviceB == device && C.BoardB == board && C.PortB == port)
               );
            foreach(var core in cores)
            {
                fliter.Add(core);
                var otherDeviceName = core.DeviceA == device ? core.DeviceB : core.DeviceA;
                var otherBoardName = core.BoardA == board ? core.BoardB : core.BoardA;
                var otherPortName = core.PortA == port ? core.PortB : core.PortA;
                var otherDevice = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == otherDeviceName);
                Tuple<string, string, string>res = null;
                if ( otherDevice != null && otherDevice.Class != "TD")
                {
                    res = FindNearestPort(sdl, otherDeviceName, otherBoardName, otherPortName, fliter);
                }
                else
                {
                    res = new Tuple<string, string, string>(otherDeviceName, otherBoardName, otherPortName);
                }
                if(res!=null && res.Item1 != "" && res.Item2 != "" && res.Item3 != "")
                {
                    return res;
                }                                           
            }        
            return new Tuple<string, string, string>("", "", "");
        }
        private Tuple<string, string, string> FindNearestPort(SDL sdl, string device, string board, string port)
        {
            var core = sdl.Cubicle.Cores.FirstOrDefault(C =>
               (C.DeviceA == device && C.BoardA == board && C.PortA == port) ||
               (C.DeviceB == device && C.BoardB == board && C.PortB == port)
               );
           if(core != null)
            {
                var otherDeviceName = core.DeviceA == device ? core.DeviceB : core.DeviceA;
                var otherBoardName = core.BoardA == board ? core.BoardB : core.BoardA;
                var otherPortName = core.PortA == port ? core.PortB : core.PortA;
                var otherDevice = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == otherDeviceName);
                return new Tuple<string, string, string>(otherDeviceName, otherBoardName, otherPortName);
            }                                     
            return new Tuple<string, string, string>("", "", "");
        }
    }
}
