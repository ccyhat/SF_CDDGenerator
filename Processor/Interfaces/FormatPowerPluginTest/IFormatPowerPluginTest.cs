using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatPowerPluginTest
{
    public interface IFormatPowerPluginTest
    {
        public Task FormatPowerPluginTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook);
    }
}
