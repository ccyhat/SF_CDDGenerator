using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;


namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{

    public class FormatAnalogQuantityInspection : IFormatAnalogQuantityInspection
    {

        private readonly IPreTestChecklist _preTestChecklist;
        private readonly IVoltageCheck _voltageCheck;

        public FormatAnalogQuantityInspection(
            IPreTestChecklist preTestChecklist,
            IVoltageCheck voltageCheck
            )
        {
            _preTestChecklist = preTestChecklist;
            _voltageCheck = voltageCheck;
        }
        private List<string> _nodename = new List<string>();
        public async Task FormatAnalogQuantityInspectionAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {

            Logger.Info($"模拟量检查");
            var boards = TargetDevice.Boards.Where(B => ACBORAD_REGEX.Any(R=>R.IsMatch(B.Desc))).ToList();
            Logger.Info($"交流插件数量：{boards.Count()}");
            if (boards.Count() == 0)
            {
                Logger.Info($"没有交流插件，不进行模拟量检查");
                guideBook.Device.Items.RemoveAll(I => I.Name.Equals("模拟量检查"));

            }
            else
            {
                var root = guideBook.Device.Items.Where(I => I.Name.Equals("模拟量检查")).FirstOrDefault();
                await _preTestChecklist.PrepareAsync(sdl, root, _nodename);
                await _voltageCheck.VoltageCheckProcess(sdl, root, _nodename);
                //去掉多余节点              
                root.ItemList.RemoveAll(I => !_nodename.Contains(I.Name));
            }
        }
    }
}