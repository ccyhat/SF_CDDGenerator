using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatVoltageSwitchCircuitTest;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;

namespace SFTemplateGenerator.Processor.Moduels.FormatVoltageSwitchCircuitTest
{
    public class FormatVoltageSwitchCircuitTest : IFormatVoltageSwitchCircuitTest
    {
        private readonly ISwitchCirucuitProcess _switchCirucuitProcess;
        private List<string> _nodename = new List<string>();
        public FormatVoltageSwitchCircuitTest(
            ISwitchCirucuitProcess switchCirucuitProcess
            )
        {
            _switchCirucuitProcess = switchCirucuitProcess;
        }
        public Task FormatVoltageSwitchCircuitTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {
            Logger.Info($"电压切换回路测试");
            var boards = TargetDevice.Boards.Where(B => SWITCHBORAD_REGEX.IsMatch(B.Desc)).ToList();
            Logger.Info($"电压切换插件数量：{boards.Count}");
            if (boards.Count() == 0)
            {
                Logger.Info($"没有切换插件，不进行电压切换回路测试");
                guideBook.Device.Items.RemoveAll(I => I.Name.Equals("电压切换回路测试"));
            }
            else
            {
                var root = guideBook.Device.Items.Where(I => I.Name.Equals("电压切换回路测试")).FirstOrDefault()!;
                _switchCirucuitProcess.SwitchCirucuitProcessAsync(sdl, root, _nodename);
                //去掉多余节点
                root.ItemList.RemoveAll(I => !_nodename.Contains(I.Name));
            }
            return Task.CompletedTask;
        }
    }
}
