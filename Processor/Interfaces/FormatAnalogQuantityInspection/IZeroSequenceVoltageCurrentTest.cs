using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection;
using static SFTemplateGenerator.Helper.Paths.PathSaver;

namespace SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection
{
    public interface IZeroSequenceVoltageCurrentTest
    {
        public Task ZeroSequenceVoltageCurrentProcess(SDL sdl, Items root, KeyValuePair<string, ACDeviceUint> info, TESTER mode);
    }
}
