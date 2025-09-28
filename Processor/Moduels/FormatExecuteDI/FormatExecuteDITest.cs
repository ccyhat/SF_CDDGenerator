using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatExecuteDI;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;

namespace SFTemplateGenerator.Processor.Moduels.FormatExecuteDI
{
    public class FormatExecuteDITest : IFormatExecuteDITest
    {
        private readonly IPrepareExecuteDI _prepareExecuteDI;
        private readonly IExecuteDIProcess _executeDIProcess;
        private List<string> _nodename = new List<string>();
        public FormatExecuteDITest(
            IPrepareExecuteDI prepareExecuteDI,
            IExecuteDIProcess executeDIProcess)
        {
            _prepareExecuteDI = prepareExecuteDI;
            _executeDIProcess = executeDIProcess;
        }
        public async Task FormatExecuteDIAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {
            Logger.Info($"开入检测");
            var boards = TargetDevice.Boards.Where(B => DIBORAD_REGEX.IsMatch(B.Desc)).ToList();
            Logger.Info($"开入插件数量：{boards.Count()}");
            if (boards.Count() == 0)
            {
                Logger.Info($"没有开入插件，不进行开入检测");
                guideBook.Device.Items.RemoveAll(I => I.Name.Equals("开入检测"));
            }
            else
            {
                var root = guideBook.Device.Items.Where(I => I.Name.Equals("开入检测")).FirstOrDefault();
                await _prepareExecuteDI.PrepareExecuteDIAsync(_nodename);
                await _executeDIProcess.ExecuteDIProcessAsync(sdl, root, _nodename);
                //去掉多余节点
                root.ItemList.RemoveAll(I => !_nodename.Contains(I.Name));
            }
        }
    }
}
