using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatVoltageSwitchCircuitTest
{
    public interface IFormatVoltageSwitchCircuitTest
    {
        public Task FormatVoltageSwitchCircuitTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook);
    }
}
