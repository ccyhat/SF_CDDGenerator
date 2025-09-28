using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatNetworkPortTest;

namespace SFTemplateGenerator.Processor.Moduels.FormatNetworkPortTest
{
    public class FormatNetworkPortTest : IFormatNetworkPortTest
    {
        public Task FormatNetworkPortTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {
            Logger.Info($"以太网测试");
            Logger.Info($"目前不进行以太网测试");
            guideBook.Device.Items.RemoveAll(I => I.Name.Equals("以太网测试"));
            return Task.CompletedTask;
        }
    }
}
