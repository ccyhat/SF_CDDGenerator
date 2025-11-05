using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatPowerPluginTest;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;

namespace SFTemplateGenerator.Processor.Moduels.FormatPowerPluginTest
{
    public class FormatPowerPluginTest : IFormatPowerPluginTest
    {
        private readonly IFailAlarmTest _failAlarmTest;
        public FormatPowerPluginTest(IFailAlarmTest failAlarmTest)
        {
            _failAlarmTest = failAlarmTest;
        }
        public async Task FormatPowerPluginTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {
            Logger.Info($"电源插件检查");
            var boards = TargetDevice.Boards.Where(B => POWERBORAD_REGEX.Any(R=>R.IsMatch(B.Desc))).ToList();
            Logger.Info($"电源插件数量：{boards.Count()}");
            if (boards.Count() == 0)
            {
                Logger.Info($"没有电源插件，不进行电源插件检查");
                guideBook.Device.Items.RemoveAll(I => I.Name.Equals("电源插件检查"));
            }
            else
            {
                var root = guideBook.Device.Items.Where(I => I.Name.Equals("电源插件检查")).FirstOrDefault();
                bool isSuccess = await _failAlarmTest.FailAlarmTestAsync(sdl, root);
                if (!isSuccess)
                {
                    guideBook.Device.Items.RemoveAll(I => I.Name.Equals("电源插件检查"));
                }
            }
        }
    }
}
