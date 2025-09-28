using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

using SFTemplateGenerator.Processor.Interfaces.FormatOperationCircuitTest;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;

namespace SFTemplateGenerator.Processor.Moduels.FormatOperationCircuitTest
{
    public class FormatOperationCircuitTest : IFormatOperationCircuitTest
    {
        private readonly IOperationCirucuitProcess _operationCirucuitProcess;
        private List<string> _nodename = new List<string>();
        public FormatOperationCircuitTest(
            IOperationCirucuitProcess operationCirucuitProcess
            )
        {
            _operationCirucuitProcess = operationCirucuitProcess;
        }
        public Task FormatOperationCircuitTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {

            Logger.Info($"操作回路测试");
            var boards = TargetDevice.Boards.Where(B => OPBORAD_REGEX.IsMatch(B.Desc)).ToList();
            Logger.Info($"操作插件数量：{boards.Count()}");
            if (boards.Count() == 0)
            {
                Logger.Info($"没有操作插件，不进行操作回路测试");
                guideBook.Device.Items.RemoveAll(I => I.Name.Equals("操作回路测试"));
            }
            else
            {
                var root = guideBook.Device.Items.Where(I => I.Name.Equals("操作回路测试")).FirstOrDefault();
                _operationCirucuitProcess.OperationCirucuitProcessAsync(sdl, root, _nodename);
                //去掉多余节点
                root.ItemList.RemoveAll(I => !_nodename.Contains(I.Name));
            }

            return Task.CompletedTask;
        }
    }
}
