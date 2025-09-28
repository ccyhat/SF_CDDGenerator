using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatDirectCurrentTest;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;
namespace SFTemplateGenerator.Processor.Moduels.FormatDirectCurrentTest
{
    public class FormatDirectCurrentTest : IFormatDirectCurrentTest
    {
        private readonly ITargetDeviceKeeper _targetDeviceKeeper;
        public FormatDirectCurrentTest(
            ITargetDeviceKeeper targetDeviceKeeper
            )
        {
            _targetDeviceKeeper = targetDeviceKeeper;
        }
        public Task FormatDirectCurrentTestAsync(Device targetDevice, SDL sdl, GuideBook guideBook)
        {
            Logger.Info($"直流测试");
            var boards = _targetDeviceKeeper.TargetDevice.Boards.Where(B => !string.IsNullOrEmpty(B.Desc) && DCBORAD_REGEX.IsMatch(B.Desc)).ToArray();
            Logger.Info($"直流插件数量：{boards.Count()}");
            if (boards.Count() == 0)
            {
                Logger.Info($"没有直流插件，不进行直流测试");
                guideBook.Device.Items.RemoveAll(I => I.Name.Equals("直流测试"));
            }
            else
            {
                var root = guideBook.Device.Items.Where(I => I.Name.Equals("直流测试")).FirstOrDefault();
                if (root != null)
                {
                    var item = root.GetItems().FirstOrDefault(I => I.Name.StartsWith("直流测试")).Clone();
                    item.Name = "人工直流测试";
                    var safety = item.GetSafetys().FirstOrDefault(I => I.Name.Equals("接入直流线（DC1）"));
                    safety.DllCall.CData = "SpeakString=进行直流测试;ExpectString=是否完成;";
                    root.ItemList.Clear();
                    root.ItemList.Add(item);
                }
            }
            return Task.CompletedTask;
        }

    }
}
