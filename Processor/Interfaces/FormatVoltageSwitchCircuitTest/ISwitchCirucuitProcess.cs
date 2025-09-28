using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatVoltageSwitchCircuitTest
{
    public interface ISwitchCirucuitProcess
    {
        public Task SwitchCirucuitProcessAsync(SDL sdl, Items root, List<string> NodeName);
    }
}
