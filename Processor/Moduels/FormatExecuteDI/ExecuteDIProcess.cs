﻿using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatExecuteDI;
using System.Data;
using System.Text.RegularExpressions;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;
using static SFTemplateGenerator.Helper.UtilityTools.RegexProcess;

namespace SFTemplateGenerator.Processor.Moduels.FormatExecuteDI
{
    public class ExecuteDIProcess : IExecuteDIProcess
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        private readonly IDeviceModelKeeper _deviceModelKeeper;
        private readonly Regex REGEX_DIObject = new(@"(DI)_(\d{1,2})");
        private readonly List<Regex> REGEX_YB = new(){
            new Regex("YB")
        };
        private readonly List<Regex> REGEX_BS = new() {
            new Regex("BS"),
        };
        private readonly List<Regex> REGEX_FG = new() {
            new Regex("FG")
        };
        private readonly List<Regex> REGEX_SELFTEST = new() {
            new Regex("YD"),
            
            new Regex("KD"),
            new Regex(@"(\d)P(\d)?D")
        };
        private readonly List<Regex> REGEX_PUBLIC_SELFTEST = new() {
            new Regex("GD"),
        };
        private readonly List<Regex> ISCONTINUE = new() {
            new Regex(@"\d{1}-\d{1,2}DQ"),
            new Regex(@"\d{1,2}DQ")
        }; // 连线终点判断正则表达式
        private static readonly List<Regex> REGEX_POSITIVE = new(){
            new Regex(@"^装置电源\+$"),
            new Regex(@"^IN\+$"),
        };
        private static readonly List<Regex> REGEX_DI_COMM = new(){
            new Regex(@"^KM\+$"),
        };
        List<Regex> REGEX_MAINTENANCE = new(){
            new Regex(@"^保护检修状态"),
             new Regex(@"^检修\+"),
        };
        List<Regex> REGEX_IBusbar = new(){
            new Regex(@"^投I母电压$"),
        };
        List<Regex> REGEX_IIBusbar = new(){
            new Regex(@"^投II母电压$"),
        };
        List<Regex> REGEX_IBusbarNO = new(){
            new Regex(@"^I母刀闸常开$"),
        };
        List<Regex> REGEX_IIBusbarNO = new(){
            new Regex(@"^II母刀闸常开$"),
        };
        List<Regex> REGEX_IBusbarNC = new(){
            new Regex(@"^I母刀闸常闭$"),
        };
        List<Regex> REGEX_IIBusbarNC = new(){
            new Regex(@"^II母刀闸常闭$"),
        };
        private static readonly List<Regex> REGEX_VOLTAGESWITCH_7QD = new(){
            new Regex(@"7Q\d?D"),
        };
        private List<string> workerTestList = new();//存储自行测试名称
        private List<string> workerBSTestList = new();//存储自行测试名称
        private List<string> publicBoardList = new();//存储于DI公共端相连的节点
        private List<string> _nodename = new();//存储节点名称
        public ExecuteDIProcess(
            ITargetDeviceKeeper targetDeviceKeeper,
            ISDLKeeper sDLKeeper,
            IDeviceModelKeeper deviceModelKeeper
        )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
            _sDLKeeper = sDLKeeper;
            _deviceModelKeeper = deviceModelKeeper;
        }
        public Task ExecuteDIProcessAsync(SDL sdl, Items rootItem, List<string> NodeName)
        {
            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => DIBORAD_REGEX.IsMatch(B.Desc)).ToList();
            List<DIDeviceEnd> DIObjectList = new List<DIDeviceEnd>();//常用检测
            var regexMappings = new Dictionary<Regex, List<DIDeviceEnd>>
            {
                { REGEX_DIObject, DIObjectList }
            };
            foreach (var board in boards)
            {
                var boardList = FindAllPublicBoard(sdl, _targetDeviceKeeper.TargetDevice, board);
                publicBoardList.AddRange(boardList);
                var ports = board.Ports.Where(p => p.PortPair.Any(pair => pair.Contains("DI")));
                foreach (var port in ports)
                {
                    Match match = null;
                    // 遍历所有正则表达式，检查是否匹配
                    foreach (var mapping in regexMappings)
                    {
                        var regex = mapping.Key;
                        var targetDict = mapping.Value;
                        foreach (var portpair in port.PortPair)
                        {
                            match = regex.Match(portpair);
                            if (match.Success)
                            {
                                targetDict.Add(new DIDeviceEnd((_targetDeviceKeeper.TargetDevice, board, port)));
                                break;
                            }
                        }
                    }
                }
            }
            Create_DI_By_Desc(rootItem, DIObjectList);
            RemoveMaintenance(DIObjectList);
            RemoveNoConnection(DIObjectList);
            CreateDIObject(rootItem, DIObjectList);
            NodeName.AddRange(this._nodename);
            return Task.CompletedTask;
        }
        private void Create_DI_By_Desc(Items rootItem, List<DIDeviceEnd> DIObjectList)
        {
            foreach (var dIObjcet in DIObjectList)
            {
                if (REGEX_IBusbar.Any(R => R.IsMatch(dIObjcet.StartPort.Item3.Desc)))
                {
                    CreateBusbar(rootItem, dIObjcet, REGEX_IBusbarNO, REGEX_IBusbarNC);
                }
                if (REGEX_IIBusbar.Any(R => R.IsMatch(dIObjcet.StartPort.Item3.Desc)))
                {
                    CreateBusbar(rootItem, dIObjcet, REGEX_IIBusbarNO, REGEX_IIBusbarNC);
                }
            }
        }
        private void CreateBusbar(Items rootItem, DIDeviceEnd DIObject, List<Regex> BusbarNORegex, List<Regex> BusbarNCRegex)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"母电压开入x测试（示例）").Clone();
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DIObject);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                var indataSet = _deviceModelKeeper.deviceModelCache.GetDataSetId(source.Item1, source.Item2, PortReallDesc);
                {
                    var safety = item.ItemList.FirstOrDefault(p => p.Name == "提示接入开入信号") as Safety;
                    if (safety != null)
                    {
                        Tuple<string, string> BusbarNO = Find7QDDIPort(BusbarNORegex);
                        var PowerNode7QD = Find7QDPositivePort();
                        var speakStrings = $"{PowerNode7QD.Item1}{PowerNode7QD.Item2}点{BusbarNO.Item1}{BusbarNO.Item2}";
                        safety.Name = $"提示接入开入信号:{speakStrings}";
                        item.Name = $"{PortReallDesc.Replace("ProtDO:", "")}开入测试";
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=无;";
                    }
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "判断开入状态置1");
                    if (CommCMD != null)
                    {
                        CommCMD.Type = "ReadSoe";
                        if (indataSet != "")
                        {
                            CommCMD.DsvScript.InDataSet = "Device1$" + indataSet;
                            if (indataSet.Equals("dsRelayEna"))
                            {
                                CommCMD.DsvScript.Type = "query";
                            }
                        }
                        var element = CommCMD.DsvScript.Elements.FirstOrDefault();
                        if (element != null)
                        {
                            element.Name = PortReallDesc;
                            element.Id = PortReallDesc;
                            var arrtId = element.Attrs.FirstOrDefault(p => p.Name.ToLower() == "id");
                            var arrtvalue = element.Attrs.FirstOrDefault(p => p.Name.ToLower() == "value");
                            if (arrtId != null)
                            {
                                arrtId.Value = PortReallDesc;
                            }
                        }
                    }
                }
                {
                    var safety = item.ItemList.FirstOrDefault(p => p.Name == "提示接入开入信号1") as Safety;
                    if (safety != null)
                    {
                        Tuple<string, string> BusbarNC = Find7QDDIPort(BusbarNCRegex);
                        var PowerNode7QD = Find7QDPositivePort();
                        var speakStrings = $"{PowerNode7QD.Item1}{PowerNode7QD.Item2}点{BusbarNC.Item1}{BusbarNC.Item2}";
                        safety.Name = $"提示接入开入信号:{speakStrings}";
                        item.Name = $"{PortReallDesc.Replace("ProtDO:", "")}开入测试";
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=无;";
                    }
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "判断开入状态置0");
                    if (CommCMD != null)
                    {
                        CommCMD.Type = "ReadSoe";
                        if (indataSet != "")
                        {
                            CommCMD.DsvScript.InDataSet = "Device1$" + indataSet;
                            if (indataSet.Equals("dsRelayEna"))
                            {
                                CommCMD.DsvScript.Type = "query";
                            }
                        }
                        var element = CommCMD.DsvScript.Elements.FirstOrDefault();
                        if (element != null)
                        {
                            element.Name = PortReallDesc;
                            element.Id = PortReallDesc;
                            var arrtId = element.Attrs.FirstOrDefault(p => p.Name.ToLower() == "id");
                            var arrtvalue = element.Attrs.FirstOrDefault(p => p.Name.ToLower() == "value");
                            if (arrtId != null)
                            {
                                arrtId.Value = PortReallDesc;
                            }
                        }
                    }
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateDIObject(Items rootItem, List<DIDeviceEnd> DIObjectList)
        {
            var KKTOTAL = new List<Regex>().Concat(REGEX_FG).Concat(REGEX_BS).Concat(REGEX_YB).ToList();
            foreach (var dIObject in DIObjectList)
            {
                var devices = dIObject.GetDevices(_sDLKeeper.SDL);
                var FirstKKdevice = devices.FirstOrDefault(D => KKTOTAL.Any(R => R.IsMatch(D.Class)));
                //有两个BS的
                if (devices.Where(D => D.Class.Equals("BS")).Count() >= 2)
                {
                    CreateShouHeTongQi(rootItem, dIObject);
                }
                //从其他端子排引入
                else if (HasMatchingSelfTestTdDevice(devices) && !HasMatchingGDSelfTestTdDevice(devices) && IsNotPublicBoard(dIObject, publicBoardList))
                {
                    CreateTest_By_Worker(rootItem, dIObject);
                }
                else if (FirstKKdevice != null)
                {
                    //有YB
                    if (REGEX_YB.Any(R => R.IsMatch(FirstKKdevice.Class)))
                    {
                        CreateYB(rootItem, dIObject);
                    }
                    //有1个BS
                    else if (REGEX_BS.Any(R => R.IsMatch(FirstKKdevice.Class)))
                    {
                        CreateBS(rootItem, dIObject);
                    }
                    //有1个FG
                    else if (REGEX_FG.Any(R => R.IsMatch(FirstKKdevice.Class)))
                    {
                        CreateFG(rootItem, dIObject);
                    }
                }
                //从其他端子排引入
                else if (HasMatchingGDSelfTestTdDevice(devices) && IsNotPublicBoard(dIObject, publicBoardList))
                {
                    CreateTest_By_Worker(rootItem, dIObject);
                }
                else
                {
                    CreateNode(rootItem, dIObject);
                }
            }
            AddWorkerTestSummaryNode(rootItem);
        }
        private void CreateShouHeTongQi(Items rootItem, DIDeviceEnd DIObject)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"调试人员自测").Clone();
            if (item != null)
            {
                item.OrderNum = 99;
                var source = GetBoard_Port_PortDesc(DIObject);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入开入信号");
                if (safety != null)
                {
                    var tuple1 = DIObject.GetNear();
                    var speakStrings = $"调试员进行{PortReallDesc}接{tuple1.Item1}{tuple1.Item2}把手测试";
                    safety.Name = $"提示接入开入信号:{tuple1.Item1}{tuple1.Item2}";
                    item.Name = $"{PortReallDesc.Replace("ProtDO:", "")}开入测试:{tuple1.Item1}{tuple1.Item2}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=是否完成;";
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateYB(Items rootItem, DIDeviceEnd DIObject)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"开入x测试（示例）").Clone();
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DIObject);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                var indataSet = _deviceModelKeeper.deviceModelCache.GetDataSetId(source.Item1, source.Item2, PortReallDesc);
                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入开入信号");
                if (safety != null)
                {
                    var YBName = DIObject.GetKKName(_sDLKeeper.SDL, "YB");
                    var speakStrings = $"闭合{YBName}{(PortReallDesc.EndsWith("压板") ? "" : "压板")}";
                    safety.Name = $"提示接入开入信号:{speakStrings}";
                    item.Name = $"{PortReallDesc.Replace("ProtDO:", "")}开入测试:" + speakStrings;
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=无;";
                }
                var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "判断开入状态（示例）");
                if (CommCMD != null)
                {
                    CommCMD.Name = "判断开入状态";
                    CommCMD.Type = "ReadSoe";
                    if (indataSet != "")
                    {
                        CommCMD.DsvScript.InDataSet = "Device1$" + indataSet;
                        if (indataSet.Equals("dsRelayEna"))
                        {
                            CommCMD.DsvScript.Type = "query";
                        }
                    }
                    var element = CommCMD.DsvScript.Elements.FirstOrDefault();
                    if (element != null)
                    {
                        element.Name = PortReallDesc;
                        element.Id = PortReallDesc;
                        var arrtId = element.Attrs.FirstOrDefault(p => p.Name.ToLower() == "id");
                        if (arrtId != null)
                        {
                            arrtId.Value = PortReallDesc;
                        }
                    }
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateBS(Items rootItem, DIDeviceEnd DIObject)
        {
            if (DIObject.StartPort.Item3.Desc.Contains("重合闸"))
            {
                var bs = DIObject.GetKKName(_sDLKeeper.SDL, "BS");
                if (bs != string.Empty)
                {
                    if (!workerBSTestList.Contains(bs))
                    {
                        workerBSTestList.Add(bs);
                    }
                }
            }
            else
            {
                var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"开入x测试（示例）").Clone();
                if (item != null)
                {
                    var source = GetBoard_Port_PortDesc(DIObject);
                    var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                    var indataSet = _deviceModelKeeper.deviceModelCache.GetDataSetId(source.Item1, source.Item2, PortReallDesc);
                    var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入开入信号");
                    if (safety != null)
                    {
                        var KKName = DIObject.GetKKName(_sDLKeeper.SDL, "BS");
                        var speakStrings = $"把手置{KKName}{PortReallDesc.Replace("ProtDO:", "")}位置";
                        safety.Name = $"提示接入开入信号:{speakStrings}";
                        item.Name = $"{PortReallDesc.Replace("ProtDO:", "")}开入测试:{speakStrings}";
                        safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=无;";
                    }
                    var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "判断开入状态（示例）");
                    if (CommCMD != null)
                    {
                        CommCMD.Name = "判断开入状态";
                        CommCMD.Type = "ReadSoe";
                        if (indataSet != "")
                        {
                            CommCMD.DsvScript.InDataSet = "Device1$" + indataSet;
                            if (indataSet.Equals("dsRelayEna"))
                            {
                                CommCMD.DsvScript.Type = "query";
                            }
                        }
                        var element = CommCMD.DsvScript.Elements.FirstOrDefault();
                        if (element != null)
                        {
                            element.Name = PortReallDesc;
                            element.Id = PortReallDesc;
                            var arrtId = element.Attrs.FirstOrDefault(p => p.Name.ToLower() == "id");
                            var arrtvalue = element.Attrs.FirstOrDefault(p => p.Name.ToLower() == "value");
                            if (arrtId != null)
                            {
                                arrtId.Value = PortReallDesc;
                            }
                        }
                    }
                    rootItem.ItemList.Add(item);
                    _nodename.Add(item.Name);
                }
            }
        }
        private void CreateFG(Items rootItem, DIDeviceEnd DIObject)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"开入x测试（示例）").Clone();
            if (item != null)
            {
                var source = GetBoard_Port_PortDesc(DIObject);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                var indataSet = _deviceModelKeeper.deviceModelCache.GetDataSetId(source.Item1, source.Item2, PortReallDesc);
                var safety = item.GetSafetys().FirstOrDefault(p => p.Name == "提示接入开入信号");
                if (safety != null)
                {
                    var KKName = DIObject.GetKKName(_sDLKeeper.SDL, "FG");
                    var speakStrings = $"按{KKName}按钮";
                    safety.Name = $"提示接入开入信号:{speakStrings}";
                    item.Name = $"{PortReallDesc.Replace("ProtDO:", "")}开入测试:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=无;";
                }
                var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "判断开入状态（示例）");
                if (CommCMD != null)
                {
                    CommCMD.Name = "判断开入状态";
                    CommCMD.Type = "ReadSoe";
                    if (indataSet != "")
                    {
                        CommCMD.DsvScript.InDataSet = "Device1$" + indataSet;
                        if (indataSet.Equals("dsRelayEna"))
                        {
                            CommCMD.DsvScript.Type = "query";
                        }
                    }
                    var element = CommCMD.DsvScript.Elements.FirstOrDefault();
                    if (element != null)
                    {
                        element.Name = PortReallDesc;
                        element.Id = PortReallDesc;
                        var arrtId = element.Attrs.FirstOrDefault(p => p.Name.ToLower() == "id");
                        var arrtvalue = element.Attrs.FirstOrDefault(p => p.Name.ToLower() == "value");
                        if (arrtId != null)
                        {
                            arrtId.Value = PortReallDesc;
                        }
                    }
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private void CreateTest_By_Worker(Items rootItem, DIDeviceEnd DIObject)
        {

            var ybName = DIObject.GetKKName(_sDLKeeper.SDL, "YB");
            if (ybName == string.Empty)
            {
                var tuple1 = DIObject.GetNear();
                if (!workerTestList.Contains($"{tuple1.Item1}{tuple1.Item2}"))
                {
                    workerTestList.Add($"{tuple1.Item1}{tuple1.Item2}");
                }
            }
            else
            {
                workerTestList.Add(ybName);
            }



        }
        private void CreateNode(Items rootItem, DIDeviceEnd DIObject)
        {
            var item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"开入x测试（示例）").Clone();
            if (item != null)
            {
                var PowerNode = FindPowerNode();
                var source = GetBoard_Port_PortDesc(DIObject);
                var PortReallDesc = _deviceModelKeeper.deviceModelCache[source.Item1, source.Item2, source.Item3];
                var indataSet = _deviceModelKeeper.deviceModelCache.GetDataSetId(source.Item1, source.Item2, PortReallDesc);
                var safety = item.ItemList.FirstOrDefault(p => p.Name == "提示接入开入信号") as Safety;
                if (safety != null)
                {
                    var tuple1 = DIObject.GetEND(_sDLKeeper.SDL);
                    var speakStrings = $"{PowerNode.Item1}{PowerNode.Item2}点{tuple1.Item1}{tuple1.Item2}";
                    safety.Name = $"提示接入开入信号:{speakStrings}";
                    item.Name = $"{PortReallDesc.Replace("ProtDO:", "")}开入测试:{speakStrings}";
                    safety.DllCall.CData = $"SpeakString={speakStrings};ExpectString=无;";
                }
                var CommCMD = item.GetCommCMDs().FirstOrDefault(p => p.Name == "判断开入状态（示例）");
                if (CommCMD != null)
                {
                    CommCMD.Name = "判断开入状态";
                    CommCMD.Type = "ReadSoe";
                    if (indataSet != "")
                    {
                        CommCMD.DsvScript.InDataSet = "Device1$" + indataSet;
                        if (indataSet.Equals("dsRelayEna"))
                        {
                            CommCMD.DsvScript.Type = "query";
                        }
                    }
                    var element = CommCMD.DsvScript.Elements.FirstOrDefault();
                    if (element != null)
                    {
                        element.Name = PortReallDesc;
                        element.Id = PortReallDesc;
                        var arrtId = element.Attrs.FirstOrDefault(p => p.Name.ToLower() == "id");
                        if (arrtId != null)
                        {
                            arrtId.Value = PortReallDesc;
                        }
                    }
                }
                rootItem.ItemList.Add(item);
                _nodename.Add(item.Name);
            }
        }
        private Tuple<string, string> FindPowerNode()
        {
            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => POWERBORAD_REGEX.IsMatch(B.Desc)).ToList();
            foreach (var board in boards)
            {
                var powerport = board.Ports.FirstOrDefault(P => REGEX_POSITIVE.Any(R => R.IsMatch(P.Desc)));
                if (powerport != null)
                {
                    //获取电源节点
                    var core = _sDLKeeper.SDL.Cubicle.Cores.FirstOrDefault(C => C.DeviceA.Equals(_targetDeviceKeeper.TargetDevice.Name) && C.BoardA.Equals(board.Name) && C.PortA.Equals(powerport.Name));
                    if (core == null)
                    {
                        core = _sDLKeeper.SDL.Cubicle.Cores.FirstOrDefault(C => C.DeviceB.Equals(_targetDeviceKeeper.TargetDevice.Name) && C.BoardB.Equals(board.Name) && C.PortB.Equals(powerport.Name));
                        return new Tuple<string, string>(core.DeviceA, core.BoardA);
                    }
                    return new Tuple<string, string>(core.DeviceB, core.BoardB);
                }
            }
            return new Tuple<string, string>("", "");
        }
        private void RemoveMaintenance(List<DIDeviceEnd> DIList)
        {
            DIList.RemoveAll(DI => REGEX_MAINTENANCE.Any(R => R.IsMatch(DI.StartPort.Item3.Desc)));
        }
        private void RemoveNoConnection(List<DIDeviceEnd> DIList)
        {
            foreach (var di in DIList)
            {
                GetFarCore(_sDLKeeper.SDL, di.StartPort.Item1, di.StartPort.Item2, di.StartPort.Item3, new List<Core>(), di.cores);
            }
            // 移除cores为空的节点，同时去掉连到YD的线段
            DIList.RemoveAll(di => di.cores == null || di.cores.Count == 0);
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
                var coresFrist = cores.FirstOrDefault(C => C.Class.Equals("导线")) ?? cores.FirstOrDefault();
                var coresLast = cores.LastOrDefault(C => C.Class.Equals("短连片")) ?? cores.LastOrDefault();
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
                return true;
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
            var cores = sdl.Cubicle.Cores.ToList();
            //检查下一个是不是短连片
            var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == AnotherDevice);
            if (device != null)
            {
                if (device.Class.Equals("YB"))
                {
                    if (int.TryParse(AnotherPort, out int portANumber))
                    {
                        if (portANumber % 2 == 0) // 双数（偶数）的判断：除以2余数为0
                        {
                            AnotherPort = (portANumber - 1).ToString();
                        }
                        else
                        {
                            AnotherPort = portANumber.ToString();
                        }
                    }
                }
                if (device.Class.Equals("BS"))
                {
                    if (int.TryParse(AnotherPort, out int portANumber))
                    {
                        // 若为偶数则减1，奇数保持不变
                        int result = portANumber % 2 == 0 ? portANumber - 1 : portANumber;
                        AnotherPort = result.ToString();
                    }
                }
                if (device.Class.Equals("KK"))
                {
                    if (int.TryParse(AnotherPort, out int portANumber))
                    {
                        AnotherPort = (portANumber - 1).ToString();
                    }
                }
                if (device.Class.Equals("JDQ"))
                {
                    if (int.TryParse(AnotherPort, out int portANumber))
                    {
                        AnotherPort = (portANumber - 7).ToString();
                    }
                }
                if (device.Class.Equals("FG"))
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
        private bool IsCorrectEnd(string deviceName,
        string boardName,
        string portName)
        {
            if (deviceName == _targetDeviceKeeper.TargetDevice.Name)
            {
                return false;
            }
            if (deviceName.Contains("LP"))
            {
                return false;
            }
            if (deviceName.Contains("QK"))
            {
                return false;
            }
            if (deviceName.Contains("FA"))
            {
                return false;
            }
            return true;
        }
        private bool IsContinue(string deviceName, Core core)
        {
            if (ISCONTINUE.Any(I => I.IsMatch(deviceName)))
            {
                return false;
            }
            return true;
        }
        private Tuple<string, string, string> GetBoard_Port_PortDesc(DIDeviceEnd dIDeviceEnd)
        {
            return new Tuple<string, string, string>(dIDeviceEnd.StartPort.Item2.Name, dIDeviceEnd.StartPort.Item3.Name, dIDeviceEnd.StartPort.Item3.Desc);
        }
        private Tuple<string, string> Find7QDDIPort(List<Regex> BusbarNORegex)
        {
            List<Regex> modifiedRegexList = GetModifiedRegexList(_targetDeviceKeeper.TargetDevice.Name, REGEX_VOLTAGESWITCH_7QD);
            var devices = _sDLKeeper.SDL.Cubicle.Devices.Where(D => modifiedRegexList.Any(R => R.IsMatch(D.Name)));
            foreach (var device in devices)
            {
                foreach (var board in device.Boards)
                {
                    if (BusbarNORegex.Any(R => R.IsMatch(board.Desc)))
                    {
                        return new Tuple<string, string>(device.Name, board.Name);
                    }
                }
            }
            return new Tuple<string, string>("", "");
        }
        private Tuple<string, string> Find7QDPositivePort()
        {
            List<Regex> modifiedRegexList = GetModifiedRegexList(_targetDeviceKeeper.TargetDevice.Name, REGEX_VOLTAGESWITCH_7QD);
            var devices = _sDLKeeper.SDL.Cubicle.Devices.Where(D => modifiedRegexList.Any(R => R.IsMatch(D.Name))).ToList();
            foreach (var device in devices)
            {
                var board = device.Boards.FirstOrDefault(B => REGEX_POSITIVE.Any(R => R.IsMatch(B.Desc)));
                return new Tuple<string, string>(device.Name, board.Name);
            }
            return new Tuple<string, string>("", "");
        }
        private void AddWorkerTestSummaryNode(Items rootItem)
        {
            if (workerTestList.Count > 0)
            {
                var Item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"调试人员自测")?.Clone();
                if (Item != null)
                {
                    Item.OrderNum = 99;
                    var summaryText = string.Join(", ", workerTestList);
                    var safety = Item.GetSafetys().FirstOrDefault(s => s.Name == "提示接入开入信号");
                    if (safety != null)
                    {
                        Item.Name = $"调试人员自测: 开入汇总";
                        safety.Name = $"自测提示";
                        safety.DllCall.CData = $"SpeakString=调试员自行测试:{summaryText};ExpectString=是否完成;";
                    }
                    rootItem.ItemList.Add(Item);
                    _nodename.Add(Item.Name);
                }
            }
            if (workerBSTestList.Count > 0)
            {
                var Item = rootItem.GetItems().FirstOrDefault(p => p.Name == @"调试人员自测")?.Clone();
                if (Item != null)
                {
                    Item.OrderNum = 99;
                    var summaryText = string.Join(", ", workerBSTestList);
                    var safety = Item.GetSafetys().FirstOrDefault(s => s.Name == "提示接入开入信号");
                    if (safety != null)
                    {
                        Item.Name = $"调试人员自测: 重合闸汇总";
                        safety.Name = $"自测提示";
                        safety.DllCall.CData = $"SpeakString=调试员自行测试:{summaryText}把手;ExpectString=是否完成;";
                    }
                    rootItem.ItemList.Add(Item);
                    _nodename.Add(Item.Name);
                }
            }
        }
        private List<string> FindAllPublicBoard(SDL sdl, Device target, Board board)
        {
            var port = board.Ports.FirstOrDefault(P => REGEX_DI_COMM.Any(R => R.IsMatch(P.Desc)));
            if (port != null) {
                List<List<Core>> lines = new List<List<Core>>();
                GetAllLines(sdl, target, board, port, new List<Core>(), lines);
                return GetPublicBoards(lines);
            }
            return new List<string>();
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
        private List<string> GetPublicBoards(List<List<Core>> lines)
        {

            List<string> boards = new List<string>();
            foreach (var line in lines)
            {
                foreach (var core in line)
                {
                    if (!DEVICE_REGEX.IsMatch(core.DeviceA) && !DEVICE_REGEX.IsMatch(core.DeviceB))
                    {
                        var boardA = core.DeviceA + core.BoardA;
                        if (!boards.Contains(boardA))
                        {
                            boards.Add(boardA);
                        }
                        var boardB = core.DeviceB + core.BoardB;
                        if (!boards.Contains(boardB))
                        {
                            boards.Add(boardB);
                        }
                    }
                }
            }
            return boards;
        }
        // 提取为单独的方法，明确逻辑意图
        private bool HasMatchingSelfTestTdDevice( List<Device> devices)
        {
            // 筛选出Class为"TD"的设备
            var tdDevices = devices.Where(d => d.Class.Equals("TD"));
            // 检查是否有任何TD设备的Name匹配SELFTEST正则
            return REGEX_SELFTEST.Any(regex => tdDevices.Any(device => regex.IsMatch(device.Name)));
        }
        private bool HasMatchingGDSelfTestTdDevice(List<Device> devices)
        {
            // 筛选出Class为"TD"的设备
            var tdDevices = devices.Where(d => d.Class.Equals("TD"));
            // 检查是否有任何TD设备的Name匹配SELFTEST正则
            return REGEX_PUBLIC_SELFTEST.Any(regex => tdDevices.Any(device => regex.IsMatch(device.Name)));
        }
        private bool IsNotPublicBoard(DIDeviceEnd deviceEnd, List<string> publicBoardList)
        {
            return !deviceEnd.ContainsPublicBoard(publicBoardList);
        }
    }
}
