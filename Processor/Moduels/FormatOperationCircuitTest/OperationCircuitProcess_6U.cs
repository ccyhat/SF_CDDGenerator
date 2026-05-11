using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatOperationCircuitTest;
using System.Text.RegularExpressions;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;
namespace SFTemplateGenerator.Processor.Moduels.FormatOperationCircuitTest
{
    public class OperationCircuitProcess_6U : IOperationCirucuitProcess
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        private readonly ISDLKeeper _sDLKeeper;
        private readonly Regex REGEX_TripDOObject = new(@"(tripDO)_(\d{1,2})");

        public OperationCircuitProcess_6U(
           ITargetDeviceKeeper targetDeviceKeeper,
           ISDLKeeper sDLKeeper
           )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
            _sDLKeeper = sDLKeeper;
        }
        public Task OperationCirucuitProcessAsync(SDL sdl, Items root, List<string> NodeName)
        {

            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => OPBORAD_REGEX.Any(R => R.IsMatch(B.Desc)) || DOBORAD_REGEX.Any(R => R.IsMatch(B.Desc))).ToList();
            Dictionary<string, List<OperationDODeviceEnd>> TripDOObject = new Dictionary<string, List<OperationDODeviceEnd>>();
            var regexMappings = new Dictionary<Regex, Dictionary<string, List<OperationDODeviceEnd>>>
            {
                { REGEX_TripDOObject, TripDOObject }
            };
            foreach (var board in boards)
            {
                var ports = board.Ports.Where(p => p.PortPair.Any(pair => pair.Contains("tripDO")));
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
                                    group = new List<OperationDODeviceEnd>();
                                    targetDict[name] = group;
                                }
                                // 添加描述到对应分组
                                group.Add(new OperationDODeviceEnd((_targetDeviceKeeper.TargetDevice, board, port)));
                            }
                        }
                    }
                }
            }

            var a = TripDOObject;
            // 克隆模板节点创建自测提示
            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）"))?.Clone() as Items;
            if (item != null)
            {
                item.Name = "操作插件自测";

                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                if (safety != null)
                {
                    safety.Name = "操作插件由调试人员自测";
                    safety.DllCall.CData = "SpeakString=操作插件由调试人员自测;ExpectString=是否完成;";

                    // 只保留必要的提示节点
                    item.ItemList = item.ItemList.Where(I => I.Name.Equals("操作插件由调试人员自测")).ToList();
                }

                root.ItemList.Add(item);
                NodeName.Add(item.Name);
            }

            return Task.CompletedTask;
        }
    }
}
