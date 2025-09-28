using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using static SFTemplateGenerator.Helper.Paths.PathSaver;
using static SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection.FormatAnalogQuantityInspection;

namespace SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection
{
    public interface IZeroSequenceVoltageCurrentTest
    {
        public Task ZeroSequenceVoltageCurrentProcess(SDL sdl, Items root, KeyValuePair<string, CoreInfo> info, TESTER mode);
    }
}
