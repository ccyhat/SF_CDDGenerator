using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection
{
    public interface IVoltageCheck
    {
        Task VoltageCheckProcess(SDL sdl, Items root, List<string> _nodename);
    }
}
