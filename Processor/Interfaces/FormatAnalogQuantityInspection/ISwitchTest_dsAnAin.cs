using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection;
using static SFTemplateGenerator.Helper.Paths.PathSaver;
using static SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection.FormatAnalogQuantityInspection;

namespace SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection
{
    public interface ISwitchTest_dsAnAin
    {
        public Task SwitchTest_dsAnAinProcess(SDL sdl, Items root, KeyValuePair<(string, string), ACDeviceUint> info, TESTER tester);

    }
}
